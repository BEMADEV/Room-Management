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
    [MigrationNumber( 36, "1.12.7" )]
    public class Rollup237 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {    
            #region Approval Process

            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "End Workflow If No Initial Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 32, "", "E1A0E981-B206-4F68-800A-616CC44B51B0" ); // Approval Process:Pending Initial Approval:End Workflow If No Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "End Workflow If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", 64, "", "DC2E6CC2-DF0F-4600-9687-9911A35698DC" ); // Approval Process:Pending Initial Approval:End Workflow If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "End Workflow If No Special Approval Groups", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "645B7181-936B-460C-B577-C4441781FA04" ); // Approval Process:Pending Special Approval:End Workflow If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "End Workflow If No Final Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 32, "", "F66AB76B-78E2-4651-A10A-C7D60E37869B" ); // Approval Process:Pending Final Approval:End Workflow If No Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "End Workflow If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", 64, "", "32B47E6E-3CEC-48CB-B074-D78CDE283CDD" ); // Approval Process:Pending Final Approval:End Workflow If Already Approved            
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
