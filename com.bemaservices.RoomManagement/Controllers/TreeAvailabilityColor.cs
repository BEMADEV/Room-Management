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
namespace Rock.Rest.Controllers
{
    /// <summary>
    /// The colors the scheduled location and resource pickers use to show availability for a reservation's schedule,
    /// kept in one place so both trees use the same legend.
    /// </summary>
    internal static class TreeAvailabilityColor
    {
        /// <summary>
        /// Free to reserve.
        /// </summary>
        public const string Available = "Green";

        /// <summary>
        /// Only requested by other reservations so far, or otherwise a poor fit (for example, too small for the attendees).
        /// </summary>
        public const string Conflicted = "Orange";

        /// <summary>
        /// Already reserved for the schedule.
        /// </summary>
        public const string Reserved = "Red";

        /// <summary>
        /// Not a location type this reservation type can use.
        /// </summary>
        public const string Unusable = "Black";

        /// <summary>
        /// Gets the availability color: reserved wins over conflicted, and anything else is available.
        /// </summary>
        /// <param name="isReserved">Whether the item is already reserved for the schedule.</param>
        /// <param name="isConflicted">Whether the item is only requested by other reservations, or otherwise a poor fit.</param>
        /// <returns>The color to show the item in.</returns>
        public static string Get( bool isReserved, bool isConflicted )
        {
            if ( isReserved )
            {
                return Reserved;
            }

            return isConflicted ? Conflicted : Available;
        }
    }
}
