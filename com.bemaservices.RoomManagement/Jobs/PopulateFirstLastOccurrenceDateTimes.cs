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
using System.Data.Entity;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Jobs
{
    /// <summary>
    /// Class PopulateFirstLastOccurrenceDateTimes.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class PopulateFirstLastOccurrenceDateTimes : RockJob
    {
        /// <summary>
        /// The command timeout
        /// </summary>
        private int _commandTimeout = 0;

        /// <summary>
        /// The remaining reservation records
        /// </summary>
        private int _remainingReservationRecords = 0;
        /// <summary>
        /// The reservation records loaded
        /// </summary>
        private int _reservationRecordsLoaded = 0;
        /// <summary>
        /// The reservation records read
        /// </summary>
        private int _reservationRecordsRead = 0;
        /// <summary>
        /// The reservation records updated
        /// </summary>
        private int _reservationRecordsUpdated = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>

        public override void Execute()
        {
            _commandTimeout = GetAttributeValue( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                _remainingReservationRecords = rockContext.Database.SqlQuery<int>( $@"
                    SELECT COUNT(*) FROM [_com_bemaservices_RoomManagement_Reservation] WHERE [FirstOccurrenceStartDateTime] IS NULL OR [LastOccurrenceEndDateTime] IS NULL
                " ).First();

                if ( _remainingReservationRecords == 0 )
                {
                    // delete job if there are no unlined attendance records
                    var jobId = this.ServiceJobId;
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }

            PopulateOccurrenceData();

            this.UpdateLastStatusMessage( $@"Remaining Reservation Records: {_remainingReservationRecords},
                    Reservation Records Loaded: {_reservationRecordsLoaded}, 
                    Reservation Records Read: {_reservationRecordsRead}, 
                    Reservation Records Updated: { _reservationRecordsUpdated}
                    " );
        }

        /// <summary>
        /// Migrates the page views data.
        /// </summary>
        private void PopulateOccurrenceData( )
        {
            List<int> reservationIdList = new List<int>();
            _reservationRecordsUpdated = 0;
            _reservationRecordsRead = 0;
            using ( var rockContext = new RockContext() )
            {
                reservationIdList = new ReservationService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( r => r.FirstOccurrenceStartDateTime == null || r.LastOccurrenceEndDateTime == null )
                    .Select( r => r.Id )
                    .ToList();
            }
            _reservationRecordsLoaded = reservationIdList.Count();

            try
            {
                foreach ( var reservationId in reservationIdList )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = _commandTimeout;
                        var reservationService = new ReservationService( rockContext );
                        var reservation = reservationService.Get( reservationId );

                        if ( reservation != null )
                        {
                            _reservationRecordsRead++;

                            try
                            {
                                reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );
                                rockContext.SaveChanges();
                                _reservationRecordsUpdated++;
                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
