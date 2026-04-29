// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license/
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
using System.IO;
using System.Linq;
using Rock.Attribute;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Rock.Security;
using Ical.Net;
using TimeZoneConverter;
using Rock;
using Rock.Model;
using Newtonsoft.Json.Linq;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    /// <summary>
    /// Scheduling provider for Google Calendar and Google Workspace resources.
    /// </summary>
    [Export( typeof( SchedulingProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Google Resources" )]
    [Rock.SystemGuid.EntityTypeGuid( "A8F7D8B3-2C1E-4F9A-8D3B-1E5C6A7F8B9C" )]
    [FileField( Rock.SystemGuid.BinaryFiletype.MEDIA_FILE,
        Name = "Service Account JSON Key File",
        IsRequired = true,
        Description = "The Google service account JSON key file for API authentication. Download this from the Google Cloud Console.",
        Category = CategoryKey.GoogleCalendarSettings,
        Key = AttributeKey.ServiceAccountJsonKeyFile,
        Order = 0 )]
    public class GoogleResources : SchedulingProviderComponent
    {
        #region Keys
        private static class CategoryKey
        {
            public const string GoogleCalendarSettings = "Google Calendar Settings";
        }

        private static class AttributeKey
        {
            public const string ServiceAccountJsonKeyFile = "ServiceAccountJsonKeyFile";
            public const string AdminUserEmail = "AdminUserEmail";
        }

        #endregion

        #region Provider Overrides
        /// <summary>
        /// Gets provider events for a specific location (room) from Google Calendar.
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
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return events;
                }

                var eventsRequest = calendarService.Events.List( externalLocationId );

                if ( startDate.HasValue )
                {
                    eventsRequest.TimeMin = startDate.Value;
                }

                if ( endDate.HasValue )
                {
                    eventsRequest.TimeMax = endDate.Value;
                }

                eventsRequest.SingleEvents = true;
                eventsRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                var eventsResult = eventsRequest.Execute();

                if ( eventsResult.Items != null )
                {
                    foreach ( var googleEvent in eventsResult.Items )
                    {
                        events.Add( ConvertFromGoogleEvent( googleEvent, externalLocationId ) );
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
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return null;
                }

                // External event ID format: "calendarId|eventId"
                var parts = externalEventId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format. Expected 'calendarId|eventId'" );
                    return null;
                }

                var calendarId = parts[0];
                var eventId = parts[1];

                var eventRequest = calendarService.Events.Get( calendarId, eventId );
                var googleEvent = eventRequest.Execute();

                return ConvertFromGoogleEvent( googleEvent, calendarId );
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
                return null;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception getting event: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Creates a new event in Google Calendar.
        /// </summary>
        public override EventDTO CreateProviderEvent(
            SchedulingProvider schedulingProvider,
            EventDTO providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return null;
                }

                // Get the primary location/calendar to create the event in
                var primaryLocation = providerEvent.Locations?.FirstOrDefault();
                if ( primaryLocation == null || string.IsNullOrWhiteSpace( primaryLocation.ExternalId ) )
                {
                    errorMessages.Add( "Event must have at least one location with an external ID" );
                    return null;
                }

                var googleEvent = ConvertToGoogleEvent( providerEvent );

                var insertRequest = calendarService.Events.Insert( googleEvent, primaryLocation.ExternalId );
                var createdEvent = insertRequest.Execute();

                var result = ConvertFromGoogleEvent( createdEvent, primaryLocation.ExternalId );
                result.ExternalId = $"{primaryLocation.ExternalId}|{createdEvent.Id}";

                return result;
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
                return null;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception creating event: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Updates an existing event in Google Calendar.
        /// </summary>
        public override EventDTO UpdateProviderEvent(
            SchedulingProvider schedulingProvider,
            EventDTO providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return null;
                }

                if ( string.IsNullOrWhiteSpace( providerEvent.ExternalId ) )
                {
                    errorMessages.Add( "External ID is required for update" );
                    return null;
                }

                var parts = providerEvent.ExternalId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format" );
                    return null;   
                }

                var calendarId = parts[0];
                var eventId = parts[1];

                var googleEvent = ConvertToGoogleEvent( providerEvent );
                googleEvent.Id = eventId;

                var updateRequest = calendarService.Events.Update( googleEvent, calendarId, eventId );
                var updatedEvent = updateRequest.Execute();

                if ( updatedEvent != null )
                {
                    var result = ConvertFromGoogleEvent( updatedEvent, calendarId );
                    return result;
                }
                else
                {
                    errorMessages.Add( "Failed to update event" );
                    return null;
                }
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
                return null;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception updating event: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Deletes an event from Google Calendar.
        /// </summary>
        public override bool DeleteProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return false;
                }

                var parts = externalEventId.Split( '|' );
                if ( parts.Length != 2 )
                {
                    errorMessages.Add( "Invalid external event ID format" );
                    return false;
                }

                var calendarId = parts[0];
                var eventId = parts[1];

                var deleteRequest = calendarService.Events.Delete( calendarId, eventId );
                deleteRequest.Execute();

                return true;
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
                return false;
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
        /// Gets an authenticated Google Calendar service.
        /// </summary>
        private CalendarService GetCalendarService( SchedulingProvider schedulingProvider, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                schedulingProvider.LoadAttributes();
                var jsonKeyFileGuid = schedulingProvider.GetAttributeValue( AttributeKey.ServiceAccountJsonKeyFile ).AsGuidOrNull();

                if ( !jsonKeyFileGuid.HasValue )
                {
                    errorMessages.Add( "Missing required Google authentication configuration" );
                    return null;
                }

                // Read the JSON key file from BinaryFile
                var binaryFileService = new Rock.Model.BinaryFileService( new Rock.Data.RockContext() );
                var binaryFile = binaryFileService.Get( jsonKeyFileGuid.Value );

                if ( binaryFile == null )
                {
                    errorMessages.Add( "Google service account JSON key file not found" );
                    return null;
                }

                GoogleCredential credential;
                using ( var stream = binaryFile.ContentStream )
                {
                    credential = GoogleCredential.FromStream( stream ).CreateScoped( new[] { CalendarService.Scope.Calendar } );
                }

                var service = new CalendarService( new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Rock Room Management"
                } );

                return service;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception creating calendar service: {ex.Message}" );
                return null;
            }
        }

        #endregion

        #region Provider Event Conversion Methods

        /// <summary>
        /// Converts a Google Calendar event to a SchedulingProviderEvent.
        /// </summary>
        private EventDTO ConvertFromGoogleEvent( Event googleEvent, string calendarId )
        {
            var providerEvent = new EventDTO();
            providerEvent.ExternalId = $"{calendarId}|{googleEvent.Id}"; 
            providerEvent.Title = googleEvent.Summary;
            providerEvent.Description = googleEvent.Description;

            // Google Calendar Created / Updated Date Times are always in UTC
            providerEvent.CreatedDateTime = googleEvent.Created?.ToUniversalTime();
            providerEvent.ModifiedDateTime = googleEvent.Updated?.ToUniversalTime();

            // Parse organizer
            if ( googleEvent.Organizer != null )
            {
                providerEvent.Organizer = new PersonDTO
                {
                    DisplayName = googleEvent.Organizer.DisplayName,
                    Email = googleEvent.Organizer.Email
                };
            }

            // Parse attendees - filter out resources and add them to Locations
            if ( googleEvent.Attendees != null )
            {
                var resourceAttendees = googleEvent.Attendees.Where( a => a.Resource == true ).ToList();
                if ( resourceAttendees.Any() )
                {
                    providerEvent.Locations = resourceAttendees.Select( a => new LocationDTO
                    {
                        DisplayName = a.DisplayName ?? a.Email,
                        ExternalId = a.Email
                    } ).ToList();
                }
            }

            // Build CalendarEvent
            var calendarEvent = new Ical.Net.CalendarComponents.CalendarEvent();

            // Parse start and end times and convert to UTC
            if ( googleEvent.Start != null )
            {
                if ( googleEvent.Start.DateTime.HasValue )
                {
                    var startDateTime = googleEvent.Start.DateTime.Value;
                    if ( startDateTime.Kind != DateTimeKind.Utc )
                    {
                        // Google provides timezone in IANA format, convert to UTC
                        if ( !string.IsNullOrWhiteSpace( googleEvent.Start.TimeZone ) )
                        {
                            var startTimeZone = TimeZoneConverter.TZConvert.GetTimeZoneInfo( googleEvent.Start.TimeZone );
                            startDateTime = TimeZoneInfo.ConvertTimeToUtc( startDateTime, startTimeZone );
                        }
                        else
                        {
                            startDateTime = startDateTime.ToUniversalTime();
                        }
                    }
                    calendarEvent.Start = new Ical.Net.DataTypes.CalDateTime( startDateTime, "UTC" );
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.Start.Date ) )
                {
                    calendarEvent.Start = new Ical.Net.DataTypes.CalDateTime( DateTime.Parse( googleEvent.Start.Date ) );
                    calendarEvent.IsAllDay = true;
                }
            }

            if ( googleEvent.End != null )
            {
                if ( googleEvent.End.DateTime.HasValue )
                {
                    var endDateTime = googleEvent.End.DateTime.Value;
                    if ( endDateTime.Kind != DateTimeKind.Utc )
                    {
                        // Google provides timezone in IANA format, convert to UTC
                        if ( !string.IsNullOrWhiteSpace( googleEvent.End.TimeZone ) )
                        {
                            var endTimeZone = TimeZoneConverter.TZConvert.GetTimeZoneInfo( googleEvent.End.TimeZone );
                            endDateTime = TimeZoneInfo.ConvertTimeToUtc( endDateTime, endTimeZone );
                        }
                        else
                        {
                            endDateTime = endDateTime.ToUniversalTime();
                        }
                    }
                    calendarEvent.End = new Ical.Net.DataTypes.CalDateTime( endDateTime, "UTC" );
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.End.Date ) )
                {
                    calendarEvent.End = new Ical.Net.DataTypes.CalDateTime( DateTime.Parse( googleEvent.End.Date ) );
                }
            }

            // Parse recurrence
            if ( googleEvent.Recurrence != null && googleEvent.Recurrence.Any() )
            {
                // Parse RRULE from Google's recurrence array
                foreach ( var recurrenceRule in googleEvent.Recurrence )
                {
                    if ( recurrenceRule.StartsWith( "RRULE:" ) )
                    {
                        var rruleString = recurrenceRule.Substring( 6 ); // Remove "RRULE:" prefix
                        var recurrencePattern = new Ical.Net.DataTypes.RecurrencePattern( rruleString );
                        calendarEvent.RecurrenceRules = new List<Ical.Net.DataTypes.RecurrencePattern> { recurrencePattern };
                    }
                }
            }

            providerEvent.CalendarEvent = calendarEvent;

            return providerEvent;
        }

        /// <summary>
        /// Converts a SchedulingProviderEvent to a Google Calendar event.
        /// </summary>
        private Event ConvertToGoogleEvent( EventDTO providerEvent )
        {
            var googleEvent = new Event();
            googleEvent.Id = providerEvent.ExternalId?.Split( '|' ).LastOrDefault();
            googleEvent.Summary = providerEvent.Title;
            googleEvent.Description = providerEvent.Description;

            // Set attendees (including room resources)
            if ( providerEvent.Locations != null && providerEvent.Locations.Any() )
            {
                googleEvent.Attendees = new List<EventAttendee>();

                // Add locations as resource attendees
                foreach ( var location in providerEvent.Locations )
                {
                    if ( !string.IsNullOrWhiteSpace( location.ExternalId ) )
                    {
                        googleEvent.Attendees.Add( new EventAttendee
                        {
                            DisplayName = location.DisplayName,
                            Email = location.ExternalId,
                            Resource = true
                        } );
                    }
                }
            }

            // Set location string
            if ( providerEvent.Locations != null && providerEvent.Locations.Any() )
            {
                googleEvent.Location = string.Join( ", ", providerEvent.Locations.Select( l => l.DisplayName ) );
            }

            // Build schedule from CalendarEvent
            var calendarEvent = providerEvent.CalendarEvent;
            if ( calendarEvent != null )
            {
                var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );
                var orgTimeZone = RockDateTime.OrgTimeZoneInfo;

                googleEvent.Start = new EventDateTime();
                googleEvent.End = new EventDateTime();

                if ( calendarEvent.IsAllDay )
                {
                    googleEvent.Start.Date = calendarEvent.Start.Value.ToString( "yyyy-MM-dd" );
                    googleEvent.End.Date = calendarEvent.End.Value.ToString( "yyyy-MM-dd" );
                }
                else
                {
                    var startDateTime = calendarEvent.Start.AsUtc;
                    var endDateTime = calendarEvent.End.AsUtc;

                    googleEvent.Start.DateTime = startDateTime;
                    googleEvent.End.DateTime = endDateTime;
                }

                // Handle recurrence
                var eventRecurrenceRule = calendarEvent.RecurrenceRules?.FirstOrDefault();
                if ( eventRecurrenceRule != null )
                {
                    var rruleString = "RRULE:" + eventRecurrenceRule.ToString();
                    googleEvent.Recurrence = new List<string> { rruleString };
                }
            }

            return googleEvent;
        }

        #endregion
    }
}
