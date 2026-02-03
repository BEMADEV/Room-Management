// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license/
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
    /// Represents a link between a SchedulingProvider and a Reservation.
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_SchedulingProviderReservation" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "F3D52B02-F64F-461E-ADAE-7CE767366D3D" )]
    public class SchedulingProviderReservation : Model<SchedulingProviderReservation>
    {
        #region Entity Properties
        [DataMember]
        [Required]
        public int SchedulingProviderId { get; set; }

        [DataMember]
        [Required]
        public int ReservationId { get; set; }

        [DataMember]
        [Required]
        public string ExternalId { get; set; }

        #endregion

        #region Navigation Properties

        [DataMember]
        public virtual SchedulingProvider SchedulingProvider { get; set; }

        [DataMember]
        public virtual Reservation Reservation { get; set; }

        #endregion 
    }

    #region Entity Configuration

    public partial class SchedulingProviderReservationConfiguration : EntityTypeConfiguration<SchedulingProviderReservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderReservationConfiguration"/> class.
        /// </summary>
        public SchedulingProviderReservationConfiguration()
        {
            this.HasRequired( g => g.SchedulingProvider ).WithMany().HasForeignKey( a => a.SchedulingProviderId ).WillCascadeOnDelete( true );
            this.HasRequired( g => g.Reservation ).WithMany().HasForeignKey( a => a.ReservationId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "SchedulingProviderReservation" );
        }
    }

    #endregion Entity Configuration
}
