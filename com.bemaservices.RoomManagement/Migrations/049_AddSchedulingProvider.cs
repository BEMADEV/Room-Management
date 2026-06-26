using Rock;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for SchedulingProvider and related tables.
    /// </summary>
    [MigrationNumber( 049, "1.17.6" )]
    public partial class AddSchedulingProvider : Migration
    {
        #region Up

        public override void Up()
        {
            AddProviderTables();
            AddProviderEntityTypes();
            AddBlockTypes();
            AddPages();
        }

        private void AddPages()
        {

            // Add Page 
            //  Internal Name: Scheduling Providers
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Scheduling Providers", "", "F2A33E5C-7F3D-43E5-94B6-40B93F290135", "" );

            // Add Page 
            //  Internal Name: Provider Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F2A33E5C-7F3D-43E5-94B6-40B93F290135", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Provider Detail", "", "3F3930C2-D526-4208-928A-33FC1C60C817", "" );

            // Add Block 
            //  Block Name: Scheduling Provider Location List
            //  Page Name: Named Locations
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2BECFB85-D566-464F-B6AC-0BE90189A418".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9CC1B275-670E-43CC-B8C1-AA91CE0FA41D".AsGuid(), "Scheduling Provider Location List", "Main", @"", @"", 3, "36CBFBB1-E327-4464-83B4-2CA65CE15731" );

            // Add Block 
            //  Block Name: Scheduling Provider List
            //  Page Name: Scheduling Providers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F2A33E5C-7F3D-43E5-94B6-40B93F290135".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C00DE0B-B682-4712-870A-0CC8B37D3AFE".AsGuid(), "Scheduling Provider List", "Main", @"", @"", 0, "52EF3F17-38B7-4EF3-9D64-28769EEA0F64" );

            // Add Block 
            //  Block Name: Scheduling Provider Detail
            //  Page Name: Provider Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3F3930C2-D526-4208-928A-33FC1C60C817".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2A25CEFD-E380-4F64-B51A-0F6E8D0C98E8".AsGuid(), "Scheduling Provider Detail", "Main", @"", @"", 0, "72679655-92CA-4964-9468-AE30BCD74485" );


            // Update Order for Page: Named Locations,  Zone: Main,  Block: Scheduling Provider Location List
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '36CBFBB1-E327-4464-83B4-2CA65CE15731'" );


            // Add Block Attribute Value
            //   Block: Scheduling Provider List
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Block Location: Page=Scheduling Providers, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 3f3930c2-d526-4208-928a-33fc1c60c817 */
            RockMigrationHelper.AddBlockAttributeValue( "52EF3F17-38B7-4EF3-9D64-28769EEA0F64", "272C42D4-B8FF-43D2-8E09-A4134C0A0C8A", @"3f3930c2-d526-4208-928a-33fc1c60c817" );

            // Add Block Attribute Value
            //   Block: Scheduling Provider List
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Block Location: Page=Scheduling Providers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "52EF3F17-38B7-4EF3-9D64-28769EEA0F64", "FEBED76E-E5A3-46CA-B42D-B17EEB232CC1", @"False" );

            // Add Block Attribute Value
            //   Block: Scheduling Provider List
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Block Location: Page=Scheduling Providers, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "52EF3F17-38B7-4EF3-9D64-28769EEA0F64", "1502DC4C-6D98-4C18-B063-9CDE513431C8", @"True" );

        }

        private void AddBlockTypes()
        {
            RockMigrationHelper.UpdateFieldType( "Scheduling Provider", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.SchedulingProviderFieldType", "8F3B6F1E-9C2D-4E7A-B8C3-1A4F2E5D6C7B" );

            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Blocks.SchedulingProviderDetail", "Scheduling Provider Detail", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderDetail, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, false, "F2A726C1-B9FA-4950-BF8C-1C0D74C232A0" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Blocks.SchedulingProviderList", "Scheduling Provider List", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderList, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, false, "F7A6AB60-01B8-42C7-BCD6-86292F283E58" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Blocks.SchedulingProviderLocationList", "Scheduling Provider Location List", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderLocationList, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, false, "4812453E-A541-4007-9D9B-DEAAD8C5D15D" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Blocks.SchedulingProviderReservationList", "Scheduling Provider Reservation List", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderReservationList, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, false, "BE083E25-A366-449D-B21C-FC467CA10EC3" );

            // Add/Update BlockType 
            //   Name: Scheduling Provider Detail
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Detail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduling Provider Detail", "Displays the details of a particular scheduling provider.", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderDetail", "BEMA Software Services > Room Management", "2A25CEFD-E380-4F64-B51A-0F6E8D0C98E8" );

            // Add/Update BlockType 
            //   Name: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider List
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduling Provider List", "Displays a list of scheduling providers.", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderList", "BEMA Software Services > Room Management", "6C00DE0B-B682-4712-870A-0CC8B37D3AFE" );

            // Add/Update BlockType 
            //   Name: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Location List
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduling Provider Location List", "Displays a list of scheduling provider locations.", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderLocationList", "BEMA Software Services > Room Management", "9CC1B275-670E-43CC-B8C1-AA91CE0FA41D" );

            // Add/Update BlockType 
            //   Name: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Reservation List
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduling Provider Reservation List", "Displays a list of scheduling provider reservations.", "com.bemaservices.RoomManagement.Blocks.SchedulingProviderReservationList", "BEMA Software Services > Room Management", "13CDF65B-BCEE-4E7D-8D42-298A8F6676BE" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C00DE0B-B682-4712-870A-0CC8B37D3AFE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the scheduling provider details.", 0, @"", "272C42D4-B8FF-43D2-8E09-A4134C0A0C8A" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C00DE0B-B682-4712-870A-0CC8B37D3AFE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "CE6B6149-EC2E-4245-8103-2850148256CF" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C00DE0B-B682-4712-870A-0CC8B37D3AFE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1502DC4C-6D98-4C18-B063-9CDE513431C8" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9CC1B275-670E-43CC-B8C1-AA91CE0FA41D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "CF6E6AB3-72AF-483F-9A2D-DA34895EFF23" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9CC1B275-670E-43CC-B8C1-AA91CE0FA41D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B3B49A5D-1DD5-453B-8E9E-B53703EC03A6" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13CDF65B-BCEE-4E7D-8D42-298A8F6676BE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the scheduling provider reservation details.", 0, @"", "2EBA2C12-E726-471E-A7D3-BEBC67665808" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13CDF65B-BCEE-4E7D-8D42-298A8F6676BE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5D7F484E-B875-48F6-9C7B-B56D3902F486" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13CDF65B-BCEE-4E7D-8D42-298A8F6676BE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "880FE833-B3CA-4F62-9841-7DA0B3935430" );
        }

        private void AddProviderEntityTypes()
        {
            // Register model EntityTypes
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "Scheduling Provider", "com.bemaservices.RoomManagement.Model.SchedulingProvider, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", true, true, "1D4CFE5A-E0D2-4077-A822-9B2C01CC0A0F" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Model.SchedulingProviderLocation", "Scheduling Provider Location", "com.bemaservices.RoomManagement.Model.SchedulingProviderLocation, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", true, true, "B89C5287-5468-49F1-8871-590AC20D8AF2" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Model.SchedulingProviderReservation", "Scheduling Provider Reservation", "com.bemaservices.RoomManagement.Model.SchedulingProviderReservation, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", true, true, "F3D52B02-F64F-461E-ADAE-7CE767366D3D" );

            // Register provider component EntityTypes
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources", "Google Resources", "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, true, "A8F7D8B3-2C1E-4F9A-8D3B-1E5C6A7F8B9C" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.SchedulingProviders.MicrosoftSchedulingAssistant", "Microsoft Scheduling Assistant", "com.bemaservices.RoomManagement.SchedulingProviders.MicrosoftSchedulingAssistant, com.bemaservices.RoomManagement, Version=2.6.5.0, Culture=neutral, PublicKeyToken=null", false, true, "3ED7D672-76A4-41F4-9788-0404B997CC48" );

            // Add attributes for SchedulingProvider model (EntityTypeId will be dynamic)
            var googleEntityId = SqlScalar( "Select Top 1 Id from EntityType where Name = 'com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources'" ).ToStringSafe();
            var microsoftEntityId = SqlScalar( "Select Top 1 Id from EntityType where Name = 'com.bemaservices.RoomManagement.SchedulingProviders.MicrosoftSchedulingAssistant'" ).ToStringSafe();

            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "EntityTypeId", googleEntityId, "Service Account JSON Key File", "Service Account JSON Key File", @"The Google service account JSON key file for API authentication. Download this from the Google Cloud Console.", 2, @"", "6D5D7E41-F0E3-40FB-A46E-725761697667", "ServiceAccountJsonKeyFile" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "9C204CD0-1233-41C5-818A-C5DA439445AA", "EntityTypeId", googleEntityId, "Admin User Email", "Admin User Email", @"The email address of a Google Workspace admin user to impersonate for domain-wide delegation. The service account must have domain-wide delegation enabled and the admin user must have access to the calendars.", 3, @"", "A746594B-5985-44C2-AD88-3780EE74A161", "AdminUserEmail" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "36167F3E-8CB2-44F9-9022-102F171FBC9A", "EntityTypeId", microsoftEntityId, "Microsoft Graph Tenant Id", "Microsoft Graph Tenant Id", @"", 2, @"", "E2E217E6-77D8-4E97-A13D-6FBE8102E945", "TenantId" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "36167F3E-8CB2-44F9-9022-102F171FBC9A", "EntityTypeId", microsoftEntityId, "Microsoft Graph Client Id", "Microsoft Graph Client Id", @"", 3, @"", "BA762F99-E15F-4452-B516-B94072FF8AE3", "ClientId" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "36167F3E-8CB2-44F9-9022-102F171FBC9A", "EntityTypeId", microsoftEntityId, "Microsoft Graph Client Secret", "Microsoft Graph Client Secret", @"", 4, @"", "B9A86395-A7D5-4432-B663-ED1340D210A5", "ClientSecret" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.Model.SchedulingProvider", "36167F3E-8CB2-44F9-9022-102F171FBC9A", "EntityTypeId", microsoftEntityId, "Microsoft Graph UserPrincipalName", "Microsoft Graph UserPrincipalName", @"The username of the Microsoft Graph principal (user or application) that will be used to authenticate API calls. This is typically the email address of the user.", 5, @"", "2F75AB7C-DEBA-4426-B848-86C3354B0140", "UserPrincipalName" );

            // Add attributes for Google Resources provider (EntityTypeId will be dynamic)
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "Active", @"Should Service be used?", 0, @"True", "61854E38-8505-472E-9210-1C50EA1C07A5", "Active" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "Order", @"The order that this service should be used (priority)", 1, @"", "E21C2984-EEA9-4942-926A-F4E5200AC357", "Order" );
          
            // Add attributes for Microsoft Scheduling Assistant provider (EntityTypeId will be dynamic)
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.SchedulingProviders.MicrosoftSchedulingAssistant", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "Active", @"Should Service be used?", 0, @"True", "1E35D994-8C0C-4A4C-97D2-ED1F084FFF28", "Active" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "com.bemaservices.RoomManagement.SchedulingProviders.MicrosoftSchedulingAssistant", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "Order", @"The order that this service should be used (priority)", 1, @"", "06557D50-4B82-47A7-AC7C-9F75180C43C0", "Order" );
           
            // Add qualifiers for encrypted/password fields
            RockMigrationHelper.UpdateAttributeQualifier( "6D5D7E41-F0E3-40FB-A46E-725761697667", "binaryFileType", @"6CBEA3B0-E983-40C1-9712-BD3FA2466EAE", "EF00D842-C3B6-4843-BDFC-A57701966FAE" );
            RockMigrationHelper.UpdateAttributeQualifier( "E2E217E6-77D8-4E97-A13D-6FBE8102E945", "ispassword", @"True", "91235D0A-CBFB-4250-964A-CABB62B26083" );
            RockMigrationHelper.UpdateAttributeQualifier( "BA762F99-E15F-4452-B516-B94072FF8AE3", "ispassword", @"True", "530BA328-C2B0-4734-8471-2B18DC7AE161" );
            RockMigrationHelper.UpdateAttributeQualifier( "B9A86395-A7D5-4432-B663-ED1340D210A5", "ispassword", @"True", "F4C841D8-BFA2-4C9F-8349-A8FACC6D7EF5" );
            RockMigrationHelper.UpdateAttributeQualifier( "2F75AB7C-DEBA-4426-B848-86C3354B0140", "ispassword", @"True", "41D96E2F-FCB6-4B53-8EEE-CF4DA86576B3" );

            RockMigrationHelper.AddAttributeValue( "61854E38-8505-472E-9210-1C50EA1C07A5", 0, "True", "A816D336-52FC-40AF-A563-CEA396CA0F88" );
            RockMigrationHelper.AddAttributeValue( "1E35D994-8C0C-4A4C-97D2-ED1F084FFF28", 0, "True", "F6CB8E58-2973-4735-AB9B-267029E82330" );

        }

        private void AddProviderTables()
        {
            var schedulingProviderTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProvider";
            // Create SchedulingProvider table
            AddTable(
                schedulingProviderTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String( maxLength: null ),
                    EntityTypeId = c.Int( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    MappingsJson = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 50 ),
                } );
            AddPrimaryKey( schedulingProviderTableName, "Id" );
            AddForeignKey( schedulingProviderTableName, "EntityTypeId", "dbo.EntityType", "Id", false );
            AddIndex( schedulingProviderTableName, "EntityTypeId" );

            // Create SchedulingProviderReservation table
            var schedulingProviderReservationTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation";
            AddTable(
                schedulingProviderReservationTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SchedulingProviderId = c.Int( nullable: false ),
                    ReservationId = c.Int( nullable: false ),
                    ExternalId = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 50 ),
                } );
            AddPrimaryKey( schedulingProviderReservationTableName, "Id" );
            AddForeignKey( schedulingProviderReservationTableName, "SchedulingProviderId", schedulingProviderTableName, "Id", true );
            AddForeignKey( schedulingProviderReservationTableName, "ReservationId", "dbo._com_bemaservices_RoomManagement_Reservation", "Id", true );
            AddIndex( schedulingProviderReservationTableName, "SchedulingProviderId" );
            AddIndex( schedulingProviderReservationTableName, "ReservationId" );

            // Create SchedulingProviderLocation table
            var schedulingProviderLocationTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation";
            AddTable(
                schedulingProviderLocationTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SchedulingProviderId = c.Int( nullable: false ),
                    LocationId = c.Int( nullable: false ),
                    ExternalId = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } );
            AddPrimaryKey( schedulingProviderLocationTableName, "Id" );
            AddForeignKey( schedulingProviderLocationTableName, "SchedulingProviderId", schedulingProviderTableName, "Id", true );
            AddForeignKey( schedulingProviderLocationTableName, "LocationId", "dbo.Location", "Id", true );
            AddIndex( schedulingProviderLocationTableName, "SchedulingProviderId" );
            AddIndex( schedulingProviderLocationTableName, "LocationId" );
        }

        #endregion

        #region Down

        public override void Down()
        {
            RemovePages();
            RemoveBlockTypes();
            RemoveProviderEntityTypes();
            RemoveTables();
        }

        private void RemoveTables()
        {
            // Drop foreign keys and indexes first
            DropIndex( "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation", new[] { "LocationId" } );
            DropIndex( "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation", new[] { "SchedulingProviderId" } );
            DropForeignKey( "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation", "SchedulingProviderId", "dbo._com_bemaservices_RoomManagement_SchedulingProvider" );

            DropIndex( "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation", new[] { "ReservationId" } );
            DropIndex( "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation", new[] { "SchedulingProviderId" } );
            DropForeignKey( "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation", "ReservationId", "dbo._com_bemaservices_RoomManagement_Reservation" );
            DropForeignKey( "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation", "SchedulingProviderId", "dbo._com_bemaservices_RoomManagement_SchedulingProvider" );

            DropIndex( "dbo._com_bemaservices_RoomManagement_SchedulingProvider", new[] { "EntityTypeId" } );
            DropForeignKey( "dbo._com_bemaservices_RoomManagement_SchedulingProvider", "EntityTypeId", "dbo.EntityType" );

            // Drop tables
            DropTable( "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation" );
            DropTable( "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation" );
            DropTable( "dbo._com_bemaservices_RoomManagement_SchedulingProvider" );
        }

        private void RemoveProviderEntityTypes()
        {
            // Delete attributes for Microsoft Scheduling Assistant
            RockMigrationHelper.DeleteAttribute( "2F75AB7C-DEBA-4426-B848-86C3354B0140" ); // Microsoft Graph UserPrincipalName
            RockMigrationHelper.DeleteAttribute( "B9A86395-A7D5-4432-B663-ED1340D210A5" ); // Microsoft Graph Client Secret
            RockMigrationHelper.DeleteAttribute( "BA762F99-E15F-4452-B516-B94072FF8AE3" ); // Microsoft Graph Client Id
            RockMigrationHelper.DeleteAttribute( "E2E217E6-77D8-4E97-A13D-6FBE8102E945" ); // Microsoft Graph Tenant Id
            RockMigrationHelper.DeleteAttribute( "06557D50-4B82-47A7-AC7C-9F75180C43C0" ); // Order
            RockMigrationHelper.DeleteAttribute( "1E35D994-8C0C-4A4C-97D2-ED1F084FFF28" ); // Active

            // Delete attributes for Google Resources
            RockMigrationHelper.DeleteAttribute( "A746594B-5985-44C2-AD88-3780EE74A161" ); // Admin User Email
            RockMigrationHelper.DeleteAttribute( "6D5D7E41-F0E3-40FB-A46E-725761697667" ); // Service Account JSON Key File
            RockMigrationHelper.DeleteAttribute( "E21C2984-EEA9-4942-926A-F4E5200AC357" ); // Order
            RockMigrationHelper.DeleteAttribute( "61854E38-8505-472E-9210-1C50EA1C07A5" ); // Active
        }

        private void RemoveBlockTypes()
        {
            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "880FE833-B3CA-4F62-9841-7DA0B3935430" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5D7F484E-B875-48F6-9C7B-B56D3902F486" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "2EBA2C12-E726-471E-A7D3-BEBC67665808" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B3B49A5D-1DD5-453B-8E9E-B53703EC03A6" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "CF6E6AB3-72AF-483F-9A2D-DA34895EFF23" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "1502DC4C-6D98-4C18-B063-9CDE513431C8" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "CE6B6149-EC2E-4245-8103-2850148256CF" );

            // Attribute for BlockType
            //   BlockType: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "272C42D4-B8FF-43D2-8E09-A4134C0A0C8A" );

            // Delete BlockType 
            //   Name: Scheduling Provider Reservation List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Reservation List
            RockMigrationHelper.DeleteBlockType( "13CDF65B-BCEE-4E7D-8D42-298A8F6676BE" );

            // Delete BlockType 
            //   Name: Scheduling Provider Location List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Location List
            RockMigrationHelper.DeleteBlockType( "9CC1B275-670E-43CC-B8C1-AA91CE0FA41D" );

            // Delete BlockType 
            //   Name: Scheduling Provider List
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider List
            RockMigrationHelper.DeleteBlockType( "6C00DE0B-B682-4712-870A-0CC8B37D3AFE" );

            // Delete BlockType 
            //   Name: Scheduling Provider Detail
            //   Category: BEMA Software Services > Room Management
            //   Path: 
            //   EntityType: Scheduling Provider Detail
            RockMigrationHelper.DeleteBlockType( "2A25CEFD-E380-4F64-B51A-0F6E8D0C98E8" );

        }

        private void RemovePages()
        {

            // Remove Block
            //  Name: Scheduling Provider Location List, from Page: Named Locations, Site: Rock RMS
            //  from Page: Named Locations, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "36CBFBB1-E327-4464-83B4-2CA65CE15731" );

            // Remove Block
            //  Name: Scheduling Provider Detail, from Page: Provider Detail, Site: Rock RMS
            //  from Page: Provider Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "72679655-92CA-4964-9468-AE30BCD74485" );

            // Remove Block
            //  Name: Scheduling Provider List, from Page: Scheduling Providers, Site: Rock RMS
            //  from Page: Scheduling Providers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "52EF3F17-38B7-4EF3-9D64-28769EEA0F64" );

            // Delete Page 
            //  Internal Name: Provider Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "3F3930C2-D526-4208-928A-33FC1C60C817" );

            // Delete Page 
            //  Internal Name: Scheduling Providers
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F2A33E5C-7F3D-43E5-94B6-40B93F290135" );
        }

        #endregion
    }
}
