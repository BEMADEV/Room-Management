// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock;
using Rock.Attribute;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    /// <summary>
    /// Scheduling provider for Microsoft 365 / Exchange Online using Microsoft Graph API.
    /// </summary>
    [Export( typeof( SchedulingProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Microsoft Scheduling Assistant" )]
    [Rock.SystemGuid.EntityTypeGuid( "3ED7D672-76A4-41F4-9788-0404B997CC48" )]
    [TextField( "Tenant ID", "The Azure AD Tenant ID for your organization.", true, order: 0, key: "TenantId" )]
    [TextField( "Client ID", "The Application (client) ID from your Azure AD app registration.", true, order: 1, key: "ClientId" )]
    [TextField( "Client Secret", "The client secret from your Azure AD app registration.", true, order: 2, key: "ClientSecret" )]
    public class MicrosoftSchedulingAssistant : SchedulingProviderComponent
    {
        private const string GraphApiBaseUrl = "https://graph.microsoft.com/v1.0";
        private const string AuthUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";

        /// <summary>
        /// Gets provider events for a specific location from Microsoft Graph.
        /// </summary>
        public override List<SchedulingProviderEvent> GetProviderEventsForLocation(
            SchedulingProvider schedulingProvider,
            string externalId,
            DateTime? startDate,
            DateTime? endDate,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var events = new List<SchedulingProviderEvent>();

            try
            {
                var accessToken = GetAccessToken( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return events;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                var filter = BuildDateFilter( startDate, endDate );
                var url = $"{GraphApiBaseUrl}/users/{externalId}/events{filter}";

                var response = client.GetAsync( url ).Result;
                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to get events: {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}" );
                    return events;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var json = JObject.Parse( content );
                var eventItems = json["value"] as JArray;

                if ( eventItems != null )
                {
                    foreach ( var item in eventItems )
                    {
                        events.Add( ConvertFromGraphEvent( item as JObject, externalId ) );
                    }
                }
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception getting events: {ex.Message}" );
            }

            return events;
        }

        /// <summary>
        /// Gets a single provider event by its external identifier.
        /// </summary>
        public override SchedulingProviderEvent GetProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var accessToken = GetAccessToken( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                // Extract room email from the event ID metadata or use default approach
                // For now, we'll need to parse from metadata - this might need adjustment based on your implementation
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                // Note: This assumes the externalEventId is in format "roomEmail|eventId"
                var parts = externalEventId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format" );
                    return null;
                }

                var roomEmail = parts[0];
                var eventId = parts[1];

                var url = $"{GraphApiBaseUrl}/users/{roomEmail}/events/{eventId}";
                var response = client.GetAsync( url ).Result;

                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to get event: {response.StatusCode}" );
                    return null;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var eventJson = JObject.Parse( content );
                return ConvertFromGraphEvent( eventJson, roomEmail );
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception getting event: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Creates a new event in Microsoft Graph.
        /// </summary>
        public override SchedulingProviderEvent CreateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var accessToken = GetAccessToken( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                // Get the first location to determine which calendar to create the event in
                var primaryLocation = providerEvent.Locations.FirstOrDefault();
                if ( primaryLocation == null )
                {
                    errorMessages.Add( "Event must have at least one location" );
                    return null;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                var eventData = ConvertToGraphEvent( providerEvent );
                var jsonContent = JsonConvert.SerializeObject( eventData );
                var httpContent = new StringContent( jsonContent, Encoding.UTF8, "application/json" );

                var url = $"{GraphApiBaseUrl}/users/{primaryLocation.ExternalId}/events";
                var response = client.PostAsync( url, httpContent ).Result;

                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to create event: {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}" );
                    return null;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var createdEvent = JObject.Parse( content );
                var result = ConvertFromGraphEvent( createdEvent, primaryLocation.ExternalId );
                result.ExternalId = $"{primaryLocation.ExternalId}|{createdEvent["id"].ToString()}";

                return result;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception creating event: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Updates an existing event in Microsoft Graph.
        /// </summary>
        public override bool UpdateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var accessToken = GetAccessToken( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return false;
                }

                if ( string.IsNullOrWhiteSpace( providerEvent.ExternalId ) )
                {
                    errorMessages.Add( "External ID is required for update" );
                    return false;
                }

                var parts = providerEvent.ExternalId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format" );
                    return false;
                }

                var roomEmail = parts[0];
                var eventId = parts[1];

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                var eventData = ConvertToGraphEvent( providerEvent );
                var jsonContent = JsonConvert.SerializeObject( eventData );
                var httpContent = new StringContent( jsonContent, Encoding.UTF8, "application/json" );

                var url = $"{GraphApiBaseUrl}/users/{roomEmail}/events/{eventId}";
                var request = new HttpRequestMessage( new HttpMethod( "PATCH" ), url )
                {
                    Content = httpContent
                };
                var response = client.SendAsync( request ).Result;

                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to update event: {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}" );
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception updating event: {ex.Message}" );
                return false;
            }
        }

        /// <summary>
        /// Deletes an event from Microsoft Graph.
        /// </summary>
        public override bool DeleteProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var accessToken = GetAccessToken( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return false;
                }

                var parts = externalEventId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format" );
                    return false;
                }

                var roomEmail = parts[0];
                var eventId = parts[1];

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                var url = $"{GraphApiBaseUrl}/users/{roomEmail}/events/{eventId}";
                var response = client.DeleteAsync( url ).Result;

                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to delete event: {response.StatusCode}" );
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception deleting event: {ex.Message}" );
                return false;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Gets an access token for Microsoft Graph API.
        /// </summary>
        private string GetAccessToken( SchedulingProvider schedulingProvider, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                schedulingProvider.LoadAttributes();
                var tenantId = schedulingProvider.GetAttributeValue( "TenantId" );
                var clientId = schedulingProvider.GetAttributeValue( "ClientId" );
                var clientSecret = schedulingProvider.GetAttributeValue( "ClientSecret" );

                if ( string.IsNullOrWhiteSpace( tenantId ) || string.IsNullOrWhiteSpace( clientId ) || string.IsNullOrWhiteSpace( clientSecret ) )
                {
                    errorMessages.Add( "Missing required authentication configuration" );
                    return null;
                }

                var client = new HttpClient();
                var tokenUrl = string.Format( AuthUrl, tenantId );

                var requestData = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "scope", "https://graph.microsoft.com/.default" },
                    { "grant_type", "client_credentials" }
                };

                var request = new HttpRequestMessage( HttpMethod.Post, tokenUrl )
                {
                    Content = new FormUrlEncodedContent( requestData )
                };

                var response = client.SendAsync( request ).Result;
                if ( !response.IsSuccessStatusCode )
                {
                    errorMessages.Add( $"Failed to authenticate: {response.StatusCode}" );
                    return null;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var json = JObject.Parse( content );
                return json["access_token"]?.ToString();
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception during authentication: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Builds a date filter for the Graph API query.
        /// </summary>
        private string BuildDateFilter( DateTime? startDate, DateTime? endDate )
        {
            var filters = new List<string>();

            if ( startDate.HasValue )
            {
                filters.Add( $"start/dateTime ge '{startDate.Value:yyyy-MM-ddTHH:mm:ss}'" );
            }

            if ( endDate.HasValue )
            {
                filters.Add( $"end/dateTime le '{endDate.Value:yyyy-MM-ddTHH:mm:ss}'" );
            }

            return filters.Any() ? $"?$filter={string.Join( " and ", filters )}" : string.Empty;
        }

        /// <summary>
        /// Converts a Microsoft Graph event to a SchedulingProviderEvent.
        /// </summary>
        private SchedulingProviderEvent ConvertFromGraphEvent( JObject graphEvent, string roomEmail )
        {
            var providerEvent = new SchedulingProviderEvent
            {
                ExternalId = $"{roomEmail}|{graphEvent["id"]}",
                Title = graphEvent["subject"]?.ToString(),
                Description = graphEvent["bodyPreview"]?.ToString(),
                Status = graphEvent["isCancelled"]?.ToObject<bool>() == true ? "cancelled" : "confirmed",
                CreatedDateTime = graphEvent["createdDateTime"]?.ToObject<DateTime>(),
                ModifiedDateTime = graphEvent["lastModifiedDateTime"]?.ToObject<DateTime>(),
                ICalendarContent = GenerateICalendarContent( graphEvent )
            };

            // Parse start and end times
            var start = graphEvent["start"];
            if ( start != null )
            {
                providerEvent.StartDateTime = DateTime.Parse( start["dateTime"]?.ToString() );
            }

            var end = graphEvent["end"];
            if ( end != null )
            {
                providerEvent.EndDateTime = DateTime.Parse( end["dateTime"]?.ToString() );
            }

            // Parse organizer
            var organizer = graphEvent["organizer"]?["emailAddress"];
            if ( organizer != null )
            {
                providerEvent.Organizer = new SchedulingProviderPerson
                {
                    DisplayName = organizer["name"]?.ToString(),
                    Email = organizer["address"]?.ToString()
                };
            }

            // Parse attendees
            var attendees = graphEvent["attendees"] as JArray;
            if ( attendees != null )
            {
                foreach ( var attendee in attendees )
                {
                    var email = attendee["emailAddress"];
                    if ( email != null )
                    {
                        providerEvent.Attendees.Add( new SchedulingProviderPerson
                        {
                            DisplayName = email["name"]?.ToString(),
                            Email = email["address"]?.ToString()
                        } );
                    }
                }
            }

            // Parse locations
            var locations = graphEvent["locations"] as JArray;
            if ( locations != null )
            {
                foreach ( var location in locations )
                {
                    providerEvent.Locations.Add( new Data.SchedulingProviderLocation
                    {
                        Name = location["displayName"]?.ToString(),
                        Email = location["locationEmailAddress"]?.ToString()
                    } );
                }
            }

            return providerEvent;
        }

        /// <summary>
        /// Converts a SchedulingProviderEvent to Microsoft Graph event format.
        /// </summary>
        private object ConvertToGraphEvent( SchedulingProviderEvent providerEvent )
        {
            var graphEvent = new
            {
                subject = providerEvent.Title,
                body = new
                {
                    contentType = "Text",
                    content = providerEvent.Description
                },
                start = new
                {
                    dateTime = providerEvent.StartDateTime?.ToString( "yyyy-MM-ddTHH:mm:ss" ),
                    timeZone = "UTC"
                },
                end = new
                {
                    dateTime = providerEvent.EndDateTime?.ToString( "yyyy-MM-ddTHH:mm:ss" ),
                    timeZone = "UTC"
                },
                locations = providerEvent.Locations.Select( l => new
                {
                    displayName = l.Name,
                    locationEmailAddress = l.Email
                } ).ToArray(),
                attendees = providerEvent.Attendees.Select( a => new
                {
                    emailAddress = new
                    {
                        name = a.DisplayName,
                        address = a.Email
                    },
                    type = "required"
                } ).ToArray()
            };

            return graphEvent;
        }

        /// <summary>
        /// Generates iCalendar (ICS) content from a Microsoft Graph event.
        /// </summary>
        private string GenerateICalendarContent( JObject graphEvent )
        {
            if ( graphEvent == null )
            {
                return null;
            }

            var icsBuilder = new StringBuilder();
            icsBuilder.AppendLine( "BEGIN:VCALENDAR" );
            icsBuilder.AppendLine( "VERSION:2.0" );
            icsBuilder.AppendLine( "PRODID:-//Rock RMS//Room Management//EN" );
            icsBuilder.AppendLine( "BEGIN:VEVENT" );

            // UID
            var uid = graphEvent["iCalUId"]?.ToString();
            if ( !string.IsNullOrWhiteSpace( uid ) )
            {
                icsBuilder.AppendLine( $"UID:{uid}" );
            }
            else
            {
                icsBuilder.AppendLine( $"UID:{graphEvent["id"]}" );
            }

            // Summary
            var subject = graphEvent["subject"]?.ToString();
            if ( !string.IsNullOrWhiteSpace( subject ) )
            {
                icsBuilder.AppendLine( $"SUMMARY:{EscapeICalText( subject )}" );
            }

            // Description
            var body = graphEvent["body"];
            if ( body != null )
            {
                var content = body["content"]?.ToString();
                if ( !string.IsNullOrWhiteSpace( content ) )
                {
                    icsBuilder.AppendLine( $"DESCRIPTION:{EscapeICalText( content )}" );
                }
            }

            // Start time
            var start = graphEvent["start"];
            if ( start != null )
            {
                var startDateTime = start["dateTime"]?.ToString();
                if ( !string.IsNullOrWhiteSpace( startDateTime ) )
                {
                    var dt = DateTime.Parse( startDateTime ).ToUniversalTime();
                    icsBuilder.AppendLine( $"DTSTART:{dt:yyyyMMddTHHmmss}Z" );
                }
            }

            // End time
            var end = graphEvent["end"];
            if ( end != null )
            {
                var endDateTime = end["dateTime"]?.ToString();
                if ( !string.IsNullOrWhiteSpace( endDateTime ) )
                {
                    var dt = DateTime.Parse( endDateTime ).ToUniversalTime();
                    icsBuilder.AppendLine( $"DTEND:{dt:yyyyMMddTHHmmss}Z" );
                }
            }

            // Location
            var locations = graphEvent["locations"] as JArray;
            if ( locations != null && locations.Any() )
            {
                var locationNames = locations.Select( l => l["displayName"]?.ToString() ).Where( n => !string.IsNullOrWhiteSpace( n ) );
                var locationString = string.Join( ", ", locationNames );
                if ( !string.IsNullOrWhiteSpace( locationString ) )
                {
                    icsBuilder.AppendLine( $"LOCATION:{EscapeICalText( locationString )}" );
                }
            }

            // Status
            var isCancelled = graphEvent["isCancelled"]?.ToObject<bool>() == true;
            icsBuilder.AppendLine( isCancelled ? "STATUS:CANCELLED" : "STATUS:CONFIRMED" );

            // Created
            var created = graphEvent["createdDateTime"]?.ToObject<DateTime?>();
            if ( created.HasValue )
            {
                var createdUtc = created.Value.ToUniversalTime();
                icsBuilder.AppendLine( $"CREATED:{createdUtc:yyyyMMddTHHmmss}Z" );
            }

            // Last Modified
            var modified = graphEvent["lastModifiedDateTime"]?.ToObject<DateTime?>();
            if ( modified.HasValue )
            {
                var modifiedUtc = modified.Value.ToUniversalTime();
                icsBuilder.AppendLine( $"LAST-MODIFIED:{modifiedUtc:yyyyMMddTHHmmss}Z" );
            }

            // Organizer
            var organizer = graphEvent["organizer"];
            if ( organizer != null )
            {
                var emailAddress = organizer["emailAddress"];
                if ( emailAddress != null )
                {
                    var email = emailAddress["address"]?.ToString();
                    var name = emailAddress["name"]?.ToString();
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        var organizerLine = $"ORGANIZER;CN={EscapeICalText( name ?? email )}:mailto:{email}";
                        icsBuilder.AppendLine( organizerLine );
                    }
                }
            }

            // Attendees
            var attendees = graphEvent["attendees"] as JArray;
            if ( attendees != null )
            {
                foreach ( var attendee in attendees )
                {
                    var emailAddress = attendee["emailAddress"];
                    if ( emailAddress != null )
                    {
                        var email = emailAddress["address"]?.ToString();
                        var name = emailAddress["name"]?.ToString();
                        if ( !string.IsNullOrWhiteSpace( email ) )
                        {
                            var type = attendee["type"]?.ToString()?.ToLower();
                            var role = type == "resource" ? "NON-PARTICIPANT" : "REQ-PARTICIPANT";
                            var responseStatus = attendee["status"]?["response"]?.ToString();
                            var partstat = ConvertResponseStatus( responseStatus );
                            var attendeeLine = $"ATTENDEE;ROLE={role};PARTSTAT={partstat};CN={EscapeICalText( name ?? email )}:mailto:{email}";
                            icsBuilder.AppendLine( attendeeLine );
                        }
                    }
                }
            }

            // Recurrence
            var recurrence = graphEvent["recurrence"];
            if ( recurrence != null )
            {
                var pattern = recurrence["pattern"];
                var range = recurrence["range"];

                if ( pattern != null )
                {
                    var rrule = ConvertGraphRecurrenceToRRule( pattern, range );
                    if ( !string.IsNullOrWhiteSpace( rrule ) )
                    {
                        icsBuilder.AppendLine( $"RRULE:{rrule}" );
                    }
                }
            }

            // Check for exception dates and additional occurrence dates
            // Microsoft Graph may have these in the event's extensions or as separate properties
            var exceptionOccurrences = graphEvent["exceptionOccurrences"] as JArray;
            if ( exceptionOccurrences != null && exceptionOccurrences.Any() )
            {
                var exdates = new List<string>();
                foreach ( var exception in exceptionOccurrences )
                {
                    var originalStart = exception["originalStart"]?.ToString();
                    if ( !string.IsNullOrWhiteSpace( originalStart ) )
                    {
                        var dt = DateTime.Parse( originalStart ).ToUniversalTime();
                        exdates.Add( dt.ToString( "yyyyMMddTHHmmss" ) + "Z" );
                    }
                }

                if ( exdates.Any() )
                {
                    icsBuilder.AppendLine( $"EXDATE:{string.Join( ",", exdates )}" );
                }
            }

            icsBuilder.AppendLine( "END:VEVENT" );
            icsBuilder.AppendLine( "END:VCALENDAR" );

            return icsBuilder.ToString();
        }

        /// <summary>
        /// Escapes special characters in iCalendar text.
        /// </summary>
        private string EscapeICalText( string text )
        {
            if ( string.IsNullOrWhiteSpace( text ) )
            {
                return string.Empty;
            }

            return text
                .Replace( "\\", "\\\\" )
                .Replace( ",", "\\," )
                .Replace( ";", "\\;" )
                .Replace( "\n", "\\n" );
        }

        /// <summary>
        /// Converts Microsoft Graph response status to iCalendar PARTSTAT.
        /// </summary>
        private string ConvertResponseStatus( string responseStatus )
        {
            if ( string.IsNullOrWhiteSpace( responseStatus ) )
            {
                return "NEEDS-ACTION";
            }

            switch ( responseStatus.ToLower() )
            {
                case "accepted":
                    return "ACCEPTED";
                case "declined":
                    return "DECLINED";
                case "tentativelyaccepted":
                    return "TENTATIVE";
                case "notresponded":
                    return "NEEDS-ACTION";
                default:
                    return "NEEDS-ACTION";
            }
        }

        /// <summary>
        /// Converts Microsoft Graph recurrence pattern to iCalendar RRULE.
        /// </summary>
        private string ConvertGraphRecurrenceToRRule( JToken pattern, JToken range )
        {
            if ( pattern == null )
            {
                return null;
            }

            var rruleParts = new List<string>();

            var type = pattern["type"]?.ToString()?.ToLower();
            if ( !string.IsNullOrWhiteSpace( type ) )
            {
                switch ( type )
                {
                    case "daily":
                        rruleParts.Add( "FREQ=DAILY" );
                        break;
                    case "weekly":
                        rruleParts.Add( "FREQ=WEEKLY" );

                        // Add day of week if specified
                        var daysOfWeek = pattern["daysOfWeek"] as JArray;
                        if ( daysOfWeek != null && daysOfWeek.Any() )
                        {
                            var days = daysOfWeek.Select( d => ConvertDayOfWeek( d.ToString() ) )
                                                  .Where( d => !string.IsNullOrWhiteSpace( d ) );
                            if ( days.Any() )
                            {
                                rruleParts.Add( $"BYDAY={string.Join( ",", days )}" );
                            }
                        }
                        break;
                    case "absolutemonthly":
                        rruleParts.Add( "FREQ=MONTHLY" );

                        var dayOfMonth = pattern["dayOfMonth"]?.ToObject<int?>();
                        if ( dayOfMonth.HasValue )
                        {
                            rruleParts.Add( $"BYMONTHDAY={dayOfMonth.Value}" );
                        }
                        break;
                    case "relativemonthly":
                        rruleParts.Add( "FREQ=MONTHLY" );

                        var index = pattern["index"]?.ToString()?.ToLower();
                        var relativeDays = pattern["daysOfWeek"] as JArray;
                        if ( !string.IsNullOrWhiteSpace( index ) && relativeDays != null && relativeDays.Any() )
                        {
                            var indexValue = ConvertIndex( index );
                            var dayList = relativeDays.Select( d => ConvertDayOfWeek( d.ToString() ) )
                                                      .Where( d => !string.IsNullOrWhiteSpace( d ) )
                                                      .Select( d => indexValue + d );
                            if ( dayList.Any() )
                            {
                                rruleParts.Add( $"BYDAY={string.Join( ",", dayList )}" );
                            }
                        }
                        break;
                    case "absoluteyearly":
                        rruleParts.Add( "FREQ=YEARLY" );

                        var month = pattern["month"]?.ToObject<int?>();
                        var yearDay = pattern["dayOfMonth"]?.ToObject<int?>();
                        if ( month.HasValue )
                        {
                            rruleParts.Add( $"BYMONTH={month.Value}" );
                        }
                        if ( yearDay.HasValue )
                        {
                            rruleParts.Add( $"BYMONTHDAY={yearDay.Value}" );
                        }
                        break;
                    case "relativeyearly":
                        rruleParts.Add( "FREQ=YEARLY" );

                        var yearMonth = pattern["month"]?.ToObject<int?>();
                        var yearIndex = pattern["index"]?.ToString()?.ToLower();
                        var yearDays = pattern["daysOfWeek"] as JArray;

                        if ( yearMonth.HasValue )
                        {
                            rruleParts.Add( $"BYMONTH={yearMonth.Value}" );
                        }
                        if ( !string.IsNullOrWhiteSpace( yearIndex ) && yearDays != null && yearDays.Any() )
                        {
                            var indexVal = ConvertIndex( yearIndex );
                            var daysList = yearDays.Select( d => ConvertDayOfWeek( d.ToString() ) )
                                                   .Where( d => !string.IsNullOrWhiteSpace( d ) )
                                                   .Select( d => indexVal + d );
                            if ( daysList.Any() )
                            {
                                rruleParts.Add( $"BYDAY={string.Join( ",", daysList )}" );
                            }
                        }
                        break;
                }
            }

            var interval = pattern["interval"]?.ToObject<int?>();
            if ( interval.HasValue && interval.Value > 1 )
            {
                rruleParts.Add( $"INTERVAL={interval.Value}" );
            }

            // Add range information if available
            if ( range != null )
            {
                var rangeType = range["type"]?.ToString()?.ToLower();

                if ( rangeType == "enddate" )
                {
                    var endDate = range["endDate"]?.ToString();
                    if ( !string.IsNullOrWhiteSpace( endDate ) )
                    {
                        var dt = DateTime.Parse( endDate ).ToUniversalTime();
                        rruleParts.Add( $"UNTIL={dt:yyyyMMddTHHmmss}Z" );
                    }
                }
                else if ( rangeType == "numbered" )
                {
                    var numberOfOccurrences = range["numberOfOccurrences"]?.ToObject<int?>();
                    if ( numberOfOccurrences.HasValue )
                    {
                        rruleParts.Add( $"COUNT={numberOfOccurrences.Value}" );
                    }
                }
            }

            return rruleParts.Any() ? string.Join( ";", rruleParts ) : null;
        }

        /// <summary>
        /// Converts Microsoft Graph day of week to iCalendar format.
        /// </summary>
        private string ConvertDayOfWeek( string day )
        {
            if ( string.IsNullOrWhiteSpace( day ) )
            {
                return null;
            }

            switch ( day.ToLower() )
            {
                case "sunday":
                    return "SU";
                case "monday":
                    return "MO";
                case "tuesday":
                    return "TU";
                case "wednesday":
                    return "WE";
                case "thursday":
                    return "TH";
                case "friday":
                    return "FR";
                case "saturday":
                    return "SA";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts Microsoft Graph index (first, second, etc.) to iCalendar format.
        /// </summary>
        private string ConvertIndex( string index )
        {
            if ( string.IsNullOrWhiteSpace( index ) )
            {
                return string.Empty;
            }

            switch ( index.ToLower() )
            {
                case "first":
                    return "1";
                case "second":
                    return "2";
                case "third":
                    return "3";
                case "fourth":
                    return "4";
                case "last":
                    return "-1";
                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}