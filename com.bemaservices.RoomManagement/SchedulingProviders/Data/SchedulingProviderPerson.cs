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
using System.Collections.Generic;

namespace com.bemaservices.RoomManagement.SchedulingProviders.Data
{
    /// <summary>
    /// Generic person data transfer object for scheduling providers.
    /// Represents an event organizer or attendee in the scheduling provider system.
    /// </summary>
    public class SchedulingProviderPerson
    {
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the external identifier.
        /// Provider-specific identifier for this person.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is the organizer.
        /// </summary>
        public bool IsOrganizer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is a resource (e.g., room).
        /// </summary>
        public bool IsResource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is optional.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets or sets the response status.
        /// Common values: "accepted", "declined", "tentative", "needsAction"
        /// </summary>
        public string ResponseStatus { get; set; }

        /// <summary>
        /// Gets or sets the provider-specific metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderPerson"/> class.
        /// </summary>
        public SchedulingProviderPerson()
        {
            Metadata = new Dictionary<string, object>();
        }
    }
}
