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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 47, "1.16.6" )]
    public class OrphanedApprovalProcessHandling : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            FixDeletedSpecialApprovalHandling();
        }

        /// <summary>
        /// Fixes the deleted special approval handling.
        /// </summary>
        private void FixDeletedSpecialApprovalHandling()
        {           
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Delay 14 Days if Notifications Already Sent", 0, "D22E73F7-86E2-46CA-AD5B-7770A866726B", true, false, "", "98EF8E62-8094-4799-A7CE-A5D4713E9372", 1, "Yes", "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Send Notifications to Any Special Approval Groups", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Mark Initial Notifications as Sent", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Complete Workflow If Approval State Has Changed Since Activity Started", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 512, "0", "E9EA7248-69AD-4C71-9A90-342E55D2739A" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Resource States", 4, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "39FBCAFE-4594-4D5A-9111-C37E8011C76C" ); // Approval Process:Pending Special Approval:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Location States", 5, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "664A5031-3B33-4FD8-9BBA-C16E5A644ADF" ); // Approval Process:Pending Special Approval:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Activate Send Special Approval Reminders if Any Special Approval Groups", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 128, "0", "C7A536B5-DE10-4B0E-B02B-3C02C8306C04" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Special Approval Date If Blank", 7, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", 32, "", "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Reservation to Pending Final Approval If No Special Approval Groups", 8, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "665A49A2-5ED1-4994-A348-CDD6F9CC184A" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "End Workflow If No Special Approval Groups", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "645B7181-936B-460C-B577-C4441781FA04" ); // Approval Process:Pending Special Approval:End Workflow If No Special Approval Groups  
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}