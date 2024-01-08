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
    [MigrationNumber( 041, "1.14.0" )]
    public class RoomDetails : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Entity: Rock.Model.Location Attribute: Room Details
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.Location", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "", "", "Room Details", "Room Details", @"Any details you would like displayed when reserving a room in the Room Management plugin.", 6562, @"", "775D049F-7857-4EAA-B112-243A734275B8", "RoomManagement_RoomDetails" );
            // Qualifier for attribute: RoomManagement_RoomDetails
            RockMigrationHelper.UpdateAttributeQualifier( "775D049F-7857-4EAA-B112-243A734275B8", "toolbar", @"Light", "DF03B5F1-4DE9-44FC-8904-C411C4A7F5AD" );
            // Qualifier for attribute: RoomManagement_RoomDetails
            RockMigrationHelper.UpdateAttributeQualifier( "775D049F-7857-4EAA-B112-243A734275B8", "documentfolderroot", @"", "ADA6D6CB-1A9B-4A05-8362-877EC92C0E78" );
            // Qualifier for attribute: RoomManagement_RoomDetails
            RockMigrationHelper.UpdateAttributeQualifier( "775D049F-7857-4EAA-B112-243A734275B8", "imagefolderroot", @"", "101D8F99-224B-496E-A047-FA25B94040F7" );
            // Qualifier for attribute: RoomManagement_RoomDetails
            RockMigrationHelper.UpdateAttributeQualifier( "775D049F-7857-4EAA-B112-243A734275B8", "userspecificroot", @"False", "250F24D8-2A88-4560-8570-3A136B41A620" );


            // Attrib for BlockType: Reservation Detail:Location Detail Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "27718256-C1EB-4B1F-9B4B-AC53249F78DF", "Location Detail Template", "LocationDetailTemplate", "Location Detail Template", @"A customizable template to dictate what details are displayed when selecting a location", 1, @"<div class='row'>
    {% if Location.ImageId != null %}
        {% capture imgUrl %}/GetImage.ashx?id={{Location.ImageId}}{% endcapture %}
        {% capture imgTag %}<img src='{{imgUrl}}&maxwidth=200&maxheight=200'/>{% endcapture %}
        <div class='col-md-4'>
            <div class='photo'>
                <a href='{{imgUrl}}' target='_blank'>{{imgTag}}</a>
            </div>
        </div>
    {% endif %}

    {% assign details = Location | Attribute:'RoomManagement_RoomDetails' %}
    {% if details != null and details != empty and details != '' %}
        <div class='col-md-8'>
            {{details}}
        </div>
    {% endif %}
</div>", "D01E47A3-E0BB-48C0-A594-4E4C282A0C55" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
