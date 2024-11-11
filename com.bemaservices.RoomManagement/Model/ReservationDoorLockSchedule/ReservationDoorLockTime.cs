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
    public class ReservationDoorLockTime : RockDynamic
    {

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        public string Note { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationDoorLockTime"/> class.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="note">The note.</param>
        public ReservationDoorLockTime( DateTime startDateTime, DateTime endDateTime, string note )
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Note = note;
        }
    }
}
