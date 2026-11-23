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
        /// The reservation records that failed to update.
        /// </summary>
        private int _reservationRecordsFailed = 0;

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
                    Reservation Records Updated: {_reservationRecordsUpdated},
                    Reservation Records Failed: {_reservationRecordsFailed}
                    " );
        }

        /// <summary>
        /// Updates missing occurrence timestamps for reservations. One failing reservation doesn't stop the rest.
        /// Failures are logged as a single exception per run, since the same reservations keep failing every
        /// night until their schedules are fixed and one entry per reservation would flood the exception log.
        /// </summary>
        private void PopulateOccurrenceData( )
        {
            List<int> reservationIdList = new List<int>();
            var failedReservationIds = new List<int>();
            Exception firstFailure = null;
            _reservationRecordsUpdated = 0;
            _reservationRecordsRead = 0;
            _reservationRecordsFailed = 0;
            using ( var rockContext = new RockContext() )
            {
                reservationIdList = new ReservationService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( r => r.FirstOccurrenceStartDateTime == null || r.LastOccurrenceEndDateTime == null )
                    .Select( r => r.Id )
                    .ToList();
            }
            _reservationRecordsLoaded = reservationIdList.Count();

            foreach ( var reservationId in reservationIdList )
            {
                try
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = _commandTimeout;
                        var reservationService = new ReservationService( rockContext );
                        var reservation = reservationService.Get( reservationId );

                        if ( reservation != null )
                        {
                            _reservationRecordsRead++;

                            reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );
                            rockContext.SaveChanges();
                            _reservationRecordsUpdated++;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    _reservationRecordsFailed++;
                    failedReservationIds.Add( reservationId );
                    firstFailure = firstFailure ?? ex;
                }
            }

            // Log one summary for the run, with the first error attached for its stack trace
            if ( failedReservationIds.Any() )
            {
                const int maxListedIds = 25;
                var listedIds = string.Join( ", ", failedReservationIds.Take( maxListedIds ) );
                var moreIds = failedReservationIds.Count > maxListedIds ? string.Format( " and {0} more", failedReservationIds.Count - maxListedIds ) : string.Empty;

                ExceptionLogService.LogException(
                    new Exception( string.Format( "Failed to populate occurrence timestamps for {0} reservation(s): {1}{2}.", failedReservationIds.Count, listedIds, moreIds ), firstFailure ),
                    System.Web.HttpContext.Current );
            }
        }
    }
}
