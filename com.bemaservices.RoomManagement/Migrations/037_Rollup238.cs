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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 37, "1.13.0" )]
    public class Rollup238 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowType( false, false, "(Deprecated) Room Reservation Approval Notification", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "543D4FCD-310B-4048-BFCB-BAE582CBB890", 0 ); // (Deprecated) Room Reservation Approval Notification
            RockMigrationHelper.UpdateWorkflowType( false, false, "(Deprecated) Post-Approval Modification Process", "A workflow that changes the reservation's approval status if it was modified by someone not in the Final Approval Group after being approved.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Update", "fa fa-list-ol", 28800, false, 0, "13D0361C-0552-43CA-8F27-D47DB120608D", 0 ); // Post-Approval Modification Process


            RockMigrationHelper.UpdateDefinedValue( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Detailed Calendar", "An detailed calendar view for the Room Management home page.", "EFA1ADEE-09E2-44D0-9529-8B390BE82678", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EFA1ADEE-09E2-44D0-9529-8B390BE82678", "466DC361-B813-445A-8883-FED7E5D4229B", @"{% include ''~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/VueCalendar.lava'' %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EFA1ADEE-09E2-44D0-9529-8B390BE82678", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F", @"" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
