﻿// <copyright>
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
    [Table( "_com_bemaservices_RoomManagement_ReservationLocation" )]
    [DataContract]
    public class ReservationLocation : Rock.Data.Model<ReservationLocation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        [DataMember]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        [DataMember]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location layout identifier.
        /// </summary>
        /// <value>The location layout identifier.</value>
        [DataMember]
        public int? LocationLayoutId { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>The state of the approval.</value>
        [DataMember]
        public ReservationLocationApprovalState ApprovalState { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>The reservation.</value>
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [LavaVisibleAttribute]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the location layout.
        /// </summary>
        /// <value>The location layout.</value>
        [LavaVisibleAttribute]
        public virtual LocationLayout LocationLayout { get; set; }


        /// <summary>
        /// Gets or sets the reservation resources.
        /// </summary>
        /// <value>The reservation resources.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<ReservationResource> ReservationResources
        {
            get { return _reservationResources ?? ( _reservationResources = new Collection<ReservationResource>() ); }
            set { _reservationResources = value; }
        }

        /// <summary>
        /// The reservation resources
        /// </summary>
        private ICollection<ReservationResource> _reservationResources;

        #endregion

        #region Methods

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( ReservationLocation source )
        {
            this.Id = source.Id;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ReservationId = source.ReservationId;
            this.LocationId = source.LocationId;
            this.LocationLayoutId = source.LocationLayoutId;
            this.ApprovalState = source.ApprovalState;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;
        }

        /// <summary>
        /// Determines whether [has approval rights to state] [the specified person].
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if [has approval rights to state] [the specified person]; otherwise, <c>false</c>.</returns>
        public bool HasApprovalRightsToState( Person person )
        {
            bool hasApprovalRightsToState = false;
            if ( Reservation.ReservationType.HasApprovalRights( person, ApprovalGroupType.OverrideApprovalGroup, Reservation.CampusId ) )
            {
                hasApprovalRightsToState = true;
            }
            else
            {
                switch ( Reservation.ApprovalState )
                {
                    case ReservationApprovalState.PendingSpecialApproval:
                        if ( Location != null )
                        {
                            Location.LoadAttributes();
                            var approvalGroupGuid = Location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();

                            if ( approvalGroupGuid.HasValue )
                            {
                                hasApprovalRightsToState = ReservationTypeService.IsPersonInGroupWithGuid( person, approvalGroupGuid );
                            }
                        }
                        break;
                }
            }

            return hasApprovalRightsToState;
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

                    var reservationLocation = entry.Entity as ReservationLocation;
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
    /// The EF configuration class for the ReservationLocation.
    /// </summary>
    public partial class ReservationLocationConfiguration : EntityTypeConfiguration<ReservationLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLocationConfiguration" /> class.
        /// </summary>
        public ReservationLocationConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationLocations ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.LocationLayout ).WithMany().HasForeignKey( r => r.LocationLayoutId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationLocation" );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// An enum that represents when a Job notification status should be sent.
    /// </summary>
    public enum ReservationLocationApprovalState
    {
        /// <summary>
        /// Notifications should be sent when a job completes with any notification status.
        /// </summary>
        Unapproved = 1,

        /// <summary>
        /// Notification should be sent when the job has completed successfully.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Notification should be sent when the job has completed with an error status.
        /// </summary>
        Denied = 3
    }

    #endregion
}
