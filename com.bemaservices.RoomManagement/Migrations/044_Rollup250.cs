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
    [MigrationNumber( 44, "1.14.0" )]
    public class Rollup250 : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddReservationResourceColumn();
        }

        private void AddReservationResourceColumn()
        {

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ADD [ReservationLocationId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId] FOREIGN KEY([ReservationLocationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId]

" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RemoveReservationResourceColumn();           
        }

        private void RemoveReservationResourceColumn()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP COLUMN[ReservationLocationId]
" );
        }
    }
}