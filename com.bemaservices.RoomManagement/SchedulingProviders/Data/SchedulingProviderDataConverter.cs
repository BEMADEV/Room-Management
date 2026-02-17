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
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.SchedulingProviders.Data
{
    /// <summary>
    /// Helper class to convert between Rock Reservations and generic SchedulingProviderEvent objects.
    /// </summary>
    public static class SchedulingProviderDataConverter
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
            SchedulingProviderEvent providerEvent,
            RockContext rockContext,
            int defaultReservationTypeId,
            ReservationApprovalState defaultApprovalState )
        {
            if ( providerEvent == null )
            {
                return null;
            }

            var reservation = new Reservation
            {
                Name = providerEvent.Title ?? "Imported Event",
                Note = providerEvent.Description,
                ReservationTypeId = defaultReservationTypeId,
                ApprovalState = defaultApprovalState
            };

            reservation = UpdateReservationFromProviderEvent( reservation, providerEvent, rockContext );

            return reservation;
        }

        public static Reservation UpdateReservationFromProviderEvent(
            Reservation reservation,
            SchedulingProviderEvent providerEvent,
            RockContext rockContext )
        {
            // Update basic properties
            reservation.Name = providerEvent.Title ?? reservation.Name;
            reservation.Note = providerEvent.Description ?? reservation.Note;

            // Update schedule if dates have changed
            if ( providerEvent.StartDateTime.HasValue && providerEvent.EndDateTime.HasValue )
            {
                var schedule = reservation.Schedule;

                if ( schedule != null )
                {
                    // Update existing schedule
                    schedule.iCalendarContent = CreateiCalendarContent( providerEvent );
                }
                else
                {
                    // Create new schedule
                    schedule = new Schedule
                    {
                        iCalendarContent = CreateiCalendarContent( providerEvent )
                    };
                    var scheduleService = new ScheduleService( rockContext );
                    scheduleService.Add( schedule );
                    rockContext.SaveChanges();
                    reservation.ScheduleId = schedule.Id;
                }
            }

            // Update locations
            if ( providerEvent.Locations != null && providerEvent.Locations.Any() )
            {
                var locationService = new LocationService( rockContext );

                // Remove existing locations that are not in the provider event
                var providerLocationNames = providerEvent.Locations.Select( l => l.ExternalId ).ToList();
                var locationsToRemove = reservation.ReservationLocations
                    .Where( rl => !providerLocationNames.Contains( rl.Location?.Name ) )
                    .ToList();

                foreach ( var locationToRemove in locationsToRemove )
                {
                    reservation.ReservationLocations.Remove( locationToRemove );
                }

                // Add new locations
                var existingLocationNames = reservation.ReservationLocations
                    .Select( rl => rl.Location?.Name )
                    .ToList();

                foreach ( var providerLocation in providerEvent.Locations )
                {
                    if ( existingLocationNames.Contains( providerLocation.Name ) )
                    {
                        continue;
                    }

                    var location = locationService.Queryable()
                        .FirstOrDefault( l => l.Name == providerLocation.Name );

                    if ( location != null )
                    {
                        var reservationLocation = new ReservationLocation
                        {
                            LocationId = location.Id,
                            ApprovalState = ReservationLocationApprovalState.Approved
                        };
                        reservation.ReservationLocations.Add( reservationLocation );
                    }
                }
            }

            // Update event contact if organizer is available
            if ( providerEvent.Organizer != null && !string.IsNullOrWhiteSpace( providerEvent.Organizer.Email ) )
            {
                reservation.EventContactEmail = providerEvent.Organizer.Email;

                var personService = new PersonService( rockContext );
                var person = personService.Queryable()
                    .FirstOrDefault( p => p.Email != null && p.Email.Equals( providerEvent.Organizer.Email, StringComparison.OrdinalIgnoreCase ) );
                if ( person != null )
                {
                    reservation.EventContactPersonAliasId = person.PrimaryAliasId;
                }
            }

            return reservation;
        }

        /// <summary>
        /// Converts a Rock Reservation to a SchedulingProviderEvent.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A new SchedulingProviderEvent instance.</returns>
        public static SchedulingProviderEvent GenerateProviderEventFromReservation( Reservation reservation, RockContext rockContext )
        {
            if ( reservation == null )
            {
                return null;
            }

            var providerEvent = new SchedulingProviderEvent
            {
                Title = reservation.Name,
                Description = reservation.Note,
                Status = reservation.ApprovalState == ReservationApprovalState.Approved ? "confirmed" : "tentative"
            };

            // Get schedule times
            if ( reservation.Schedule != null )
            {
                var beginDate = RockDateTime.Today.AddMonths( -1 );
                var endDate = RockDateTime.Today.AddMonths( 6 );
                var reservationTimes = reservation.GetReservationTimes( beginDate, endDate );
                if ( reservationTimes != null && reservationTimes.Any() )
                {
                    var firstOccurrence = reservationTimes.First();
                    providerEvent.StartDateTime = firstOccurrence.StartDateTime;
                    providerEvent.EndDateTime = firstOccurrence.EndDateTime;

                    // If there are multiple occurrences, this might be a recurring event
                    if ( reservationTimes.Count > 1 )
                    {
                        // You could set RecurrenceRule here if you want to parse the iCalendar content
                        providerEvent.Metadata["IsRecurring"] = true;
                        providerEvent.Metadata["OccurrenceCount"] = reservationTimes.Count;
                    }
                }
            }

            // Add locations
            if ( reservation.ReservationLocations != null )
            {
                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    var location = reservationLocation.Location;
                    if ( location != null )
                    {
                        var providerLocation = new SchedulingProviderLocation
                        {
                            Name = location.Name,
                            ExternalId = location.Guid.ToString()
                        };
                        providerEvent.Locations.Add( providerLocation );
                    }
                }
            }

            // Add organizer information
            if ( reservation.EventContactPersonAlias != null && reservation.EventContactPersonAlias.Person != null )
            {
                var person = reservation.EventContactPersonAlias.Person;
                providerEvent.Organizer = new SchedulingProviderPerson
                {
                    Email = person.Email,
                    DisplayName = person.FullName,
                    ExternalId = person.Guid.ToString(),
                    IsOrganizer = true
                };
            }
            else if ( !string.IsNullOrWhiteSpace( reservation.EventContactEmail ) )
            {
                providerEvent.Organizer = new SchedulingProviderPerson
                {
                    Email = reservation.EventContactEmail,
                    DisplayName = reservation.EventContactEmail,
                    IsOrganizer = true
                };
            }

            // Add Rock reservation ID to metadata
            providerEvent.Metadata["RockReservationId"] = reservation.Id;
            providerEvent.Metadata["RockReservationGuid"] = reservation.Guid;

            return providerEvent;
        }

        /// <summary>
        /// Creates the iCalendar content for a scheduling provider event.
        /// </summary>
        /// <param name="providerEvent">The provider event.</param>
        /// <returns>iCalendar format string.</returns>
        private static string CreateiCalendarContent( SchedulingProviderEvent providerEvent )
        {
            if ( !providerEvent.StartDateTime.HasValue || !providerEvent.EndDateTime.HasValue )
            {
                return null;
            }

            var startDate = providerEvent.StartDateTime.Value;
            var endDate = providerEvent.EndDateTime.Value;

            // Simple iCalendar format for a single event
            var icalContent = $@"BEGIN:VCALENDAR
PRODID:-//Rock RMS//Room Management//EN
VERSION:2.0
BEGIN:VEVENT
DTSTART:{startDate:yyyyMMddTHHmmss}
DTEND:{endDate:yyyyMMddTHHmmss}
SUMMARY:{providerEvent.Title}
END:VEVENT
END:VCALENDAR";

            return icalContent;
        }
    }
}
