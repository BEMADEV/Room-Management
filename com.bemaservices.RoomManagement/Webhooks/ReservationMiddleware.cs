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

                context.Response.Headers.Add( "content-disposition", new[] { string.Format( "attachment; filename={0}_ical.ics", RockDateTime.Now.ToString( "yyyy-MM-dd_hhmmss" ) ) } );
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

        private void SendNotAuthorized( IOwinContext context )
        {
            context.Response.StatusCode = ( int ) HttpStatusCode.Forbidden;
            context.Response.ReasonPhrase = "Not authorized to view reservation type.";
        }

        private void SendBadRequest( IOwinContext context, string addlInfo = "" )
        {
            context.Response.StatusCode = ( int ) HttpStatusCode.BadRequest;
            context.Response.ReasonPhrase = "Request is invalid or malformed. " + addlInfo;
        }

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

            var authorizedReservationTypeIds = new List<int>();
            foreach ( var reservationType in potentialReservationTypeQry )
            {
                if ( reservationType.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    authorizedReservationTypeIds.Add( reservationType.Id );
                }
            }

            reservationCalendarOptions.ReservationTypeIds = authorizedReservationTypeIds;

            if ( reservationCalendarOptions.ReservationTypeIds.Any() )
            {
                return true;
            }

            SendNotAuthorized( context );
            return false;
        }

        private ReservationCalendarOptions ValidateRequestData( IOwinContext context )
        {
            var query = context.Request.Query;
            ReservationCalendarOptions reservationCalendarOptions = new ReservationCalendarOptions();

            reservationCalendarOptions.Name = query.Get( "name" ) ?? string.Empty;

            reservationCalendarOptions.ApprovalsByPersonId = query.Get( "approvalsbypersonid" ).AsIntegerOrNull();
            reservationCalendarOptions.ReservationsByPersonId = query.Get( "reservationsbypersonid" ).AsIntegerOrNull();
            reservationCalendarOptions.CreatorPersonId = query.Get( "creatorpersonid" ).AsIntegerOrNull();
            reservationCalendarOptions.EventContactPersonId = query.Get( "eventcontactpersonid" ).AsIntegerOrNull();
            reservationCalendarOptions.AdministrativeContactPersonId = query.Get( "administrativecontactpersonid" ).AsIntegerOrNull();
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
                approvalStates = new List<ReservationApprovalState> {
                    ReservationApprovalState.Approved,
                    ReservationApprovalState.PendingInitialApproval,
                    ReservationApprovalState.PendingSpecialApproval,
                    ReservationApprovalState.PendingFinalApproval,
                    ReservationApprovalState.ChangesNeeded
                };
            }
            reservationCalendarOptions.ApprovalStates = approvalStates;

            string startDate = query.Get( "startdate" );
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                reservationCalendarOptions.StartDate = DateTime.ParseExact( startDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }
            else
            {
                reservationCalendarOptions.StartDate = DateTime.Now.AddMonths( -3 ).Date;
            }

            string endDate = query.Get( "enddate" );
            if ( !string.IsNullOrWhiteSpace( endDate ) )
            {
                reservationCalendarOptions.EndDate = DateTime.ParseExact( endDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }
            else
            {
                reservationCalendarOptions.EndDate = DateTime.Now.AddMonths( 12 ).Date;
            }

            return reservationCalendarOptions;
        }
    }
}
