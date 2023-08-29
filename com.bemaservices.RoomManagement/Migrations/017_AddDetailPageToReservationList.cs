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
using Rock;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 17, "1.9.4" )]
    public class AddDetailPageToReservationList : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var isExistingUser = RoomManagementMigrationHelper.IsExistingUser( 17 );
            if ( !isExistingUser )
            {
                RoomManagementMigrationHelper.AddBlockAttributeValue( true, "4D4882F8-5ACC-4AE1-BC75-4FFDDA26F270", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8", @"4cbd2b96-e076-46df-a576-356bca5e577f,893ff97e-57d2-42e0-bf9a-6027d673773c" ); // Detail Page
            }
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
