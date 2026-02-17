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
using System.Linq;
using Rock.Attribute;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    /// <summary>
    /// Scheduling provider for Google Calendar and Google Workspace resources.
    /// </summary>
    [Export( typeof( SchedulingProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Google Calendar Resources" )]
    [Rock.SystemGuid.EntityTypeGuid( "A8F7D8B3-2C1E-4F9A-8D3B-1E5C6A7F8B9C" )]
    [TextField( "Service Account Email", "The service account email for Google API authentication.", true, order: 0, key: "ServiceAccountEmail" )]
    [TextField( "Service Account Private Key", "The private key for the Google service account (P12 format or JSON key).", true, order: 1, key: "ServiceAccountPrivateKey" )]
    [TextField( "Admin User Email", "The admin user email to impersonate for accessing Google Workspace resources.", true, order: 2, key: "AdminUserEmail" )]
    public class GoogleCalendarResources : SchedulingProviderComponent
    {
        /// <summary>
        /// Gets provider events for a specific location (room) from Google Calendar.
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
                var calendarService = GetCalendarService( schedulingProvider, out var serviceErrors );
                if ( serviceErrors.Any() )
                {
                    errorMessages.AddRange( serviceErrors );
                    return events;
                }

                var eventsRequest = calendarService.Events.List( externalId );

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
                        events.Add( ConvertFromGoogleEvent( googleEvent, externalId ) );
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
        public override SchedulingProviderEvent CreateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
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
                var primaryLocation = providerEvent.Locations.FirstOrDefault();
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
        public override bool UpdateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
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

                var calendarId = parts[0];
                var eventId = parts[1];

                var googleEvent = ConvertToGoogleEvent( providerEvent );
                googleEvent.Id = eventId;

                var updateRequest = calendarService.Events.Update( googleEvent, calendarId, eventId );
                updateRequest.Execute();

                return true;
            }
            catch ( Google.GoogleApiException ex )
            {
                errorMessages.Add( $"Google API error: {ex.Message}" );
                return false;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( $"Exception updating event: {ex.Message}" );
                return false;
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

        #region Helper Methods

        /// <summary>
        /// Gets an authenticated Google Calendar service.
        /// </summary>
        private CalendarService GetCalendarService( SchedulingProvider schedulingProvider, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var serviceAccountEmail = schedulingProvider.GetAttributeValue( "ServiceAccountEmail" );
                var serviceAccountPrivateKey = schedulingProvider.GetAttributeValue( "ServiceAccountPrivateKey" );
                var adminUserEmail = schedulingProvider.GetAttributeValue( "AdminUserEmail" );

                if ( string.IsNullOrWhiteSpace( serviceAccountEmail ) ||
                     string.IsNullOrWhiteSpace( serviceAccountPrivateKey ) ||
                     string.IsNullOrWhiteSpace( adminUserEmail ) )
                {
                    errorMessages.Add( "Missing required Google authentication configuration" );
                    return null;
                }

                var credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer( serviceAccountEmail )
                    {
                        Scopes = new[] { CalendarService.Scope.Calendar },
                        User = adminUserEmail
                    }.FromPrivateKey( serviceAccountPrivateKey )
                );

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

        /// <summary>
        /// Converts a Google Calendar event to a SchedulingProviderEvent.
        /// </summary>
        private SchedulingProviderEvent ConvertFromGoogleEvent( Event googleEvent, string calendarId )
        {
            var providerEvent = new SchedulingProviderEvent
            {
                ExternalId = $"{calendarId}|{googleEvent.Id}",
                Title = googleEvent.Summary,
                Description = googleEvent.Description,
                Status = googleEvent.Status,
                Visibility = googleEvent.Visibility,
                RecurrenceRule = googleEvent.Recurrence != null ? string.Join( ";", googleEvent.Recurrence ) : null,
                CreatedDateTime = googleEvent.Created,
                ModifiedDateTime = googleEvent.Updated,
                ICalendarContent = GenerateICalendarContent( googleEvent )
            };

            // Parse start and end times
            if ( googleEvent.Start != null )
            {
                if ( googleEvent.Start.DateTime.HasValue )
                {
                    providerEvent.StartDateTime = googleEvent.Start.DateTime.Value;
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.Start.Date ) )
                {
                    providerEvent.StartDateTime = DateTime.Parse( googleEvent.Start.Date );
                    providerEvent.IsAllDay = true;
                }
            }

            if ( googleEvent.End != null )
            {
                if ( googleEvent.End.DateTime.HasValue )
                {
                    providerEvent.EndDateTime = googleEvent.End.DateTime.Value;
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.End.Date ) )
                {
                    providerEvent.EndDateTime = DateTime.Parse( googleEvent.End.Date );
                }
            }

            // Parse organizer
            if ( googleEvent.Organizer != null )
            {
                providerEvent.Organizer = new SchedulingProviderPerson
                {
                    DisplayName = googleEvent.Organizer.DisplayName,
                    Email = googleEvent.Organizer.Email
                };
            }

            // Parse attendees
            if ( googleEvent.Attendees != null )
            {
                foreach ( var attendee in googleEvent.Attendees )
                {
                    providerEvent.Attendees.Add( new SchedulingProviderPerson
                    {
                        DisplayName = attendee.DisplayName,
                        Email = attendee.Email,
                        Metadata = new Dictionary<string, object>
                        {
                            { "ResponseStatus", attendee.ResponseStatus },
                            { "IsResource", attendee.Resource ?? false }
                        }
                    } );
                }
            }

            // Parse location
            if ( !string.IsNullOrWhiteSpace( googleEvent.Location ) )
            {
                providerEvent.Locations.Add( new Data.SchedulingProviderLocation
                {
                    Name = googleEvent.Location,
                    ExternalId = calendarId
                } );
            }

            return providerEvent;
        }

        /// <summary>
        /// Converts a SchedulingProviderEvent to a Google Calendar event.
        /// </summary>
        private Event ConvertToGoogleEvent( SchedulingProviderEvent providerEvent )
        {
            var googleEvent = new Event
            {
                Summary = providerEvent.Title,
                Description = providerEvent.Description,
                Status = providerEvent.Status ?? "confirmed",
                Visibility = providerEvent.Visibility
            };

            // Set start and end times
            if ( providerEvent.StartDateTime.HasValue )
            {
                googleEvent.Start = new EventDateTime();
                if ( providerEvent.IsAllDay )
                {
                    googleEvent.Start.Date = providerEvent.StartDateTime.Value.ToString( "yyyy-MM-dd" );
                }
                else
                {
                    googleEvent.Start.DateTime = providerEvent.StartDateTime.Value;
                }
            }

            if ( providerEvent.EndDateTime.HasValue )
            {
                googleEvent.End = new EventDateTime();
                if ( providerEvent.IsAllDay )
                {
                    googleEvent.End.Date = providerEvent.EndDateTime.Value.ToString( "yyyy-MM-dd" );
                }
                else
                {
                    googleEvent.End.DateTime = providerEvent.EndDateTime.Value;
                }
            }

            // Set attendees (including room resources)
            if ( providerEvent.Attendees.Any() || providerEvent.Locations.Any() )
            {
                googleEvent.Attendees = new List<EventAttendee>();

                foreach ( var attendee in providerEvent.Attendees )
                {
                    googleEvent.Attendees.Add( new EventAttendee
                    {
                        DisplayName = attendee.DisplayName,
                        Email = attendee.Email
                    } );
                }

                // Add locations as resource attendees
                foreach ( var location in providerEvent.Locations )
                {
                    if ( !string.IsNullOrWhiteSpace( location.Email ) )
                    {
                        googleEvent.Attendees.Add( new EventAttendee
                        {
                            DisplayName = location.Name,
                            Email = location.Email,
                            Resource = true
                        } );
                    }
                }
            }

            // Set location string
            if ( providerEvent.Locations.Any() )
            {
                googleEvent.Location = string.Join( ", ", providerEvent.Locations.Select( l => l.Name ) );
            }

            // Set recurrence if provided
            if ( !string.IsNullOrWhiteSpace( providerEvent.RecurrenceRule ) )
            {
                googleEvent.Recurrence = new List<string> { providerEvent.RecurrenceRule };
            }

            return googleEvent;
        }

        /// <summary>
        /// Generates iCalendar (ICS) content from a Google Calendar event.
        /// </summary>
        private string GenerateICalendarContent( Event googleEvent )
        {
            if ( googleEvent == null )
            {
                return null;
            }

            var icsBuilder = new System.Text.StringBuilder();
            icsBuilder.AppendLine( "BEGIN:VCALENDAR" );
            icsBuilder.AppendLine( "VERSION:2.0" );
            icsBuilder.AppendLine( "PRODID:-//Rock RMS//Room Management//EN" );
            icsBuilder.AppendLine( "BEGIN:VEVENT" );

            // UID
            if ( !string.IsNullOrWhiteSpace( googleEvent.ICalUID ) )
            {
                icsBuilder.AppendLine( $"UID:{googleEvent.ICalUID}" );
            }

            // Summary
            if ( !string.IsNullOrWhiteSpace( googleEvent.Summary ) )
            {
                icsBuilder.AppendLine( $"SUMMARY:{EscapeICalText( googleEvent.Summary )}" );
            }

            // Description
            if ( !string.IsNullOrWhiteSpace( googleEvent.Description ) )
            {
                icsBuilder.AppendLine( $"DESCRIPTION:{EscapeICalText( googleEvent.Description )}" );
            }

            // Start time
            if ( googleEvent.Start != null )
            {
                if ( googleEvent.Start.DateTime.HasValue )
                {
                    var dtStart = googleEvent.Start.DateTime.Value.ToUniversalTime();
                    icsBuilder.AppendLine( $"DTSTART:{dtStart:yyyyMMddTHHmmss}Z" );
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.Start.Date ) )
                {
                    icsBuilder.AppendLine( $"DTSTART;VALUE=DATE:{googleEvent.Start.Date.Replace( "-", "" )}" );
                }
            }

            // End time
            if ( googleEvent.End != null )
            {
                if ( googleEvent.End.DateTime.HasValue )
                {
                    var dtEnd = googleEvent.End.DateTime.Value.ToUniversalTime();
                    icsBuilder.AppendLine( $"DTEND:{dtEnd:yyyyMMddTHHmmss}Z" );
                }
                else if ( !string.IsNullOrWhiteSpace( googleEvent.End.Date ) )
                {
                    icsBuilder.AppendLine( $"DTEND;VALUE=DATE:{googleEvent.End.Date.Replace( "-", "" )}" );
                }
            }

            // Location
            if ( !string.IsNullOrWhiteSpace( googleEvent.Location ) )
            {
                icsBuilder.AppendLine( $"LOCATION:{EscapeICalText( googleEvent.Location )}" );
            }

            // Status
            if ( !string.IsNullOrWhiteSpace( googleEvent.Status ) )
            {
                icsBuilder.AppendLine( $"STATUS:{googleEvent.Status.ToUpper()}" );
            }

            // Created
            if ( googleEvent.Created.HasValue )
            {
                var created = googleEvent.Created.Value.ToUniversalTime();
                icsBuilder.AppendLine( $"CREATED:{created:yyyyMMddTHHmmss}Z" );
            }

            // Last Modified
            if ( googleEvent.Updated.HasValue )
            {
                var updated = googleEvent.Updated.Value.ToUniversalTime();
                icsBuilder.AppendLine( $"LAST-MODIFIED:{updated:yyyyMMddTHHmmss}Z" );
            }

            // Organizer
            if ( googleEvent.Organizer != null && !string.IsNullOrWhiteSpace( googleEvent.Organizer.Email ) )
            {
                var organizerLine = $"ORGANIZER;CN={EscapeICalText( googleEvent.Organizer.DisplayName ?? googleEvent.Organizer.Email )}:mailto:{googleEvent.Organizer.Email}";
                icsBuilder.AppendLine( organizerLine );
            }

            // Attendees
            if ( googleEvent.Attendees != null )
            {
                foreach ( var attendee in googleEvent.Attendees )
                {
                    if ( !string.IsNullOrWhiteSpace( attendee.Email ) )
                    {
                        var role = attendee.Resource == true ? "NON-PARTICIPANT" : "REQ-PARTICIPANT";
                        var partstat = ConvertResponseStatus( attendee.ResponseStatus );
                        var attendeeLine = $"ATTENDEE;ROLE={role};PARTSTAT={partstat};CN={EscapeICalText( attendee.DisplayName ?? attendee.Email )}:mailto:{attendee.Email}";
                        icsBuilder.AppendLine( attendeeLine );
                    }
                }
            }

            // Recurrence rules and dates
            if ( googleEvent.Recurrence != null && googleEvent.Recurrence.Any() )
            {
                foreach ( var rule in googleEvent.Recurrence )
                {
                    // Google stores RRULE, EXRULE, RDATE, and EXDATE in the recurrence array
                    // We'll pass them through as-is since they're already in iCalendar format
                    if ( !string.IsNullOrWhiteSpace( rule ) )
                    {
                        icsBuilder.AppendLine( rule );
                    }
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
        /// Converts Google Calendar response status to iCalendar PARTSTAT.
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
                case "tentative":
                    return "TENTATIVE";
                default:
                    return "NEEDS-ACTION";
            }
        }

        #endregion
    }
}
