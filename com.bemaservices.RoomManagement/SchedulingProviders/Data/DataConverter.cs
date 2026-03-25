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
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using Microsoft.Graph.Models;
using Rock;
using Rock.Data;
using Rock.Lava.RockLiquid.Blocks;
using Rock.Model;
using Rock.Search.Person;
using Rock.SystemGuid;

namespace com.bemaservices.RoomManagement.SchedulingProviders.Data
{
    /// <summary>
    /// Helper class to convert between Rock Reservations and generic SchedulingProviderEvent objects.
    /// </summary>
    public static class DataConverter
    {
        /// <summary>
        /// Converts a SchedulingProviderEvent to a Rock Reservation.
        /// </summary>
        /// <param name="providerEvent">The provider event.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="defaultReservationTypeId">The default reservation type identifier.</param>
        /// <param name="defaultApprovalState">Default state of the approval.</param>
        /// <returns>A new Reservation instance.</returns>
        public static Reservation GenerateReservationFromProviderEvent(
            EventDTO providerEvent,
            RockContext rockContext,
            ReservationType defaultReservationType,
            ReservationApprovalState defaultApprovalState,
            int schedulingProviderId )
        {
            if ( providerEvent == null )
            {
                return null;
            }

            var reservation = new Reservation
            {
                Name = (providerEvent.Title ?? "Imported Event").Left( 50 ),
                Note = providerEvent.Description.StripHtml().Left(2500),
                ReservationTypeId = defaultReservationType.Id,
                ReservationType = defaultReservationType,
                ApprovalState = defaultApprovalState
            };

            reservation.ReservationLocations = new List<ReservationLocation>();

            reservation = UpdateReservationFromProviderEvent( reservation, providerEvent, rockContext, schedulingProviderId );

            return reservation;
        }

        public static Reservation UpdateReservationFromProviderEvent(
            Reservation reservation,
            EventDTO providerEvent,
            RockContext rockContext,
            int schedulingProviderId )
        {
            // Update basic properties
            reservation.Name = (providerEvent.Title ?? reservation.Name).Left(50);
            reservation.Note = (providerEvent.Description.StripHtml() ?? reservation.Note).Left( 2500 );


            var scheduleErrorMessage = String.Empty;
            
            // Convert the calendar event from UTC to Rock's organization timezone if needed
            var calendarEvent = providerEvent.CalendarEvent;
            if ( calendarEvent != null )
            {
                var orgTimeZone = RockDateTime.OrgTimeZoneInfo;
                
                // Convert Start time from UTC to org timezone
                if ( calendarEvent.Start != null )
                {
                    var startDateTime = calendarEvent.Start.AsUtc;
                    var orgStartTime = TimeZoneInfo.ConvertTimeFromUtc( startDateTime, orgTimeZone );
                    calendarEvent.Start = new Ical.Net.DataTypes.CalDateTime( orgStartTime );
                }
                
                // Convert End time from UTC to org timezone
                if ( calendarEvent.End != null )
                {
                    var endDateTime = calendarEvent.End.AsUtc;
                    var orgEndTime = TimeZoneInfo.ConvertTimeFromUtc( endDateTime, orgTimeZone );
                    calendarEvent.End = new Ical.Net.DataTypes.CalDateTime( orgEndTime );
                }
                
                // If there's a DTSTART in recurrence rules, convert it too
                if ( calendarEvent.RecurrenceRules != null )
                {
                    foreach ( var rrule in calendarEvent.RecurrenceRules )
                    {
                        if ( rrule.Until != DateTime.MinValue )
                        {
                            var untilUtc = DateTime.SpecifyKind( rrule.Until, DateTimeKind.Utc );
                            rrule.Until = TimeZoneInfo.ConvertTimeFromUtc( untilUtc, orgTimeZone );
                        }
                    }
                }
            }
            
            var iCalContent = InetCalendarHelper.SerializeToCalendarString( calendarEvent );
            var reservationSchedule = ReservationService.BuildScheduleFromICalContent( iCalContent );
            reservation.Schedule = ReservationService.UpdateScheduleWithMaxEndDate( reservationSchedule, reservation.ReservationType, out scheduleErrorMessage );
            reservation = ReservationService.UpdateFirstLastOccurrenceDateTimes( reservation );            

            // Update locations
            if ( providerEvent.Locations != null && providerEvent.Locations.Any() )
            {
                var schedulingProviderLocationService = new SchedulingProviderLocationService( rockContext );

                // Get all scheduling provider locations for this provider only
                var schedulingProviderLocations = schedulingProviderLocationService.Queryable()
                    .Where( spl => spl.SchedulingProviderId == schedulingProviderId )
                    .ToList();

                // Remove existing locations that are managed by this provider but not in the current provider event
                // Note: This will NOT remove locations that have no SchedulingProviderLocation mapping or are managed by a different provider
                var providerLocationExternalIds = providerEvent.Locations.Select( l => l.ExternalId ).ToList();
                var locationsToRemove = reservation.ReservationLocations
                    .Where( rl => 
                    {
                        // Check if this location is managed by THIS scheduling provider
                        var schedulingProviderLocation = schedulingProviderLocations
                            .FirstOrDefault( spl => spl.LocationId == rl.LocationId );
                        // Only remove if managed by this provider AND not in the current provider event
                        return schedulingProviderLocation != null && 
                               !providerLocationExternalIds.Contains( schedulingProviderLocation.ExternalId );
                    } )
                    .ToList();

                foreach ( var locationToRemove in locationsToRemove )
                {
                    reservation.ReservationLocations.Remove( locationToRemove );
                }

                // Add new locations
                var existingLocationIds = reservation.ReservationLocations
                    .Select( rl => rl.LocationId )
                    .ToList();

                foreach ( var providerLocation in providerEvent.Locations )
                {
                    var schedulingProviderLocation = schedulingProviderLocations
                        .FirstOrDefault( spl => spl.ExternalId == providerLocation.ExternalId );

                    if ( schedulingProviderLocation != null && !existingLocationIds.Contains( schedulingProviderLocation.LocationId ) )
                    {
                        var reservationLocation = new ReservationLocation
                        {
                            LocationId = schedulingProviderLocation.LocationId,
                            ApprovalState = ReservationLocationApprovalState.Approved
                        };
                        reservation.ReservationLocations.Add( reservationLocation );
                    }
                }
            }

            // Update event contact and administrative contact if organizer is available
            if ( providerEvent.Organizer != null && !string.IsNullOrWhiteSpace( providerEvent.Organizer.Email ) )
            {
                var firstName = string.Empty;
                var lastName = string.Empty;

                // Try to parse the display name into first and last name
                if ( !string.IsNullOrWhiteSpace( providerEvent.Organizer.DisplayName ) )
                {
                    var nameParts = providerEvent.Organizer.DisplayName.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameParts.Length > 0 )
                    {
                        firstName = nameParts[0];
                        if ( nameParts.Length > 1 )
                        {
                            lastName = string.Join( " ", nameParts.Skip( 1 ) );
                        }
                        else
                        {
                            lastName = nameParts[0];
                        }
                    }
                }
                else
                {
                    // Use email as name if no display name is available
                    var emailUsername = providerEvent.Organizer.Email.Split( '@' )[0];
                    firstName = emailUsername;
                    lastName = emailUsername;
                }

                var personService = new PersonService( rockContext );
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, providerEvent.Organizer.Email, null );
                var person = personService.FindPerson( personQuery, false );

