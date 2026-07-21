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
    [MigrationNumber( 049, "1.16.6" )]
    public class CentralPurge : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
            -- Drop tables with dependencies first (child tables)
            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationResource];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationLocation];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_Question];

            -- Drop tables with intermediate dependencies
            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_Reservation];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationMinistry];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_LocationLayout];

            -- Drop parent tables (no dependencies on other centralaz tables)
            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_ReservationType];

            DROP TABLE IF EXISTS [dbo].[_com_centralaz_RoomManagement_Resource];
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
