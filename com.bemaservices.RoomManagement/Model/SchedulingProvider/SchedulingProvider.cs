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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using com.bemaservices.RoomManagement.SchedulingProviders;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;
using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Represents a scheduling provider configuration.
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_SchedulingProvider" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "1D4CFE5A-E0D2-4077-A822-9B2C01CC0A0F" )]
    public class SchedulingProvider : Model<SchedulingProvider>
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the name of the scheduling provider.
        /// </summary>
        [DataMember]
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the scheduling provider.
        /// </summary>
        [DataMember]
        [MaxLength( 500 )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        [DataMember]
        [Required]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this scheduling provider is active.
        /// </summary>
        [DataMember]
        [Required]
        public bool IsActive { get; set; }

        public string MappingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        [DataMember]
        public virtual EntityType EntityType { get; set; }


        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Gets the asset storage component.
        /// </summary>
        /// <returns></returns>
        public virtual SchedulingProviderComponent GetSchedulingComponent()
        {
            var entityType = EntityTypeCache.Get( EntityTypeId );
            if ( entityType != null )
            {
                return SchedulingProviderContainer.GetComponent( entityType.Name );
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    public partial class SchedulingProviderConfiguration : EntityTypeConfiguration<SchedulingProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderConfiguration"/> class.
        /// </summary>
        public SchedulingProviderConfiguration()
        {
            this.HasRequired( g => g.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "SchedulingProvider" );
        }
    }

    #endregion Entity Configuration
}
