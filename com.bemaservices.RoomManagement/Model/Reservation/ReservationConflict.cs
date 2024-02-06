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
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// The view model for a Reservation Conflict
    /// </summary>
    public class ReservationConflict
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
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public Resource Resource { get; set; }

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
        public Reservation Reservation { get; set; }
    }
}
