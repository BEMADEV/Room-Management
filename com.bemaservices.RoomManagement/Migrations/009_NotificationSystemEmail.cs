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
    [MigrationNumber( 9, "1.9.4" )]
    public class NotificationSystemEmail : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var isExistingUser = RoomManagementMigrationHelper.IsExistingUser( 9 );
            if ( !isExistingUser )
            {
                #region Update Reservation Detail Block settings

                RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "System Email", "SystemEmail", "", "A system email to use when notifying approvers about a reservation request.", 0, @"", "F3FBDD84-5E9B-40C2-B199-3FAE1C2308DC" );
                RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "", "An optional group that provides final approval for a reservation. If used, this should be the same group as in the Reservation Approval Workflow.", 0, @"", "E715D25F-CA53-4B16-B8B2-4A94FD3A3560" );
                RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Super Admin Group", "SuperAdminGroup", "", "The superadmin group that can force an approve / deny status on reservations, i.e. a facilities team.", 0, @"FBE0324F-F29A-4ACF-8EC3-5386C5562D70", "BBA41563-5379-43FA-955B-93C1926A4F66" );
                RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "", "Should a record of this communication be saved to the recipient's profile", 2, @"False", "B90006F5-9B17-48DD-B455-5BAA2BE1A9A2" );

                #endregion
            }
            #region Increase size of the Note column in the Reservation table
            Sql( @"
    ALTER TABLE[_com_bemaservices_RoomManagement_Reservation]
    ALTER COLUMN[Note] NVARCHAR( 2500 )
    " );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}
