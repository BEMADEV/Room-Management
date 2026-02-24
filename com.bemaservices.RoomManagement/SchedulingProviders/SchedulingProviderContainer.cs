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
using System.ComponentModel.Composition;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders;
using Rock.Data;
using Rock.Extension;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.SchedulingProviders
{

    public class SchedulingProviderContainer : Container<SchedulingProviderComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<SchedulingProviderContainer> instance =
            new Lazy<SchedulingProviderContainer>( () => new SchedulingProviderContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SchedulingProviderContainer Instance
        {
            get { return instance.Value; }
        }

        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            int schedulingProviderEntityTypeId = EntityTypeCache.Get( typeof( Model.SchedulingProvider ) ).Id;
            using ( var rockContext = new RockContext() )
            {
                foreach ( var providerComponent in this.Components )
                {
                    Type providerComponentType = providerComponent.Value.Value.GetType();
                    int providerComponentEntityTypeId = EntityTypeCache.Get( providerComponentType ).Id;
                    Rock.Attribute.Helper.UpdateAttributes( providerComponentType, schedulingProviderEntityTypeId, "EntityTypeId", providerComponentEntityTypeId.ToString(), rockContext );
                }
            }
        }

        public static SchedulingProviderComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>System.String.</returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        [ImportMany( typeof( SchedulingProviderComponent ) )]
        protected override IEnumerable<Lazy<SchedulingProviderComponent, IComponentData>> MEFComponents { get; set; }
    }
}
