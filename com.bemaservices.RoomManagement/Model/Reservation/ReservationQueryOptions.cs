﻿// <copyright>
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
using Rock;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// The view model for a Reservation Date
    /// </summary>
    public class ReservationQueryOptions
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; } = null;
        /// <summary>
        /// Gets or sets the approvals by person identifier.
        /// </summary>
        /// <value>The approvals by person identifier.</value>
        public int? ApprovalsByPersonId { get; set; } = null;
        /// <summary>
        /// Gets or sets the reservations by person identifier.
        /// </summary>
        /// <value>The reservations by person identifier.</value>
        public int? ReservationsByPersonId { get; set; } = null;

        /// <summary>
        /// Gets or sets the creator person identifier.
        /// </summary>
        /// <value>The creator person identifier.</value>
        public int? CreatorPersonId { get; set; } = null;

        /// <summary>
        /// Gets or sets the event contact person identifier.
        /// </summary>
        /// <value>The event contact person identifier.</value>
        public int? EventContactPersonId { get; set; } = null;

        /// <summary>
        /// Gets or sets the administrative contact person identifier.
        /// </summary>
        /// <value>The administrative contact person identifier.</value>
        public int? AdministrativeContactPersonId { get; set; } = null;

        /// <summary>
        /// Gets or sets the reservation type ids.
        /// </summary>
        /// <value>The reservation type ids.</value>
        public List<int> ReservationTypeIds { get; set; } = new List<int>();
        /// <summary>
        /// Gets or sets the reservation ids.
        /// </summary>
        /// <value>The reservation ids.</value>
        public List<int> ReservationIds { get; set; } = new List<int>();
        /// <summary>
        /// Gets or sets the location ids.
        /// </summary>
        /// <value>The location ids.</value>
        public List<int> LocationIds { get; set; } = new List<int>();
        /// <summary>
        /// Gets or sets the resource ids.
        /// </summary>
        /// <value>The resource ids.</value>
        public List<int> ResourceIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the campus ids.
        /// </summary>
        /// <value>The campus ids.</value>
        public List<int> CampusIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the ministry ids.
        /// </summary>
        /// <value>The ministry ids.</value>
        public List<int> MinistryIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the ministry names.
        /// </summary>
        /// <value>The ministry names.</value>
        public List<string> MinistryNames { get; set; } = new List<string>();
        /// <summary>
        /// Gets or sets the approval states.
        /// </summary>
        /// <value>The approval states.</value>
        public List<ReservationApprovalState> ApprovalStates { get; set; } = new List<ReservationApprovalState>();

    }

    /// <summary>
    /// Class ReservationCalendarOptions.
    /// Implements the <see cref="com.bemaservices.RoomManagement.Model.ReservationQueryOptions" />
    /// </summary>
    /// <seealso cref="com.bemaservices.RoomManagement.Model.ReservationQueryOptions" />
    public class ReservationCalendarOptions : ReservationQueryOptions
    {
        /// <summary>
        /// The start date
        /// </summary>
        private DateTime? _startDate;
        /// <summary>
        /// The end date
        /// </summary>
        private DateTime? _endDate;

        /// <summary>
        /// Gets or sets the start date. if not explicitly set returns 3 months prior to the current date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate
        {
            get
            {
                return _startDate ?? RockDateTime.Now.AddMonths( -3 ).Date;
            }

            set
            {
                _startDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the end date. If not explicitly set returns 12 months from current date.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate
        {
            get
            {
                return _endDate ?? RockDateTime.Now.AddMonths( 12 ).Date;
            }

            set
            {
                _endDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the client device.
        /// </summary>
        /// <value>The type of the client device.</value>
        public string ClientDeviceType { get; set; } = null;
    }
}
