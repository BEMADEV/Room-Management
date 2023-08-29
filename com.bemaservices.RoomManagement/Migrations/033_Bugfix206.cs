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
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 33, "1.10.3" )]
    public class Bugfix206 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Linkage Detail
            RockMigrationHelper.MovePage( "2D25F333-4F47-462B-94C0-6771ABF426D6", "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2" );
            Sql( @"ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation]
ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation] FOREIGN KEY([ReservationId])
REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage] FOREIGN KEY([EventItemOccurrenceId])
REFERENCES [dbo].[EventItemOccurrence] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage]" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

    }
}