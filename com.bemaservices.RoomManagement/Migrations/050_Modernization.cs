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
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Holds every database change for the Room Management modernization. Each change lives in its own
    /// method so it can be reviewed on its own, and every step must be safe to run more than once.
    /// </summary>
    [MigrationNumber( 50, "1.16.6" )]
    public class Modernization : Migration
    {
        /// <summary>
        /// Applies the modernization changes in order.
        /// </summary>
        public override void Up()
        {
            AddReservationLavaCampusContextSetting();
            QualifyQuestionAttributes();
        }

        /// <summary>
        /// Reverts the modernization changes in the reverse order of <see cref="Up" />.
        /// </summary>
        public override void Down()
        {
            UnqualifyQuestionAttributes();
            RemoveReservationLavaCampusContextSetting();
        }

        /// <summary>
        /// Registers the Reservation Lava block's "Enable Campus Context" setting. The block already reads this
        /// setting to default the campus filter to the page's campus context, but it was never declared, so
        /// administrators could not turn it on. The helper matches on the attribute key, so an attribute Rock
        /// auto-registered from the block's declaration is adopted and given this GUID rather than duplicated.
        /// </summary>
        private void AddReservationLavaCampusContextSetting()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                RESERVATION_LAVA_BLOCK_TYPE_GUID,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Enable Campus Context",
                "EnableCampusContext",
                "",
                "When enabled, select the current campus as the default campus filter when no campus preference is saved.",
                8,
                @"False",
                ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID );
        }

        /// <summary>
        /// Removes the Reservation Lava block's "Enable Campus Context" setting.
        /// </summary>
        private void RemoveReservationLavaCampusContextSetting()
        {
            RockMigrationHelper.DeleteAttribute( ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID );
        }

        /// <summary>
        /// Scopes each existing question's attribute to its own resource or location by setting the attribute's qualifier to
        /// ResourceId or LocationId and the owner's id, so Rock loads it only for that resource or location (in Lava and REST
        /// as well as on the reservation page). Attribute ids and keys are unchanged, so every answer is kept.
        /// Only clean rows are changed. A question is skipped, and left as it is, when its attribute is linked by more than one
        /// question, when it has both or neither of a resource and a location, when the attribute's entity type doesn't match
        /// its owner, or when the attribute is already qualified some other way. Skipped questions are summarized in one
        /// exception log entry so they can be reviewed after the update. Questions already scoped correctly (for example by the
        /// Question List block) are left alone and not reported, so running this again changes nothing.
        /// </summary>
        private void QualifyQuestionAttributes()
        {
            var skippedSummary = SqlScalar( $@"
SET NOCOUNT ON;

DECLARE @ResourceEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{RESERVATION_RESOURCE_ENTITY_TYPE_GUID}' );
DECLARE @LocationEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{RESERVATION_LOCATION_ENTITY_TYPE_GUID}' );

-- Work out the qualifier each question's attribute should have, and whether it can safely be set
SELECT
    [q].[AttributeId],
    CASE WHEN [q].[ResourceId] IS NOT NULL THEN 'ResourceId' ELSE 'LocationId' END AS [QualifierColumn],
    CAST( COALESCE( [q].[ResourceId], [q].[LocationId] ) AS NVARCHAR(200) ) AS [QualifierValue],
    CASE
        WHEN COUNT(*) OVER ( PARTITION BY [q].[AttributeId] ) > 1
            THEN 'Attribute linked by more than one question'
        WHEN ( [q].[ResourceId] IS NULL AND [q].[LocationId] IS NULL ) OR ( [q].[ResourceId] IS NOT NULL AND [q].[LocationId] IS NOT NULL )
            THEN 'Question has both or neither of a resource and a location'
        WHEN ( [q].[ResourceId] IS NOT NULL AND ISNULL( [a].[EntityTypeId], 0 ) <> @ResourceEntityTypeId )
            OR ( [q].[LocationId] IS NOT NULL AND ISNULL( [a].[EntityTypeId], 0 ) <> @LocationEntityTypeId )
            THEN 'Attribute entity type does not match the question'
        WHEN ISNULL( [a].[EntityTypeQualifierColumn], '' ) = ''
            THEN 'Qualify'
        WHEN [a].[EntityTypeQualifierColumn] = CASE WHEN [q].[ResourceId] IS NOT NULL THEN 'ResourceId' ELSE 'LocationId' END
            AND [a].[EntityTypeQualifierValue] = CAST( COALESCE( [q].[ResourceId], [q].[LocationId] ) AS NVARCHAR(200) )
            THEN 'Already qualified'
        ELSE 'Attribute already qualified some other way'
    END AS [Outcome]
INTO #QuestionQualifiers
FROM [_com_bemaservices_RoomManagement_Question] [q]
INNER JOIN [Attribute] [a] ON [a].[Id] = [q].[AttributeId];

-- Qualify the clean rows
UPDATE [a]
SET [a].[EntityTypeQualifierColumn] = [qq].[QualifierColumn],
    [a].[EntityTypeQualifierValue] = [qq].[QualifierValue]
FROM [Attribute] [a]
INNER JOIN #QuestionQualifiers [qq] ON [qq].[AttributeId] = [a].[Id]
WHERE [qq].[Outcome] = 'Qualify';

-- Summarize what was skipped: each reason, its count, and up to 25 attribute ids
SELECT STUFF( (
    SELECT '; ' + [r].[Outcome] + ' (' + CAST( COUNT(*) AS VARCHAR(10) ) + '): attribute ids ' +
        STUFF( (
            SELECT TOP 25 ', ' + CAST( [x].[AttributeId] AS VARCHAR(10) )
            FROM #QuestionQualifiers [x]
            WHERE [x].[Outcome] = [r].[Outcome]
            GROUP BY [x].[AttributeId]
            ORDER BY [x].[AttributeId]
            FOR XML PATH( '' ) ), 1, 2, '' )
    FROM #QuestionQualifiers [r]
    WHERE [r].[Outcome] NOT IN ( 'Qualify', 'Already qualified' )
    GROUP BY [r].[Outcome]
    FOR XML PATH( '' ) ), 1, 2, '' );

DROP TABLE #QuestionQualifiers;
" ) as string;

            // Leave a record of anything skipped; the migration itself has nowhere else to report it
            if ( !string.IsNullOrWhiteSpace( skippedSummary ) )
            {
                ExceptionLogService.LogException( new Exception( "Room Management: some reservation questions were not scoped to their resource or location and were left unchanged. " + skippedSummary ) );
            }
        }

        /// <summary>
        /// Removes the qualifiers that scope question attributes to their resource or location, returning them to
        /// unqualified attributes. Only attributes whose qualifier matches their own question's owner are changed.
        /// </summary>
        private void UnqualifyQuestionAttributes()
        {
            Sql( @"
UPDATE [a]
SET [a].[EntityTypeQualifierColumn] = NULL,
    [a].[EntityTypeQualifierValue] = NULL
FROM [Attribute] [a]
INNER JOIN [_com_bemaservices_RoomManagement_Question] [q] ON [q].[AttributeId] = [a].[Id]
WHERE ( [q].[ResourceId] IS NOT NULL AND [a].[EntityTypeQualifierColumn] = 'ResourceId' AND [a].[EntityTypeQualifierValue] = CAST( [q].[ResourceId] AS NVARCHAR(200) ) )
   OR ( [q].[LocationId] IS NOT NULL AND [a].[EntityTypeQualifierColumn] = 'LocationId' AND [a].[EntityTypeQualifierValue] = CAST( [q].[LocationId] AS NVARCHAR(200) ) );
" );
        }

        /// <summary>
        /// The ReservationResource entity type (SystemGuid.EntityType.RESERVATION_RESOURCE).
        /// </summary>
        private const string RESERVATION_RESOURCE_ENTITY_TYPE_GUID = SystemGuid.EntityType.RESERVATION_RESOURCE;

        /// <summary>
        /// The ReservationLocation entity type (SystemGuid.EntityType.RESERVATION_LOCATION).
        /// </summary>
        private const string RESERVATION_LOCATION_ENTITY_TYPE_GUID = SystemGuid.EntityType.RESERVATION_LOCATION;

        /// <summary>
        /// The Reservation Lava block type, registered in migration 002.
        /// </summary>
        private const string RESERVATION_LAVA_BLOCK_TYPE_GUID = "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635";

        /// <summary>
        /// The Reservation Lava block's "Enable Campus Context" attribute.
        /// </summary>
        private const string ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID = "2CABFAB8-E48F-4CAF-A723-0FD0DC4FC2AA";
    }
}
