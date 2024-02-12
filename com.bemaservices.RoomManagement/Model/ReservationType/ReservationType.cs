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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Room Reservation Type
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationType" )]
    [DataContract]
    public class ReservationType : Rock.Data.Model<ReservationType>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>The icon CSS class.</value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the default setup time.
        /// </summary>
        /// <value>The default setup time.</value>
        [DataMember]
        public int? DefaultSetupTime { get; set; }

        /// <summary>
        /// Gets or sets the default cleanup time.
        /// </summary>
        /// <value>The default cleanup time.</value>
        [DataMember]
        public int? DefaultCleanupTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is number attending required.
        /// </summary>
        /// <value><c>true</c> if this instance is number attending required; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsNumberAttendingRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is contact details required.
        /// </summary>
        /// <value><c>true</c> if this instance is contact details required; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsContactDetailsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is campus required.
        /// </summary>
        /// <value><c>true</c> if this instance is campus required; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsCampusRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is setup time required.
        /// </summary>
        /// <value><c>true</c> if this instance is setup time required; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSetupTimeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reservation booked on approval.
        /// </summary>
        /// <value><c>true</c> if this instance is reservation booked on approval; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsReservationBookedOnApproval { get; set; }

        /// <summary>
        /// Gets or sets the contact phone type value identifier.
        /// </summary>
        /// <value>The contact phone type value identifier.</value>
        [DataMember]
        public int? ContactPhoneTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration of the reservation.
        /// </summary>
        /// <value>The maximum duration of the reservation.</value>
        [DataMember]
        public int? MaximumReservationDuration { get; set; }

        /// <summary>
        /// Gets or sets the default duration of the reservation.
        /// </summary>
        /// <value>The default duration of the reservation.</value>
        [DataMember]
        public int DefaultReservationDuration { get; set; }

        /// <summary>
        /// Gets or sets the location requirement.
        /// </summary>
        /// <value>The location requirement.</value>
        [DataMember]
        public ReservationTypeRequirement? LocationRequirement { get; set; }

        /// <summary>
        /// Gets or sets the resource requirement.
        /// </summary>
        /// <value>The resource requirement.</value>
        [DataMember]
        public ReservationTypeRequirement? ResourceRequirement { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the contact phone type value.
        /// </summary>
        /// <value>The contact phone type value.</value>
        public DefinedValue ContactPhoneTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the final approval group identifier.
        /// </summary>
        /// <value>The final approval group identifier.</value>
        [DataMember]
        public int? FinalApprovalGroupId
        {
            get
            {
                if ( FinalApprovalGroup != null )
                {
                    return FinalApprovalGroup.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the initial approval group identifier.
        /// </summary>
        /// <value>The initial approval group identifier.</value>
        [DataMember]
        public int? InitialApprovalGroupId
        {
            get
            {
                if ( InitialApprovalGroup != null )
                {
                    return InitialApprovalGroup.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the super admin group identifier.
        /// </summary>
        /// <value>The super admin group identifier.</value>
        [DataMember]
        public int? OverrideApprovalGroupId
        {
            get
            {
                if ( OverrideApprovalGroup != null )
                {
                    return OverrideApprovalGroup.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the final approval group.
        /// </summary>
        /// <value>The final approval group.</value>
        [LavaVisibleAttribute]
        public virtual Group FinalApprovalGroup
        {
            get
            {
                return GetApprovalGroup( ApprovalGroupType.FinalApprovalGroup, null );
            }
        }

        /// <summary>
        /// Gets or sets the initial approval group.
        /// </summary>
        /// <value>The initial approval group.</value>
        [LavaVisibleAttribute]
        public virtual Group InitialApprovalGroup
        {
            get
            {
                return GetApprovalGroup( ApprovalGroupType.InitialApprovalGroup, null );
            }
        }

        /// <summary>
        /// Gets or sets the super admin group.
        /// </summary>
        /// <value>The super admin group.</value>
        [LavaVisibleAttribute]
        public virtual Group OverrideApprovalGroup
        {
            get
            {
                return GetApprovalGroup( ApprovalGroupType.OverrideApprovalGroup, null );
            }
        }

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>The reservations.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<Reservation> Reservations
        {
            get { return _reservations ?? ( _reservations = new Collection<Reservation>() ); }
            set { _reservations = value; }
        }

        /// <summary>
        /// The reservations
        /// </summary>
        private ICollection<Reservation> _reservations;

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>The reservations.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<ReservationApprovalGroup> ReservationApprovalGroups
        {
            get { return _reservationApprovalGroups ?? ( _reservationApprovalGroups = new Collection<ReservationApprovalGroup>() ); }
            set { _reservationApprovalGroups = value; }
        }

        /// <summary>
        /// The reservation approval groups
        /// </summary>
        private ICollection<ReservationApprovalGroup> _reservationApprovalGroups;

        /// <summary>
        /// Gets or sets the reservation ministries.
        /// </summary>
        /// <value>The reservation ministries.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<ReservationMinistry> ReservationMinistries
        {
            get { return _reservationMinistries ?? ( _reservationMinistries = new Collection<ReservationMinistry>() ); }
            set { _reservationMinistries = value; }
        }

        /// <summary>
        /// The reservation ministries
        /// </summary>
        private ICollection<ReservationMinistry> _reservationMinistries;

        /// <summary>
        /// Gets or sets the reservation workflow triggers.
        /// </summary>
        /// <value>The reservation workflow triggers.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<ReservationWorkflowTrigger> ReservationWorkflowTriggers
        {
            get { return _reservationWorkflowTriggers ?? ( _reservationWorkflowTriggers = new Collection<ReservationWorkflowTrigger>() ); }
            set { _reservationWorkflowTriggers = value; }
        }

        /// <summary>
        /// The reservation workflow triggers
        /// </summary>
        private ICollection<ReservationWorkflowTrigger> _reservationWorkflowTriggers;


        /// <summary>
        /// Gets or sets the reservation location types.
        /// </summary>
        /// <value>The reservation location types.</value>
        [LavaVisibleAttribute]
        public virtual ICollection<ReservationLocationType> ReservationLocationTypes
        {
            get { return _reservationLocationTypes ?? ( _reservationLocationTypes = new Collection<ReservationLocationType>() ); }
            set { _reservationLocationTypes = value; }
        }

        /// <summary>
        /// The reservation location types
        /// </summary>
        private ICollection<ReservationLocationType> _reservationLocationTypes;

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>The supported actions.</value>
        [NotMapped]
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.DELETE, "Additional roles and/or users that have access to delete reservations outside of the traditional approval process." );
                supportedActions.AddOrReplace( "EditAfterApproval", "Roles and/or users that have access to edit reservations after they have been approved." );
                return supportedActions;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the approval groups.
        /// </summary>
        /// <param name="approvalGroupType">Type of the approval group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>List&lt;ReservationApprovalGroup&gt;.</returns>
        public List<ReservationApprovalGroup> GetApprovalGroups( ApprovalGroupType approvalGroupType, int? campusId )
        {
            var approvalGroups = ReservationApprovalGroups.Where( g => g.ApprovalGroupType == approvalGroupType );

            if ( campusId.HasValue )
            {
                approvalGroups = approvalGroups.Where( g => g.CampusId == null || g.CampusId == campusId.Value );
            }
            else
            {
                approvalGroups = approvalGroups.Where( g => g.CampusId == null );
            }

            return approvalGroups.ToList();
        }

        /// <summary>
        /// Gets the approval group.
        /// </summary>
        /// <param name="approvalGroupType">Type of the approval group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>Group.</returns>
        public Group GetApprovalGroup( ApprovalGroupType approvalGroupType, int? campusId )
        {
            Group group = null;
            var approvalGroups = GetApprovalGroups( approvalGroupType, campusId );

            var approvalGroup = approvalGroups.OrderByDescending( g => g.CampusId.HasValue ).ThenBy( g => g.CampusId ).FirstOrDefault();
            if ( approvalGroup != null )
            {
                group = approvalGroup.ApprovalGroup;
            }

            return group;
        }

        /// <summary>
        /// Gets the approval group identifier.
        /// </summary>
        /// <param name="approvalGroupType">Type of the approval group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        public int? GetApprovalGroupId( ApprovalGroupType approvalGroupType, int? campusId )
        {
            int? approvalGroupId = null;
            var approvalGroup = GetApprovalGroup( approvalGroupType, campusId );
            if ( approvalGroup != null )
            {
                approvalGroupId = approvalGroup.Id;
            }

            return approvalGroupId;
        }

        /// <summary>
        /// Determines whether [has approval rights] [the specified person].
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="approvalGroupType">Type of the approval group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns><c>true</c> if [has approval rights] [the specified person]; otherwise, <c>false</c>.</returns>
        public bool HasApprovalRights( Person person, ApprovalGroupType approvalGroupType, int? campusId )
        {
            var approvalGroups = GetApprovalGroups( approvalGroupType, campusId );

            return approvalGroups.SelectMany( ag => ag.ApprovalGroup.Members ).Where( gm => gm.PersonId == person.Id && gm.GroupMemberStatus == GroupMemberStatus.Active ).Any();
        }


        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// EF configuration class for the ReservationType model
    /// </summary>
    public partial class ReservationTypeConfiguration : EntityTypeConfiguration<ReservationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationTypeConfiguration" /> class.
        /// </summary>
        public ReservationTypeConfiguration()
        {
            this.HasOptional( p => p.ContactPhoneTypeValue ).WithMany().HasForeignKey( p => p.ContactPhoneTypeValueId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationType" );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Enum ReservationTypeRequirement
    /// </summary>
    public enum ReservationTypeRequirement
    {
        /// <summary>
        /// The hide
        /// </summary>
        Hide = 0,
        /// <summary>
        /// The allow
        /// </summary>
        Allow = 1,
        /// <summary>
        /// The require
        /// </summary>
        Require = 2       
    }

    #endregion

}
