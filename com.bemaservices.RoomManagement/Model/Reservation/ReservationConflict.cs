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
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// The view model for a Reservation Conflict
    /// </summary>
    public class ReservationConflict : IEquatable<ReservationConflict>
    {
        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the resource quantity.
        /// </summary>
        /// <value>The resource quantity.</value>
        public int ResourceQuantity { get; set; }

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>The reservation.</value>
        public string ReservationName { get; set; }

        /// <summary>
        /// Gets or sets the reservation schedule.
        /// </summary>
        /// <value>The reservation schedule.</value>
        public string ReservationSchedule { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals( ReservationConflict other )
        {
            if(
                ReservationId == other.ReservationId &&
                LocationId == other.LocationId &&
                ResourceId == other.ResourceId &&
                ResourceQuantity == other.ResourceQuantity &&
                ReservationSchedule == other.ReservationSchedule )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            int reservationIdHash = ReservationId == null ? 0 : ReservationId.GetHashCode();
            int locationIdHash = LocationId == null ? 0 : LocationId.GetHashCode();
            int resourceIdHash = ResourceId == null ? 0 : ResourceId.GetHashCode();
            int resourceQuantityHash = ResourceQuantity == null ? 0 : ResourceQuantity.GetHashCode();
            int reservationScheduleHash = ReservationSchedule == null ? 0 : ReservationSchedule.GetHashCode();
            return reservationIdHash^locationIdHash^resourceIdHash^resourceQuantityHash^reservationScheduleHash;
        }
    }
}
