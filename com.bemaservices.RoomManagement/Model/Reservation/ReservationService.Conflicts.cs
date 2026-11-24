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
using System.Linq;
using System.Text;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Conflict checks for many reservations at once, and the conflict message formatting they share with the
    /// single-reservation checks in ReservationService.cs. The checks themselves are in <see cref="ReservationConflictChecker" />.
    /// </summary>
    public partial class ReservationService
    {
        #region Batch Conflict Methods

        /// <summary>
        /// Generates the conflict information for many reservations at once. Each reservation's result is the same as calling
        /// <see cref="GenerateConflictInfo(Reservation, string, bool)" /> on it, but <see cref="ReservationConflictChecker" /> does
        /// the database work once for the whole list instead of several times per reservation. Used by the Conflicting Reservation List.
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

            var conflictChecker = new ReservationConflictChecker( this, reservations, arePotentialConflictsReturned );
            foreach ( var reservation in reservations )
            {
                var reservationCheck = conflictChecker.Check( reservation );
                conflictInfoByReservationId[reservation.Id] = BuildConflictInfo( reservation, reservationCheck, detailPageRoute, arePotentialConflictsReturned );
            }

            return conflictInfoByReservationId;
        }

        /// <summary>
        /// Builds one reservation's conflict HTML: each of its locations that's already reserved, then each of its resources
        /// that would be over-booked, with the existing reservations causing each conflict.
        /// </summary>
        /// <param name="reservation">The reservation being checked.</param>
        /// <param name="reservationCheck">The reservation's conflict checks.</param>
        /// <param name="detailPageRoute">The reservation detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c>, the checks are against requested but not yet booked reservations.</param>
        /// <returns>The conflict HTML, or an empty string if there are no conflicts.</returns>
        private static string BuildConflictInfo( Reservation reservation, ReservationConflictChecker.ReservationCheck reservationCheck, string detailPageRoute, bool arePotentialConflictsReturned )
        {
            bool hasConflict = false;
            var sb = new StringBuilder();
            sb.Append( FormatConflictInfoHeader( arePotentialConflictsReturned ) );

            // Locations
            var reservedLocationIds = reservationCheck.GetReservedLocationIds();
            foreach ( var location in reservation.ReservationLocations.Where( l => reservedLocationIds.Contains( l.LocationId ) ) )
            {
                var conflicts = reservationCheck.GetLocationConflicts( location.Location.Id );
                sb.Append( FormatLocationConflictItem( location.Location.Name, FormatLocationConflicts( conflicts, detailPageRoute ) ) );
                hasConflict = true;
            }

            // Resources
            var resources = reservation.ReservationResources.Select( rr => rr.Resource ).Distinct().ToList();
            foreach ( var resource in resources )
            {
                var reservationQuantity = reservation.ReservationResources.Where( rr => rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity );
                var availableQuantity = reservationCheck.GetAvailableResourceQuantity( resource );
                if ( availableQuantity.HasValue && availableQuantity - reservationQuantity < 0 )
                {
                    var conflicts = reservationCheck.GetResourceConflicts( resource.Id );
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
    }
}
