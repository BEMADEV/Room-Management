﻿// <copyright>
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
    [MigrationNumber( 4, "1.9.4" )]
    public class UpdateFieldType : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RoomManagementMigrationHelper.UpdateFieldTypeByGuid( "Reservation", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );

            RoomManagementMigrationHelper.UpdateFieldTypeByGuid( "ReservationStatus", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationStatusFieldType", "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteFieldType( "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );
            RockMigrationHelper.DeleteFieldType( "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );
        }
    }
}
