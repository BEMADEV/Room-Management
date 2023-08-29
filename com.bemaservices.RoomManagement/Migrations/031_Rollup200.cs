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
    [MigrationNumber( 31, "1.10.3" )]
    public class Rollup200 : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [OverrideApprovalGroupId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_OverrideApprovalGroupId] FOREIGN KEY([OverrideApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_OverrideApprovalGroupId]             
" );

            Sql( @" UPDATE _com_bemaservices_RoomManagement_ReservationType
                    SET [OverrideApprovalGroupId] = [SuperAdminGroupId]" );

            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [EventItemOccurrenceId] [int] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLinkage] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage] FOREIGN KEY([EventItemOccurrenceId])
                REFERENCES [dbo].[EventItemOccurrence] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId]

" );

            UpdateNoteType103( "Reservation Note", "com.bemaservices.RoomManagement.Model.Reservation", true, "2D02BFC9-EE35-4297-9957-146AF9EB1660" );

            // Page: New Reservation
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Notes", "Context aware block for adding notes to an entity.", "~/Blocks/Core/Notes.ascx", "Core", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            // Add Block to Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0" );
            // Add Block to Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 2, "D7F1AD31-6E68-47E8-8C39-D29E42508FC0" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: Notes:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" );
            // Attrib for BlockType: Notes:Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", @"The text to display as the heading.  If left blank, the Note Type name will be used.", 1, @"", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Notes:Heading Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon CSS Class", "HeadingIcon", "", @"The css class name to use for the heading icon. ", 2, @"fa fa-sticky-note-o", "B69937BE-000A-4B94-852F-16DE92344392" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Notes:Note Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Term", "NoteTerm", "", @"The term to use for note (i.e. 'Note', 'Comment').", 3, @"Note", "FD0727DC-92F4-4765-82CB-3A08B7D864F8" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: Notes:Display Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Type", "DisplayType", "", @"The format to use for displaying notes.", 4, @"Full", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E" );
            // Attrib for BlockType: Notes:Use Person Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Icon", "UsePersonIcon", "", @"", 5, @"False", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: Notes:Show Alert Checkbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Alert Checkbox", "ShowAlertCheckbox", "", @"", 6, @"True", "20243A98-4802-48E2-AF61-83956056AC65" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: Notes:Show Private Checkbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Private Checkbox", "ShowPrivateCheckbox", "", @"", 7, @"True", "D68EE1F5-D29F-404B-945D-AD0BE76594C3" );
            // Attrib for BlockType: Notes:Show Security Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Button", "ShowSecurityButton", "", @"", 8, @"True", "00B6EBFF-786D-453E-8746-119D0B45CB3E" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: Notes:Allow Anonymous
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Anonymous", "AllowAnonymous", "", @"", 9, @"False", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7" );
            // Attrib for BlockType: Notes:Add Always Visible
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Always Visible", "AddAlwaysVisible", "", @"Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", 10, @"False", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib for BlockType: Notes:Display Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Order", "DisplayOrder", "", @"Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", 11, @"Descending", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1" );
            // Attrib for BlockType: Notes:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "", @"Optional list of note types to limit display to", 12, @"", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4" );
            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Backdated Notes", "AllowBackdatedNotes", "", @"", 12, @"False", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61" );
            // Attrib for BlockType: Notes:Display Note Type Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Note Type Heading", "DisplayNoteTypeHeading", "", @"Should each note's Note Type be displayed as a heading above each note?", 13, @"False", "C5FD0719-1E03-4C17-BE31-E02A3637C39A" );
            // Attrib for BlockType: Notes:Expand Replies
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Expand Replies", "ExpandReplies", "", @"Should replies to automatically expanded?", 14, @"False", "84E53A88-32D2-432C-8BB5-600BDBA10949" );
            // Attrib for BlockType: Notes:Note View Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Note View Lava Template", "NoteViewLavaTemplate", "", @"The Lava Template to use when rendering the readonly view of all the notes.", 15, @"{% include '~~/Assets/Lava/NoteViewList.lava' %}", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2D02BFC9-EE35-4297-9957-146AF9EB1660" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"839768a3-10d6-446c-a65b-b8f9efd7808f" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Add/Update PageContext for Page:New Reservation, Entity: com.bemaservices.RoomManagement.Model.Reservation, Parameter: ReservationId
            RockMigrationHelper.UpdatePageContext( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "com.bemaservices.RoomManagement.Model.Reservation", "ReservationId", "18F65AE1-1485-40CD-B63C-7EC27196B4E5" );


            // Page: History
            RockMigrationHelper.AddPage( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "History", "", "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "History Log", "Block for displaying the history of changes to a particular entity.", "~/Blocks/Core/HistoryLog.ascx", "Core", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "Reservation Detail", "Main", "", "", 0, "178C1B2A-1371-46AA-9BAD-2FC4A423DE9A" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 2, "3CFA3A78-8CBB-49EF-8195-F7304A554B12" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: History Log:Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", @"The Lava template to use for the heading. <span class='tip tip-lava'></span>", 0, @"{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );
            // Attrib for BlockType: History Log:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:History Log, Attribute:Heading Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3CFA3A78-8CBB-49EF-8195-F7304A554B12", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"{{ Entity.Name}} (ID:{{ Entity.Id }})" );
            // Attrib Value for Block:History Log, Attribute:Entity Type Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3CFA3A78-8CBB-49EF-8195-F7304A554B12", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"839768a3-10d6-446c-a65b-b8f9efd7808f" );
            // Add/Update PageContext for Page:History, Entity: com.bemaservices.RoomManagement.Model.Reservation, Parameter: ReservationId
            RockMigrationHelper.UpdatePageContext( "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "com.bemaservices.RoomManagement.Model.Reservation", "ReservationId", "74CC7218-20E3-4ACA-8235-799FCC2E11B7" );

            // Page: Linkage Detail
            RockMigrationHelper.AddPage( "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Linkage Detail", "", "2D25F333-4F47-462B-94C0-6771ABF426D6", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation Linkage Detail", "Block for creating a Reservation Linkage", "~/Plugins/com_bemaservices/RoomManagement/ReservationLinkageDetail.ascx", "BEMA Services > Room Management", "B25263A0-F51B-4F62-A402-973707528572" );
            // Add Block to Page: Linkage Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2D25F333-4F47-462B-94C0-6771ABF426D6", "", "B25263A0-F51B-4F62-A402-973707528572", "Reservation Linkage Detail", "Main", "", "", 0, "95B727BB-3ADC-4106-8CE0-41B522FC030E" );
            // Attrib for BlockType: Reservation Linkage Detail:Default Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "EC0D9528-1A22-404E-A776-566404987363", "Default Calendar", "DefaultCalendar", "Default Calendar", @"The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.", 0, @"", "CE976B3C-EBAD-4A7E-8E31-CCF23B6B694E" );
            // Attrib for BlockType: Reservation Linkage Detail:Default Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "EC0D9528-1A22-404E-A776-566404987363", "Default Calendar", "DefaultCalendar", "Default Calendar", @"The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.", 0, @"", "1A5649D4-EE00-46AE-B16C-3A49077D7354" );
            // Attrib for BlockType: Reservation Linkage Detail:Allow Creating New Calendar Events
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Creating New Calendar Events", "AllowCreatingNewCalendarEvents", "Allow Creating New Calendar Events", @"If set to ""Yes"", the staff person will be offered the ""New Event"" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.", 1, @"true", "55AABE18-39B3-4DFD-BF0B-057714CFCE5B" );
            // Attrib for BlockType: Reservation Linkage Detail:Include Inactive Calendar Items
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Calendar Items", "IncludeInactiveCalendarItems", "Include Inactive Calendar Items", @"Check this box to hide inactive calendar items.", 2, @"true", "226AFC08-CB47-46E8-A9F2-C51475EFA69E" );
            // Attrib for BlockType: Reservation Linkage Detail:Completion Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Completion Workflow", "CompletionWorkflow", "Completion Workflow", @"A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:
 + Reservation
 + EventItemOccurrenceGuid", 3, @"", "FDB1410F-1D7F-4B86-87DC-F445154CD44C" );
            // Attrib for BlockType: Reservation Linkage Detail:Event Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Instructions Lava Template", "LavaInstruction_Event", "Event Instructions Lava Template", @"Instructions here will show up on the fourth panel of the wizard.", 4, @"", "6AD8EB95-A5DB-4302-B85D-BABA0C59E088" );
            // Attrib for BlockType: Reservation Linkage Detail:Display Link to Event Details Page on Confirmation Screen
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Link to Event Details Page on Confirmation Screen", "DisplayEventDetailsLink", "Display Link to Event Details Page on Confirmation Screen", @"Check this box to show the link to the event details page in the wizard confirmation screen.", 4, @"true", "93D9EAAB-5672-485A-A9FC-77E73D5EB6CF" );
            // Attrib for BlockType: Reservation Linkage Detail:External Event Details Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "External Event Details Page", @"Determines which page the link in the final confirmation screen will take you to (if ""Display Link to Event Details ... "" is selected).", 5, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "4768E7F8-3BC5-46DF-9AFF-38244FEDA336" );
            // Attrib for BlockType: Reservation Linkage Detail:External Event Details Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "External Event Details Page", @"Determines which page the link in the final confirmation screen will take you to (if ""Display Link to Event Details ... "" is selected).", 5, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "420A1979-A875-4248-838B-25E040FBBE3A" );
            // Attrib for BlockType: Reservation Linkage Detail:Event Occurrence Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Occurrence Instructions Lava Template", "LavaInstruction_EventOccurrence", "Event Occurrence Instructions Lava Template", @"Instructions here will show up on the fifth panel of the wizard.", 5, @"", "D7EE02D0-C8C7-451F-925B-9C2BF0FD3F2E" );
            // Attrib for BlockType: Reservation Linkage Detail:Summary Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Summary Instructions Lava Template", "LavaInstruction_Summary", "Summary Instructions Lava Template", @"Instructions here will show up on the sixth panel of the wizard.", 6, @"", "74024AF7-EEF0-4C9B-A90F-19DAF2DAD453" );
            // Attrib for BlockType: Reservation Linkage Detail:Summary Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Summary Instructions Lava Template", "LavaInstruction_Summary", "Summary Instructions Lava Template", @"Instructions here will show up on the sixth panel of the wizard.", 6, @"", "772FFED2-C8C5-4F40-87A9-BFC23E9CA08B" );
            // Attrib for BlockType: Reservation Linkage Detail:Wizard Finished Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Wizard Finished Instructions Lava Template", "LavaInstruction_Finished", "Wizard Finished Instructions Lava Template", @"Instructions here will show up on the final panel of the wizard.", 7, @"", "93654146-7156-4A9F-A88A-62C501836A17" );

            // Page: Events
            RockMigrationHelper.AddPage( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Events", "", "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            RockMigrationHelper.UpdateBlockType( "Reservation Linkage List", "Displays the linkages associated with a reservation.", "~/Plugins/com_bemaservices/RoomManagement/ReservationLinkageList.ascx", "BEMA Services > Room Management", "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "Reservation Detail", "Main", "", "", 0, "94BDF4E7-CA48-4C66-B0FB-5B13B87A5E0A" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "Reservation Linkage List", "Main", "", "", 2, "4F6E7570-FD87-4991-8C6F-8E2568B1377E" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Reservation Linkage List:Linkage Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "Linkage Page", @"The page for creating a reservation linkage", 1, @"DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "95759D3F-5E2C-44B7-9C13-C53D78916A56" );
            // Attrib for BlockType: Reservation Linkage List:Calendar Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemDetailPage", "Calendar Item Page", @"The page to view calendar item details", 2, @"7FB33834-F40A-4221-8849-BB8C06903B04", "28863235-DE8C-402E-B227-A91D3827AD54" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Reservation Linkage List:Content Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemDetailPage", "Content Item Page", @"The page for viewing details about a content channel item", 3, @"D18E837C-9E65-4A38-8647-DFF04A595D97", "0E06771C-3522-466E-BBE6-5578ED22C3FF" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Linkage Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "95759D3F-5E2C-44B7-9C13-C53D78916A56", @"2D25F333-4F47-462B-94C0-6771ABF426D6" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Content Item Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "0E06771C-3522-466E-BBE6-5578ED22C3FF", @"d18e837c-9e65-4a38-8647-dff04a595d97" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Calendar Item Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "28863235-DE8C-402E-B227-A91D3827AD54", @"7fb33834-f40a-4221-8849-bb8c06903b04" );

            RockMigrationHelper.UpdateHtmlContentBlock( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "4D5B4438-D3A3-468F-BCAD-F7A0DC36F2F3" );
            RockMigrationHelper.UpdateHtmlContentBlock( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "E1A199D7-AA1F-4F8E-94EC-C3EE8386E2B6" );
            RockMigrationHelper.UpdateHtmlContentBlock( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "16A29568-775C-4E14-9EC4-B822D3B1C6A4" );

            RockMigrationHelper.DeleteBlock( "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B" );

            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [InitialApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [InitialApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_InitialApproverAliasId] FOREIGN KEY([InitialApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_InitialApproverAliasId]             
" );
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [SpecialApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [SpecialApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_SpecialApproverAliasId] FOREIGN KEY([SpecialApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_SpecialApproverAliasId]             
" );
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [FinalApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [FinalApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_FinalApproverAliasId] FOREIGN KEY([FinalApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_FinalApproverAliasId]             
" );

            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [InitialApprovalGroupId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_InitialApprovalGroupId] FOREIGN KEY([InitialApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_InitialApprovalGroupId]             
" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_NotificationEmailId]
                ALTER TABLE [_com_bemaservices_RoomManagement_ReservationType] DROP COLUMN NotificationEmailId;
                ALTER TABLE [_com_bemaservices_RoomManagement_ReservationType] DROP COLUMN IsCommunicationHistorySaved;
                " );

            RockMigrationHelper.UpdateFieldType( "Reservation Type", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationTypeFieldType", "1AF29883-3028-4867-8DC7-0848953E8B6C" );
            RockMigrationHelper.UpdateFieldType( "Resource", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ResourceFieldType", "7CFF9796-C8A1-4544-A90C-9CA0C07C27D6" );

            AddReservationCreationWorkflowActions();

            SpecialApprovalNotification();
            ApprovalProcess();

            RockMigrationHelper.UpdateWorkflowType( false, false, "(Depreciated) Room Reservation Approval Notification", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "543D4FCD-310B-4048-BFCB-BAE582CBB890", 0 ); // (Depreciated) Room Reservation Approval Notification
            RockMigrationHelper.UpdateWorkflowType( false, true, "Reminder Notification", "Used for sending a reminder email to the event contact regarding their upcoming resource reservation.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Reservation Reminders", "fa fa-list-ol", 28800, true, 0, "A219357D-4992-415E-BF5F-33C242BB3BD2", 0 ); // Reminder Notification
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% assign Reservation = Workflow | Attribute: 'Reservation', 'Object' %}

{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    We wanted to remind you about your upcoming scheduled reservation:<br/>
</p>

<table border='0' cellpadding='2' cellspacing='0' width='600' id='emailContainer'>
    <tr>
        <td align='left' valign='top' width='100'>Name:</td>
        <td>{{ Reservation.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Event Contact:</td>
        <td><b>{{ Reservation.EventContactPersonAlias.Person.FullName }} ({{ Reservation.EventContactEmail }})</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Administrative Contact:</td>
        <td><b>{{ Reservation.AdministrativeContactPersonAlias.Person.FullName }} ({{ Reservation.AdministrativeContactEmail }})</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Entered By:</td>
        <td><b>{{ Reservation.RequesterAlias.Person.FullName }}</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Campus:</td>
        <td>{{ Reservation.Campus.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Ministry:</td>
        <td>{{ Reservation.ReservationMinistry.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Number Attending:</td>
        <td>{{ Reservation.NumberAttending }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Schedule:</td>
        <td>{{reservation.FriendlyReservationTime}}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Setup Time:</td>
        <td>{{ Reservation.SetupTime }} min</td>
    </tr>
    
    <tr>
        <td align='left' valign='top'>Cleanup Time:</td>
        <td>{{ Reservation.CleanupTime }} min</td>
    </tr>
</table>

<p>&nbsp;</p>
{% assign locationSize = Reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Location(s): <b>{% assign firstLocation = Reservation.ReservationLocations | First %}{% for location in Reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}</b><br/>{% endif %}
{% assign resourceSize = Reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resource(s): <b>{% assign firstResource = Reservation.ReservationResources | First %}{% for resource in Reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}</b><br/>{% endif %}

<br/><br/>

{% if Reservation.Note and Reservation.Note != '' %}
    <p>
        Notes: {{ Reservation.Note }}<br/>
    </p>
{% endif %}

<!-- The button to view the reservation -->
<table>
    <tr>
        <td style='background-color: #ee7624;border-color: #e76812;border: 2px solid #e76812;padding: 10px;text-align: center;'>
            <a style='display: block;color: #ffffff;font-size: 12px;text-decoration: none;' href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{Reservation.Id}}'>
                View Reservation
            </a>
        </td>
    </tr>
</table>
<br/>
<br/>

{{ 'Global' | Attribute:'EmailFooter' }}
            " );

            RockMigrationHelper.UpdateWorkflowType( false, true, "Post-Approval Modification Process", "A workflow that changes the reservation's approval status if it was modified by someone not in the Final Approval Group after being approved.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Update", "fa fa-list-ol", 28800, false, 0, "13D0361C-0552-43CA-8F27-D47DB120608D", 0 ); // Post-Approval Modification Process
            RockMigrationHelper.UpdateWorkflowActionType( "4CBF518F-AB76-4C67-9B70-597CCDABEEBA", "Send Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "52A9B3E2-0DA0-4972-9172-F5C62ECC8076", 8, "True", "40190F7C-DDC6-4F2D-8E6F-E604B5CA273C" ); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email
            RockMigrationHelper.AddActionTypeAttributeValue( "40190F7C-DDC6-4F2D-8E6F-E604B5CA273C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your final approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Body

            Sql( @"
                    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] Where [Guid] in ('5339e1c4-ac09-4bd5-9416-628dba200ba5','68f6de62-cdbb-4ec0-8440-8b1740c21e65')
                    DECLARE @WorkflowId int = (Select Top 1 Id From WorkflowType Where Guid = '83907883-4803-4AFB-8A20-49FDC0BE4788')
                    INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId],[ReservationTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId,1, 2, N'|||', CAST(N'2020-10-23 14:02:11.953' AS DateTime), CAST(N'2020-10-23 14:02:11.953' AS DateTime), NULL, NULL, N'68F6DE62-CDBB-4EC0-8440-8B1740C21E65', NULL, NULL, NULL)
            " );

            Sql( @"
            ALTER TABLE [_com_bemaservices_RoomManagement_Resource] Alter COLUMN Quantity [int] NULL;
            ALTER TABLE [_com_bemaservices_RoomManagement_ReservationResource] Alter COLUMN Quantity [int] NULL;
                " );

            // Attrib Value for Block:Calendar Item Campus List, Attribute:core.CustomGridColumnsConfig Page: Event Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "46647F7B-2DC3-43FF-88F7-962F516FA969", @"{""ColumnsConfig"":[{""HeaderText"":""Reservations"",""HeaderClass"":"""",""ItemClass"":"""",""LavaTemplate"":""{% reservationlinkage where:'EventItemOccurrenceId == {{Row.Id}}' %}\n    {% for reservationLinkage in reservationlinkageItems %}\n        {% reservation id:'{{reservationLinkage.ReservationId}}' %}\n                <small><a href='/ReservationDetail?ReservationId={{reservation.Id}}'>{{reservation.Name}}</a></small></br>\n        {% endreservation %}\n    {% endfor %}\n{% endreservationlinkage %}"",""PositionOffsetType"":1,""PositionOffset"":2}]}" );

        }

        /// <summary>
        /// Adds the reservation creation workflow actions.
        /// </summary>
        private void AddReservationCreationWorkflowActions()
        {
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation", "514493E9-4688-4926-9BCB-B945C8722578", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource", "2441F4FC-3812-4511-9E55-6BA46141D767", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation", "36B233BE-A202-4D58-B1AE-00A49EC20D44", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C8AC59C8-681A-4050-A9EC-4CB7B700E4AB" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "5AD0666A-C616-48A8-A97A-EAE9B68EAA4E" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Resource Attribute", "ResourceAttribute", "The attribute that contains the resource.", 1, @"", "2166489E-0975-40B8-AC1B-02BB1F0495BD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Resource Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Quantity|Attribute Value", "Quantity", "The quantity or an attribute that contains the quantity of the resource. <span class='tip tip-lava'></span>", 0, @"", "1FB9F74A-C771-49CE-86A2-96D3A5065ACD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Quantity|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "70F5C258-B07B-4866-9300-5407815D2A68" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "1AF29883-3028-4867-8DC7-0848953E8B6C", "Reservation Type", "ReservationType", "The reservation type to use (if Reservation Type Attribute is not specified).", 2, @"", "97C5C582-FA66-4E8C-B93A-6BA4562378E4" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "EB74366A-FF16-4303-9913-137AC222B2AD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 3, @"", "B7AECA5C-43E9-4FD6-A281-F15A89E10AFD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The reservation attribute to set the value to the reservation created.", 6, @"", "156DEE15-53E0-4D29-8788-1984CC371664" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Type Attribute", "ReservationTypeAttribute", "The attribute that contains the reservation type of the reservation.", 1, @"", "5F754B62-49C5-4CD5-80F3-A58A0E5F5A31" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Reservation Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Schedule Attribute", "ScheduleAttribute", "The attribute that contains the reservation schedule.", 5, @"", "3332A18E-D8ED-4233-B5A4-02F79F04C7AD" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Schedule Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Name|Attribute Value", "Name", "The name or an attribute that contains the name of the reservation. <span class='tip tip-lava'></span>", 0, @"", "A2B703AC-9CDE-4501-BDA8-F4D01A93AB2B" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "63A7ECDC-5AE4-4CEF-AB9F-BE0A27DC7687" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36B233BE-A202-4D58-B1AE-00A49EC20D44", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 4, @"", "512BFEBD-64FF-40B7-BF23-2039285B9E2B" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.CreateReservation:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "7BB104D9-35AC-431B-9359-E4721BF3CF03" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Location Attribute", "LocationAttribute", "The attribute that contains the location.", 1, @"", "62CC3DA8-C6F8-4535-9102-B4C32C9191C7" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Location Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "6F416249-8B9A-4002-9A2D-4A500235BEC9" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "514493E9-4688-4926-9BCB-B945C8722578", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5EB174ED-4401-4E46-AABD-4D56C94FEC13" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationLocation:Order
        }

        /// <summary>
        /// Specials the approval notification.
        /// </summary>
        private void SpecialApprovalNotification()
        {
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "E3667110-339F-4FE3-B6B7-084CF9633580" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Special Approval Notification

            RockMigrationHelper.UpdateWorkflowType( false, true, "Special Approval Notification", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Work", "fa fa-list-ol", 28800, true, 0, "66899922-D665-4839-8742-BD8556D7FB61", 0 ); // Special Approval Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Approval Group", "ApprovalGroup", "", 0, @"", "8F0172A6-3C38-4B62-AD4B-76AEBF19905F", false ); // Special Approval Notification:Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 1, @"", "C39F0BCB-A832-4D99-ACC1-568C8C6BA202", false ); // Special Approval Notification:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "66899922-D665-4839-8742-BD8556D7FB61", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Relevant Item", "RelevantItem", "", 2, @"", "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", false ); // Special Approval Notification:Relevant Item
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "ispassword", @"False", "C7183398-3221-4AE3-9312-2D4C930D724E" ); // Special Approval Notification:Relevant Item:ispassword
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "maxcharacters", @"", "82AE881A-AEE2-4172-B27C-FE12E7950B01" ); // Special Approval Notification:Relevant Item:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "B8F2580D-64F1-4610-9E93-6F0DE2CEFAFB", "showcountdown", @"False", "78A68997-C5D2-41A1-AA53-9F6BA8F82E3C" ); // Special Approval Notification:Relevant Item:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "66899922-D665-4839-8742-BD8556D7FB61", true, "Start", "", true, 0, "E92F7E39-7C7B-45E4-97BA-7ECD197E7642" ); // Special Approval Notification:Start
            RockMigrationHelper.UpdateWorkflowActionType( "E92F7E39-7C7B-45E4-97BA-7ECD197E7642", "Send Notification Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "F07AA85B-B666-4D8C-B770-7B5377A9EF34" ); // Special Approval Notification:Start:Send Notification Email
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Special Approval Notification:Start:Send Notification Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "0C4C13B8-7076-4872-925A-F950886B5E16", @"8f0172a6-3c38-4b62-ad4b-76aebf19905f" ); // Special Approval Notification:Start:Send Notification Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Special Approval Needed: {{ Workflow | Attribute:'RelevantItem'}} for {{Workflow | Attribute:'Reservation'}}" ); // Special Approval Notification:Start:Send Notification Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your special approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Reservation</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Special Approval Notification:Start:Send Notification Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Special Approval Notification:Start:Send Notification Email:Save Communication History

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'         
            And ( Select top 1 Id from AttributeQualifier aq1 Where aq1.[Key] = 'definedtype' and aq1.AttributeId = aq.AttributeId ) is null
		" );

            #endregion        }
        }

        /// <summary>
        /// Approvals the process.
        /// </summary>
        private void ApprovalProcess()
        {
            #region FieldTypes

            RockMigrationHelper.UpdateFieldType( "Reservation Location Approval State", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationLocationApprovalStateFieldType", "CAA46A41-583F-420F-AB2D-10B7D4B57828" );
            RockMigrationHelper.UpdateFieldType( "Reservation Resource Approval State", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationResourceApprovalStateFieldType", "F42935CE-9676-4C72-8664-C291C2965C5B" );

            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState", "3894452A-E763-41AC-8260-10373646D8A0", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates", "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates", "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.Delay", "D22E73F7-86E2-46CA-AD5B-7770A866726B", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetEntityProperty", "C545211C-1143-498E-8B3A-FEE9D59C7C96", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetWorkflowName", "36005473-BD5D-470B-B28D-98E6D7ED808D", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0A800013-51F7-4902-885A-5BE215D67D3D" ); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5D95C15A-CCAE-40AD-A9DD-F929DA587115" ); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "ACA008E2-2406-457E-8E4C-6922E03757A4" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 1, @"", "2E185FB5-FC8E-41BE-B7FE-702F74B47539" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "1D4F819F-145D-4A7F-AB4E-AD7C06759042" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25954FDC-F486-417D-ABBB-E2DF2C67B186" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ignore Locations With Approval Groups", "IgnoreLocationsWithApprovalGroups", "Whether to skip updating the statuses of locations with approval groups", 3, @"True", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Ignore Locations With Approval Groups
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation locations' approval state.", 1, @"", "26E349B1-61EF-441F-8700-A19E9FA7BCED" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "272F7FD1-ABC9-4B58-94D8-7973A0146330" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", "CAA46A41-583F-420F-AB2D-10B7D4B57828", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "53B06E3D-22CD-488B-86D3-4C4D81511334" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationLocationsApprovalStates:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "E3667110-339F-4FE3-B6B7-084CF9633580" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "B524B00C-29CB-49E9-9896-8BB60F209783" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Ignore Resources With Approval Groups", "IgnoreResourcesWithApprovalGroups", "Whether to skip updating the statuses of resources with approval groups", 3, @"True", "3CA5B94F-5571-497C-85EF-2586B62BDFC2" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Ignore Resources With Approval Groups
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation resources' approval state.", 1, @"", "1F7A0B12-5EA3-41AF-913D-F45659E28D59" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3BE60722-BE2B-4DE5-8CC7-FD3F90EC2373" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", "F42935CE-9676-4C72-8664-C291C2965C5B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "0E07B013-2C74-4CD7-AFBC-C535151AA60A" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationResourcesApprovalStates:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FCA61786-8EC0-44D7-8A3D-152721FF2353" ); // Rock.Workflow.Action.SetEntityProperty:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "8373A55C-E023-4DE0-B583-06FF906520FC" ); // Rock.Workflow.Action.SetEntityProperty:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "913D7A95-BC44-4874-92F9-66DB85DF9FEF" ); // Rock.Workflow.Action.SetEntityProperty:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Name|Property Name Attribute", "PropertyName", "The name of the property to set. <span class='tip tip-lava'></span>", 2, @"", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5" ); // Rock.Workflow.Action.SetEntityProperty:Property Name|Property Name Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Value|Property Value Attribute", "PropertyValue", "The value to set. <span class='tip tip-lava'></span>", 3, @"", "0415C959-BF89-4D19-9C47-3AB1098E1FBA" ); // Rock.Workflow.Action.SetEntityProperty:Property Value|Property Value Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Empty Value Handling", "EmptyValueHandling", "How to handle empty property values.", 4, @"", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C" ); // Rock.Workflow.Action.SetEntityProperty:Empty Value Handling
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C545211C-1143-498E-8B3A-FEE9D59C7C96", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "22EF2F99-C277-4D3A-A779-E5D4D71D28C5" ); // Rock.Workflow.Action.SetEntityProperty:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D916CB65-9413-479F-8F5E-6E599CE48025" ); // Rock.Workflow.Action.Delay:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Date In Attribute", "DateInAttribute", "The date or date/time attribute value to use for the delay.", 1, @"", "55F1DD31-6F42-464F-A9A1-2B9484C07AB4" ); // Rock.Workflow.Action.Delay:Date In Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Next Weekday", "NextWeekday", "The next day of the week to wait till.", 2, @"", "8AE43CB6-2DF9-4DE0-856A-98F846357274" ); // Rock.Workflow.Action.Delay:Next Weekday
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minutes To Delay", "MinutesToDelay", "The number of minutes to delay successful execution of action", 0, @"", "3C501BE2-FE9E-479D-BC59-8F3B72FF6E4A" ); // Rock.Workflow.Action.Delay:Minutes To Delay
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "D22E73F7-86E2-46CA-AD5B-7770A866726B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "77AA4791-AD54-4944-9827-2997BA3B1ED9" ); // Rock.Workflow.Action.Delay:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "385A255B-9F48-4625-862B-26231DBAC53A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Approval Process

            RockMigrationHelper.UpdateWorkflowType( false, true, "Approval Process", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "83907883-4803-4AFB-8A20-49FDC0BE4788", 0 ); // Approval Process
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Approval State", "ApprovalState", "", 0, @"", "680C5D16-3D16-4DC7-811B-92E272332F0C", true ); // Approval Process:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Initial Approval Group", "InitialApprovalGroup", "An optional group that gives initial approval for reservations.", 1, @"", "F48113BA-060A-4BC2-ACD4-56949A32694D", false ); // Approval Process:Initial Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "An optional group that gives final approval for reservations.", 2, @"", "72993462-A4D1-405F-BE5B-A57625EDEA70", false ); // Approval Process:Final Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 3, @"", "E21D220B-8251-417C-9C8E-91AFD1C677F9", true ); // Approval Process:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "", 4, @"", "071BDAC1-6333-47B1-A376-24D97EAA9326", false ); // Approval Process:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Admin Contact", "AdminContact", "", 5, @"", "35AE460C-60EB-4DC5-98E9-B77B7C8A181C", false ); // Approval Process:Admin Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Event Contact", "EventContact", "", 6, @"", "137CB45C-A1EC-4293-A6F6-0DCFDC40A7C8", false ); // Approval Process:Event Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Initial Approval Date Time", "InitialApprovalDateTime", "", 7, @"", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", false ); // Approval Process:Initial Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Special Approval Date Time", "SpecialApprovalDateTime", "", 8, @"", "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", false ); // Approval Process:Special Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Final Approval Date Time", "FinalApprovalDateTime", "", 9, @"", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", false ); // Approval Process:Final Approval Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Groups Notified", "GroupsNotified", "", 10, @"", "3A7EC907-D5D4-4DDA-B616-904B17570636", false ); // Approval Process:Groups Notified
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "83907883-4803-4AFB-8A20-49FDC0BE4788", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Initial Special Approval Reminders Sent", "InitialSpecialApprovalRemindersSent", "", 11, @"False", "98EF8E62-8094-4799-A7CE-A5D4713E9372", false ); // Approval Process:Initial Special Approval Reminders Sent
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "ispassword", @"False", "C7AC0806-97D3-4275-8E52-C04F8CC00EA2" ); // Approval Process:Approval State:ispassword
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "maxcharacters", @"", "CA8A492B-221E-4A3B-A3E7-263C05A70FC2" ); // Approval Process:Approval State:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "680C5D16-3D16-4DC7-811B-92E272332F0C", "showcountdown", @"False", "AA0CA17A-5AEA-4D3D-8AA3-CF160E2B3512" ); // Approval Process:Approval State:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "071BDAC1-6333-47B1-A376-24D97EAA9326", "EnableSelfSelection", @"False", "B236A9B4-6A25-447F-ACE0-518160A2E6BC" ); // Approval Process:Requester:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "35AE460C-60EB-4DC5-98E9-B77B7C8A181C", "EnableSelfSelection", @"False", "6EBDBB50-F712-4A9B-8B63-9F7FF488198E" ); // Approval Process:Admin Contact:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "137CB45C-A1EC-4293-A6F6-0DCFDC40A7C8", "EnableSelfSelection", @"False", "F52A1F93-3BA4-4A5D-BB00-0B9413774748" ); // Approval Process:Event Contact:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "datePickerControlType", @"Date Picker", "4A874E1E-9A67-480C-8FAA-29E9E994854A" ); // Approval Process:Initial Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "displayCurrentOption", @"False", "331401B5-B968-479F-B8AE-480825FE66D1" ); // Approval Process:Initial Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "displayDiff", @"False", "A74B7301-D688-482C-8A26-207D5EBEAC7C" ); // Approval Process:Initial Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "format", @"", "5352AB50-6320-4B27-A6D0-FF46281CFF4A" ); // Approval Process:Initial Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", "futureYearCount", @"", "C3656E00-74C8-4F33-8012-6B7E1E5CCD20" ); // Approval Process:Initial Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "datePickerControlType", @"Date Picker", "D36E26DE-8D64-4C53-AC6F-8D698F6FB7DF" ); // Approval Process:Special Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "displayCurrentOption", @"False", "155B57A3-55B1-4CB1-8733-A27822D5F499" ); // Approval Process:Special Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "displayDiff", @"False", "90D8FD47-0657-4EB9-A260-CA1C2C5701CC" ); // Approval Process:Special Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "format", @"", "A30E03F0-60D7-45F2-BD82-C0E106F2D661" ); // Approval Process:Special Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", "futureYearCount", @"", "0C94D6C9-EE66-4DD5-A773-CC70EA0B05A3" ); // Approval Process:Special Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "datePickerControlType", @"Date Picker", "095711E4-3099-40BC-8736-1AC85C695140" ); // Approval Process:Final Approval Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "displayCurrentOption", @"False", "F98ACEC6-AB17-4100-8125-56160D4918A0" ); // Approval Process:Final Approval Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "displayDiff", @"False", "1FBA4188-C9A5-439A-B549-ABADD236ED8D" ); // Approval Process:Final Approval Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "format", @"", "AAA55C4A-DB06-4FDE-934A-4C280BBC7B61" ); // Approval Process:Final Approval Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "43301CCE-CFB2-4CB0-839D-3B9E9006D159", "futureYearCount", @"", "6270C19D-BD01-4B8C-8A4B-CA312EA24047" ); // Approval Process:Final Approval Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "ispassword", @"False", "108CACE6-E221-47E4-BF74-9DD96661ED9F" ); // Approval Process:Groups Notified:ispassword
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "maxcharacters", @"", "B67376A5-24A7-4FE6-901C-77D538D1A521" ); // Approval Process:Groups Notified:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "3A7EC907-D5D4-4DDA-B616-904B17570636", "showcountdown", @"False", "3ED881CA-96EE-4D25-8278-144282DE6BFF" ); // Approval Process:Groups Notified:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "BooleanControlType", @"0", "F5CC1F87-8CBB-4B83-BB77-0AB297E4362C" ); // Approval Process:Initial Special Approval Reminders Sent:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "falsetext", @"No", "4CBEBDB1-84C0-4AE9-B8A0-DDA184072BA1" ); // Approval Process:Initial Special Approval Reminders Sent:falsetext
            RockMigrationHelper.AddAttributeQualifier( "98EF8E62-8094-4799-A7CE-A5D4713E9372", "truetext", @"Yes", "EF6EEDE5-9FC0-414E-A241-BEC3419D8F5D" ); // Approval Process:Initial Special Approval Reminders Sent:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Set Attributes and Launch State Activity", "", true, 0, "7DE18C65-E54F-4159-86C1-EB9B7AB9733B" ); // Approval Process:Set Attributes and Launch State Activity
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Draft", "", false, 1, "1E06F3F0-0C81-414F-B759-DCF21941E286" ); // Approval Process:Draft
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Initial Approval", "", false, 2, "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04" ); // Approval Process:Pending Initial Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Special Approval", "", false, 3, "C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Pending Special Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Pending Final Approval", "", false, 4, "7D8E5A78-E443-4657-9B10-39F9F0ADCF15" ); // Approval Process:Pending Final Approval
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Approved", "", false, 5, "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64" ); // Approval Process:Approved
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Changes Needed", "", false, 6, "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7" ); // Approval Process:Changes Needed
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Denied", "", false, 7, "7272A2DF-926C-46E1-87A9-DCD4BC992C67" ); // Approval Process:Denied
            RockMigrationHelper.UpdateWorkflowActivityType( "83907883-4803-4AFB-8A20-49FDC0BE4788", true, "Cancelled", "", false, 8, "1ACA9FB8-4792-4A4E-9EB8-8489E300550E" ); // Approval Process:Cancelled
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Delay Activated", "acd421c1-0f01-4d3f-8d49-c1afc8a81adb", "", 0, @"", "09A25CB0-F0A7-4FBE-BE06-44337D4D096F" ); // Approval Process:Pending Special Approval:Delay Activated
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Initial Approval Group From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "B37587B4-EECE-479B-BE13-FC7188395CC0" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Final Approval Group from Entity", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "BB592021-5DC7-4E43-B524-33B018BDDBE6" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Reservation From Entity", 2, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Event Contact from Entity", 3, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "230AD55E-0439-4952-80B2-7D0D6A94AAFE" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Admin Contact From Entity", 4, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "34AE3A26-0475-4BF5-8111-C1248EF8DC01" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Approval State From Entity", 5, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "E432E227-E78F-4325-A94B-AA4791623C0C" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Initial Approval Date Time", 6, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "9545A775-4307-4E90-B88F-EFF33BB53557" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Special Approval Date Time", 7, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "245EF97F-6E16-4B9E-B803-5648AA3B7E07" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Final Approval Date Time", 8, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Set Workflow Name", 9, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "2D61751F-BA93-4DA3-B348-1CB81F25E7EA" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Draft Activity", 10, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Draft", "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Initial Approval Activity", 11, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingInitialApproval", "D8219216-2053-4456-BF91-A79867ADDE5F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Special Approval Activity", 12, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingSpecialApproval", "3385A369-28C9-45A5-86D6-8A10A36F81E4" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Pending Final Approval Activity", 13, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "PendingFinalApproval", "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Approved Activity", 14, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Approved", "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Requires Changes Activity", 15, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "ChangesNeeded", "0635F211-92CD-492F-A932-CEF30CDA5DB3" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Denied Activity", 16, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Denied", "0A40E2A1-0C12-49E2-A02A-69645678C031" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Activate Cancelled Activity", 17, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "680C5D16-3D16-4DC7-811B-92E272332F0C", 1, "Cancelled", "6AF8CB0D-09FE-4D15-B70E-828670B70A72" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7DE18C65-E54F-4159-86C1-EB9B7AB9733B", "Complete Workflow if No Matching Approval State", 18, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "0E6829B8-E61A-44EF-96D7-1713C51F178F" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State
            RockMigrationHelper.UpdateWorkflowActionType( "1E06F3F0-0C81-414F-B759-DCF21941E286", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "AF3DBE3B-C511-4970-BB66-8B438F9028E9" ); // Approval Process:Draft:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Set Reservation to Pending Special Approval If No Initial Approval Group", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 32, "", "19CB34A3-C5E0-453A-8DE1-7BFE43648439" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Activate Pending Special Approval Activity If No Initial Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 32, "", "E1A0E981-B206-4F68-800A-616CC44B51B0" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Set Reservation to Pending Special Approval If Already Approved", 2, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", 64, "", "284044DD-DD22-43CB-A540-A7297C89843D" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Activate Pending Special Approval Activity If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "B85C25FD-2A6D-4402-9F86-25D9C3179A8E", 64, "", "DC2E6CC2-DF0F-4600-9687-9911A35698DC" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Send Email", 4, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "F48113BA-060A-4BC2-ACD4-56949A32694D", 64, "", "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2" ); // Approval Process:Pending Initial Approval:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04", "Complete Workflow", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "F8F6ECEA-728F-4ABB-B02A-D69D45B09388" ); // Approval Process:Pending Initial Approval:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Delay 14 Days if Notifications Already Sent", 0, "D22E73F7-86E2-46CA-AD5B-7770A866726B", true, false, "", "98EF8E62-8094-4799-A7CE-A5D4713E9372", 1, "Yes", "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "39FBCAFE-4594-4D5A-9111-C37E8011C76C" ); // Approval Process:Pending Special Approval:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Location States", 2, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "664A5031-3B33-4FD8-9BBA-C16E5A644ADF" ); // Approval Process:Pending Special Approval:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Send Notifications to Any Special Approval Groups", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Mark Initial Notifications as Sent", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Complete Workflow If Approval State Has Changed Since Activity Started", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 512, "0", "E9EA7248-69AD-4C71-9A90-342E55D2739A" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Activate Send Special Approval Reminders if Any Special Approval Groups", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 128, "0", "C7A536B5-DE10-4B0E-B02B-3C02C8306C04" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Special Approval Date If Blank", 7, "C545211C-1143-498E-8B3A-FEE9D59C7C96", true, false, "", "A2B592BA-9C64-4FAF-AADD-DF2D12D12989", 32, "", "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Set Reservation to Pending Final Approval If No Special Approval Groups", 8, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "665A49A2-5ED1-4994-A348-CDD6F9CC184A" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "C21EF7B1-3B5C-4820-B123-F9241E206E27", "Activate Pending Final Approval Activity If No Special Approval Groups", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "3A7EC907-D5D4-4DDA-B616-904B17570636", 1, "0", "645B7181-936B-460C-B577-C4441781FA04" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Set Reservation to Approved If No Final Approval Group", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 32, "", "4B4BD44D-A122-44CF-B097-458E2DE91E78" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Activate Approved Activity If No Final Approval Group", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 32, "", "F66AB76B-78E2-4651-A10A-C7D60E37869B" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Set Reservation to Approved If Already Approved", 2, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", 64, "", "2B7E16A5-3702-4129-8FD4-702CFCCD7F18" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Activate Approved Activity If Already Approved", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "43301CCE-CFB2-4CB0-839D-3B9E9006D159", 64, "", "32B47E6E-3CEC-48CB-B074-D78CDE283CDD" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Send Email", 4, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "E13B02B1-0BA4-4F23-AE58-01E822406522" ); // Approval Process:Pending Final Approval:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "7D8E5A78-E443-4657-9B10-39F9F0ADCF15", "Complete Workflow", 5, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "72993462-A4D1-405F-BE5B-A57625EDEA70", 64, "", "1EEE73A3-7E3A-4FCE-B241-827FB929AF89" ); // Approval Process:Pending Final Approval:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Set Location States", 0, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "20A1F5F6-9612-4BE1-B2B6-94A2424AC060" ); // Approval Process:Approved:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "EEB50C19-FF94-408E-A456-4B875655A23E" ); // Approval Process:Approved:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Send Email to Admin Contact", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "AFA4DBFB-2581-4004-AA01-6FF621C4725C" ); // Approval Process:Approved:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Send Email to Event Contact", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "969CD77B-022A-4518-A0A6-2C19D3ED3A65" ); // Approval Process:Approved:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "F5ECAD7E-8855-4249-A3BB-EFBF626FBA64", "Complete Workflow", 4, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80" ); // Approval Process:Approved:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Send Email to Admin Contact", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "9DB9F652-5EA4-483E-B922-3530D1188379" ); // Approval Process:Changes Needed:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Send Email to Event Contact", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "A31A317A-8F98-4D94-866A-C56505BF6756" ); // Approval Process:Changes Needed:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "A9068051-4EE2-4529-9C06-7EFC7E0EF7D7", "Complete Workflow", 2, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "6A1E6886-D4BA-48C2-A860-D067AADAC8CE" ); // Approval Process:Changes Needed:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Set Location States", 0, "5D0E4F02-A39B-49DB-AC53-BEF45E4AF8E3", true, false, "", "", 1, "", "5336AC78-3B7D-4229-99B0-09E8D43ED058" ); // Approval Process:Denied:Set Location States
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Set Resource States", 1, "A87C07F7-8E94-4BC5-96BF-40B817EDC0AC", true, false, "", "", 1, "", "69F1D448-C818-4A9F-B01D-C136D6D9275A" ); // Approval Process:Denied:Set Resource States
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Send Email to Admin Contact", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4" ); // Approval Process:Denied:Send Email to Admin Contact
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Send Email to Event Contact", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "79551640-080B-44F9-A00B-1AFB1A6D3AD7" ); // Approval Process:Denied:Send Email to Event Contact
            RockMigrationHelper.UpdateWorkflowActionType( "7272A2DF-926C-46E1-87A9-DCD4BC992C67", "Complete Workflow", 4, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "22043E5A-45F7-41C4-93AF-9308B1222291" ); // Approval Process:Denied:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "1ACA9FB8-4792-4A4E-9EB8-8489E300550E", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E" ); // Approval Process:Cancelled:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"f48113ba-060a-4bc2-acd4-56949a32694d" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "B37587B4-EECE-479B-BE13-FC7188395CC0", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.ReservationType.InitialApprovalGroup.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Group From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"72993462-a4d1-405f-be5b-a57625edea70" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "BB592021-5DC7-4E43-B524-33B018BDDBE6", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.ReservationType.FinalApprovalGroup.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Group from Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "04BA3EA4-BD2E-4A71-AE46-E1B910AF82E6", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "230AD55E-0439-4952-80B2-7D0D6A94AAFE", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.EventContactPersonAlias.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Event Contact from Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "34AE3A26-0475-4BF5-8111-C1248EF8DC01", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.AdministrativeContactPersonAlias.Guid }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Admin Contact From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"680c5d16-3d16-4dc7-811b-92e272332f0c" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E432E227-E78F-4325-A94B-AA4791623C0C", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{% assign reservation =  Workflow | Attribute:'Reservation', 'object' %}{{ reservation.ApprovalState }}" ); // Approval Process:Set Attributes and Launch State Activity:Set Approval State From Entity:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"b85c25fd-2a6d-4402-9f86-25d9c3179a8e" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9545A775-4307-4E90-B88F-EFF33BB53557", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{{ Workflow | Attribute:'Reservation','InitialApprovalDateTime'}}" ); // Approval Process:Set Attributes and Launch State Activity:Set Initial Approval Date Time:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"a2b592ba-9c64-4faf-aadd-df2d12d12989" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "245EF97F-6E16-4B9E-B803-5648AA3B7E07", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{{ Workflow | Attribute:'Reservation','SpecialApprovalDateTime'}}" ); // Approval Process:Set Attributes and Launch State Activity:Set Special Approval Date Time:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"43301cce-cfb2-4cb0-839d-3b9e9006d159" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "55AF39A6-51E8-4D5A-95B6-741AA8CBFDF3", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{{ Workflow | Attribute:'Reservation','FinalApprovalDateTime'}}" ); // Approval Process:Set Attributes and Launch State Activity:Set Final Approval Date Time:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "2D61751F-BA93-4DA3-B348-1CB81F25E7EA", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2D61751F-BA93-4DA3-B348-1CB81F25E7EA", "93852244-A667-4749-961A-D47F88675BE4", @"{{Workflow | Attribute:'Reservation','Name'}} (ID:{{Workflow | Attribute:'Reservation','Id'}}): {{Workflow | Attribute:'ApprovalState'}}" ); // Approval Process:Set Attributes and Launch State Activity:Set Workflow Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1CC7342E-CD69-4CC7-9BBC-FA99BAFA014F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"1E06F3F0-0C81-414F-B759-DCF21941E286" ); // Approval Process:Set Attributes and Launch State Activity:Activate Draft Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "D8219216-2053-4456-BF91-A79867ADDE5F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D8219216-2053-4456-BF91-A79867ADDE5F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BAEBEF3F-6F04-41B1-A361-F9DF81C7AB04" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Initial Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3385A369-28C9-45A5-86D6-8A10A36F81E4", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3385A369-28C9-45A5-86D6-8A10A36F81E4", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Special Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "730125CC-0A1E-4CE5-B782-C4EC50CBAC8F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"7D8E5A78-E443-4657-9B10-39F9F0ADCF15" ); // Approval Process:Set Attributes and Launch State Activity:Activate Pending Final Approval Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C34FE8E8-9E7A-4AD5-823F-A2A7343EE4D4", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"F5ECAD7E-8855-4249-A3BB-EFBF626FBA64" ); // Approval Process:Set Attributes and Launch State Activity:Activate Approved Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0635F211-92CD-492F-A932-CEF30CDA5DB3", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0635F211-92CD-492F-A932-CEF30CDA5DB3", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"A9068051-4EE2-4529-9C06-7EFC7E0EF7D7" ); // Approval Process:Set Attributes and Launch State Activity:Activate Requires Changes Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0A40E2A1-0C12-49E2-A02A-69645678C031", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0A40E2A1-0C12-49E2-A02A-69645678C031", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"7272A2DF-926C-46E1-87A9-DCD4BC992C67" ); // Approval Process:Set Attributes and Launch State Activity:Activate Denied Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF8CB0D-09FE-4D15-B70E-828670B70A72", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF8CB0D-09FE-4D15-B70E-828670B70A72", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"1ACA9FB8-4792-4A4E-9EB8-8489E300550E" ); // Approval Process:Set Attributes and Launch State Activity:Activate Cancelled Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0E6829B8-E61A-44EF-96D7-1713C51F178F", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0E6829B8-E61A-44EF-96D7-1713C51F178F", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Set Attributes and Launch State Activity:Complete Workflow if No Matching Approval State:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AF3DBE3B-C511-4970-BB66-8B438F9028E9", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Draft:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AF3DBE3B-C511-4970-BB66-8B438F9028E9", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Draft:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "19CB34A3-C5E0-453A-8DE1-7BFE43648439", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"6" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If No Initial Approval Group:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "E1A0E981-B206-4F68-800A-616CC44B51B0", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E1A0E981-B206-4F68-800A-616CC44B51B0", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If No Initial Approval Group:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "284044DD-DD22-43CB-A540-A7297C89843D", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"6" ); // Approval Process:Pending Initial Approval:Set Reservation to Pending Special Approval If Already Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "DC2E6CC2-DF0F-4600-9687-9911A35698DC", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DC2E6CC2-DF0F-4600-9687-9911A35698DC", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Activate Pending Special Approval Activity If Already Approved:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Pending Initial Approval:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "0C4C13B8-7076-4872-925A-F950886B5E16", @"f48113ba-060a-4bc2-acd4-56949a32694d" ); // Approval Process:Pending Initial Approval:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Initial Approval Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Pending Initial Approval:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your initial approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Pending Initial Approval:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "F783EE15-AB52-49EA-AD16-BDCFABB1A2D2", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Pending Initial Approval:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "F8F6ECEA-728F-4ABB-B02A-D69D45B09388", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Initial Approval:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F8F6ECEA-728F-4ABB-B02A-D69D45B09388", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Initial Approval:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB", "D916CB65-9413-479F-8F5E-6E599CE48025", @"False" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "ACD421C1-0F01-4D3F-8D49-C1AFC8A81ADB", "3C501BE2-FE9E-479D-BC59-8F3B72FF6E4A", @"20160" ); // Approval Process:Pending Special Approval:Delay 14 Days if Notifications Already Sent:Minutes To Delay
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Pending Special Approval:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"2" ); // Approval Process:Pending Special Approval:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "39FBCAFE-4594-4D5A-9111-C37E8011C76C", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"True" ); // Approval Process:Pending Special Approval:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Pending Special Approval:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"2" ); // Approval Process:Pending Special Approval:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "664A5031-3B33-4FD8-9BBA-C16E5A644ADF", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"True" ); // Approval Process:Pending Special Approval:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign groupsNotified = 0 %}
{% assign reservationId = Workflow | Attribute:'Reservation','Id' %}
{% reservation id:'{{reservationId}}' %}
    {% if reservation.ApprovalState == 'PendingSpecialApproval' %}
        {% for reservationLocation in reservation.ReservationLocations %}
            {% if reservationLocation.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationLocation.Location | Attribute:'ApprovalGroup','Object' %}
                {% if approvalGroup != empty %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' ApprovalGroup:'{{approvalGroup.Guid}}' Reservation:'{{ reservation.Guid }}' RelevantItem:'{{reservationLocation.Location.Name}}' %}
                      {% assign groupsNotified = groupsNotified | Plus:1 %}
                    {% endworkflowactivate %}
                {% endif %}
            {% endif %}
        {% endfor %}
        {% for reservationResource in reservation.ReservationResources %}
            {% if reservationResource.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationResource.Resource.ApprovalGroup %}
                {% if approvalGroup != null %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' ApprovalGroup:'{{approvalGroup.Guid}}' Reservation:'{{ reservation.Guid }}' RelevantItem:'{{reservationResource.Resource.Name}}' %}
                      {% assign groupsNotified = groupsNotified | Plus:1 %}
                    {% endworkflowactivate %}
                {% endif %}
            {% endif %}
        {% endfor %}
    {% else %}
        {% assign groupsNotified = -1 %}
    {% endif %}
{% endreservation %}

{{groupsNotified}}" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"3a7ec907-d5d4-4dda-b616-904b17570636" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"All" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"98ef8e62-8094-4799-a7ce-a5d4713e9372" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D0D12B3F-A5C3-4DE4-9D10-B44AD4ED0573", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"Yes" ); // Approval Process:Pending Special Approval:Mark Initial Notifications as Sent:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E9EA7248-69AD-4C71-9A90-342E55D2739A", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E9EA7248-69AD-4C71-9A90-342E55D2739A", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Special Approval:Complete Workflow If Approval State Has Changed Since Activity Started:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C7A536B5-DE10-4B0E-B02B-3C02C8306C04", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C7A536B5-DE10-4B0E-B02B-3C02C8306C04", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"C21EF7B1-3B5C-4820-B123-F9241E206E27" ); // Approval Process:Pending Special Approval:Activate Send Special Approval Reminders if Any Special Approval Groups:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"SpecialApprovalDateTime" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "0415C959-BF89-4D19-9C47-3AB1098E1FBA", @"{{ 'Now' | Date }}" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Property Value|Property Value Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BFCBF7B6-5FD8-446B-BB58-D1549DCA40B4", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"IGNORE" ); // Approval Process:Pending Special Approval:Set Special Approval Date If Blank:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "665A49A2-5ED1-4994-A348-CDD6F9CC184A", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"5" ); // Approval Process:Pending Special Approval:Set Reservation to Pending Final Approval If No Special Approval Groups:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "645B7181-936B-460C-B577-C4441781FA04", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "645B7181-936B-460C-B577-C4441781FA04", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Special Approval:Activate Pending Final Approval Activity If No Special Approval Groups:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4B4BD44D-A122-44CF-B097-458E2DE91E78", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If No Final Approval Group:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "F66AB76B-78E2-4651-A10A-C7D60E37869B", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F66AB76B-78E2-4651-A10A-C7D60E37869B", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Activate Approved Activity If No Final Approval Group:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2B7E16A5-3702-4129-8FD4-702CFCCD7F18", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Approval Process:Pending Final Approval:Set Reservation to Approved If Already Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "32B47E6E-3CEC-48CB-B074-D78CDE283CDD", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "32B47E6E-3CEC-48CB-B074-D78CDE283CDD", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Activate Approved Activity If Already Approved:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Pending Final Approval:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "0C4C13B8-7076-4872-925A-F950886B5E16", @"72993462-a4d1-405f-be5b-a57625edea70" ); // Approval Process:Pending Final Approval:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Final Approval Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Pending Final Approval:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your final approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Pending Final Approval:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "E13B02B1-0BA4-4F23-AE58-01E822406522", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Pending Final Approval:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "1EEE73A3-7E3A-4FCE-B241-827FB929AF89", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Pending Final Approval:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1EEE73A3-7E3A-4FCE-B241-827FB929AF89", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Pending Final Approval:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Approved:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Approved:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"2" ); // Approval Process:Approved:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "20A1F5F6-9612-4BE1-B2B6-94A2424AC060", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"False" ); // Approval Process:Approved:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Approved:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Approved:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"2" ); // Approval Process:Approved:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "EEB50C19-FF94-408E-A456-4B875655A23E", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"False" ); // Approval Process:Approved:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Approved:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Approved:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Approved: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Approved:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been approved:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Approved:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "AFA4DBFB-2581-4004-AA01-6FF621C4725C", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Approved:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Approved:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "0C4C13B8-7076-4872-925A-F950886B5E16", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Approved:Send Email to Event Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Approved: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Approved:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been approved:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Approved:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "969CD77B-022A-4518-A0A6-2C19D3ED3A65", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Approved:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Approved:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9C0CCC1F-1E1F-4879-89E9-AA34B5DB9B80", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Approved:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Changes Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation requires changes:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "9DB9F652-5EA4-483E-B922-3530D1188379", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Changes Needed:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Changes Needed:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "0C4C13B8-7076-4872-925A-F950886B5E16", @"137cb45c-a1ec-4293-a6f6-0dcfdc40a7c8" ); // Approval Process:Changes Needed:Send Email to Event Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Changes Needed: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Changes Needed:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation requires changes:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {{reservation.FriendlyReservationTime}}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Changes Needed:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "A31A317A-8F98-4D94-866A-C56505BF6756", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Changes Needed:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "6A1E6886-D4BA-48C2-A860-D067AADAC8CE", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Changes Needed:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6A1E6886-D4BA-48C2-A860-D067AADAC8CE", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Changes Needed:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "D530F451-D1FA-416A-86F1-7E87FBCC4EC7", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Denied:Set Location States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "15A2EEA8-53DA-4ADD-9DDE-FF2FCA70023F", @"False" ); // Approval Process:Denied:Set Location States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "53B06E3D-22CD-488B-86D3-4C4D81511334", @"3" ); // Approval Process:Denied:Set Location States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "5336AC78-3B7D-4229-99B0-09E8D43ED058", "22FAA8A0-DE01-4402-B0A6-89A5C58B180A", @"False" ); // Approval Process:Denied:Set Location States:Ignore Locations With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "4FBD0A05-603C-4F7E-B122-2865F31A4AD0", @"e21d220b-8251-417c-9c8e-91afd1c677f9" ); // Approval Process:Denied:Set Resource States:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "C04B4BA1-65E6-493F-BD1B-834D6A1961E8", @"False" ); // Approval Process:Denied:Set Resource States:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "0E07B013-2C74-4CD7-AFBC-C535151AA60A", @"3" ); // Approval Process:Denied:Set Resource States:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "69F1D448-C818-4A9F-B01D-C136D6D9275A", "3CA5B94F-5571-497C-85EF-2586B62BDFC2", @"False" ); // Approval Process:Denied:Set Resource States:Ignore Resources With Approval Groups
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Denied:Send Email to Admin Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "0C4C13B8-7076-4872-925A-F950886B5E16", @"35ae460c-60eb-4dc5-98e9-b77b7c8a181c" ); // Approval Process:Denied:Send Email to Admin Contact:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Denied: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Denied:Send Email to Admin Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been denied:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Denied:Send Email to Admin Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "27DA6AF4-DFAC-4D72-B068-FB5C5B4BD9A4", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Denied:Send Email to Admin Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Approval Process:Denied:Send Email to Event Contact:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Denied: {{Workflow | Attribute:'Reservation'}}" ); // Approval Process:Denied:Send Email to Event Contact:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been denied:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}" ); // Approval Process:Denied:Send Email to Event Contact:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "79551640-080B-44F9-A00B-1AFB1A6D3AD7", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // Approval Process:Denied:Send Email to Event Contact:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "22043E5A-45F7-41C4-93AF-9308B1222291", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Denied:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "22043E5A-45F7-41C4-93AF-9308B1222291", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Denied:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Approval Process:Cancelled:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D4B2E0BB-6FC7-4B68-AF08-72B4A8CD7C7E", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Approval Process:Cancelled:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'         
            And ( Select top 1 Id from AttributeQualifier aq1 Where aq1.[Key] = 'definedtype' and aq1.AttributeId = aq.AttributeId ) is null
		" );

            #endregion

            RoomManagementMigrationHelper.AddSecurityAuthForReservationType( "E443F926-0882-41D5-91EF-480EA366F660", 0, "EditAfterApproval", true, "", SpecialRole.AllAuthenticatedUsers, "87b6aa45-e4aa-4691-9350-bf2e030c2889" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Updates the note type103.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="userSelectable">if set to <c>true</c> [user selectable].</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="IsSystem">if set to <c>true</c> [is system].</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="AllowWatching">if set to <c>true</c> [allow watching].</param>
        public void UpdateNoteType103( string name, string entityTypeName, bool userSelectable, string guid, bool IsSystem = true, string iconCssClass = null, bool AllowWatching = false )
        {
            if ( iconCssClass == null )
            {
                iconCssClass = "NULL";
            }
            else
            {
                iconCssClass = $"'{iconCssClass}'";
            }

            Sql( $@"

                DECLARE @EntityTypeId int = (SELECT top 1 [Id] FROM [EntityType] WHERE [Name] = '{entityTypeName}')

                DECLARE @Id int = (SELECT [Id] FROM [NoteType] WHERE [Name] = '{name}' AND [EntityTypeId] = @EntityTypeId)

                IF @Id IS NULL
                BEGIN
                    INSERT INTO [NoteType] (
                        [Name],[EntityTypeId],[UserSelectable],[Guid],[IsSystem], [IconCssClass], [AllowsWatching])
                    VALUES(
                        '{name}',@EntityTypeId,{userSelectable.Bit()},'{guid}',{IsSystem.Bit()}, {iconCssClass}, {AllowWatching.Bit()})
                END
                ELSE
                BEGIN
                    UPDATE [NoteType] SET
                        [Name] = '{name}',
                        [EntityTypeId] = @EntityTypeId,
                        [UserSelectable] = {userSelectable.Bit()},
                        [Guid] = '{guid}',
                        [IsSystem] = {IsSystem.Bit()},
                        [IconCssClass] = {iconCssClass},
                        [AllowsWatching] = {AllowWatching.Bit()}
                    WHERE Id = @Id;
                END" );

        }
    }
}