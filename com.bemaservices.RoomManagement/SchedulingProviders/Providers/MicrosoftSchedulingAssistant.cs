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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using com.bemaservices.RoomManagement.Utility.RockInternalMethods;
using DotLiquid.Tags;
using Ical.Net;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Users.Item.Calendar;
using Microsoft.Kiota.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using TimeZoneConverter;
using static com.bemaservices.RoomManagement.Model.ReservationService;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    /// <summary>
    /// Scheduling provider for Microsoft 365 / Exchange Online using Microsoft Graph API.
    /// </summary>
    [Export( typeof( SchedulingProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Microsoft Scheduling Assistant" )]
    [Rock.SystemGuid.EntityTypeGuid( "3ED7D672-76A4-41F4-9788-0404B997CC48" )]
    [EncryptedTextField( "Microsoft Graph Tenant Id",
        IsRequired = true,
        IsPassword = true,
        Category = CategoryKey.MicrosoftGraphSettings,
        Key = AttributeKey.TenantId,
        Order = 0 )]
    [EncryptedTextField( "Microsoft Graph Client Id",
        IsRequired = true,
        IsPassword = true,
        Category = CategoryKey.MicrosoftGraphSettings,
        Key = AttributeKey.ClientId,
        Order = 0 )]
    [EncryptedTextField( "Microsoft Graph Client Secret",
        IsRequired = true,
        IsPassword = true,
        Category = CategoryKey.MicrosoftGraphSettings,
        Key = AttributeKey.ClientSecret,
        Order = 0 )]
    [EncryptedTextField( "Microsoft Graph UserPrincipalName",
        IsRequired = true,
        IsPassword = true,
        Description = "The username of the Microsoft Graph principal (user or application) that will be used to authenticate API calls. This is typically the email address of the user.",
        Category = CategoryKey.MicrosoftGraphSettings,
        Key = AttributeKey.UserPrincipalName,
        Order = 0 )]
    public class MicrosoftSchedulingAssistant : SchedulingProviderComponent
    {
        private const string GraphApiBaseUrl = "https://graph.microsoft.com/v1.0";
        private const string AuthUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        public const string ROCK_EVENT_KEY_ID = "String {92f6867e-8c6b-4c0c-b679-4fbaae2a6e09} Name RockReservationProviderKey";

        #region Keys
        private static class CategoryKey
        {
            public const string MicrosoftGraphSettings = "Microsoft Graph Settings";
        }

        private static class AttributeKey
        {
            // Microsoft Graph Settings
            public const string TenantId = "TenantId";
            public const string ClientId = "ClientId";
            public const string ClientSecret = "ClientSecret";
            public const string UserPrincipalName = "UserPrincipalName";
        }

        #endregion

        #region Provider Overrides

        /// <summary>
        /// Gets provider events for a specific location from Microsoft Graph.
        /// </summary>
        public override List<EventDTO> GetProviderEventsForLocation(
            SchedulingProvider schedulingProvider,
            string externalLocationId,
            DateTime? startDate,
            DateTime? endDate,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var events = new List<EventDTO>();

            try
            {
                var calendarRequestBuilder = GetCalendarRequestBuilder( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return events;
                }

                Action<RequestConfiguration<Microsoft.Graph.Users.Item.Calendar.CalendarView.CalendarViewRequestBuilder.CalendarViewRequestBuilderGetQueryParameters>> requestConfig = ( requestConfiguration ) =>
                {
                    if ( startDate.HasValue )
                    {
                        requestConfiguration.QueryParameters.StartDateTime = startDate.Value.ToISO8601DateString();
                    }
                    if ( endDate.HasValue )
                    {
                        requestConfiguration.QueryParameters.EndDateTime = endDate.Value.ToISO8601DateString();
                    }
                };

                var getExistingEventsResponse = calendarRequestBuilder.CalendarView.GetAsync( requestConfig ).Result;

                var existingEvents = getExistingEventsResponse.Value;

                if ( existingEvents != null )
                {
                    foreach ( var existingEvent in existingEvents )
                    {
                        events.Add( ConvertFromGraphEvent( existingEvent ) );
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
        public override EventDTO GetProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarRequestBuilder = GetCalendarRequestBuilder( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                var existingEvent = calendarRequestBuilder.Events[externalEventId].GetAsync().Result;

                if ( existingEvent == null )
                {
                    errorMessages.Add( $"Failed to get event" );
                    return null;
                }

                var schedulingProviderEvent = ConvertFromGraphEvent( existingEvent );
                return schedulingProviderEvent;
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
        public override EventDTO CreateProviderEvent(
            SchedulingProvider schedulingProvider,
            EventDTO providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarRequestBuilder = GetCalendarRequestBuilder( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                Event graphEvent = ConvertToGraphEvent( providerEvent );

                var returnedEvent = calendarRequestBuilder.Events.PostAsync( graphEvent ).Result;
                if ( returnedEvent != null )
                {
                    var createdEvent = ConvertFromGraphEvent( returnedEvent );
                    return createdEvent;
                }
                else
                {
                    errorMessages.Add( "Failed to create event" );
                    return null;
                }
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
        public override EventDTO UpdateProviderEvent(
            SchedulingProvider schedulingProvider,
            EventDTO providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarRequestBuilder = GetCalendarRequestBuilder( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                Event graphEvent = ConvertToGraphEvent( providerEvent );

                var returnedEvent = calendarRequestBuilder.Events[graphEvent.Id].PatchAsync( graphEvent ).Result;
                if ( returnedEvent != null )
                {
                    var updatedEvent = ConvertFromGraphEvent( returnedEvent );
                    return updatedEvent;
                }
                else
                {
                    errorMessages.Add( "Failed to update event" );
                    return null;
                }
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception updating event: {ex.Message}" );
                return null;
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
                var calendarRequestBuilder = GetCalendarRequestBuilder( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return false;
                }

                calendarRequestBuilder.Events[externalEventId].DeleteAsync().Wait();

                return true;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception deleting event: {ex.Message}" );
                return false;
            }
        }

        #endregion

        #region API Client Methods

        /// <summary>
        /// Gets an access token for Microsoft Graph API.
        /// </summary>
        private CalendarRequestBuilder GetCalendarRequestBuilder( SchedulingProvider schedulingProvider, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                schedulingProvider.LoadAttributes();
                var graphClient = GetGraphClient( schedulingProvider, out var authErrors );
                if ( authErrors.Any() )
                {
                    errorMessages.AddRange( authErrors );
                    return null;
                }

                var encryptedUserPrincipalName = schedulingProvider.GetAttributeValue( AttributeKey.UserPrincipalName );
                var userPrincipalName = Encryption.DecryptString( encryptedUserPrincipalName ) ?? encryptedUserPrincipalName;

                return graphClient.Users[userPrincipalName].Calendar;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception during authentication: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Gets an access token for Microsoft Graph API.
        /// </summary>
        private GraphServiceClient GetGraphClient( SchedulingProvider schedulingProvider, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var encryptedTenantId = schedulingProvider.GetAttributeValue( AttributeKey.TenantId );
                var encryptedClientId = schedulingProvider.GetAttributeValue( AttributeKey.ClientId );
                var encryptedClientSecret = schedulingProvider.GetAttributeValue( AttributeKey.ClientSecret );
                var tenantId = Encryption.DecryptString( encryptedTenantId ) ?? encryptedTenantId;
                var clientId = Encryption.DecryptString( encryptedClientId ) ?? encryptedClientId;
                var clientSecret = Encryption.DecryptString( encryptedClientSecret ) ?? encryptedClientSecret;

                var scopes = new[] { "https://graph.microsoft.com/.default" };
                var clientSecretCredential = new ClientSecretCredential( tenantId, clientId, clientSecret );
                var graphClient = new GraphServiceClient( clientSecretCredential, scopes );
                return graphClient;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception during authentication: {ex.Message}" );
                return null;
            }
        }

        #endregion

        #region Provider Event Conversion Methods

        /// <summary>
        /// Converts a Microsoft Graph event to a SchedulingProviderEvent.
        /// </summary>
        private EventDTO ConvertFromGraphEvent( Event graphEvent )
        {
            var providerEvent = new EventDTO();
            providerEvent.ExternalId = graphEvent.Id;
            providerEvent.Title = graphEvent.Subject;
            providerEvent.Description = graphEvent.Body.Content;

            // Graph Event Created / Modified Date Times are always in UTC
            providerEvent.CreatedDateTime = graphEvent.CreatedDateTime?.DateTime;
            providerEvent.ModifiedDateTime = graphEvent.LastModifiedDateTime?.DateTime;

            var graphOrganizer = graphEvent.Organizer;
            if ( graphOrganizer != null )
            {
                providerEvent.Organizer = new PersonDTO
                {
                    DisplayName = graphOrganizer.EmailAddress.Name,
                    Email = graphOrganizer.EmailAddress.Address
                };
            }

            var locationAttendees = graphEvent.Attendees?.Where( a => a.Type == AttendeeType.Resource ).ToList();
            if ( locationAttendees != null )
            {
                providerEvent.Locations = locationAttendees.Select( a => new LocationDTO
                {
                    DisplayName = a.EmailAddress.Name,
                    ExternalId = a.EmailAddress.Address
                } ).ToList();
            }

            var calendarEvent = new Ical.Net.CalendarComponents.CalendarEvent();
            
            // Parse start datetime and convert to UTC if needed
            var startDateTime = DateTime.Parse( graphEvent.Start.DateTime );
            if ( startDateTime.Kind != DateTimeKind.Utc )
            {
                // Graph provides timezone in IANA format, convert to UTC
                var startTimeZone = TimeZoneConverter.TZConvert.GetTimeZoneInfo( graphEvent.Start.TimeZone );
                startDateTime = TimeZoneInfo.ConvertTimeToUtc( startDateTime, startTimeZone );
            }

            calendarEvent.Start = new Ical.Net.DataTypes.CalDateTime( startDateTime, "UTC" );
            
            // Parse end datetime and convert to UTC if needed
            var endDateTime = DateTime.Parse( graphEvent.End.DateTime );
            if ( endDateTime.Kind != DateTimeKind.Utc )
            {
                // Graph provides timezone in IANA format, convert to UTC
                var endTimeZone = TimeZoneConverter.TZConvert.GetTimeZoneInfo( graphEvent.End.TimeZone );
                endDateTime = TimeZoneInfo.ConvertTimeToUtc( endDateTime, endTimeZone );
            }
            calendarEvent.End = new Ical.Net.DataTypes.CalDateTime( endDateTime, "UTC" );

            if ( graphEvent.Recurrence != null )
            {
                var eventRecurrenceRule = new Ical.Net.DataTypes.RecurrencePattern();
                eventRecurrenceRule.Interval = graphEvent.Recurrence.Pattern.Interval ?? 1;
                eventRecurrenceRule.Frequency = MapGraphRecurrenceType( graphEvent.Recurrence.Pattern.Type );

                var pattern = graphEvent.Recurrence.Pattern;

                // Handle BYDAY (weekly and relative monthly patterns)
                if ( pattern.DaysOfWeek != null && pattern.DaysOfWeek.Any() )
                {
                    eventRecurrenceRule.ByDay = pattern.DaysOfWeek.Select( d => MapGraphDayOfWeek( d ) ).ToList();
                }

                // Handle absolute monthly patterns (e.g., "day 15 of every month")
                if ( pattern.DayOfMonth.HasValue && pattern.DayOfMonth.Value > 0 )
                {
                    eventRecurrenceRule.ByMonthDay = new List<int> { pattern.DayOfMonth.Value };
                }

                // Handle relative monthly patterns (e.g., "second Tuesday of every month")
                if ( pattern.Index.HasValue )
                {
                    var index = MapGraphWeekIndex( pattern.Index.Value );
                    if ( index.HasValue )
                    {
                        eventRecurrenceRule.BySetPosition = new List<int> { index.Value };
                    }
                }

                if ( graphEvent.Recurrence.Range != null )
                {
                    if ( graphEvent.Recurrence.Range.Type == RecurrenceRangeType.Numbered && graphEvent.Recurrence.Range.NumberOfOccurrences.HasValue )
                    {
                        eventRecurrenceRule.Count = graphEvent.Recurrence.Range.NumberOfOccurrences.Value;
                    }
                    else if ( graphEvent.Recurrence.Range.Type == RecurrenceRangeType.EndDate && graphEvent.Recurrence.Range.EndDate.HasValue )
                    {
                        var untilDateTime = DateTime.Parse( graphEvent.Recurrence.Range.EndDate.Value.ToString() );
                        if ( untilDateTime.Kind != DateTimeKind.Utc )
                        {
                            // Convert to UTC using the event's timezone
                            var eventTimeZone = TimeZoneConverter.TZConvert.GetTimeZoneInfo( graphEvent.Start.TimeZone );
                            untilDateTime = TimeZoneInfo.ConvertTimeToUtc( untilDateTime, eventTimeZone );
                        }
                        eventRecurrenceRule.Until = untilDateTime;
                    }
                }

                calendarEvent.RecurrenceRules = new List<Ical.Net.DataTypes.RecurrencePattern> { eventRecurrenceRule };
            }

            providerEvent.CalendarEvent = calendarEvent;

            return providerEvent;
        }

        /// <summary>
        /// Converts a SchedulingProviderEvent to Microsoft Graph event format.
        /// </summary>
        private Event ConvertToGraphEvent( EventDTO providerEvent )
        {
            // Build Event
            var graphEvent = new Event();
            graphEvent.Id = providerEvent.ExternalId;
            graphEvent.Subject = providerEvent.Title;
            graphEvent.Body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = providerEvent.Description
            };

            graphEvent.CreatedDateTime = providerEvent.CreatedDateTime ?? DateTime.UtcNow;
            graphEvent.LastModifiedDateTime = providerEvent.ModifiedDateTime ?? DateTime.UtcNow;

            // Add Event Contact as Attendee
            graphEvent.Attendees = new List<Attendee>();

            if ( providerEvent.Organizer != null )
            {
                graphEvent.Attendees.Add( new Attendee
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = providerEvent.Organizer.Email,
                        Name = providerEvent.Organizer.DisplayName,
                    },
                    Type = AttendeeType.Required,
                } );

                graphEvent.Organizer = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = providerEvent.Organizer.Email,
                        Name = providerEvent.Organizer.DisplayName,
                    }
                };
                graphEvent.IsOrganizer = false;
            }

            // Add Rooms as Attendees
            if ( providerEvent.Locations != null )
            {
                foreach ( var location in providerEvent.Locations )
                {
                    graphEvent.Attendees.Add( new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = location.ExternalId,
                            Name = location.DisplayName,
                        },
                        Type = AttendeeType.Resource,
                    } );
                }
            }

            // Build Schedule
            var calendarEvent = providerEvent.CalendarEvent;
            if ( calendarEvent != null )
            {
                var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );
                EventCalendarServiceOverrides.SetCalendarEventDateTimeInfo( calendarEvent, timeZoneId );

                graphEvent.IsAllDay = calendarEvent.IsAllDay;
                graphEvent.Start = new DateTimeTimeZone
                {
                    DateTime = EventCalendarServiceOverrides.ConvertToCalDateTime( calendarEvent.Start, timeZoneId ).Value.ToString( "yyyy-MM-ddTHH:mm:ss" ),
                    TimeZone = timeZoneId,
                };
                graphEvent.End = new DateTimeTimeZone
                {
                    DateTime = EventCalendarServiceOverrides.ConvertToCalDateTime( calendarEvent.End, timeZoneId ).Value.ToString( "yyyy-MM-ddTHH:mm:ss" ),
                    TimeZone = timeZoneId,
                };

                // Graph recurrence mapping can get complex (BYDAY/BYMONTHDAY/INTERVAL/UNTIL/COUNT/EXDATE).
                // This implementation covers a common subset: a single RRULE with frequency/interval/until/count/byday/bymonthday/bysetpos.
                PatternedRecurrence graphRecurrence = null;
                var eventRecurrenceRule = calendarEvent.RecurrenceRules?.FirstOrDefault();
                if ( eventRecurrenceRule != null )
                {
                    var graphPattern = new RecurrencePattern
                    {
                        Interval = eventRecurrenceRule.Interval <= 0 ? 1 : eventRecurrenceRule.Interval,
                        Type = MapRecurrenceType( eventRecurrenceRule.Frequency )
                    };

                    // BYDAY (weekly and relative monthly patterns)
                    if ( eventRecurrenceRule.ByDay != null && eventRecurrenceRule.ByDay.Count > 0 )
                    {
                        graphPattern.DaysOfWeek = eventRecurrenceRule.ByDay
                            .Select( d => MapDayOfWeek( d.DayOfWeek ) )
                            .Distinct()
                            .ToList();
                    }

                    // Handle absolute monthly patterns (e.g., "day 15 of every month")
                    if ( eventRecurrenceRule.ByMonthDay != null && eventRecurrenceRule.ByMonthDay.Count > 0 )
                    {
                        graphPattern.DayOfMonth = eventRecurrenceRule.ByMonthDay.First();
                    }

                    // Handle relative monthly patterns (e.g., "second Tuesday of every month")
                    if ( eventRecurrenceRule.BySetPosition != null && eventRecurrenceRule.BySetPosition.Count > 0 )
                    {
                        var setPosition = eventRecurrenceRule.BySetPosition.First();
                        graphPattern.Index = MapWeekIndex( setPosition );
                    }

                    var range = new RecurrenceRange
                    {
                        Type = MapRangeType( eventRecurrenceRule ),
                        StartDate = new Microsoft.Kiota.Abstractions.Date( calendarEvent.DtStart.Value.Date )
                    };

                    if ( eventRecurrenceRule.Count > 0 )
                        range.NumberOfOccurrences = eventRecurrenceRule.Count;

                    if ( eventRecurrenceRule.Until != null )
                        range.EndDate = new Microsoft.Kiota.Abstractions.Date( eventRecurrenceRule.Until.Date );

                    graphRecurrence = new PatternedRecurrence
                    {
                        Pattern = graphPattern,
                        Range = range
                    };
                }

                graphEvent.Recurrence = graphRecurrence;

            }
            else
            {
                // If iCalendar content is not available or failed to parse, fall back to using StartDateTime and EndDateTime
            }

            return graphEvent;
        }

        #endregion

        #region Mapping Methods

        private static RecurrencePatternType MapRecurrenceType( Ical.Net.FrequencyType freq )
        {
            switch ( freq )
            {
                case Ical.Net.FrequencyType.Daily:
                    return RecurrencePatternType.Daily;
                case Ical.Net.FrequencyType.Weekly:
                    return RecurrencePatternType.Weekly;
                case Ical.Net.FrequencyType.Monthly:
                    return RecurrencePatternType.AbsoluteMonthly;
                case Ical.Net.FrequencyType.Yearly:
                    return RecurrencePatternType.AbsoluteYearly;
                default:
                    return RecurrencePatternType.Daily;
            }
        }

        private static RecurrenceRangeType MapRangeType( Ical.Net.DataTypes.RecurrencePattern rrule )
        {
            if ( rrule.Count > 0 ) return RecurrenceRangeType.Numbered;
            if ( rrule.Until != null ) return RecurrenceRangeType.EndDate;
            return RecurrenceRangeType.NoEnd;
        }

        private static Microsoft.Graph.Models.DayOfWeekObject? MapDayOfWeek( DayOfWeek day )
        {
            switch ( day )
            {
                case DayOfWeek.Monday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Monday;
                case DayOfWeek.Tuesday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Tuesday;
                case DayOfWeek.Wednesday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Wednesday;
                case DayOfWeek.Thursday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Thursday;
                case DayOfWeek.Friday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Friday;
                case DayOfWeek.Saturday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Saturday;
                case DayOfWeek.Sunday:
                    return Microsoft.Graph.Models.DayOfWeekObject.Sunday;
                default:
                    return Microsoft.Graph.Models.DayOfWeekObject.Monday;
            }
        }

        private static Ical.Net.FrequencyType MapGraphRecurrenceType( RecurrencePatternType? type )
        {
            switch ( type )
            {
                case RecurrencePatternType.Daily:
                    return Ical.Net.FrequencyType.Daily;
                case RecurrencePatternType.Weekly:
                    return Ical.Net.FrequencyType.Weekly;
                case RecurrencePatternType.AbsoluteMonthly:
                case RecurrencePatternType.RelativeMonthly:
                    return Ical.Net.FrequencyType.Monthly;
                case RecurrencePatternType.AbsoluteYearly:
                case RecurrencePatternType.RelativeYearly:
                    return Ical.Net.FrequencyType.Yearly;
                default:
                    return Ical.Net.FrequencyType.Daily;
            }
        }

        private static Ical.Net.DataTypes.WeekDay MapGraphDayOfWeek( Microsoft.Graph.Models.DayOfWeekObject? day )
        {
            switch ( day )
            {
                case Microsoft.Graph.Models.DayOfWeekObject.Monday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Monday );
                case Microsoft.Graph.Models.DayOfWeekObject.Tuesday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Tuesday );
                case Microsoft.Graph.Models.DayOfWeekObject.Wednesday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Wednesday );
                case Microsoft.Graph.Models.DayOfWeekObject.Thursday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Thursday );
                case Microsoft.Graph.Models.DayOfWeekObject.Friday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Friday );
                case Microsoft.Graph.Models.DayOfWeekObject.Saturday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Saturday );
                case Microsoft.Graph.Models.DayOfWeekObject.Sunday:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Sunday );
                default:
                    return new Ical.Net.DataTypes.WeekDay( DayOfWeek.Monday );
            }
        }

        private static WeekIndex? MapWeekIndex( int setPosition )
        {
            switch ( setPosition )
            {
                case 1:
                    return WeekIndex.First;
                case 2:
                    return WeekIndex.Second;
                case 3:
                    return WeekIndex.Third;
                case 4:
                    return WeekIndex.Fourth;
                case -1:
                    return WeekIndex.Last;
                default:
                    return null;
            }
        }

        private static int? MapGraphWeekIndex( WeekIndex? index )
        {
            switch ( index )
            {
                case WeekIndex.First:
                    return 1;
                case WeekIndex.Second:
                    return 2;
                case WeekIndex.Third:
                    return 3;
                case WeekIndex.Fourth:
                    return 4;
                case WeekIndex.Last:
                    return -1;
                default:
                    return null;
            }
        }

        #endregion
    }
}