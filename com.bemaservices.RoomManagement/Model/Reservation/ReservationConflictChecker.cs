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
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Finds the conflicts between reservations and the existing reservations they share locations or resources with. Every
    /// existing reservation that could conflict with any of the checked reservations is loaded in one query, each location's
    /// ancestors and descendants are looked up once, and each reservation's occurrences are worked out once and reused for all
    /// of its location and resource checks. Call <see cref="Check" /> for each reservation, then ask the result what it needs.
    /// </summary>
    internal class ReservationConflictChecker
    {
        #region Fields

        private readonly DateTime _now;
        private readonly LocationFamilyCache _locationFamilies;
        private readonly List<Reservation> _candidateReservations;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationConflictChecker" /> class and loads the existing reservations
        /// that could conflict with any of the given reservations.
        /// </summary>
        /// <param name="reservationService">The reservation service used to load the existing reservations.</param>
        /// <param name="reservations">The reservations that will be checked.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, check against requested but not yet booked reservations instead.</param>
        public ReservationConflictChecker( ReservationService reservationService, List<Reservation> reservations, bool arePotentialConflictsReturned )
        {
            _now = RockDateTime.Now;
            _locationFamilies = new LocationFamilyCache( new LocationService( ( RockContext ) reservationService.Context ) );
            _candidateReservations = GetCandidateReservations( reservationService, reservations, arePotentialConflictsReturned );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the conflict checks for one reservation, narrowing the loaded existing reservations to the ones that share a
        /// location (or one of that location's ancestors or descendants) or a resource with it.
        /// </summary>
        /// <param name="reservation">The reservation to check; it must be one of the reservations the checker was created with.</param>
        /// <returns>The reservation's conflict checks.</returns>
        public ReservationCheck Check( Reservation reservation )
        {
            return new ReservationCheck( reservation, _candidateReservations, _locationFamilies, _now );
        }

        /// <summary>
        /// Loads, in one query, every existing reservation that could conflict with any of the given reservations: it must pass
        /// <see cref="ReservationExtensionMethods.ValidExistingReservations" />, share a location (or one of that location's
        /// ancestors or descendants) or a resource with one of them, and pass the date prefilter
        /// <see cref="ReservationExtensionMethods.GetReservationSummaries" /> applies, widened to cover every reservation's
        /// windows. This is a superset; each reservation's checks narrow it down further.
        /// </summary>
        /// <param name="reservationService">The reservation service.</param>
        /// <param name="reservations">The reservations being checked.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, load requested but not yet booked reservations instead.</param>
        /// <returns>The candidate reservations, ordered by identifier, with their schedules, locations and resources loaded.</returns>
        private List<Reservation> GetCandidateReservations( ReservationService reservationService, List<Reservation> reservations, bool arePotentialConflictsReturned )
        {
            var locationIds = reservations
                .SelectMany( r => r.ReservationLocations.Select( rl => rl.LocationId ) )
                .Distinct()
                .SelectMany( locationId => _locationFamilies.GetFamilyIds( locationId ) )
                .Distinct()
                .ToList();

            var resourceIds = reservations
                .SelectMany( r => r.ReservationResources.Select( rr => rr.ResourceId ) )
                .Distinct()
                .ToList();

            if ( !locationIds.Any() && !resourceIds.Any() )
            {
                return new List<Reservation>();
            }

            // Cover both windows of every reservation; GetReservationSummaries reapplies each exact window later
            var windows = reservations
                .SelectMany( r => new[] { ConflictWindow.ForConflicts( r, _now ), ConflictWindow.ForBookedQuantity( r, _now ) } )
                .ToList();
            var earliestStart = windows.Min( w => w.Start );
            var latestEnd = windows.Max( w => w.End );

            return reservationService.Queryable( "Schedule,ReservationLocations.Location,ReservationResources.Resource,ReservationResources.ReservationLocation,ReservationDoorLockSchedules" )
                .ValidExistingReservations( null, arePotentialConflictsReturned )
                .Where( r =>
                    r.ReservationLocations.Any( rl => locationIds.Contains( rl.LocationId ) ) ||
                    r.ReservationResources.Any( rr => resourceIds.Contains( rr.ResourceId ) ) )
                .Where( r => r.FirstOccurrenceStartDateTime == null || r.FirstOccurrenceStartDateTime <= latestEnd )
                .Where( r => r.LastOccurrenceEndDateTime == null || r.LastOccurrenceEndDateTime >= earliestStart )
                .OrderBy( r => r.Id )
                .ToList();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// The conflict checks for one reservation. Its occurrences in the conflict window are worked out once when it's created;
        /// the booked-quantity window's occurrences are worked out the first time a resource with a limited quantity is checked.
        /// </summary>
        internal class ReservationCheck
        {
            private readonly Reservation _reservation;
            private readonly LocationFamilyCache _locationFamilies;
            private readonly DateTime _now;
            private readonly HashSet<int> _relatedLocationIds;
            private readonly HashSet<int> _resourceIds;
            private readonly List<Reservation> _relatedReservations;
            private readonly List<ReservationSummary> _newSummaries;
            private readonly List<ReservationSummary> _existingSummaries;
            private List<ReservationSummary> _bookedNewSummaries;
            private List<ReservationSummary> _bookedExistingSummaries;
            private HashSet<int> _reservedLocationIds;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReservationCheck" /> class.
            /// </summary>
            /// <param name="reservation">The reservation being checked.</param>
            /// <param name="candidateReservations">The existing reservations loaded by the checker.</param>
            /// <param name="locationFamilies">The location family cache for this check.</param>
            /// <param name="now">The time the check started.</param>
            public ReservationCheck( Reservation reservation, List<Reservation> candidateReservations, LocationFamilyCache locationFamilies, DateTime now )
            {
                _reservation = reservation;
                _locationFamilies = locationFamilies;
                _now = now;

                // Narrow the candidates to those sharing a related location or a resource with this reservation; a reservation never conflicts with itself
                _relatedLocationIds = new HashSet<int>( reservation.ReservationLocations.SelectMany( rl => locationFamilies.GetFamilyIds( rl.LocationId ) ) );
                _resourceIds = new HashSet<int>( reservation.ReservationResources.Select( rr => rr.ResourceId ) );
                _relatedReservations = candidateReservations
                    .Where( r => r.Id != reservation.Id )
                    .Where( r =>
                        r.ReservationLocations.Any( rl => _relatedLocationIds.Contains( rl.LocationId ) ) ||
                        r.ReservationResources.Any( rr => _resourceIds.Contains( rr.ResourceId ) ) )
                    .ToList();

                // Work out occurrences once for the conflict window and reuse them for every location and resource check
                var conflictWindow = ConflictWindow.ForConflicts( reservation, now );
                _newSummaries = conflictWindow.GetSummaries( new List<Reservation> { reservation } );
                _existingSummaries = conflictWindow.GetSummaries( _relatedReservations );
            }

            /// <summary>
            /// Gets the locations the reservation can't use: every non-denied location booked by an overlapping existing reservation
            /// that shares a related location, plus those locations' descendants and ancestors.
            /// </summary>
            /// <returns>The reserved location identifiers.</returns>
            public HashSet<int> GetReservedLocationIds()
            {
                if ( _reservedLocationIds == null )
                {
                    var reservedLocationIds = _existingSummaries
                        .Where( s => s.ReservationLocations.Any( rl => _relatedLocationIds.Contains( rl.LocationId ) ) )
                        .ToList()
                        .WhereConflictsExist( _newSummaries )
                        .SelectMany( s => s.ReservationLocations
                            .Where( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied )
                            .Select( rl => rl.LocationId ) )
                        .Distinct();

                    _reservedLocationIds = new HashSet<int>( reservedLocationIds.SelectMany( locationId => _locationFamilies.GetFamilyIds( locationId ) ) );
                }

                return _reservedLocationIds;
            }

            /// <summary>
            /// Lists the non-denied bookings of a location's family (the location, its descendants and its ancestors) by
            /// overlapping existing reservations.
            /// </summary>
            /// <param name="locationId">The location identifier.</param>
            /// <returns>The distinct location conflicts.</returns>
            public List<ReservationConflict> GetLocationConflicts( int locationId )
            {
                var familyIds = new HashSet<int>( _locationFamilies.GetFamilyIds( locationId ) );
                return _existingSummaries
                    .Where( s => s.ReservationLocations.Any( rl => familyIds.Contains( rl.LocationId ) ) )
                    .ToList()
                    .WhereConflictsExist( _newSummaries )
                    .SelectMany( s => s.ReservationLocations
                        .Where( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && familyIds.Contains( rl.LocationId ) )
                        .Select( rl => new ReservationConflict
                        {
                            LocationId = rl.LocationId,
                            LocationName = rl.Location.Name,
                            ReservationId = rl.ReservationId,
                            ReservationName = rl.Reservation.Name,
                            ReservationSchedule = rl.Reservation.Schedule.ToFriendlyScheduleText()
                        } ) )
                    .Distinct()
                    .ToList();
            }

            /// <summary>
            /// Gets how many of a resource are left once the most booked at once by overlapping existing reservations is taken out.
            /// </summary>
            /// <param name="resource">The resource.</param>
            /// <returns>The available quantity, or <c>null</c> if the resource's quantity isn't limited.</returns>
            public int? GetAvailableResourceQuantity( Resource resource )
            {
                if ( !resource.Quantity.HasValue )
                {
                    return null;
                }

                return resource.Quantity - GetBookedResourceQuantity( resource.Id );
            }

            /// <summary>
            /// Gets the most of a resource booked at once by existing reservations overlapping any one of the reservation's
            /// occurrences in the booked-quantity window.
            /// </summary>
            /// <param name="resourceId">The resource identifier.</param>
            /// <returns>The peak booked quantity, or 0 if the reservation has no occurrences in the window.</returns>
            public int GetBookedResourceQuantity( int resourceId )
            {
                // The booked-quantity window is wider than the conflict window, so its occurrences are worked out separately
                if ( _bookedNewSummaries == null )
                {
                    var bookedQuantityWindow = ConflictWindow.ForBookedQuantity( _reservation, _now );
                    _bookedNewSummaries = bookedQuantityWindow.GetSummaries( new List<Reservation> { _reservation } );
                    _bookedExistingSummaries = bookedQuantityWindow.GetSummaries( _relatedReservations.Where( r => r.ReservationResources.Any( rr => _resourceIds.Contains( rr.ResourceId ) ) ) );
                }

                var existingSummariesWithResource = _bookedExistingSummaries
                    .Where( s => s.ReservationResources.Any( rr => rr.ResourceId == resourceId ) )
                    .ToList();

                var reservedQuantities = _bookedNewSummaries
                    .Select( newSummary => newSummary.MatchingSummaries( existingSummariesWithResource ).ReservedResourceQuantity( resourceId ) )
                    .ToList();

                return reservedQuantities.Any() ? reservedQuantities.Max() : 0;
            }

            /// <summary>
            /// Lists the non-denied, quantified bookings of a resource by overlapping existing reservations.
            /// </summary>
            /// <param name="resourceId">The resource identifier.</param>
            /// <returns>The distinct resource conflicts.</returns>
            public List<ReservationConflict> GetResourceConflicts( int resourceId )
            {
                return _existingSummaries
                    .Where( s => s.ReservationResources.Any( rr => rr.ResourceId == resourceId ) )
                    .ToList()
                    .WhereConflictsExist( _newSummaries )
                    .SelectMany( s => s.ReservationResources
                        .Where( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resourceId && rr.Quantity.HasValue )
                        .Select( rr => new ReservationConflict
                        {
                            ResourceId = rr.ResourceId,
                            ResourceName = rr.Resource.Name,
                            ResourceQuantity = rr.Quantity.Value,
                            ReservationId = rr.ReservationId,
                            ReservationName = rr.Reservation.Name,
                            ReservationSchedule = rr.Reservation.Schedule.ToFriendlyScheduleText()
                        } ) )
                    .Distinct()
                    .ToList();
            }
        }

        /// <summary>
        /// A time window for the conflict checks.
        /// </summary>
        internal struct ConflictWindow
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConflictWindow" /> struct.
            /// </summary>
            /// <param name="start">The window start.</param>
            /// <param name="end">The window end.</param>
            private ConflictWindow( DateTime start, DateTime end )
            {
                Start = start;
                End = end;
            }

            /// <summary>
            /// Gets the window start.
            /// </summary>
            public DateTime Start { get; }

            /// <summary>
            /// Gets the window end.
            /// </summary>
            public DateTime End { get; }

            /// <summary>
            /// The window for conflict lists: from now, or the first occurrence if that's later, to the last occurrence, or a year from now.
            /// </summary>
            /// <param name="reservation">The reservation being checked.</param>
            /// <param name="now">The current time.</param>
            /// <returns>The conflict window.</returns>
            public static ConflictWindow ForConflicts( Reservation reservation, DateTime now )
            {
                var start = now;
                if ( reservation.FirstOccurrenceStartDateTime.HasValue && reservation.FirstOccurrenceStartDateTime > start )
                {
                    start = reservation.FirstOccurrenceStartDateTime.Value;
                }

                return new ConflictWindow( start, reservation.LastOccurrenceEndDateTime ?? now.AddYears( 1 ) );
            }

            /// <summary>
            /// The window for booked resource quantities: from the first occurrence, or a month ago, to the last occurrence, or a year from now.
            /// </summary>
            /// <param name="reservation">The reservation being checked.</param>
            /// <param name="now">The current time.</param>
            /// <returns>The booked-quantity window.</returns>
            public static ConflictWindow ForBookedQuantity( Reservation reservation, DateTime now )
            {
                return new ConflictWindow(
                    reservation.FirstOccurrenceStartDateTime ?? now.AddMonths( -1 ),
                    reservation.LastOccurrenceEndDateTime ?? now.AddYears( 1 ) );
            }

            /// <summary>
            /// Gets the reservations' occurrences in this window, the same way the database-backed checks do.
            /// </summary>
            /// <param name="reservations">The reservations, already loaded.</param>
            /// <returns>The reservation summaries.</returns>
            public List<ReservationSummary> GetSummaries( IEnumerable<Reservation> reservations )
            {
                return reservations.AsQueryable().GetReservationSummaries( Start, End );
            }
        }

        /// <summary>
        /// Caches each location's family (itself, its descendants and its ancestors) for one conflict check, since each
        /// lookup is a separate recursive query.
        /// </summary>
        internal class LocationFamilyCache
        {
            private readonly LocationService _locationService;
            private readonly Dictionary<int, List<int>> _familyIdsByLocationId = new Dictionary<int, List<int>>();

            /// <summary>
            /// Initializes a new instance of the <see cref="LocationFamilyCache" /> class.
            /// </summary>
            /// <param name="locationService">The location service used for the lookups.</param>
            public LocationFamilyCache( LocationService locationService )
            {
                _locationService = locationService;
            }

            /// <summary>
            /// Gets the location's identifier followed by its descendants' and ancestors' identifiers.
            /// </summary>
            /// <param name="locationId">The location identifier.</param>
            /// <returns>The family's location identifiers.</returns>
            public List<int> GetFamilyIds( int locationId )
            {
                List<int> familyIds;
                if ( !_familyIdsByLocationId.TryGetValue( locationId, out familyIds ) )
                {
                    familyIds = new List<int> { locationId };
                    familyIds.AddRange( _locationService.GetAllDescendentIds( locationId ) );
                    familyIds.AddRange( _locationService.GetAllAncestorIds( locationId ) );
                    _familyIdsByLocationId.Add( locationId, familyIds );
                }

                return familyIds;
            }
        }

        #endregion
    }
}
