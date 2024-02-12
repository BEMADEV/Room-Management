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

using com.bemaservices.RoomManagement.Model;
using Quartz;
using Rock;
using Rock.Data;
using Rock.Model;
using System.ComponentModel;
using System.Linq;

namespace com.bemaservices.RoomManagement.Jobs
{
    /// <summary>
    /// Run once job for v14 to update current sessions
    /// </summary>
    [DisallowConcurrentExecution]

    public class V247TemporarySetupPhotoFix : IJob
    {

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {

            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );

            var reservationList = reservationService.Queryable().Where( r => r.SetupPhoto != null && r.SetupPhoto.IsTemporary == true ).ToList();
            foreach ( var reservation in reservationList )
            {
                var newPhoto = reservation.SetupPhoto.CloneWithoutIdentity();
                newPhoto.DatabaseData = reservation.SetupPhoto.DatabaseData.CloneWithoutIdentity();
                newPhoto.IsTemporary = false;
                var binaryFileService = new BinaryFileService( rockContext );
                binaryFileService.Add( newPhoto );
                reservation.SetupPhoto = newPhoto;

                rockContext.SaveChanges();
            }

            DeleteJob( context.GetJobId() );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
