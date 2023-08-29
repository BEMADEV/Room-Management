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
    [MigrationNumber( 32, "1.10.3" )]
    public class LinkageMigration : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
               INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]
    ( [ReservationId]
      ,[EventItemOccurrenceId]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId] )
SELECT R.Id 
    , R.EventItemOccurrenceId
    , NewID()
    , R.CreatedDateTime
    , R.ModifiedDateTime
    , R.CreatedByPersonAliasId
    , R.ModifiedByPersonAliasId
FROM [dbo].[_com_bemaservices_RoomManagement_Reservation] R
INNER JOIN [dbo].[EventItemOccurrence] EIO ON R.EventItemOccurrenceId = EIO.Id
WHERE EventItemOccurrenceId IS NOT NULL             
" );


        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

    }
}