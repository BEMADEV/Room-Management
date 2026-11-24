using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.bemaservices.RoomManagement.Model;
using Microsoft.Owin;
using Rock.Data;
using Rock.Model;
using Rock;
using Rock.Web.Cache;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;
using System.Net;

namespace com.bemaservices.RoomManagement.Webhooks
{
    public class ReservationMiddleware : OwinMiddleware
    {
        public ReservationMiddleware( OwinMiddleware next )
            : base( next )
        {
        }

        /// <inheritdoc/>
        public override async Task Invoke( IOwinContext context )
        {
            var path = context.Request.Uri.AbsolutePath;

            if ( !path.EndsWith( "/GetReservationCalendarFeed.ashx", StringComparison.OrdinalIgnoreCase ) )
            {
                await Next.Invoke( context );
                return;
            }

            try
            {
                RockContext rockContext = new RockContext();
                ReservationCalendarOptions reservationCalendarOptions = ValidateRequestData( context );

                if ( reservationCalendarOptions == null )
                {
                    SendBadRequest( context );
                    return;
                }

                if ( !ValidateSecurity( context, reservationCalendarOptions ) )
                {
                    return;
                }

                reservationCalendarOptions.ClientDeviceType = InteractionDeviceType.GetClientType( context.Request.Headers["User-Agent"] );

                var reservationService = new ReservationService( rockContext );
                var icalendarString = reservationService.CreateICalendar( reservationCalendarOptions );

                context.Response.Headers.Add( "content-disposition", new[] { string.Format( "attachment; filename={0}_ical.ics", RockDateTime.Now.ToString( "yyyy-MM-dd_HHmmss" ) ) } );
                context.Response.ContentType = "text/calendar";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync( icalendarString );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                SendBadRequest( context );
            }
        }

        /// <summary>
        /// Responds 403 Forbidden: the caller may not view the requested reservation data.
        /// </summary>
        /// <param name="context">The OWIN request context.</param>
        private void SendNotAuthorized( IOwinContext context )
        {
            context.Response.StatusCode = ( int ) HttpStatusCode.Forbidden;
            context.Response.ReasonPhrase = "Not authorized to view requested reservation data.";
        }

        /// <summary>
        /// Responds 400 Bad Request, adding any detail about what was wrong to the reason phrase.
        /// </summary>
        /// <param name="context">The OWIN request context.</param>
        /// <param name="additionalInfo">Extra detail about what was wrong, if any.</param>
        private void SendBadRequest( IOwinContext context, string additionalInfo = "" )
        {
            context.Response.StatusCode = ( int ) HttpStatusCode.BadRequest;
            context.Response.ReasonPhrase = "Request is invalid or malformed. " + additionalInfo;
        }

