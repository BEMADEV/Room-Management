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
using Ical.Net.CalendarComponents;

namespace com.bemaservices.RoomManagement.SchedulingProviders.Data
{
    /// <summary>
    /// Generic event data transfer object for scheduling providers.
    /// This class serves as an intermediary format between provider-specific event formats
    /// (Google Calendar, Microsoft Outlook, etc.) and Rock reservations.
    /// </summary>
    public class EventDTO
    {
        /// <summary>
        /// Gets or sets the external identifier.
        /// The unique identifier from the scheduling provider (e.g., Google Calendar Event ID).
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the title or summary of the event.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or notes for the event.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the calendar event with schedule information.
        /// </summary>
        public CalendarEvent CalendarEvent { get; set; }

        /// <summary>
        /// Gets or sets the locations/rooms associated with this event.
        /// </summary>
        public List<LocationDTO> Locations { get; set; }

        /// <summary>
        /// Gets or sets the organizer information.
        /// </summary>
        public PersonDTO Organizer { get; set; }

        /// <summary>
        /// Gets or sets the created date time from the provider.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified date time from the provider.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDTO"/> class.
        /// </summary>
        public EventDTO()
        {
            Locations = new List<LocationDTO>();
        }
    }
}
