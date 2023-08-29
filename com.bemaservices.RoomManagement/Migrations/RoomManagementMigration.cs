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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;
using Rock;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Class RoomManagementMigration.
    /// Implements the <see cref="Rock.Plugin.Migration" />
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    public abstract class RoomManagementMigration : Migration
    {
        /// <summary>
        /// Gets the room management migration helper.
        /// </summary>
        /// <value>The room management migration helper.</value>
        public RoomManagementMigrationHelper RoomManagementMigrationHelper
        {
            get
            {
                if ( _roomManagementMigrationHelper == null )
                {
                    _roomManagementMigrationHelper = new RoomManagementMigrationHelper( this );
                }
                return _roomManagementMigrationHelper;
            }
        }
        /// <summary>
        /// The room management migration helper
        /// </summary>
        private RoomManagementMigrationHelper _roomManagementMigrationHelper = null;
    }
}
