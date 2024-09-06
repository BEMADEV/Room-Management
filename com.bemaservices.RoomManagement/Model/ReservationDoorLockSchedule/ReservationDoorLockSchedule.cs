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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Model;
using Rock.Data;
using System;
using Rock;
using Rock.Lava;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Location
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationDoorLockSchedule" )]
    [DataContract]
    public class ReservationDoorLockSchedule : Rock.Data.Model<ReservationDoorLockSchedule>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        [DataMember]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the start time offset.
        /// </summary>
        /// <value>The start time offset.</value>
        [DataMember]
        public int StartTimeOffset { get; set; }

        /// <summary>
        /// Gets or sets the end time offset.
        /// </summary>
        /// <value>The end time offset.</value>
        [DataMember]
        public int EndTimeOffset { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [DataMember]
        public string Note { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>The reservation.</value>
        public virtual Reservation Reservation { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( ReservationDoorLockSchedule source )
        {
            this.Id = source.Id;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ReservationId = source.ReservationId;
            this.StartTimeOffset = source.StartTimeOffset;
            this.EndTimeOffset = source.EndTimeOffset;
            this.Note = source.Note;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            if ( entry.State == System.Data.Entity.EntityState.Added || entry.State == System.Data.Entity.EntityState.Modified || entry.State == System.Data.Entity.EntityState.Deleted )
            {
                try
                {

                    var reservationLocation = entry.Entity as ReservationDoorLockSchedule;
                    var reservation = new ReservationService( dbContext as RockContext ).Get( reservationLocation.ReservationId );
                    if ( reservation != null )
                    {
                        reservation.ModifiedDateTime = RockDateTime.Now;
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration class for the ReservationDoorLockSchedule.
    /// </summary>
    public partial class ReservationDoorLockScheduleConfiguration : EntityTypeConfiguration<ReservationDoorLockSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationDoorLockScheduleConfiguration" /> class.
        /// </summary>
        public ReservationDoorLockScheduleConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationDoorLockSchedules ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationDoorLockSchedule" );
        }
    }

    #endregion
}
