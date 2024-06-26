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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Resource
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationResource" )]
    [DataContract]
    public class ReservationResource : Rock.Data.Model<ReservationResource>, Rock.Data.IRockEntity
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        [Required]
        [DataMember]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        [Required]
        [DataMember]
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the reservation location identifier.
        /// </summary>
        /// <value>The reservation location identifier.</value>
        [DataMember]
        public int? ReservationLocationId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>The quantity.</value>
        [DataMember]
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>The state of the approval.</value>
        [Required]
        [DataMember]
        public ReservationResourceApprovalState ApprovalState { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>The reservation.</value>
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>The resource.</value>
        [LavaVisibleAttribute]
        public virtual Resource Resource { get; set; }

        /// <summary>
        /// Gets or sets the reservation location.
        /// </summary>
        /// <value>The reservation location.</value>
        [LavaVisibleAttribute]
        public virtual ReservationLocation ReservationLocation { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( ReservationResource source )
        {
            this.Id = source.Id;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ReservationId = source.ReservationId;
            this.ResourceId = source.ResourceId;
            this.Quantity = source.Quantity;
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
                        if ( Resource.ApprovalGroupId.HasValue )
                        {
                            hasApprovalRightsToState = ReservationTypeService.IsPersonInGroupWithId( person, Resource.ApprovalGroupId );
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
            try
            {

                var reservationResource = entry.Entity as ReservationResource;
                var reservation = new ReservationService( dbContext as RockContext ).Get( reservationResource.ReservationId );
                if ( reservation != null )
                {
                    reservation.ModifiedDateTime = RockDateTime.Now;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EF configuration class for the ReservationResource model
    /// </summary>
    public partial class ReservationResourceConfiguration : EntityTypeConfiguration<ReservationResource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationResourceConfiguration" /> class.
        /// </summary>
        public ReservationResourceConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationResources ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Resource ).WithMany().HasForeignKey( r => r.ResourceId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.ReservationLocation ).WithMany( r => r.ReservationResources ).HasForeignKey( p => p.ReservationLocationId ).WillCascadeOnDelete( true );


            // IMPORTANT!!
            this.HasEntitySetName( "ReservationResource" );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// An enum that represents when a Job notification status should be sent.
    /// </summary>
    public enum ReservationResourceApprovalState
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