                // Create a new person record if one doesn't exist
                if ( person == null )
                {
                    var recordTypePersonId = Rock.Web.Cache.DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )?.Id;
                    var recordStatusActiveId = Rock.Web.Cache.DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() )?.Id;

                    person = new Rock.Model.Person();
                    person.RecordTypeValueId = recordTypePersonId;
                    person.RecordStatusValueId = recordStatusActiveId;
                    person.FirstName = firstName.FixCase();
                    person.LastName = lastName.FixCase();
                    person.IsEmailActive = true;
                    person.Email = providerEvent.Organizer.Email;
                    person.EmailPreference = EmailPreference.EmailAllowed;

                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );
                    if ( familyGroup != null && familyGroup.Members.Any() )
                    {
                        person = familyGroup.Members.Select( m => m.Person ).First();
                    }
                }

                // Update both event contact and administrative contact
                reservation.EventContactEmail = providerEvent.Organizer.Email;
                reservation.EventContactPersonAliasId = person.PrimaryAliasId;

                reservation.AdministrativeContactEmail = providerEvent.Organizer.Email;
                reservation.AdministrativeContactPersonAliasId = person.PrimaryAliasId;
            }

            return reservation;
        }

        /// <summary>
        /// Converts a Rock Reservation to a SchedulingProviderEvent.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="schedulingProviderId">The scheduling provider identifier.</param>
        /// <returns>A new SchedulingProviderEvent instance.</returns>
        public static EventDTO GenerateProviderEventFromReservation( Reservation reservation, RockContext rockContext, int schedulingProviderId )
        {
            if ( reservation == null )
            {
                return null;
            }

            var providerEvent = new EventDTO();
            providerEvent.Title = reservation.Name;
            providerEvent.Description = reservation.Note;
            providerEvent.CalendarEvent = reservation.Schedule.GetICalEvent(); 
            providerEvent.CreatedDateTime = reservation.CreatedDateTime;
            providerEvent.ModifiedDateTime = reservation.ModifiedDateTime;

            // Add locations
            if ( reservation.ReservationLocations != null )
            {
                var schedulingProviderLocationService = new SchedulingProviderLocationService( rockContext );

                // Get all scheduling provider locations for this provider
                var schedulingProviderLocations = schedulingProviderLocationService.Queryable()
                    .Where( spl => spl.SchedulingProviderId == schedulingProviderId )
                    .ToList();

                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    var location = reservationLocation.Location;
                    if ( location != null )
                    {
                        // Find the external ID for this location from the scheduling provider mapping
                        var schedulingProviderLocation = schedulingProviderLocations
                            .FirstOrDefault( spl => spl.LocationId == location.Id );

                        // Only add locations that are mapped to this scheduling provider
                        if ( schedulingProviderLocation != null )
                        {
                            var providerLocation = new LocationDTO
                            {
                                DisplayName = location.Name,
                                ExternalId = schedulingProviderLocation.ExternalId
                            };
                            providerEvent.Locations.Add( providerLocation );
                        }
                    }
                }
            }

            // Add organizer information
            if ( reservation.EventContactPersonAlias != null && reservation.EventContactPersonAlias.Person != null )
            {
                var person = reservation.EventContactPersonAlias.Person;
                providerEvent.Organizer = new PersonDTO
                {
                    Email = person.Email,
                    DisplayName = person.FullName
                };
            }
            else if ( !string.IsNullOrWhiteSpace( reservation.EventContactEmail ) )
            {
                providerEvent.Organizer = new PersonDTO
                {
                    Email = reservation.EventContactEmail,
                    DisplayName = reservation.EventContactEmail
                };
            }

            return providerEvent;
        }
    }
}
