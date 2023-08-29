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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Ministry
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationApprovalGroup" )]
    [DataContract]
    public class ReservationApprovalGroup : Rock.Data.Model<ReservationApprovalGroup>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>The reservation type identifier.</value>
        [Required]
        [DataMember]
        public int ReservationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the approval group identifier.
        /// </summary>
        /// <value>The approval group identifier.</value>
        [Required]
        [DataMember]
        public int ApprovalGroupId { get; set; }

        /// <summary>
        /// Gets or sets the type of the approval group.
        /// </summary>
        /// <value>The type of the approval group.</value>
        [DataMember]
        public ApprovalGroupType ApprovalGroupType { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        [DataMember]
        public int? CampusId { get; set; }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the type of the friendly group.
        /// </summary>
        /// <value>The type of the friendly group.</value>
        public virtual string FriendlyGroupType
        {
            get
            {
                return ApprovalGroupType != null ? ApprovalGroupType.ToString().SplitCase() : "";
            }
        }

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>The type of the reservation.</value>
        [DataMember]
        public virtual ReservationType ReservationType { get; set; }

        /// <summary>
        /// Gets or sets the approval group.
        /// </summary>
        /// <value>The approval group.</value>
        [DataMember]
        public virtual Group ApprovalGroup { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>The campus.</value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets the name of the campus.
        /// </summary>
        /// <value>The name of the campus.</value>
        public virtual string CampusName
        {
            get
            {
                return Campus != null ? Campus.Name : "All";
            }
        }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration class for the ReservationApprovalGroup
    /// </summary>
    public partial class ReservationApprovalGroupConfiguration : EntityTypeConfiguration<ReservationApprovalGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationApprovalGroupConfiguration" /> class.
        /// </summary>
        public ReservationApprovalGroupConfiguration()
        {
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.ReservationApprovalGroups ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.ApprovalGroup ).WithMany().HasForeignKey( p => p.ApprovalGroupId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationApprovalGroup" );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of workflow trigger
    /// </summary>
    public enum ApprovalGroupType
    {
        /// <summary>
        /// The reservation created
        /// </summary>
        InitialApprovalGroup = 0,

        /// <summary>
        /// The reservation updated
        /// </summary>
        FinalApprovalGroup = 1,

        /// <summary>
        /// The state changed
        /// </summary>
        OverrideApprovalGroup = 2
    }

    #endregion

}
