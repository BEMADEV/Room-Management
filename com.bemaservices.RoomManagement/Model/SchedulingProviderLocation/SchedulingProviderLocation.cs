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
using Rock.SystemGuid;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Represents a link between a SchedulingProvider and a Location.
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_SchedulingProviderLocation" )]
    [DataContract]
    [EntityTypeGuid( "B89C5287-5468-49F1-8871-590AC20D8AF2" )]
    public class SchedulingProviderLocation : Model<SchedulingProviderLocation>
    {
        #region Entity Properties

        [Required]
        public int SchedulingProviderId { get; set; }

        [Required]
        public int LocationId { get; set; }

        #endregion

        #region Navigation Properties

        [DataMember]
        public virtual SchedulingProvider SchedulingProvider { get; set; }

        [DataMember]
        public virtual Location Location { get; set; }

        #endregion
    }

    #region Entity Configuration

    public partial class SchedulingProviderLocationConfiguration : EntityTypeConfiguration<SchedulingProviderLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderLocationConfiguration"/> class.
        /// </summary>
        public SchedulingProviderLocationConfiguration()
        {
            this.HasRequired( g => g.SchedulingProvider ).WithMany().HasForeignKey( a => a.SchedulingProviderId ).WillCascadeOnDelete( true );
            this.HasRequired( g => g.Location ).WithMany().HasForeignKey( a => a.LocationId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "SchedulingProviderLocation" );
        }
    }

    #endregion Entity Configuration
}
