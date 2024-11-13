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
    [MigrationNumber( 048, "1.16.6" )]
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

            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportLandscapeTemplate", "Lava V2 Landscape Template", "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportLandscapeTemplate, com.bemaservices.RoomManagement, Version=2.4.6.14, Culture=neutral, PublicKeyToken=null", false, true, "bf66fe07-e6f1-417b-baa4-a7c31bee4239" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.ReportTemplates.LavaV2ReportLandscapeTemplate", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "02B87D1F-6B11-4B65-9627-DE21448C5DDB", "Active" );
            RockMigrationHelper.AddAttributeValue( "02B87D1F-6B11-4B65-9627-DE21448C5DDB", 0, "True", "E63981D8-2D57-4C6B-A72C-8D90038C044E" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
