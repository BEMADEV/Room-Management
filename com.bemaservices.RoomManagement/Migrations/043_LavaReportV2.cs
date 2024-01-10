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
    [MigrationNumber( 043, "1.14.0" )]
    public class LavaReportV2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportTemplate", "Lava V2 Template", "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportTemplate, com.bemaservices.RoomManagement, Version=2.4.6.14, Culture=neutral, PublicKeyToken=null", false, true, "bfdefc3d-3d1d-431c-b20a-92e56fadd7cc" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportTemplate", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "55F7D4F7-B235-4130-8FB3-E951A2EF61E9", "Active" );
            RockMigrationHelper.AddAttributeValue( "55F7D4F7-B235-4130-8FB3-E951A2EF61E9", 0, "True", "37432DE6-D601-4903-82F5-BDDCE80B02B3" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
