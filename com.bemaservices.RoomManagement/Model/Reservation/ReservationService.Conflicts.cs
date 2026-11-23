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
using System.Text;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Conflict checks for many reservations at once, and the conflict message formatting they share with the
    /// single-reservation checks in ReservationService.cs.
    /// </summary>
    public partial class ReservationService
    {
        #region Batch Conflict Methods

        /// <summary>
        /// Generates the conflict information for many reservations at once. Each reservation's result is the same as calling
        /// <see cref="GenerateConflictInfo(Reservation, string, bool)" /> on it, but the database work is done once for the whole
        /// list instead of several times per reservation: every existing reservation that could conflict with any of them is
        /// loaded in one query, each location's ancestors and descendants are looked up once, and each reservation's occurrences
        /// are worked out once and reused for all of its location and resource checks. The conflict rules are unchanged; the same
        /// summary and overlap methods run in memory over the same time windows. Used by the Conflicting Reservation List.
        /// </summary>
        /// <param name="reservations">The reservations to check.</param>
        /// <param name="detailPageRoute">The reservation detail page route, used to link to conflicting reservations.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, check against requested but not yet booked reservations instead.</param>
        /// <returns>Each reservation's conflict HTML keyed by reservation identifier; an empty string means it has no conflicts.</returns>
        public Dictionary<int, string> GenerateConflictInfo( List<Reservation> reservations, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            var conflictInfoByReservationId = new Dictionary<int, string>();
            if ( reservations == null || !reservations.Any() )
            {
                return conflictInfoByReservationId;
            }

            var now = RockDateTime.Now;
            var locationFamilies = new LocationFamilyCache( new LocationService( ( RockContext ) Context ) );
            var candidateReservations = GetConflictCandidates( reservations, locationFamilies, now, arePotentialConflictsReturned );

            foreach ( var reservation in reservations )
            {
                // Same as ValidExistingReservations( reservation.Id ): a reservation never conflicts with itself
                var existingReservations = candidateReservations.Where( r => r.Id != reservation.Id ).ToList();
                conflictInfoByReservationId[reservation.Id] = GenerateConflictInfoFromCandidates( reservation, existingReservations, locationFamilies, now, detailPageRoute, arePotentialConflictsReturned );
            }

            return conflictInfoByReservationId;
        }

        /// <summary>
        /// Loads, in one query, every existing reservation that could conflict with any of the given reservations: it must pass
        /// <see cref="ReservationExtensionMethods.ValidExistingReservations" />, share a location (or one of that location's
        /// ancestors or descendants) or a resource with one of them, and pass the date prefilter
        /// <see cref="ReservationExtensionMethods.GetReservationSummaries" /> applies, widened to cover every reservation's
        /// windows. This is a superset; each reservation's checks narrow it down exactly as the single-reservation queries do.
        /// </summary>
        /// <param name="reservations">The reservations being checked.</param>
        /// <param name="locationFamilies">The location family cache for this check.</param>
        /// <param name="now">The time the check started.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, load requested but not yet booked reservations instead.</param>
        /// <returns>The candidate reservations, ordered by identifier, with their schedules, locations and resources loaded.</returns>
        private List<Reservation> GetConflictCandidates( List<Reservation> reservations, LocationFamilyCache locationFamilies, DateTime now, bool arePotentialConflictsReturned )
        {
            var locationIds = reservations
                .SelectMany( r => r.ReservationLocations.Select( rl => rl.LocationId ) )
                .Distinct()
                .SelectMany( locationId => locationFamilies.GetFamilyIds( locationId ) )
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
                .SelectMany( r => new[] { ConflictWindow.ForConflicts( r, now ), ConflictWindow.ForBookedQuantity( r, now ) } )
                .ToList();
            var earliestStart = windows.Min( w => w.Start );
            var latestEnd = windows.Max( w => w.End );

            return Queryable( "Schedule,ReservationLocations.Location,ReservationResources.Resource,ReservationResources.ReservationLocation,ReservationDoorLockSchedules" )
                .ValidExistingReservations( null, arePotentialConflictsReturned )
                .Where( r =>
                    r.ReservationLocations.Any( rl => locationIds.Contains( rl.LocationId ) ) ||
                    r.ReservationResources.Any( rr => resourceIds.Contains( rr.ResourceId ) ) )
                .Where( r => r.FirstOccurrenceStartDateTime == null || r.FirstOccurrenceStartDateTime <= latestEnd )
                .Where( r => r.LastOccurrenceEndDateTime == null || r.LastOccurrenceEndDateTime >= earliestStart )
                .OrderBy( r => r.Id )
                .ToList();
        }

        /// <summary>
        /// Builds one reservation's conflict HTML from preloaded candidates. Follows <see cref="GenerateConflictInfo(Reservation, string, bool)" />
        /// step for step: the reserved-location check (<see cref="GetReservedLocationIds" />), each reserved location's conflict list
        /// (<see cref="GetConflictsForLocationId" />), the available-quantity check (<see cref="GetAvailableResourceQuantity" />) and each
        /// over-booked resource's conflict list (<see cref="GetConflictsForResourceId" />).
        /// </summary>
        /// <param name="reservation">The reservation being checked.</param>
        /// <param name="existingReservations">The candidate reservations, not including this one.</param>
        /// <param name="locationFamilies">The location family cache for this check.</param>
        /// <param name="now">The time the check started.</param>
        /// <param name="detailPageRoute">The reservation detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, the candidates are requested but not yet booked reservations.</param>
        /// <returns>The conflict HTML, or an empty string if there are no conflicts.</returns>
        private string GenerateConflictInfoFromCandidates( Reservation reservation, List<Reservation> existingReservations, LocationFamilyCache locationFamilies, DateTime now, string detailPageRoute, bool arePotentialConflictsReturned )
        {
            // Narrow the candidates to those sharing a related location or a resource with this reservation
            var relatedLocationIds = new HashSet<int>( reservation.ReservationLocations.SelectMany( rl => locationFamilies.GetFamilyIds( rl.LocationId ) ) );
            var resourceIds = new HashSet<int>( reservation.ReservationResources.Select( rr => rr.ResourceId ) );
            var relatedReservations = existingReservations
                .Where( r =>
                    r.ReservationLocations.Any( rl => relatedLocationIds.Contains( rl.LocationId ) ) ||
                    r.ReservationResources.Any( rr => resourceIds.Contains( rr.ResourceId ) ) )
                .ToList();

            // Work out occurrences once for the conflict window and reuse them for every location and resource list
            var conflictWindow = ConflictWindow.ForConflicts( reservation, now );
            var newSummaries = conflictWindow.GetSummaries( new List<Reservation> { reservation } );
            var existingSummaries = conflictWindow.GetSummaries( relatedReservations );

            bool hasConflict = false;
            var sb = new StringBuilder();
            sb.Append( FormatConflictInfoHeader( arePotentialConflictsReturned ) );

            // Locations
            var reservedLocationIds = GetReservedLocationIds( existingSummaries, newSummaries, relatedLocationIds, locationFamilies );
            foreach ( var location in reservation.ReservationLocations.Where( l => reservedLocationIds.Contains( l.LocationId ) ) )
            {
                var conflicts = GetLocationConflicts( existingSummaries, newSummaries, locationFamilies.GetFamilyIds( location.Location.Id ) );
                sb.Append( FormatLocationConflictItem( location.Location.Name, FormatLocationConflicts( conflicts, detailPageRoute ) ) );
                hasConflict = true;
            }

            // Resources; booked quantities use their own window, worked out only if a resource has a limited quantity
            List<Model.ReservationSummary> bookedNewSummaries = null;
            List<Model.ReservationSummary> bookedExistingSummaries = null;
            var resources = reservation.ReservationResources.Select( rr => rr.Resource ).Distinct().ToList();
            foreach ( var resource in resources )
            {
                if ( !resource.Quantity.HasValue )
                {
                    continue;
                }

                if ( bookedNewSummaries == null )
                {
                    var bookedQuantityWindow = ConflictWindow.ForBookedQuantity( reservation, now );
                    bookedNewSummaries = bookedQuantityWindow.GetSummaries( new List<Reservation> { reservation } );
                    bookedExistingSummaries = bookedQuantityWindow.GetSummaries( relatedReservations.Where( r => r.ReservationResources.Any( rr => resourceIds.Contains( rr.ResourceId ) ) ) );
                }

                var reservationQuantity = reservation.ReservationResources.Where( rr => rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity );
                var availableQuantity = resource.Quantity - GetMaxBookedQuantity( bookedExistingSummaries, bookedNewSummaries, resource.Id );
                if ( availableQuantity - reservationQuantity < 0 )
                {
                    var conflicts = GetResourceConflicts( existingSummaries, newSummaries, resource.Id );
                    sb.Append( FormatResourceConflictItem( resource.Name, availableQuantity, FormatResourceConflicts( conflicts, detailPageRoute ) ) );
                    hasConflict = true;
                }
            }

            if ( hasConflict )
            {
                sb.Append( FormatConflictInfoFooter() );
                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the locations the reservation can't use, as <see cref="GetReservedLocationIds" /> does: every non-denied location
        /// booked by an overlapping existing reservation that shares a related location, plus those locations' descendants and ancestors.
        /// </summary>
        /// <param name="existingSummaries">The candidate reservations' occurrences in the conflict window.</param>
        /// <param name="newSummaries">The reservation's own occurrences in the conflict window.</param>
        /// <param name="relatedLocationIds">The reservation's locations with their ancestors and descendants.</param>
        /// <param name="locationFamilies">The location family cache for this check.</param>
        /// <returns>The reserved location identifiers.</returns>
        private static HashSet<int> GetReservedLocationIds( List<Model.ReservationSummary> existingSummaries, List<Model.ReservationSummary> newSummaries, HashSet<int> relatedLocationIds, LocationFamilyCache locationFamilies )
        {
            var conflictingSummaries = existingSummaries
                .Where( s => s.ReservationLocations.Any( rl => relatedLocationIds.Contains( rl.LocationId ) ) )
                .ToList()
                .WhereConflictsExist( newSummaries );

            var reservedLocationIds = conflictingSummaries
                .SelectMany( s => s.ReservationLocations
                    .Where( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied )
                    .Select( rl => rl.LocationId ) )
                .Distinct();

            return new HashSet<int>( reservedLocationIds.SelectMany( locationId => locationFamilies.GetFamilyIds( locationId ) ) );
        }

        /// <summary>
        /// Lists the non-denied bookings of a location's family by overlapping existing reservations, as
        /// <see cref="GetConflictsForLocationId" /> does.
        /// </summary>
        /// <param name="existingSummaries">The candidate reservations' occurrences in the conflict window.</param>
        /// <param name="newSummaries">The reservation's own occurrences in the conflict window.</param>
        /// <param name="locationFamilyIds">The location with its descendants and ancestors.</param>
        /// <returns>The distinct location conflicts.</returns>
        private static List<ReservationConflict> GetLocationConflicts( List<Model.ReservationSummary> existingSummaries, List<Model.ReservationSummary> newSummaries, List<int> locationFamilyIds )
        {
            var familyIds = new HashSet<int>( locationFamilyIds );
            return existingSummaries
                .Where( s => s.ReservationLocations.Any( rl => familyIds.Contains( rl.LocationId ) ) )
                .ToList()
                .WhereConflictsExist( newSummaries )
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
        /// Gets the most of a resource booked at once by existing reservations overlapping any one of the reservation's
        /// occurrences, as <see cref="GetBookedResourceQuantity" /> does.
        /// </summary>
        /// <param name="existingSummaries">The candidate reservations' occurrences in the booked-quantity window.</param>
        /// <param name="newSummaries">The reservation's own occurrences in the booked-quantity window.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>The peak booked quantity, or 0 if the reservation has no occurrences in the window.</returns>
        private static int GetMaxBookedQuantity( List<Model.ReservationSummary> existingSummaries, List<Model.ReservationSummary> newSummaries, int resourceId )
        {
            var existingSummariesWithResource = existingSummaries
                .Where( s => s.ReservationResources.Any( rr => rr.ResourceId == resourceId ) )
                .ToList();

            var reservedQuantities = newSummaries
                .Select( newSummary => newSummary.MatchingSummaries( existingSummariesWithResource ).ReservedResourceQuantity( resourceId ) )
                .ToList();

            return reservedQuantities.Any() ? reservedQuantities.Max() : 0;
        }

        /// <summary>
        /// Lists the non-denied, quantified bookings of a resource by overlapping existing reservations, as
        /// <see cref="GetConflictsForResourceId" /> does.
        /// </summary>
        /// <param name="existingSummaries">The candidate reservations' occurrences in the conflict window.</param>
        /// <param name="newSummaries">The reservation's own occurrences in the conflict window.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>The distinct resource conflicts.</returns>
        private static List<ReservationConflict> GetResourceConflicts( List<Model.ReservationSummary> existingSummaries, List<Model.ReservationSummary> newSummaries, int resourceId )
        {
            return existingSummaries
                .Where( s => s.ReservationResources.Any( rr => rr.ResourceId == resourceId ) )
                .ToList()
                .WhereConflictsExist( newSummaries )
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

        #endregion

        #region Conflict Formatting

        /// <summary>
        /// Gets the heading that starts a conflict message.
        /// </summary>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, the message lists potential rather than booked conflicts.</param>
        /// <returns>The heading HTML, which opens the item list.</returns>
        private static string FormatConflictInfoHeader( bool arePotentialConflictsReturned )
        {
            return arePotentialConflictsReturned
                ? "<b>The following items can be reserved, but have also been requested for the scheduled times:</b><br><ul>"
                : "<b>The following items can not be reserved, as they are already reserved for the scheduled times:</b><br><ul>";
        }

        /// <summary>
        /// Gets the HTML that closes a conflict message's item list.
        /// </summary>
        /// <returns>The closing HTML.</returns>
        private static string FormatConflictInfoFooter()
        {
            return "</ul>";
        }

        /// <summary>
        /// Formats one location in a conflict message.
        /// </summary>
        /// <param name="locationName">The location name.</param>
        /// <param name="conflictList">The list from <see cref="FormatLocationConflicts" />, or <c>null</c> if it found none.</param>
        /// <returns>The list item HTML.</returns>
        private static string FormatLocationConflictItem( string locationName, string conflictList )
        {
            return conflictList != null
                ? string.Format( "<li>{0} due to:<ul>{1}</ul></li>", locationName, conflictList )
                : string.Format( "<li>{0}</li>", locationName );
        }

        /// <summary>
        /// Formats one over-booked resource in a conflict message.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <param name="availableQuantity">How many of the resource are still available.</param>
        /// <param name="conflictList">The list from <see cref="FormatResourceConflicts" />, or <c>null</c> if it found none.</param>
        /// <returns>The list item HTML.</returns>
        private static string FormatResourceConflictItem( string resourceName, int? availableQuantity, string conflictList )
        {
            return string.Format( "<li>{0} [note: only {1} available] due to:<ul>{2}</ul></li>", resourceName, availableQuantity, conflictList );
        }

        /// <summary>
        /// Formats location conflicts as HTML list items linking to the conflicting reservations.
        /// </summary>
        /// <param name="conflicts">The location conflicts.</param>
        /// <param name="detailPageRoute">The reservation detail page route.</param>
        /// <returns>The list items, or <c>null</c> if there are no conflicts.</returns>
        private static string FormatLocationConflicts( List<ReservationConflict> conflicts, string detailPageRoute )
        {
            if ( !conflicts.Any() )
            {
                return null;
            }

            var sb = new StringBuilder();
            detailPageRoute = detailPageRoute.StartsWith( "/" ) ? detailPageRoute : "/" + detailPageRoute;

            foreach ( var conflict in conflicts )
            {
                sb.AppendFormat( "<li>{0} [on {1} via <a href='{4}?ReservationId={2}' target='_blank'>'{3}'</a>]</li>",
                    conflict.LocationName,
                    conflict.ReservationSchedule,
                    conflict.ReservationId,
                    conflict.ReservationName,
                    detailPageRoute
                    );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats resource conflicts as HTML list items linking to the conflicting reservations.
        /// </summary>
        /// <param name="conflicts">The resource conflicts.</param>
        /// <param name="detailPageRoute">The reservation detail page route.</param>
        /// <returns>The list items, or <c>null</c> if there are no conflicts.</returns>
        private static string FormatResourceConflicts( List<ReservationConflict> conflicts, string detailPageRoute )
        {
            if ( !conflicts.Any() )
            {
                return null;
            }

            var sb = new StringBuilder();
            detailPageRoute = detailPageRoute.StartsWith( "/" ) ? detailPageRoute : "/" + detailPageRoute;

            foreach ( var conflict in conflicts )
            {
                sb.AppendFormat( "<li>{0} ({5}) [on {1} via <a href='{4}?ReservationId={2}' target='_blank'>'{3}'</a>]</li>",
                    conflict.ResourceName,
                    conflict.ReservationSchedule,
                    conflict.ReservationId,
                    conflict.ReservationName,
                    detailPageRoute,
                    conflict.ResourceQuantity
                    );
            }

            return sb.ToString();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A time window for the conflict checks, created the same way the single-reservation checks choose theirs.
        /// </summary>
        private struct ConflictWindow
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
            public List<Model.ReservationSummary> GetSummaries( IEnumerable<Reservation> reservations )
            {
                return reservations.AsQueryable().GetReservationSummaries( Start, End );
            }
        }

        /// <summary>
        /// Caches each location's family (itself, its descendants and its ancestors) for one batch conflict check, since each
        /// lookup is a separate recursive query.
        /// </summary>
        private class LocationFamilyCache
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
