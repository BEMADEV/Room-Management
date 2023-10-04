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
    [MigrationNumber( 38, "1.13.0" )]
    public class Rollup240 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddResourceLocationOptions();
            DisconnectOldResources();
            UpdateDefaultCategories();
        }

        private void UpdateDefaultCategories()
        {

            Sql( @"Update Category
                Set IsSystem = 0
                Where [Guid] in ( 'AE3F4A8D-46D7-4520-934C-85D80167B22C'
				,'BAF88943-64EA-4A6A-8E1E-F4EFC5A6CECA'
				,'D29A2AFC-BD90-428B-9065-2FFD09FB6F6B'
				,'355AC2FD-0831-4A11-9294-5568FDFA8FC3'
                , 'DDEDE1A7-C02B-4322-9D5B-A73CDB9224C6')" );
        }

        private void DisconnectOldResources()
        {
            try
            {
                Sql( @"
                ALTER TABLE[dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Category]
            " );
            }
            catch
            {

            }
        }

        private void AddResourceLocationOptions()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [LocationRequirement] INT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [ResourceRequirement] INT NULL
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
