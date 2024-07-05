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
            RemoveReservationResourceColumn();
            RemoveReservedItemView();
        }

        /// <summary>
        /// Removes the reserved item view.
        /// </summary>
        private void RemoveReservedItemView()
        {
            RockMigrationHelper.DeleteDefinedValue( "BD2162EB-5CC4-438A-8C0E-F2760897E2A0" ); // Location-Based
        }

        /// <summary>
        /// Removes the reservation resource column.
        /// </summary>
        private void RemoveReservationResourceColumn()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP COLUMN[ReservationLocationId]
" );
        }
    }
}