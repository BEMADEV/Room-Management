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
using System.Text.RegularExpressions;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    /// <summary>
    /// Base component for scheduling providers (Google Calendar, Microsoft Outlook, etc.).
    /// Provides methods for importing and exporting reservation data between Rock and external scheduling systems.
    /// Derived classes should implement provider-specific logic for creating, reading, updating, and deleting events.
    /// </summary>
    public abstract class SchedulingProviderComponent : Component
    {

        /// <summary>
        /// Gets provider events for a specific location from the scheduling provider.
        /// Implementations should convert provider-specific event formats to SchedulingProviderEvent objects.
        /// This method is typically used during import operations to retrieve events within a date range.
        /// </summary>
        /// <param name="schedulingProvider">The scheduling provider containing connection details and settings.</param>
        /// <param name="externalId">The external identifier for the location in the provider's system.</param>
        /// <param name="startDate">The start date to filter events. If null, no lower bound is applied.</param>
        /// <param name="endDate">The end date to filter events. If null, no upper bound is applied.</param>
        /// <param name="errorMessages">The collection of error messages encountered during the operation.</param>
        /// <returns>List of provider events for the specified location and date range.</returns>
        public virtual List<SchedulingProviderEvent> GetProviderEventsForLocation(
            SchedulingProvider schedulingProvider,
            string externalId,
            DateTime? startDate,
            DateTime? endDate,
            out List<string> errorMessages )
        {
            errorMessages = new List<string> { "GetProviderEventsForLocation is not implemented for this provider." };
            return new List<SchedulingProviderEvent>();
        }

        /// <summary>
        /// Gets a single provider event by its external identifier.
        /// Implementations should convert provider-specific event format to SchedulingProviderEvent object.
        /// This method is typically used to check if an event has been modified before attempting an update.
        /// </summary>
        /// <param name="schedulingProvider">The scheduling provider containing connection details and settings.</param>
        /// <param name="externalEventId">The external event identifier from the provider's system.</param>
        /// <param name="errorMessages">The collection of error messages encountered during the operation.</param>
        /// <returns>The provider event if found; otherwise, null.</returns>
        public virtual SchedulingProviderEvent GetProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string> { "GetProviderEvent is not implemented for this provider." };
            return null;
        }

        /// <summary>
        /// Creates a new event in the scheduling provider.
        /// Implementations should convert SchedulingProviderEvent object to provider-specific format and
        /// populate the ExternalId property of the returned event with the provider's identifier.
        /// </summary>
        /// <param name="schedulingProvider">The scheduling provider containing connection details and settings.</param>
        /// <param name="providerEvent">The provider event to create containing event details and locations.</param>
        /// <param name="errorMessages">The collection of error messages encountered during the operation.</param>
        /// <returns>The created provider event with ExternalId populated if successful; otherwise, null.</returns>
        public virtual SchedulingProviderEvent CreateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string> { "CreateProviderEvent is not implemented for this provider." };
            return null;
        }

        /// <summary>
        /// Updates an existing event in the scheduling provider.
        /// Implementations should convert SchedulingProviderEvent object to provider-specific format and
        /// use the ExternalId property to identify which event to update in the provider's system.
        /// </summary>
        /// <param name="schedulingProvider">The scheduling provider containing connection details and settings.</param>
        /// <param name="providerEvent">The provider event to update. The ExternalId must be populated.</param>
        /// <param name="errorMessages">The collection of error messages encountered during the operation.</param>
        /// <returns><c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
        public virtual bool UpdateProviderEvent(
            SchedulingProvider schedulingProvider,
            SchedulingProviderEvent providerEvent,
            out List<string> errorMessages )
        {
            errorMessages = new List<string> { "UpdateProviderEvent is not implemented for this provider." };
            return false;
        }

        /// <summary>
        /// Deletes an event from the scheduling provider.
        /// Implementations should use the external event identifier to remove the event from the provider's system.
        /// </summary>
        /// <param name="schedulingProvider">The scheduling provider containing connection details and settings.</param>
        /// <param name="externalEventId">The external event identifier from the provider's system.</param>
        /// <param name="errorMessages">The collection of error messages encountered during the operation.</param>
        /// <returns><c>true</c> if the deletion was successful; otherwise, <c>false</c>.</returns>
        public virtual bool DeleteProviderEvent(
            SchedulingProvider schedulingProvider,
            string externalEventId,
            out List<string> errorMessages )
        {
            errorMessages = new List<string> { "DeleteProviderEvent is not implemented for this provider." };
            return false;
        }
    }
}
