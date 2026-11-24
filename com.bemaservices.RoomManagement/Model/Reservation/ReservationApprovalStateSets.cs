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
using System.Collections.Generic;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Named groups of reservation approval states, so lists that mean the same thing are defined once.
    /// </summary>
    public static class ReservationApprovalStateSets
    {
        /// <summary>
        /// Gets the states of reservations that are still in play and hold their locations and resources: every state except
        /// Draft, Denied and Cancelled, the same states <see cref="ReservationExtensionMethods.ValidExistingReservations" />
        /// treats as existing reservations. A new list is returned each time, so callers may change it.
        /// </summary>
        /// <returns>The active approval states.</returns>
        public static List<ReservationApprovalState> GetActiveStates()
        {
            return new List<ReservationApprovalState>
            {
                ReservationApprovalState.Approved,
                ReservationApprovalState.PendingInitialApproval,
                ReservationApprovalState.PendingSpecialApproval,
                ReservationApprovalState.PendingFinalApproval,
                ReservationApprovalState.ChangesNeeded
            };
        }
    }
}
