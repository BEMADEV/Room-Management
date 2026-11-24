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
            ReconcileReservationQuestionEntityType();
        }

        /// <summary>
        /// Reverts the modernization changes in the reverse order of <see cref="Up" />.
        /// </summary>
        public override void Down()
        {
            RestoreReservationQuestionEntityTypeName();
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
        /// Gives the ReservationQuestion entity type a single row with a stable Guid, so the model can later declare
        /// [EntityTypeGuid]. Migration 013 registered it as "com.bemaservices.RoomManagement.Model.Question" with Guid
        /// F42FB8A5…, but the class is now ReservationQuestion, so Rock may also have registered a second row under the
        /// current name with a random Guid. EntityType names and Guids must stay unique, so:
        /// - only the 013 row: rename it to the current class name;
        /// - only the newer row: give it the 013 Guid;
        /// - both rows: keep whichever is referenced and delete the other, but only if nothing references it through a
        ///   foreign key. If both are referenced, nothing changes and the reference counts are logged for manual cleanup.
        /// Running this again changes nothing once there is one row with the current name and the 013 Guid.
        /// </summary>
        private void ReconcileReservationQuestionEntityType()
        {
            var conflictSummary = SqlScalar( $@"
SET NOCOUNT ON;

DECLARE @QuestionGuid UNIQUEIDENTIFIER = '{RESERVATION_QUESTION_ENTITY_TYPE_GUID}';
DECLARE @CurrentName NVARCHAR(100) = '{RESERVATION_QUESTION_ENTITY_TYPE_NAME}';
DECLARE @GuidRowId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = @QuestionGuid );
DECLARE @NameRowId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = @CurrentName AND [Guid] <> @QuestionGuid );
DECLARE @Summary NVARCHAR(MAX) = NULL;

IF @GuidRowId IS NOT NULL AND @NameRowId IS NULL
BEGIN
    -- Only the migration 013 row: give it the current class name
    UPDATE [EntityType] SET [Name] = @CurrentName WHERE [Id] = @GuidRowId AND [Name] <> @CurrentName;
END
ELSE IF @GuidRowId IS NULL AND @NameRowId IS NOT NULL
BEGIN
    -- Only the newer row: give it the stable Guid
    UPDATE [EntityType] SET [Guid] = @QuestionGuid WHERE [Id] = @NameRowId;
END
ELSE IF @GuidRowId IS NOT NULL AND @NameRowId IS NOT NULL
BEGIN
    -- Both rows: count the foreign-key references to each, across every table that references EntityType
    CREATE TABLE #EntityTypeReferences ( [EntityTypeId] INT, [ReferencedFrom] NVARCHAR(400), [ReferenceCount] INT );

    DECLARE @CountSql NVARCHAR(MAX) = N'';
    SELECT @CountSql = @CountSql
        + N'INSERT INTO #EntityTypeReferences SELECT ' + QUOTENAME( [c].[name] ) + N', N'''
        + [s].[name] + N'.' + [t].[name] + N'.' + [c].[name] + N''', COUNT(*) FROM '
        + QUOTENAME( [s].[name] ) + N'.' + QUOTENAME( [t].[name] )
        + N' WHERE ' + QUOTENAME( [c].[name] ) + N' IN ( @GuidRowId, @NameRowId ) GROUP BY ' + QUOTENAME( [c].[name] ) + N';'
    FROM [sys].[foreign_key_columns] [fkc]
    INNER JOIN [sys].[tables] [t] ON [t].[object_id] = [fkc].[parent_object_id]
    INNER JOIN [sys].[schemas] [s] ON [s].[schema_id] = [t].[schema_id]
    INNER JOIN [sys].[columns] [c] ON [c].[object_id] = [fkc].[parent_object_id] AND [c].[column_id] = [fkc].[parent_column_id]
    WHERE [fkc].[referenced_object_id] = OBJECT_ID( N'[dbo].[EntityType]' );

    EXEC sp_executesql @CountSql, N'@GuidRowId INT, @NameRowId INT', @GuidRowId = @GuidRowId, @NameRowId = @NameRowId;

    DECLARE @GuidRowReferences INT = ISNULL( ( SELECT SUM( [ReferenceCount] ) FROM #EntityTypeReferences WHERE [EntityTypeId] = @GuidRowId ), 0 );
    DECLARE @NameRowReferences INT = ISNULL( ( SELECT SUM( [ReferenceCount] ) FROM #EntityTypeReferences WHERE [EntityTypeId] = @NameRowId ), 0 );

    IF @NameRowReferences = 0
    BEGIN
        -- Keep the 013 row: remove the unreferenced newer row first, since names must be unique
        DELETE FROM [EntityType] WHERE [Id] = @NameRowId;
        UPDATE [EntityType] SET [Name] = @CurrentName WHERE [Id] = @GuidRowId;
    END
    ELSE IF @GuidRowReferences = 0
    BEGIN
        -- Keep the newer row: remove the unreferenced 013 row first, since Guids must be unique
        DELETE FROM [EntityType] WHERE [Id] = @GuidRowId;
        UPDATE [EntityType] SET [Guid] = @QuestionGuid WHERE [Id] = @NameRowId;
    END
    ELSE
    BEGIN
        SET @Summary = 'EntityType ' + CAST( @GuidRowId AS VARCHAR(10) ) + ' (013 Guid) and ' + CAST( @NameRowId AS VARCHAR(10) )
            + ' (current name) are both referenced: ' + STUFF( (
                SELECT '; ' + CAST( [EntityTypeId] AS VARCHAR(10) ) + ' in ' + [ReferencedFrom] + ' (' + CAST( [ReferenceCount] AS VARCHAR(10) ) + ')'
                FROM #EntityTypeReferences
                ORDER BY [EntityTypeId], [ReferencedFrom]
                FOR XML PATH( '' ) ), 1, 2, '' );
    END

    DROP TABLE #EntityTypeReferences;
END

SELECT @Summary;
" ) as string;

            // Leave a record when the rows couldn't be reconciled automatically
            if ( !string.IsNullOrWhiteSpace( conflictSummary ) )
            {
                ExceptionLogService.LogException( new Exception( "Room Management: the ReservationQuestion entity type has two rows that both have references and was left unchanged. " + conflictSummary ) );
            }
        }

        /// <summary>
        /// Restores the name migration 013 gave the ReservationQuestion entity type row. A row deleted by
        /// <see cref="ReconcileReservationQuestionEntityType" /> isn't recreated; Rock registers the class again by name on startup.
        /// </summary>
        private void RestoreReservationQuestionEntityTypeName()
        {
            Sql( $@"
UPDATE [EntityType]
SET [Name] = '{RESERVATION_QUESTION_ENTITY_TYPE_OLD_NAME}'
WHERE [Guid] = '{RESERVATION_QUESTION_ENTITY_TYPE_GUID}'
    AND [Name] = '{RESERVATION_QUESTION_ENTITY_TYPE_NAME}'
    AND NOT EXISTS ( SELECT 1 FROM [EntityType] WHERE [Name] = '{RESERVATION_QUESTION_ENTITY_TYPE_OLD_NAME}' );
" );
        }

        /// <summary>
        /// The Guid migration 013 gave the question entity type.
        /// </summary>
        private const string RESERVATION_QUESTION_ENTITY_TYPE_GUID = "F42FB8A5-D646-4FA8-AABE-D47B53A9CE35";

        /// <summary>
        /// The question entity type's current name, matching the ReservationQuestion class.
        /// </summary>
        private const string RESERVATION_QUESTION_ENTITY_TYPE_NAME = "com.bemaservices.RoomManagement.Model.ReservationQuestion";

        /// <summary>
        /// The name migration 013 registered for the question entity type.
        /// </summary>
        private const string RESERVATION_QUESTION_ENTITY_TYPE_OLD_NAME = "com.bemaservices.RoomManagement.Model.Question";

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
