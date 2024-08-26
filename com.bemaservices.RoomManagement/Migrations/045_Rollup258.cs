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
    [MigrationNumber( 45, "1.14.0" )]
    public class Rollup258 : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            UpdateReservationLocationCascade();
            AddConflictPage();
        }

        private void AddConflictPage()
        {
            // Page: Conflicting Reservations
            RockMigrationHelper.AddPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Conflicting Reservations", "", "A9409D4C-8CDC-4B55-A471-808EB66C5348", "fa fa-calendar-times" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Conflicting Reservation List", "Block for viewing a list of conflicting reservations.", "~/Plugins/com_bemaservices/RoomManagement/ConflictingReservationList.ascx", "BEMA Services > Room Management", "698FC2C7-C8B3-4BFC-AD55-BD4306373222" );
            // Add Block to Page: Conflicting Reservations, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A9409D4C-8CDC-4B55-A471-808EB66C5348", "", "698FC2C7-C8B3-4BFC-AD55-BD4306373222", "Conflicting Reservation List", "Main", "", "", 0, "259896FD-1700-4A36-B315-4B66A7AB94C8" );
            // Attrib for BlockType: Conflicting Reservation List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "698FC2C7-C8B3-4BFC-AD55-BD4306373222", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "CFEFD97C-65C3-493E-9E7D-2B38AE9F18B4" );
            // Attrib for BlockType: Conflicting Reservation List:Related Entity Query String Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "698FC2C7-C8B3-4BFC-AD55-BD4306373222", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Related Entity Query String Parameter", "RelatedEntityQueryStringParameter", "Related Entity Query String Parameter", @"The query string parameter that holds id to the related entity.", 0, @"", "4C3795B8-C641-4CC2-A944-68C7A0ED162F" );
            // Attrib Value for Block:Conflicting Reservation List, Attribute:Detail Page Page: Conflicting Reservations, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "259896FD-1700-4A36-B315-4B66A7AB94C8", "CFEFD97C-65C3-493E-9E7D-2B38AE9F18B4", @"4cbd2b96-e076-46df-a576-356bca5e577f" );

        }

        /// <summary>
        /// Adds the reservation resource column.
        /// </summary>
        private void UpdateReservationLocationCascade()
        {

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location]
        " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RemoveConflictPage();
        }

        private void RemoveConflictPage()
        {
            RockMigrationHelper.DeleteAttribute( "4C3795B8-C641-4CC2-A944-68C7A0ED162F" );
            RockMigrationHelper.DeleteAttribute( "CFEFD97C-65C3-493E-9E7D-2B38AE9F18B4" );
            RockMigrationHelper.DeleteBlock( "259896FD-1700-4A36-B315-4B66A7AB94C8" );
            RockMigrationHelper.DeleteBlockType( "698FC2C7-C8B3-4BFC-AD55-BD4306373222" );
            RockMigrationHelper.DeletePage( "A9409D4C-8CDC-4B55-A471-808EB66C5348" ); //  Page: Conflicting Reservations
        }
    }
}