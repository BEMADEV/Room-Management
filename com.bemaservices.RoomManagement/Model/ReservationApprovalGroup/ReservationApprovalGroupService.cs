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
using System.Data.Entity;
using System.Runtime.Caching;

using Rock.Data;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class ReservationApprovalGroupService.
    /// Implements the <see cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.ReservationApprovalGroup}" />
    /// </summary>
    /// <seealso cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.ReservationApprovalGroup}" />
    public class ReservationApprovalGroupService : Service<ReservationApprovalGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLocationTypeService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationApprovalGroupService( RockContext context ) : base( context ) { }
    }

    /// <summary>
    /// Class for ReservationLocationType extension methods.
    /// </summary>
    public static partial class ReservationApprovalGroupExtensionMethods
    {
        /// <summary>
        /// Clones a specified deep copy.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> [deep copy].</param>
        /// <returns>ReservationApprovalGroup.</returns>
        public static ReservationApprovalGroup Clone( this ReservationApprovalGroup source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ReservationApprovalGroup;
            }
            else
            {
                var target = new ReservationApprovalGroup();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this ReservationApprovalGroup target, ReservationApprovalGroup source )
        {
            target.CampusId = source.CampusId;
            target.ApprovalGroupId = source.ApprovalGroupId;
            target.ApprovalGroupType = source.ApprovalGroupType;
            target.ReservationTypeId = source.ReservationTypeId;
            target.Id = source.Id;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
        }
    }

}
