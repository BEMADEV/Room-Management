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

namespace com.bemaservices.RoomManagement.SchedulingProviders.Data
{
    /// <summary>
    /// Generic event data transfer object for scheduling providers.
    /// This class serves as an intermediary format between provider-specific event formats
    /// (Google Calendar, Microsoft Outlook, etc.) and Rock reservations.
    /// </summary>
    public class SchedulingProviderEvent
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
        /// Gets or sets the start date time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        public string ICalendarContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event is an all-day event.
        /// </summary>
        public bool IsAllDay { get; set; }

        /// <summary>
        /// Gets or sets the locations/rooms associated with this event.
        /// </summary>
        public List<SchedulingProviderLocation> Locations { get; set; }

        /// <summary>
        /// Gets or sets the organizer information.
        /// </summary>
        public SchedulingProviderPerson Organizer { get; set; }

        /// <summary>
        /// Gets or sets the attendees.
        /// </summary>
        public List<SchedulingProviderPerson> Attendees { get; set; }

        /// <summary>
        /// Gets or sets the recurrence rule (iCalendar RRULE format if applicable).
        /// </summary>
        public string RecurrenceRule { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// Common values: "confirmed", "tentative", "cancelled"
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the visibility.
        /// Common values: "public", "private", "confidential"
        /// </summary>
        public string Visibility { get; set; }

        /// <summary>
        /// Gets or sets the provider-specific metadata.
        /// This dictionary can store additional provider-specific properties that don't map to standard fields.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the created date time from the provider.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified date time from the provider.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderEvent"/> class.
        /// </summary>
        public SchedulingProviderEvent()
        {
            Locations = new List<SchedulingProviderLocation>();
            Attendees = new List<SchedulingProviderPerson>();
            Metadata = new Dictionary<string, object>();
        }
    }
}
