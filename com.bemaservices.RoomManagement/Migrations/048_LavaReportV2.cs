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

            RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", "[Obsolete] Event-Based Report", "This template is now obsolete and will be removed in the future. Please use the new 'Lava V2 Template'.", "5D53E2F0-BA82-4154-B996-085C979FACB0", true );
            RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", "[Obsolete] Lava Report", "This template is now obsolete and will be removed in the future. Please use the new 'Lava V2 Template'.", "71CEBC9E-D9BA-432D-B1C9-9B3D5CB8E7ED", true );
            RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", "[Obsolete] Location-Based Report", "This template is now obsolete and will be removed in the future. Please use the new 'Lava V2 Landscape Template'.", "46C855B0-E50E-49E7-8B99-74561AFB3DD2", true );
            RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", "Lava Event-Based Report", "This is the default option for a printable report.", "75D7960B-AF0D-42E9-8756-33CBBEE65B8E", false );
            RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", "Lava Location-Based Report", "Meant primarily for facilities teams, this report has a line item for each reservation location containing information about requested layouts.", "5ED8A40D-64CC-4D05-8C8C-C5378C1C76EE", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "5ED8A40D-64CC-4D05-8C8C-C5378C1C76EE", "1C2F3975-B1E2-4F8A-B2A2-FEF8D1A37E6C", @"bf66fe07-e6f1-417b-baa4-a7c31bee4239" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5ED8A40D-64CC-4D05-8C8C-C5378C1C76EE", "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", @"{% include ""~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/LocationReport.lava"" %}" );
            
            RockMigrationHelper.AddDefinedValueAttributeValue( "75D7960B-AF0D-42E9-8756-33CBBEE65B8E", "1C2F3975-B1E2-4F8A-B2A2-FEF8D1A37E6C", @"bfdefc3d-3d1d-431c-b20a-92e56fadd7cc" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "75D7960B-AF0D-42E9-8756-33CBBEE65B8E", "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", @"{% include ""~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/EventReport.lava"" %}" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
