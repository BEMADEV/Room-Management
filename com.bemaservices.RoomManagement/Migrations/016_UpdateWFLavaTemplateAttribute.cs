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
    [MigrationNumber( 16, "1.9.4" )]
    public class UpdateWFLavaTemplateAttribute : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var isExistingUser = RoomManagementMigrationHelper.IsExistingUser( 16 );
            if ( !isExistingUser )
            {
                // Add the missing Lava Template attribute values for the Room Reservation Approval Notification workflow.
                RoomManagementMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "{{ Entity.Guid }}" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Lava Template
                RoomManagementMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "{{ Entity.RequesterAlias.Guid }}" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Lava Template
                RoomManagementMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "{{ Entity.ApprovalState }}" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Lava Template
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
