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
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Holds the view model for a Reservation Summary
    /// </summary>
    public class ReservationSummary : RockDynamic
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        public int ReservationId { get { return Id; } }

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>The type of the reservation.</value>
        public ReservationType ReservationType { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>The state of the approval.</value>
        public ReservationApprovalState ApprovalState { get; set; }

        /// <summary>
        /// Gets or sets the name of the reservation.
        /// </summary>
        /// <value>The name of the reservation.</value>
        public String ReservationName { get; set; }

        /// <summary>
        /// Gets or sets the event date time description.
        /// </summary>
        /// <value>The event date time description.</value>
        public String EventDateTimeDescription { get; set; }

        /// <summary>
        /// Gets or sets the event time description.
        /// </summary>
        /// <value>The event time description.</value>
        public String EventTimeDescription { get; set; }

        /// <summary>
        /// Gets or sets the reservation date time description.
        /// </summary>
        /// <value>The reservation date time description.</value>
        public String ReservationDateTimeDescription { get; set; }

        /// <summary>
        /// Gets or sets the reservation time description.
        /// </summary>
        /// <value>The reservation time description.</value>
        public String ReservationTimeDescription { get; set; }

        /// <summary>
        /// Gets or sets the reservation locations.
        /// </summary>
        /// <value>The reservation locations.</value>
        public List<ReservationLocation> ReservationLocations { get; set; }

        /// <summary>
        /// Gets or sets the reservation resources.
        /// </summary>
        /// <value>The reservation resources.</value>
        public List<ReservationResource> ReservationResources { get; set; }

        public List<ReservationResource> UnassignedReservationResources { get; set; }

        /// <summary>
        /// Gets or sets the reservation start date time.
        /// </summary>
        /// <value>The reservation start date time.</value>
        public DateTime ReservationStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the reservation end date time.
        /// </summary>
        /// <value>The reservation end date time.</value>
        public DateTime ReservationEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the event start date time.
        /// </summary>
        /// <value>The event start date time.</value>
        public DateTime EventStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the event end date time.
        /// </summary>
        /// <value>The event end date time.</value>
        public DateTime EventEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the reservation ministry.
        /// </summary>
        /// <value>The reservation ministry.</value>
        public ReservationMinistry ReservationMinistry { get; set; }

        /// <summary>
        /// Gets or sets the event contact person alias.
        /// </summary>
        /// <value>The event contact person alias.</value>
        public PersonAlias EventContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the event contact phone number.
        /// </summary>
        /// <value>The event contact phone number.</value>
        public String EventContactPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the event contact email.
        /// </summary>
        /// <value>The event contact email.</value>
        public String EventContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact person alias.
        /// </summary>
        /// <value>The administrative contact person alias.</value>
        public PersonAlias AdministrativeContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact phone number.
        /// </summary>
        /// <value>The administrative contact phone number.</value>
        public String AdministrativeContactPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact email.
        /// </summary>
        /// <value>The administrative contact email.</value>
        public String AdministrativeContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the setup photo identifier.
        /// </summary>
        /// <value>The setup photo identifier.</value>
        public int? SetupPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the requester alias.
        /// </summary>
        /// <value>The requester alias.</value>
        public PersonAlias RequesterAlias { get; set; }

        /// <summary>
        /// Gets or sets the number attending.
        /// </summary>
        /// <value>The number attending.</value>
        public int? NumberAttending { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>The modified date time.</value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>The schedule identifier.</value>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public Dictionary<string, AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public Dictionary<string, AttributeValueCache> AttributeValues { get; set; }
    }
}
