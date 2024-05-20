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
    public class Rollup247 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {            
            // Fix Initial Approval Group Action
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "6B643254-B991-4534-B7B5-22C661D5099B", @"927f9dd3-4809-4c5f-b396-db54007ac043" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group:Reservation Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "379AECA2-62B1-4A3C-936C-0250E0762B64", @"" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group:Reservation Type

            // Add a job that will add a few missing indexes
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'com.bemaservices.RoomManagement.Jobs.V247TemporarySetupPhotoFix'
                    AND [Guid] = '929E3CBE-6A8D-4846-943D-ADB91A8C7672'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Room Management Helper v2.4.7 - Update Temporary Images'
                    , 'Creates permanent copies of orphaned images still tied to reservations.'
                    , 'com.bemaservices.RoomManagement.Jobs.V247TemporarySetupPhotoFix'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '929E3CBE-6A8D-4846-943D-ADB91A8C7672'
                );
            END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
