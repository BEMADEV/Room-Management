<%@ WebHandler Language="C#" Class="com.bemaservices.Webhooks.GetReservationCalendarFeed" %>

using System;
using System.Collections.Generic;
using System.Web;

using Ical.Net;
using Ical.Net.Serialization;
using Ical.Net.DataTypes;
using Calendar = Ical.Net.Calendar;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using com.bemaservices.RoomManagement.Model;
using System.Linq;
using System.Net;
using System.Globalization;

namespace com.bemaservices.Webhooks
{
    public class GetReservationCalendarFeed : IHttpHandler
    {
        private HttpRequest request;
        private HttpResponse response;
        private string interactionDeviceType;

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest( HttpContext httpContext )
        {
            try
            {
                request = httpContext.Request;
                response = httpContext.Response;

                if ( !ValidateSecurity( httpContext ) )
                {
                    return;
                }

                RockContext rockContext = new RockContext();
                ReservationCalendarOptions reservationCalendarOptions = ValidateRequestData( httpContext );

                if ( reservationCalendarOptions == null )
                {
                    return;
                }

                reservationCalendarOptions.ClientDeviceType = InteractionDeviceType.GetClientType( request.UserAgent );

                var reservationService = new ReservationService( rockContext );
                var icalendarString = reservationService.CreateICalendar( reservationCalendarOptions );

                response.Clear();
                response.ClearHeaders();
                response.ClearContent();
                response.AddHeader( "content-disposition", string.Format( "attachment; filename={0}_ical.ics", RockDateTime.Now.ToString( "yyyy-MM-dd_hhmmss" ) ) );
                response.ContentType = "text/calendar";
                response.Write( icalendarString );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, httpContext );
                SendBadRequest( httpContext );
            }
        }       

        /// <summary>
        /// Sends the not authorized response
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private void SendNotAuthorized( HttpContext httpContext )
        {
            httpContext.Response.StatusCode = HttpStatusCode.Forbidden.ConvertToInt();
            httpContext.Response.StatusDescription = "Not authorized to view calendar.";
            httpContext.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Sends the bad request response
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="addlInfo">The addl information.</param>
        private void SendBadRequest( HttpContext httpContext, string addlInfo = "" )
        {
            httpContext.Response.StatusCode = HttpStatusCode.BadRequest.ConvertToInt();
            httpContext.Response.StatusDescription = "Request is invalid or malformed. " + addlInfo;
            httpContext.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Ensure the current user is authorized to view the calendar. If all are allowed then current user is not evaluated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private bool ValidateSecurity( HttpContext context )
        {
            return true;
        }

        /// <summary>
        /// Validates the request data.
        /// </summary><a href="../Scripts/">../Scripts/</a>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private ReservationCalendarOptions ValidateRequestData( HttpContext context )
        {
            ReservationCalendarOptions reservationCalendarOptions = new ReservationCalendarOptions();

            reservationCalendarOptions.Name = request.QueryString["name"] != null ? request.QueryString["name"] : string.Empty;

            reservationCalendarOptions.ApprovalsByPersonId = ( request.QueryString["approvalsbypersonid"] != null ? request.QueryString["approvalsbypersonid"] : string.Empty ).AsIntegerOrNull();
            reservationCalendarOptions.ReservationsByPersonId = ( request.QueryString["reservationsbypersonid"] != null ? request.QueryString["reservationsbypersonid"] : string.Empty ).AsIntegerOrNull();
            reservationCalendarOptions.CreatorPersonId = ( request.QueryString["creatorpersonid"] != null ? request.QueryString["creatorpersonid"] : string.Empty ).AsIntegerOrNull();
            reservationCalendarOptions.EventContactPersonId = ( request.QueryString["eventcontactpersonid"] != null ? request.QueryString["eventcontactpersonid"] : string.Empty ).AsIntegerOrNull();
            reservationCalendarOptions.AdministrativeContactPersonId = ( request.QueryString["administrativecontactpersonid"] != null ? request.QueryString["administrativecontactpersonid"] : string.Empty ).AsIntegerOrNull();

            reservationCalendarOptions.ReservationTypeIds = ( request.QueryString["reservationtypeids"] != null ? request.QueryString["reservationtypeids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.ReservationIds = ( request.QueryString["reservationids"] != null ? request.QueryString["reservationids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.LocationIds =  ( request.QueryString["locationids"] != null ? request.QueryString["locationids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.ResourceIds =  ( request.QueryString["resourceids"] != null ? request.QueryString["resourceids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.CampusIds =  ( request.QueryString["campusids"] != null ? request.QueryString["campusids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationCalendarOptions.MinistryIds =  ( request.QueryString["ministryids"] != null ? request.QueryString["ministryids"] : string.Empty ).SplitDelimitedValues().AsIntegerList();

            reservationCalendarOptions.MinistryNames = ( request.QueryString["ministrynames"] != null ? request.QueryString["ministrynames"] : string.Empty ).SplitDelimitedValues().Where( s => s.IsNotNullOrWhiteSpace() ).ToList();

            var approvalStates = ( request.QueryString["approvalstates"] != null ? request.QueryString["approvalstates"] : string.Empty ).SplitDelimitedValues().AsEnumList<ReservationApprovalState>();
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

            string startDate = request.QueryString["startdate"];
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                reservationCalendarOptions.StartDate = DateTime.ParseExact( startDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }
            else
            {
                reservationCalendarOptions.StartDate = DateTime.Now.AddMonths( -3 ).Date;
            }

            string endDate = request.QueryString["enddate"];
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