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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 38, "1.13.0" )]
    public class Rollup240 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddResourceLocationOptions();
            DisconnectOldResources();
            UpdateDefaultCategories();
            AddMyReservationsToDashboard();
        }

        /// <summary>
        /// Adds my reservations to dashboard.
        /// </summary>
        private void AddMyReservationsToDashboard()
        {
            // Page: My Dashboard
            RockMigrationHelper.UpdateBlockType( "My Reservations Lava", "Block to display reservations assigned to the current user.  The display format is controlled by a lava template.", "~/Plugins/com_bemaservices/RoomManagement/MyReservationsLava.ascx", "BEMA Services > Room Management", "37545F86-F11D-4A4D-98BA-2EDED63B02E1" );
            
            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AE1818D8-581C-4599-97B9-509EA450376A","","37545F86-F11D-4A4D-98BA-2EDED63B02E1","My Reservation Approvals Lava","Main","","",3,"A5955E96-EBF3-426A-BD28-F6B8E5ED3329"); 
            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AE1818D8-581C-4599-97B9-509EA450376A","","37545F86-F11D-4A4D-98BA-2EDED63B02E1","My Reservations Lava","Main","","",4,"B171D69D-F780-46CC-9538-AEB3D5102B51"); 
            
            // Attrib for BlockType: My Reservations Lava:Role
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37545F86-F11D-4A4D-98BA-2EDED63B02E1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Role", "Role", "Role", @"Display the active reservations that the current user Initiated / is a Contact for, or is currently Assigned To for approval.", 0, @"0", "AF3800AD-5E02-40B5-A0E2-C9DE8F3B26B1" );
            // Attrib for BlockType: My Reservations Lava:Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37545F86-F11D-4A4D-98BA-2EDED63B02E1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Status", "Status", "Status", @"Display upcoming reservations, or past reservations.", 1, @"0", "1CE26243-C4AD-4836-A44B-E6E48EB507FB" );
            // Attrib for BlockType: My Reservations Lava:Contents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37545F86-F11D-4A4D-98BA-2EDED63B02E1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Contents", "Contents", "Contents", @"The Lava template to use for displaying reservations assigned to current user.", 3, @"{% include 'Plugins/com_bemaservices/RoomManagement/Assets/Lava/MyReservationsSortable.lava' %}", "01CD1853-6E36-426C-8E63-717FC6801F26" );
            // Attrib for BlockType: My Reservations Lava:Set Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37545F86-F11D-4A4D-98BA-2EDED63B02E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Title", "SetPanelTitle", "Set Panel Title", @"The title to display in the panel header. Leave empty to have the block name.", 4, @"", "866934A6-37AF-4D88-AD7A-FEEF3F7843C8" );
            // Attrib for BlockType: My Reservations Lava:Set Panel Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37545F86-F11D-4A4D-98BA-2EDED63B02E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Panel Icon", "SetPanelIcon", "Set Panel Icon", @"The icon to display in the panel header.", 5, @"", "DC2E8A03-2BB8-4F8E-AA2F-758394457D3E" );
            
            // Attrib Value for Block:My Reservation Approvals Lava, Attribute:Role Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A5955E96-EBF3-426A-BD28-F6B8E5ED3329","AF3800AD-5E02-40B5-A0E2-C9DE8F3B26B1",@"0");
            // Attrib Value for Block:My Reservation Approvals Lava, Attribute:Status Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A5955E96-EBF3-426A-BD28-F6B8E5ED3329","1CE26243-C4AD-4836-A44B-E6E48EB507FB",@"0");
            // Attrib Value for Block:My Reservation Approvals Lava, Attribute:Contents Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A5955E96-EBF3-426A-BD28-F6B8E5ED3329","01CD1853-6E36-426C-8E63-717FC6801F26",@"{% include 'Plugins/com_bemaservices/RoomManagement/Assets/Lava/MyReservationsSortable.lava' %}");
            // Attrib Value for Block:My Reservations Lava, Attribute:Contents Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B171D69D-F780-46CC-9538-AEB3D5102B51","01CD1853-6E36-426C-8E63-717FC6801F26",@"{% include 'Plugins/com_bemaservices/RoomManagement/Assets/Lava/MyReservationsSortable.lava' %}");
            // Attrib Value for Block:My Reservations Lava, Attribute:Status Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B171D69D-F780-46CC-9538-AEB3D5102B51","1CE26243-C4AD-4836-A44B-E6E48EB507FB",@"0");
            // Attrib Value for Block:My Reservations Lava, Attribute:Role Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B171D69D-F780-46CC-9538-AEB3D5102B51","AF3800AD-5E02-40B5-A0E2-C9DE8F3B26B1",@"1");

        }

        /// <summary>
        /// Updates the default categories.
        /// </summary>
        private void UpdateDefaultCategories()
        {

            Sql( @"Update Category
                Set IsSystem = 0
                Where [Guid] in ( 'AE3F4A8D-46D7-4520-934C-85D80167B22C'
				,'BAF88943-64EA-4A6A-8E1E-F4EFC5A6CECA'
				,'D29A2AFC-BD90-428B-9065-2FFD09FB6F6B'
				,'355AC2FD-0831-4A11-9294-5568FDFA8FC3'
                , 'DDEDE1A7-C02B-4322-9D5B-A73CDB9224C6')" );
        }

        /// <summary>
        /// Disconnects the old resources.
        /// </summary>
        private void DisconnectOldResources()
        {
            try
            {
                Sql( @"
                ALTER TABLE[dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Category]
            " );
            }
            catch
            {

            }
        }

        /// <summary>
        /// Adds the resource location options.
        /// </summary>
        private void AddResourceLocationOptions()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [LocationRequirement] INT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [ResourceRequirement] INT NULL
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
