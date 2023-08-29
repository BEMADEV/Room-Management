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

using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Resource
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationLinkage" )]
    [DataContract]
    public class ReservationLinkage : Rock.Data.Model<ReservationLinkage>, Rock.Data.IRockEntity
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
        /// Gets or sets the event item occurrence identifier.
        /// </summary>
        /// <value>The event item occurrence identifier.</value>
        [DataMember]
        public int? EventItemOccurrenceId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>The reservation.</value>
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence.
        /// </summary>
        /// <value>The event item occurrence.</value>
        [LavaInclude]
        public virtual EventItemOccurrence EventItemOccurrence { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( ReservationLinkage source )
        {
            this.Id = source.Id;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ReservationId = source.ReservationId;
            this.EventItemOccurrenceId = source.EventItemOccurrenceId;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EF configuration class for the ReservationLinkage model
    /// </summary>
    public partial class ReservationLinkageConfiguration : EntityTypeConfiguration<ReservationLinkage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLinkageConfiguration" /> class.
        /// </summary>
        public ReservationLinkageConfiguration()
        {
            this.HasRequired( r => r.Reservation ).WithMany( r => r.ReservationLinkages ).HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.EventItemOccurrence ).WithMany().HasForeignKey( r => r.EventItemOccurrenceId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationLinkage" );
        }
    }

    #endregion

}
