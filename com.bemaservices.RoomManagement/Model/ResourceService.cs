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
using System.Linq;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class ResourceService.
    /// Implements the <see cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.Resource}" />
    /// </summary>
    /// <seealso cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.Resource}" />
    public class ResourceService : Service<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ResourceService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Gets an <see cref="T:System.Linq.IQueryable`1" /> list of all models
        /// Note: You can sometimes improve performance by using Queryable().AsNoTracking(), but be careful. Lazy-Loading doesn't always work with AsNoTracking  https://stackoverflow.com/a/20290275/1755417
        /// </summary>
        /// <returns>IQueryable&lt;Resource&gt;.</returns>
        public override IQueryable<Resource> Queryable()
        {
            return Queryable( false );
        }

        /// <summary>
        /// Queryables the specified include inactive.
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns>IQueryable&lt;Resource&gt;.</returns>
        public IQueryable<Resource> Queryable( bool includeInactive )
        {
            var qry = base.Queryable();

            if ( !includeInactive )
            {
                qry = qry.Where( r => r.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns><c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.</returns>
        public bool CanDelete( Resource item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<ReservationResource>( Context ).Queryable().Any( a => a.ResourceId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is used in a {1}.", Resource.FriendlyTypeName, ReservationResource.FriendlyTypeName );
                return false;
            }

            if ( new Service<ReservationQuestion>( Context ).Queryable().Any( a => a.ResourceId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} has one or more {1}s tied to it.", Resource.FriendlyTypeName, ReservationQuestion.FriendlyTypeName );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes a specified resource. Returns a boolean flag indicating if the deletion was successful.
        /// </summary>
        /// <param name="item">The <see cref="com.bemaservices.RoomManagement.Model.Resource" /> to delete.</param>
        /// <returns>A <see cref="System.Boolean" /> that indicates if the <see cref="com.bemaservices.RoomManagement.Model.Resource" /> was deleted successfully.</returns>
        public override bool Delete( Resource item )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
        }
    }
}
