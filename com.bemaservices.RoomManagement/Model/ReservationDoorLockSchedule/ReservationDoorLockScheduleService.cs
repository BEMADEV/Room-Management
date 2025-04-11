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
using System.Text;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class DoorLockOptionService.
    /// Implements the <see cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.ReservationDoorLockSchedule}" />
    /// </summary>
    /// <seealso cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.ReservationDoorLockSchedule}" />
    public class ReservationDoorLockScheduleService : Service<ReservationDoorLockSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationDoorLockScheduleService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationDoorLockScheduleService( RockContext context ) : base( context ) { }

        public static string GetFriendlyDoorLockTime( DateTime? reservationStartDateTime, DateTime doorLockStartTime )
        {
            StringBuilder sb = new StringBuilder();
            if ( reservationStartDateTime != null && doorLockStartTime != null )
            {
                var dateOffsetText = string.Empty;
                var dateDifference = ( doorLockStartTime - reservationStartDateTime );
                sb.Append( doorLockStartTime.ToShortTimeString() );

                if ( dateDifference.HasValue )
                {
                    var dayDifference = ( Int64 ) dateDifference.Value.TotalDays;
                    if ( dayDifference != 0 )
                    {
                        sb.Append( " (" );

                        if ( dayDifference >= 1 )
                        {
                            sb.Append( "+" );
                        }

                        sb.AppendFormat( "{0} day", dayDifference.ToString() );

                        if ( dayDifference < -1 || dayDifference > 1 )
                        {
                            sb.Append( "s" );
                        }

                        sb.Append( ")" );
                    }
                }
            }
            return sb.ToString();
        }
    }
}