        /// <summary>
        /// Restricts the calendar feed to reservation types and data views the caller may use. A data view
        /// must also be a Reservation data view, since its ids are used as reservation ids. Person filters given
        /// by Id are limited to the caller themselves or an administrator; see <see cref="TryGetPersonFilter" />.
        /// </summary>
        /// <param name="context">The OWIN request context.</param>
        /// <param name="reservationCalendarOptions">The requested calendar filters.</param>
        /// <returns><c>true</c> when the caller may continue; otherwise, sends a forbidden response, or a bad
        /// request response for a data view of another entity type.</returns>
        private bool ValidateSecurity( IOwinContext context, ReservationCalendarOptions reservationCalendarOptions )
        {
            RockContext rockContext = new RockContext();
            ReservationTypeService reservationTypeService = new ReservationTypeService( rockContext );

            var potentialReservationTypeQry = reservationTypeService.Queryable();
            if ( reservationCalendarOptions.ReservationTypeIds.Any() )
            {
                potentialReservationTypeQry = potentialReservationTypeQry.Where( rt => reservationCalendarOptions.ReservationTypeIds.Contains( rt.Id ) );
            }

            UserLogin currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            if ( reservationCalendarOptions.DataViewId.HasValue )
            {
                var dataView = DataViewCache.Get( reservationCalendarOptions.DataViewId.Value );
                if ( dataView == null || !dataView.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    SendNotAuthorized( context );
                    return false;
                }

                // Only a Reservation data view can filter the feed; any other type's ids aren't reservation ids
                if ( dataView.EntityTypeId != EntityTypeCache.GetId<Reservation>() )
                {
                    SendBadRequest( context, "The data view must be a Reservation data view." );
                    return false;
                }
            }

            var authorizedReservationTypes = potentialReservationTypeQry
                .ToList()
                .Where( rt => rt.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .ToList();

            reservationCalendarOptions.ReservationTypeIds = authorizedReservationTypes.Select( rt => rt.Id ).ToList();

            if ( !reservationCalendarOptions.ReservationTypeIds.Any() )
            {
                SendNotAuthorized( context );
                return false;
            }

            // The older *personid filters use a guessable person Id, so only the logged-in person themselves, or someone
            // who can administrate every reservation type in the feed, may use them. The *personguid filters are open to
            // anyone, since calendar apps subscribe without logging in.
            var query = context.Request.Query;
            var personIdsFromIdParameters = PersonFilterPrefixes
                .Where( prefix => string.IsNullOrWhiteSpace( query.Get( prefix + "guid" ) ) )
                .Select( prefix => query.Get( prefix + "id" ).AsIntegerOrNull() )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .Distinct()
                .ToList();

            if ( personIdsFromIdParameters.Any() )
            {
                var isSelf = currentPerson != null && personIdsFromIdParameters.All( id => id == currentPerson.Id );
                var isAdmin = currentPerson != null && authorizedReservationTypes.All( rt => rt.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, currentPerson ) );
                if ( !isSelf && !isAdmin )
                {
                    SendNotAuthorized( context );
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The person filter parameters, without their "id" or "guid" suffix (for example "creatorperson" for
        /// creatorpersonid and creatorpersonguid).
        /// </summary>
        private static readonly string[] PersonFilterPrefixes =
        {
            "approvalsbyperson",
            "reservationsbyperson",
            "creatorperson",
            "eventcontactperson",
            "administrativecontactperson"
        };

        /// <summary>
        /// Reads one person filter from its Guid parameter (for example creatorpersonguid) or, failing that, its older Id
        /// parameter (creatorpersonid). The Guid is preferred because it can't be guessed, so feeds that use it can be shared
        /// with calendar apps that subscribe without logging in.
        /// </summary>
        /// <param name="query">The request's query string.</param>
        /// <param name="personService">The person service used to look up the Guid.</param>
        /// <param name="parameterPrefix">The parameter name without its "id" or "guid" suffix.</param>
        /// <param name="personId">The person identifier to filter by, or <c>null</c> if neither parameter was given.</param>
        /// <returns><c>false</c> if a Guid was given that doesn't match a person; otherwise, <c>true</c>.</returns>
        private static bool TryGetPersonFilter( IReadableStringCollection query, PersonService personService, string parameterPrefix, out int? personId )
        {
            personId = query.Get( parameterPrefix + "id" ).AsIntegerOrNull();

            var personGuidValue = query.Get( parameterPrefix + "guid" );
            if ( string.IsNullOrWhiteSpace( personGuidValue ) )
            {
                return true;
            }

            var personGuid = personGuidValue.AsGuidOrNull();
            var person = personGuid.HasValue ? personService.Get( personGuid.Value ) : null;
            if ( person == null )
            {
                return false;
            }

            personId = person.Id;
            return true;
        }

        /// <summary>
        /// Reads calendar-feed filters from the request and supplies default dates in Rock's local time zone.
        /// </summary>
        /// <param name="context">The OWIN request context.</param>
        /// <returns>The calendar-feed filter options used by the request handler.</returns>
        private ReservationCalendarOptions ValidateRequestData( IOwinContext context )
        {
            var query = context.Request.Query;
            ReservationCalendarOptions reservationCalendarOptions = new ReservationCalendarOptions();

            reservationCalendarOptions.Name = query.Get( "name" ) ?? string.Empty;

            // Person filters: a *personguid parameter wins over its older *personid parameter. An unknown Guid is a bad
            // request rather than an ignored filter, which would widen the feed. ValidateSecurity limits the Id parameters.
            var personService = new PersonService( new RockContext() );
            int? personId;

            if ( !TryGetPersonFilter( query, personService, "approvalsbyperson", out personId ) )
            {
                return null;
            }

            reservationCalendarOptions.ApprovalsByPersonId = personId;

            if ( !TryGetPersonFilter( query, personService, "reservationsbyperson", out personId ) )
            {
                return null;
            }

            reservationCalendarOptions.ReservationsByPersonId = personId;

            if ( !TryGetPersonFilter( query, personService, "creatorperson", out personId ) )
            {
                return null;
            }

            reservationCalendarOptions.CreatorPersonId = personId;

            if ( !TryGetPersonFilter( query, personService, "eventcontactperson", out personId ) )
            {
                return null;
            }

            reservationCalendarOptions.EventContactPersonId = personId;

            if ( !TryGetPersonFilter( query, personService, "administrativecontactperson", out personId ) )
            {
                return null;
            }

            reservationCalendarOptions.AdministrativeContactPersonId = personId;
            reservationCalendarOptions.DataViewId = query.Get( "dataviewid" ).AsIntegerOrNull();

            reservationCalendarOptions.ReservationTypeIds = ( query.Get( "reservationtypeids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.ReservationIds = ( query.Get( "reservationids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.LocationIds = ( query.Get( "locationids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.ResourceIds = ( query.Get( "resourceids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.CampusIds = ( query.Get( "campusids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.MinistryIds = ( query.Get( "ministryids" ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();

            reservationCalendarOptions.MinistryNames = ( query.Get( "ministrynames" ) ?? string.Empty ).SplitDelimitedValues().Where( s => s.IsNotNullOrWhiteSpace() ).ToList();

            var approvalStates = ( query.Get( "approvalstates" ) ?? string.Empty ).SplitDelimitedValues().AsEnumList<ReservationApprovalState>();
            if ( !approvalStates.Any() )
            {
                approvalStates = ReservationApprovalStateSets.GetActiveStates();
            }
            reservationCalendarOptions.ApprovalStates = approvalStates;

            var currentDateTime = RockDateTime.Now;
            string startDate = query.Get( "startdate" );
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                reservationCalendarOptions.StartDate = DateTime.ParseExact( startDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }
            else
            {
                reservationCalendarOptions.StartDate = currentDateTime.AddMonths( -3 ).Date;
            }

            string endDate = query.Get( "enddate" );
            if ( !string.IsNullOrWhiteSpace( endDate ) )
            {
                reservationCalendarOptions.EndDate = DateTime.ParseExact( endDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }
            else
            {
                reservationCalendarOptions.EndDate = currentDateTime.AddMonths( 12 ).Date;
            }

            return reservationCalendarOptions;
        }
    }
}
