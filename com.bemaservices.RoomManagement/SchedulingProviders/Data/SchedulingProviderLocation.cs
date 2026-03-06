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
    /// Generic location/room data transfer object for scheduling providers.
    /// Represents a location or room resource in the scheduling provider system.
    /// </summary>
    public class SchedulingProviderLocation
    {
        /// <summary>
        /// Gets or sets the external identifier.
        /// The unique identifier from the scheduling provider (e.g., Google Calendar Resource ID).
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the location/room.
        /// Many providers use email addresses to identify room resources.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the location/room.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the location/room.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the capacity of the location/room.
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets the building name.
        /// </summary>
        public string Building { get; set; }

        /// <summary>
        /// Gets or sets the floor information.
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Gets or sets the type of location/room.
        /// Common values: "Conference Room", "Office", "Classroom", etc.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the features or amenities available.
        /// Examples: "Projector", "Whiteboard", "Video Conference", etc.
        /// </summary>
        public List<string> Features { get; set; }

        /// <summary>
        /// Gets or sets the provider-specific metadata.
        /// This dictionary can store additional provider-specific properties.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderLocation"/> class.
        /// </summary>
        public SchedulingProviderLocation()
        {
            Features = new List<string>();
            Metadata = new Dictionary<string, object>();
        }
    }
}
