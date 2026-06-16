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
    [TextField(
        "Admin User Email",
        Description = "The email address of a Google Workspace admin user to impersonate for domain-wide delegation. The service account must have domain-wide delegation enabled and the admin user must have access to the calendars.",
        IsRequired = true,
        Category = CategoryKey.GoogleCalendarSettings,
        Key = AttributeKey.AdminUserEmail,
        Order = 1 )]
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

                // Validate the calendar ID format and accessibility
                if ( !ValidateCalendarId( calendarService, externalLocationId, out var validationErrors ) )
                {
                    errorMessages.AddRange( validationErrors );
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

                // Set SingleEvents to false to retrieve recurring events with their recurrence rules
                // instead of expanding them into individual occurrences
                eventsRequest.SingleEvents = false;
                // Note: OrderBy can only be used with SingleEvents=true, so we remove it

                var eventsResult = eventsRequest.Execute();

                if ( eventsResult.Items != null )
                {
                    foreach ( var googleEvent in eventsResult.Items )
                    {
                        events.Add( ConvertFromGoogleEvent( googleEvent, externalLocationId ) );
                    }
                }
            }
            catch ( Google.GoogleApiException ex ) when ( ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound )
            {
                errorMessages.Add( $"Calendar '{externalLocationId}' not found. The ExternalId must be a valid Google Calendar email address (e.g., 'resource@yourdomain.com'), not '{externalLocationId}'. Check your SchedulingProviderLocation configuration." );
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

        /// <summary>
        /// Gets a list of available calendars/resources accessible by the service account.
        /// This helps identify the correct calendar IDs to use for SchedulingProviderLocation ExternalId values.
        /// </summary>
        public List<CalendarListEntry> GetAvailableCalendars(
            SchedulingProvider schedulingProvider,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var calendars = new List<CalendarListEntry>();

            try
            {
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return calendars;
                }

                // Get list of calendars accessible to the service account
                var calendarListRequest = calendarService.CalendarList.List();
                var calendarList = calendarListRequest.Execute();

                if ( calendarList.Items != null )
                {
                    calendars.AddRange( calendarList.Items );
                }
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception getting calendars: {ex.Message}" );
            }

            return calendars;
        }

        /// <summary>
        /// Validates that an external location ID exists and is accessible.
        /// Returns a more helpful error message if the ID is invalid.
        /// </summary>
        private bool ValidateCalendarId(
            CalendarService calendarService,
            string calendarId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                // Try to get calendar metadata to validate it exists
                var calendarRequest = calendarService.Calendars.Get( calendarId );
                var calendar = calendarRequest.Execute();
                return true;
            }
            catch ( Google.GoogleApiException ex ) when ( ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound )
            {
                errorMessages.Add( $"Calendar '{calendarId}' not found. The calendar ID must be an email address (e.g., 'resource@domain.com' or 'c_xxx@resource.calendar.google.com'), not a numeric ID. Use GetAvailableCalendars() to list valid calendar IDs." );
                return false;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Error validating calendar ID: {ex.Message}" );
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
                var adminUserEmail = schedulingProvider.GetAttributeValue( AttributeKey.AdminUserEmail );

                if ( !jsonKeyFileGuid.HasValue )
                {
                    errorMessages.Add( "Missing required Google service account JSON key file configuration" );
                    return null;
                }

                if ( string.IsNullOrWhiteSpace( adminUserEmail ) )
                {
                    errorMessages.Add( "Missing required Admin User Email for domain-wide delegation" );
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
                    // Create credential with domain-wide delegation to impersonate the admin user
                    credential = GoogleCredential.FromStream( stream )
                        .CreateScoped( new[] { CalendarService.Scope.Calendar } )
                        .CreateWithUser( adminUserEmail );
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

            // Parse organizer - check both the Organizer field and attendees with Organizer=true
            if ( googleEvent.Organizer != null )
            {
                providerEvent.Organizer = new PersonDTO
                {
                    DisplayName = googleEvent.Organizer.DisplayName,
                    Email = googleEvent.Organizer.Email
                };
            }

            // Parse attendees - filter out resources and add them to Locations
            // Also check for organizer in attendees (overrides the calendar owner organizer)
            if ( googleEvent.Attendees != null )
            {
                // Check if there's an attendee marked as organizer
                var organizerAttendee = googleEvent.Attendees.FirstOrDefault( a => a.Organizer == true && a.Resource != true );
                if ( organizerAttendee != null )
                {
                    providerEvent.Organizer = new PersonDTO
                    {
                        DisplayName = organizerAttendee.DisplayName,
                        Email = organizerAttendee.Email
                    };
                }

                var resourceAttendees = googleEvent.Attendees.Where( a => a.Resource == true ).ToList();
                if ( resourceAttendees.Any() )
                {
                    providerEvent.Locations = resourceAttendees.Select( a => new LocationDTO
                    {
                        DisplayName = a.DisplayName ?? a.Email,
                        ExternalId = a.Email
                    } ).ToList();
                }

                // If the organizer is a resource, use the first non-resource attendee as the real organizer
                if ( googleEvent.Organizer != null && resourceAttendees.Any() )
                {
                    // Check if the organizer's email matches any of the resource locations
                    var organizerIsResource = resourceAttendees.Any( loc =>
                        loc.Email.Equals( googleEvent.Organizer.Email, StringComparison.OrdinalIgnoreCase ) );

                    if ( organizerIsResource )
                    {
                        // Find the first non-resource attendee that is not marked as organizer
                        var firstNonResourceAttendee = googleEvent.Attendees
                            .FirstOrDefault( a => a.Resource != true && a.Organizer != true );

                        if ( firstNonResourceAttendee != null )
                        {
                            providerEvent.Organizer = new PersonDTO
                            {
                                DisplayName = firstNonResourceAttendee.DisplayName,
                                Email = firstNonResourceAttendee.Email
                            };
                        }
                    }
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
                            var localTime = DateTime.SpecifyKind( startDateTime, DateTimeKind.Unspecified );
                            startDateTime = TimeZoneInfo.ConvertTimeToUtc( localTime, startTimeZone );
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
                            var localTime = DateTime.SpecifyKind( endDateTime, DateTimeKind.Unspecified );
                            endDateTime = TimeZoneInfo.ConvertTimeToUtc( localTime, endTimeZone );
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

            // Set attendees (including organizer and room resources)
            googleEvent.Attendees = new List<EventAttendee>();

            // Add organizer as an attendee
            // Note: The Organizer field in Google Calendar API is read-only and automatically set to the calendar owner.
            // To specify the actual event organizer, add them as an attendee with Organizer=true.
            if ( providerEvent.Organizer != null && !string.IsNullOrWhiteSpace( providerEvent.Organizer.Email ) )
            {
                googleEvent.Attendees.Add( new EventAttendee
                {
                    Email = providerEvent.Organizer.Email,
                    DisplayName = providerEvent.Organizer.DisplayName,
                    Organizer = true
                } );
            }

            // Add locations as resource attendees
            if ( providerEvent.Locations != null && providerEvent.Locations.Any() )
            {
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
                    googleEvent.Start.TimeZone = timeZoneId;
                    googleEvent.End.DateTime = endDateTime;
                    googleEvent.End.TimeZone = timeZoneId;
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
