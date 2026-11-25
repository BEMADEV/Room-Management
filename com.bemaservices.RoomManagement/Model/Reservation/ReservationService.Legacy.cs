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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// The conflict checks as they were before <see cref="ReservationConflictChecker" />, each running its own queries. Kept
    /// only until the comparison tool shows the checker gives the same results on a real database; then this file is deleted.
    /// </summary>
    public partial class ReservationService
    {
        #region Legacy Reservation Conflict Methods

        /// <summary>
        /// Gets the conflicting reservation summaries.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IEnumerable&lt;ReservationSummary&gt;.</returns>
        private IEnumerable<Model.ReservationSummary> LegacyGetConflictingReservationSummaries( Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            return LegacyGetConflictingReservationSummaries( newReservation, Queryable(), arePotentialConflictsReturned );
        }

        /// <summary>
        /// Gets the conflicting reservation summaries.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="existingReservationQry">The existing reservation qry.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IEnumerable&lt;ReservationSummary&gt;.</returns>
        private List<Model.ReservationSummary> LegacyGetConflictingReservationSummaries( Reservation newReservation, IQueryable<Reservation> existingReservationQry, bool arePotentialConflictsReturned = false )
        {
            var newReservationQry = new List<Reservation>() { newReservation }.AsQueryable();
            var filteredExistingReservationQry = existingReservationQry.AsNoTracking().ValidExistingReservations( newReservation.Id, arePotentialConflictsReturned );

            var window = ReservationConflictChecker.ConflictWindow.ForConflicts( newReservation, RockDateTime.Now );
            var newReservationSummaries = newReservationQry.GetReservationSummaries( window.Start, window.End );
            var existingReservationSummaries = filteredExistingReservationQry.GetReservationSummaries( window.Start, window.End );

            return existingReservationSummaries.WhereConflictsExist( newReservationSummaries );
        }

        /// <summary>
        /// Generates the conflict information.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>System.String.</returns>
        internal string LegacyGenerateConflictInfo( Reservation reservation, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            // Check to make sure that nothing has a scheduling conflict.
            bool hasConflict = false;
            StringBuilder sb = new StringBuilder();
            sb.Append( FormatConflictInfoHeader( arePotentialConflictsReturned ) );

            var reservedLocationIds = LegacyGetReservedLocationIds( reservation, true, arePotentialConflictsReturned );

            // Check self
            string message = string.Empty;
            foreach ( var location in reservation.ReservationLocations.Where( l => reservedLocationIds.Contains( l.LocationId ) ) )
            {
                message = LegacyBuildLocationConflictHtmlList( reservation, location.Location.Id, detailPageRoute, arePotentialConflictsReturned );
                sb.Append( FormatLocationConflictItem( location.Location.Name, message ) );
                hasConflict = true;
            }

            // Check resources...
            var resources = reservation.ReservationResources.Select( rr => rr.Resource ).Distinct().ToList();
            foreach ( var resource in resources )
            {
                var reservationQuantity = reservation.ReservationResources.Where( rr => rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity );
                var availableQuantity = LegacyGetAvailableResourceQuantity( resource, reservation, arePotentialConflictsReturned );
                if ( availableQuantity.HasValue && availableQuantity - reservationQuantity < 0 )
                {
                    message = LegacyBuildResourceConflictHtmlList( reservation, resource.Id, detailPageRoute, arePotentialConflictsReturned );
                    sb.Append( FormatResourceConflictItem( resource.Name, availableQuantity, message ) );
                    hasConflict = true;
                }
            }

            if ( hasConflict )
            {
                sb.Append( FormatConflictInfoFooter() );
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Legacy Location Conflict Methods

        /// <summary>
        /// Gets locations occupied by reservations that conflict with the new reservation.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="filterByLocations">if set to <c>true</c> [filter by locations].</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <param name="areAncestorsReturned">if set to <c>true</c> [are ancestors returned].</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        internal List<int> LegacyGetReservedLocationIds( Reservation newReservation, bool filterByLocations = true, bool arePotentialConflictsReturned = false, bool areAncestorsReturned = true )
        {
            using ( var locationRockContext = new RockContext() )
            {
                var locationService = new LocationService( locationRockContext );

                // Get any Locations related to those reserved by the new Reservation
                var newReservationLocationIds = newReservation.ReservationLocations.Select( rl => rl.LocationId ).ToList();
                var relevantLocationIds = new List<int>();
                relevantLocationIds.AddRange( newReservationLocationIds );
                relevantLocationIds.AddRange( newReservationLocationIds.SelectMany( l => locationService.GetAllAncestorIds( l ) ) );
                relevantLocationIds.AddRange( newReservationLocationIds.SelectMany( l => locationService.GetAllDescendentIds( l ) ) );

                // Get any Reservations containing related Locations
                var existingReservationQry = Queryable();
                if ( filterByLocations )
                {
                    existingReservationQry = existingReservationQry.Where( r => r.ReservationLocations.Any( rl => relevantLocationIds.Contains( rl.LocationId ) ) );
                }

                // Check existing Reservations for conflicts
                IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = LegacyGetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );

                // Grab any locations booked by conflicting Reservations
                var reservedLocationIds = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                        currentReservationSummary.ReservationLocations.Where( rl =>
                            rl.ApprovalState != ReservationLocationApprovalState.Denied )
                            .Select( rl => rl.LocationId )
                            )
                      .Distinct();

                var reservedLocationAndChildIds = new List<int>();
                reservedLocationAndChildIds.AddRange( reservedLocationIds );
                reservedLocationAndChildIds.AddRange( reservedLocationIds.SelectMany( l => locationService.GetAllDescendentIds( l ) ) );

                if ( areAncestorsReturned )
                {
                    reservedLocationAndChildIds.AddRange( reservedLocationIds.SelectMany( l => locationService.GetAllAncestorIds( l ) ) );
                }

                return reservedLocationAndChildIds;
            }
        }

        /// <summary>
        /// Gets non-denied reservation conflicts for a location and its related locations.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>List&lt;ReservationConflict&gt;.</returns>
        internal List<ReservationConflict> LegacyGetConflictsForLocationId( int locationId, Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            using ( var locationRockContext = new RockContext() )
            {
                var locationService = new LocationService( locationRockContext );

                var relevantLocationIds = new List<int>();
                relevantLocationIds.Add( locationId );
                relevantLocationIds.AddRange( locationService.GetAllAncestorIds( locationId ) );
                relevantLocationIds.AddRange( locationService.GetAllDescendentIds( locationId ) );

                // Get any Reservations containing related Locations
                var existingReservationQry = Queryable().Where( r => r.ReservationLocations.Any( rl => relevantLocationIds.Contains( rl.LocationId ) ) );

                // Check existing Reservations for conflicts
                IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = LegacyGetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );
                var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                        currentReservationSummary.ReservationLocations.Where( rl =>
                            rl.ApprovalState != ReservationLocationApprovalState.Denied &&
                            relevantLocationIds.Contains( rl.LocationId ) )
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

                return locationConflicts;
            }
        }

        /// <summary>
        /// Builds a conflict message string (as HTML List) and returns it if there are location conflicts.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>an HTML List if conflicts exists; null otherwise.</returns>
        internal string LegacyBuildLocationConflictHtmlList( Reservation newReservation, int locationId, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            var conflicts = LegacyGetConflictsForLocationId( locationId, newReservation, arePotentialConflictsReturned );
            return FormatLocationConflicts( conflicts, detailPageRoute );
        }

        #endregion

        #region Legacy Resource Conflict Methods

        /// <summary>
        /// Gets the available resource quantity for the given resource, during the time (schedule)
        /// of the given resource -- but excluding the ones used by the given resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>a quantity of available resources at</returns>
        internal int? LegacyGetAvailableResourceQuantity( Resource resource, Reservation reservation, bool arePotentialConflictsReturned = false )
        {
            // For each new reservation summary, make sure that the quantities of existing summaries that come into contact with it
            // do not exceed the resource's quantity

            if ( !resource.Quantity.HasValue )
            {
                return null;
            }
            else
            {
                var maxReservedQuantity = LegacyGetBookedResourceQuantity( resource, reservation, arePotentialConflictsReturned );
                return resource.Quantity - maxReservedQuantity;
            }
        }

        /// <summary>
        /// Gets the available resource quantity for the given resource, during the time (schedule)
        /// of the given resource -- but excluding the ones used by the given resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>a quantity of available resources at</returns>
        internal int? LegacyGetBookedResourceQuantity( Resource resource, Reservation reservation, bool arePotentialConflictsReturned = false )
        {
            var window = ReservationConflictChecker.ConflictWindow.ForBookedQuantity( reservation, RockDateTime.Now );

            // Get all existing non-denied reservations (for a huge time period; a month before now and a year after
            // now) which have the given resource in them.
            var existingValidReservations = Queryable().AsNoTracking().ValidExistingReservations( reservation.Id, arePotentialConflictsReturned ).Where( r => r.ReservationResources.Any( rr => resource.Id == rr.ResourceId ) );
            var existingReservationSummaries = existingValidReservations.GetReservationSummaries( window.Start, window.End );

            // Now narrow the reservations down to only the ones in the matching/overlapping time frame
            var newReservationList = new List<Reservation>() { reservation }.AsQueryable();
            var newReservationSummaries = newReservationList.GetReservationSummaries( window.Start, window.End );
            var reservedQuantities = newReservationSummaries
                .Select( newReservationSummary =>
                    newReservationSummary.MatchingSummaries( existingReservationSummaries ).ReservedResourceQuantity( resource.Id )
               );

            var maxReservedQuantity = reservedQuantities.Count() > 0 ? reservedQuantities.Max() : 0;
            return maxReservedQuantity;
        }

        /// <summary>
        /// Gets the conflicts for resource identifier.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>List&lt;ReservationConflict&gt;.</returns>
        internal List<ReservationConflict> LegacyGetConflictsForResourceId( int resourceId, Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            // Get any Reservations containing related Locations
            var existingReservationQry = Queryable().Where( r => r.ReservationResources.Any( rl => rl.ResourceId == resourceId ) );

            // Check existing Reservations for conflicts
            IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = LegacyGetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );
            var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationResources.Where( rr =>
                        rr.ApprovalState != ReservationResourceApprovalState.Denied &&
                        rr.ResourceId == resourceId &&
                        rr.Quantity.HasValue )
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
            return locationConflicts;
        }

        /// <summary>
        /// Builds a conflict message string (as HTML List) and returns it if there are resource conflicts.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>an HTML List if conflicts exists; null otherwise.</returns>
        internal string LegacyBuildResourceConflictHtmlList( Reservation newReservation, int resourceId, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            var conflicts = LegacyGetConflictsForResourceId( resourceId, newReservation, arePotentialConflictsReturned );
            return FormatResourceConflicts( conflicts, detailPageRoute );
        }

        #endregion
    }
}
