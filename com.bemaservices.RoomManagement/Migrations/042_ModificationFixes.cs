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
    [MigrationNumber( 042, "1.14.0" )]
    public class ModificationFixes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup", "3A8010E3-C836-423F-B5DD-37D7BEE2815C", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState", "3894452A-E763-41AC-8260-10373646D8A0", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetEntityProperty", "2B3502EA-5531-4345-AA01-23AE273F0B6F", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FCA61786-8EC0-44D7-8A3D-152721FF2353" ); // Rock.Workflow.Action.SetEntityProperty:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "8373A55C-E023-4DE0-B583-06FF906520FC" ); // Rock.Workflow.Action.SetEntityProperty:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "913D7A95-BC44-4874-92F9-66DB85DF9FEF" ); // Rock.Workflow.Action.SetEntityProperty:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Name|Property Name Attribute", "PropertyName", "The name of the property to set. <span class='tip tip-lava'></span>", 2, @"", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5" ); // Rock.Workflow.Action.SetEntityProperty:Property Name|Property Name Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Property Value|Property Value Attribute", "PropertyValue", "The value to set. <span class='tip tip-lava'></span>", 3, @"", "0415C959-BF89-4D19-9C47-3AB1098E1FBA" ); // Rock.Workflow.Action.SetEntityProperty:Property Value|Property Value Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Empty Value Handling", "EmptyValueHandling", "How to handle empty property values.", 4, @"", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C" ); // Rock.Workflow.Action.SetEntityProperty:Empty Value Handling
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2B3502EA-5531-4345-AA01-23AE273F0B6F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "22EF2F99-C277-4D3A-A779-E5D4D71D28C5" ); // Rock.Workflow.Action.SetEntityProperty:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "ACA008E2-2406-457E-8E4C-6922E03757A4" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 1, @"", "2E185FB5-FC8E-41BE-B7FE-702F74B47539" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "1D4F819F-145D-4A7F-AB4E-AD7C06759042" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25954FDC-F486-417D-ABBB-E2DF2C67B186" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The approval state to use (if Approval State Attribute is not specified).", 2, @"", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "1AF29883-3028-4867-8DC7-0848953E8B6C", "Reservation Type", "ReservationType", "The reservation type to pull the approval group for.", 2, @"", "379AECA2-62B1-4A3C-936C-0250E0762B64" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Reservation Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "The campus to pull the approval group type for.", 6, @"", "5B5D62B3-8409-48BA-880F-F2F0AB946B12" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Campus
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C3020488-A64D-4EA1-929F-7EB67242F816" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval Group Type Attribute", "ApprovalGroupTypeAttribute", "The attribute that contains the approval group type.", 3, @"", "B6AE991A-0299-44BA-8DDD-51D7F0A850D6" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Approval Group Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Campus Attribute", "CampusAttribute", "The attribute that contains the campus.", 5, @"", "E651A41E-AE9B-45EC-849E-84E8AF1D02B1" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Campus Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Group Attribute", "GroupAttribute", "The attribute to set the matching group to.", 7, @"", "82FEE285-9225-47D5-862E-1BF4251E5218" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Group Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Type Attribute", "ReservationTypeAttribute", "The attribute that contains the reservation type to pull the approval group for", 1, @"", "6B643254-B991-4534-B7B5-22C661D5099B" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Reservation Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Approval Group Type", "ApprovalGroupType", "The approval group type to pull.", 4, @"", "8DA2FBB6-ABCF-4C9C-A19C-14FC305AB712" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Approval Group Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A8010E3-C836-423F-B5DD-37D7BEE2815C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3654A548-1730-42D1-B125-E00879EFED28" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.GetApprovalGroup:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "B524B00C-29CB-49E9-9896-8BB60F209783" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "385A255B-9F48-4625-862B-26231DBAC53A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Modification Process

            RockMigrationHelper.UpdateWorkflowType( false, true, "Modification Process", "A workflow that changes the reservation's approval status if it was modified by someone not in an approval group.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Update", "fa fa-list-ol", 28800, true, 0, "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", 0 ); // Modification Process
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Approval State", "ApprovalState", "", 0, @"", "51088268-4E4A-4AE5-A2D2-841BCF72AFAB", false ); // Modification Process:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 1, @"", "951F0FA1-A0EF-4996-B5E3-3AA881AF7ED4", false ); // Modification Process:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "FE95430C-322D-4B67-9C77-DFD1D4408725", "Previous Modified Date Time", "PreviousModifiedDateTime", "", 2, @"", "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", false ); // Modification Process:Previous Modified Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Reservation Changes", "ReservationChanges", "", 3, @"", "10F0C3B4-6A25-4491-9159-55B95772AC10", false ); // Modification Process:Reservation Changes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Modifier In Approval Group", "ModifierInApprovalGroup", "", 4, @"", "20D22B1D-0394-4E1B-AD69-46041EB8466E", false ); // Modification Process:Modifier In Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "New Approval State", "NewApprovalState", "", 5, @"", "CD7F828D-AC42-496F-B4D2-9C13A2A8F50C", false ); // Modification Process:New Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Approval Level", "ApprovalLevel", "", 6, @"", "4C2E413F-C526-4D25-95F3-85F669B3002F", false ); // Modification Process:Approval Level
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "1AF29883-3028-4867-8DC7-0848953E8B6C", "Reservation Type", "ReservationType", "", 7, @"", "7B6C201F-ABD8-4A26-BCFE-225AE2E73FBF", false ); // Modification Process:Reservation Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Initial Approval Group", "InitialApprovalGroup", "", 8, @"", "DEB2F0F4-7AB3-4BB0-8194-28DD8865CD71", false ); // Modification Process:Initial Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "", 9, @"", "42EFAED2-90E7-4BF4-8CA2-213DA9E6C778", false ); // Modification Process:Final Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "", 10, @"", "CB86F1B3-F375-4F72-9612-F5A1AC99DC13", false ); // Modification Process:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Status Updated", "IsStatusUpdated", "", 11, @"", "46215AB7-2064-4066-BBB9-CE9C9800AC9C", false ); // Modification Process:Is Status Updated
            RockMigrationHelper.AddAttributeQualifier( "51088268-4E4A-4AE5-A2D2-841BCF72AFAB", "ispassword", @"False", "28F9EC00-9631-4CC3-BC90-9AC343ECE173" ); // Modification Process:Approval State:ispassword
            RockMigrationHelper.AddAttributeQualifier( "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", "datePickerControlType", @"Date Picker", "2F2CDBCA-02A7-4FB4-AC54-B4F3BFB76160" ); // Modification Process:Previous Modified Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", "displayCurrentOption", @"False", "DC2A3D4D-771C-4A17-B8DB-AF0C9884A90B" ); // Modification Process:Previous Modified Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", "displayDiff", @"False", "48EE14C1-66F3-40B5-A756-0AEA0E42FB7B" ); // Modification Process:Previous Modified Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", "format", @"", "7E99558E-1B97-4D2D-B745-F1FDBF551E29" ); // Modification Process:Previous Modified Date Time:format
            RockMigrationHelper.AddAttributeQualifier( "F22AA1DC-DC72-4C8D-B5BE-6795A0E6D7BC", "futureYearCount", @"", "5974AC52-945D-4343-AC18-A85C282AEA9F" ); // Modification Process:Previous Modified Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "10F0C3B4-6A25-4491-9159-55B95772AC10", "ispassword", @"False", "435E7028-E4BA-4048-82D9-3935E7D56AF9" ); // Modification Process:Reservation Changes:ispassword
            RockMigrationHelper.AddAttributeQualifier( "10F0C3B4-6A25-4491-9159-55B95772AC10", "maxcharacters", @"", "36226AB3-9969-4B63-810D-1A00AA2C381D" ); // Modification Process:Reservation Changes:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "10F0C3B4-6A25-4491-9159-55B95772AC10", "showcountdown", @"False", "DEC1B217-3FD5-4E64-8705-55ADFA39CC4D" ); // Modification Process:Reservation Changes:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "20D22B1D-0394-4E1B-AD69-46041EB8466E", "BooleanControlType", @"0", "CF96FC93-16E5-48F7-9748-420CA16B40A4" ); // Modification Process:Modifier In Approval Group:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "20D22B1D-0394-4E1B-AD69-46041EB8466E", "falsetext", @"No", "8B654D33-96DA-41A5-95CD-5A18463F7DEA" ); // Modification Process:Modifier In Approval Group:falsetext
            RockMigrationHelper.AddAttributeQualifier( "20D22B1D-0394-4E1B-AD69-46041EB8466E", "truetext", @"Yes", "D7EF15E0-C77D-4AA9-9F78-A725864EE417" ); // Modification Process:Modifier In Approval Group:truetext
            RockMigrationHelper.AddAttributeQualifier( "CD7F828D-AC42-496F-B4D2-9C13A2A8F50C", "repeatColumns", @"", "8DDB93D7-4560-4232-9F14-DA4C3C859490" ); // Modification Process:New Approval State:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "CB86F1B3-F375-4F72-9612-F5A1AC99DC13", "includeInactive", @"False", "073413B9-C4BD-46C0-ACA4-9B71D35DE17D" ); // Modification Process:Campus:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "46215AB7-2064-4066-BBB9-CE9C9800AC9C", "BooleanControlType", @"0", "E582D1DB-C3CD-42F5-AF4B-9B3DFDF266B7" ); // Modification Process:Is Status Updated:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "46215AB7-2064-4066-BBB9-CE9C9800AC9C", "falsetext", @"No", "38709A48-4550-4918-AE17-68335BCB4785" ); // Modification Process:Is Status Updated:falsetext
            RockMigrationHelper.AddAttributeQualifier( "46215AB7-2064-4066-BBB9-CE9C9800AC9C", "truetext", @"Yes", "FD4A548A-13E5-4E09-B733-0B6D7BE96496" ); // Modification Process:Is Status Updated:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", true, "Set Attributes", "", true, 0, "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF" ); // Modification Process:Set Attributes
            RockMigrationHelper.UpdateWorkflowActivityType( "96A2F6D0-D9A7-4F58-B3D5-D7B37CFE5EB0", true, "Change Approval Status", "", false, 1, "8F653339-CEA7-45C0-ABE0-D5706520EB6F" ); // Modification Process:Change Approval Status
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Delay Activated", "8167ed7e-35f9-4ab4-85d3-e929cf965f44", "", 0, @"", "FDA6517E-B164-4A11-9EFF-1ABF560B402F" ); // Modification Process:Set Attributes:Delay Activated
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Reservation From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF" ); // Modification Process:Set Attributes:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Reservation Type", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "6D375E71-0FAE-4FFD-A1AD-F59040BAAD59" ); // Modification Process:Set Attributes:Set Reservation Type
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Campus", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "638F6D61-6EEA-43CA-B76D-F2F778382CFC" ); // Modification Process:Set Attributes:Set Campus
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Initial Approval Group", 3, "3A8010E3-C836-423F-B5DD-37D7BEE2815C", true, false, "", "", 1, "", "9E4B1911-A3B1-42BA-8067-529DCF9CABDA" ); // Modification Process:Set Attributes:Set Initial Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Final Approval Group", 4, "3A8010E3-C836-423F-B5DD-37D7BEE2815C", true, false, "", "", 1, "", "FAF0AB03-7249-4B54-9077-759533CA5D33" ); // Modification Process:Set Attributes:Set Final Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Approval State", 5, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "CE44E767-6211-46D3-BA0D-FD3E91C68F95" ); // Modification Process:Set Attributes:Set Approval State
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Check if Modifier is in an Approval Group", 6, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "C51264F9-CD22-427A-8EBA-B024ACC846B3" ); // Modification Process:Set Attributes:Check if Modifier is in an Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "End Workflow if Modifier in Approval Group", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "20D22B1D-0394-4E1B-AD69-46041EB8466E", 1, "Yes", "6ADEFA0E-A54E-47C0-A266-B00742B92A15" ); // Modification Process:Set Attributes:End Workflow if Modifier in Approval Group
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Reservation Changes", 8, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "951F0FA1-A0EF-4996-B5E3-3AA881AF7ED4", 64, "", "22723854-CEC9-442D-BDCC-92B8D283AFA2" ); // Modification Process:Set Attributes:Set Reservation Changes
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set New Approval State", 9, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "DBA7D080-21A9-4AA3-A7AD-664E7E0CF8AB" ); // Modification Process:Set Attributes:Set New Approval State
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Approval Level", 10, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "DBF9A69D-8863-48E5-B4AE-B0BB640099BD" ); // Modification Process:Set Attributes:Set Approval Level
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Set Is Status Updated", 11, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "01E55491-06A3-457B-8288-E4CFB4F17DFA" ); // Modification Process:Set Attributes:Set Is Status Updated
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Close Workflow if No Change", 12, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "CD7F828D-AC42-496F-B4D2-9C13A2A8F50C", 32, "", "6B159DC6-475D-4DEC-AEE9-D709309C6931" ); // Modification Process:Set Attributes:Close Workflow if No Change
            RockMigrationHelper.UpdateWorkflowActionType( "66D9CCF1-DA15-43B2-88EA-E6322DF91CCF", "Activate Change Approval Status Activity", 13, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 1, "", "83AF3B2C-870C-438E-B782-EAC5B8C979B7" ); // Modification Process:Set Attributes:Activate Change Approval Status Activity
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Final Approval Date", 0, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1024, "3", "C241E775-895E-4B3F-8737-F95125545A75" ); // Modification Process:Change Approval Status:Clear Final Approval Date
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Final Approver", 1, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1024, "3", "17389CA4-A9E0-42B0-9D4C-86877A1F8D04" ); // Modification Process:Change Approval Status:Clear Final Approver
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Special Approval Date", 2, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1024, "2", "58273E8E-6291-4B70-9DF9-F9BD246DDF6E" ); // Modification Process:Change Approval Status:Clear Special Approval Date
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Special Approver", 3, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1024, "2", "7E94AAC4-222C-45AD-BEA2-699189167DA7" ); // Modification Process:Change Approval Status:Clear Special Approver
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Initial Approval Date Time", 4, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1, "1", "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Clear Initial Approver", 5, "2B3502EA-5531-4345-AA01-23AE273F0B6F", true, false, "", "4C2E413F-C526-4D25-95F3-85F669B3002F", 1, "1", "0B2D4123-08D1-4899-A3E6-78744060798C" ); // Modification Process:Change Approval Status:Clear Initial Approver
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Set Reservation Status", 6, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "46215AB7-2064-4066-BBB9-CE9C9800AC9C", 8, "True", "9BD0FDFA-8432-40A6-A8B3-DFC1F242140C" ); // Modification Process:Change Approval Status:Set Reservation Status
            RockMigrationHelper.UpdateWorkflowActionType( "8F653339-CEA7-45C0-ABE0-D5706520EB6F", "Close Workflow", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "DDE2AEFA-D6D1-4B93-A2D2-61D80E42E03D" ); // Modification Process:Change Approval Status:Close Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Modification Process:Set Attributes:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Set Attributes:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF", "B524B00C-29CB-49E9-9896-8BB60F209783", @"True" ); // Modification Process:Set Attributes:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Modification Process:Set Attributes:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "4D3AD067-B3B3-4BF7-8B5A-FBF7515424EF", "7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199", @"{{ Entity.Guid }}" ); // Modification Process:Set Attributes:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "6D375E71-0FAE-4FFD-A1AD-F59040BAAD59", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign reservation = Workflow | Attribute:'Reservation','Object' %}{{reservation.ReservationType.Guid}}" ); // Modification Process:Set Attributes:Set Reservation Type:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "6D375E71-0FAE-4FFD-A1AD-F59040BAAD59", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set Reservation Type:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6D375E71-0FAE-4FFD-A1AD-F59040BAAD59", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"7b6c201f-abd8-4a26-bcfe-225ae2e73fbf" ); // Modification Process:Set Attributes:Set Reservation Type:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "638F6D61-6EEA-43CA-B76D-F2F778382CFC", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign reservation = Workflow | Attribute:'Reservation','Object' %}{% if reservation.CampusId != null %}{{reservation.Campus.Guid}}{% endif %}" ); // Modification Process:Set Attributes:Set Campus:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "638F6D61-6EEA-43CA-B76D-F2F778382CFC", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set Campus:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "638F6D61-6EEA-43CA-B76D-F2F778382CFC", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"cb86f1b3-f375-4f72-9612-f5a1ac99dc13" ); // Modification Process:Set Attributes:Set Campus:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9E4B1911-A3B1-42BA-8067-529DCF9CABDA", "C3020488-A64D-4EA1-929F-7EB67242F816", @"False" ); // Modification Process:Set Attributes:Set Initial Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9E4B1911-A3B1-42BA-8067-529DCF9CABDA", "6B643254-B991-4534-B7B5-22C661D5099B", @"7b6c201f-abd8-4a26-bcfe-225ae2e73fbf" ); // Modification Process:Set Attributes:Set Initial Approval Group:Reservation Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9E4B1911-A3B1-42BA-8067-529DCF9CABDA", "8DA2FBB6-ABCF-4C9C-A19C-14FC305AB712", @"0" ); // Modification Process:Set Attributes:Set Initial Approval Group:Approval Group Type
            RockMigrationHelper.AddActionTypeAttributeValue( "9E4B1911-A3B1-42BA-8067-529DCF9CABDA", "E651A41E-AE9B-45EC-849E-84E8AF1D02B1", @"cb86f1b3-f375-4f72-9612-f5a1ac99dc13" ); // Modification Process:Set Attributes:Set Initial Approval Group:Campus Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9E4B1911-A3B1-42BA-8067-529DCF9CABDA", "82FEE285-9225-47D5-862E-1BF4251E5218", @"deb2f0f4-7ab3-4bb0-8194-28dd8865cd71" ); // Modification Process:Set Attributes:Set Initial Approval Group:Group Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FAF0AB03-7249-4B54-9077-759533CA5D33", "C3020488-A64D-4EA1-929F-7EB67242F816", @"False" ); // Modification Process:Set Attributes:Set Final Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FAF0AB03-7249-4B54-9077-759533CA5D33", "6B643254-B991-4534-B7B5-22C661D5099B", @"7b6c201f-abd8-4a26-bcfe-225ae2e73fbf" ); // Modification Process:Set Attributes:Set Final Approval Group:Reservation Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FAF0AB03-7249-4B54-9077-759533CA5D33", "8DA2FBB6-ABCF-4C9C-A19C-14FC305AB712", @"1" ); // Modification Process:Set Attributes:Set Final Approval Group:Approval Group Type
            RockMigrationHelper.AddActionTypeAttributeValue( "FAF0AB03-7249-4B54-9077-759533CA5D33", "E651A41E-AE9B-45EC-849E-84E8AF1D02B1", @"cb86f1b3-f375-4f72-9612-f5a1ac99dc13" ); // Modification Process:Set Attributes:Set Final Approval Group:Campus Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FAF0AB03-7249-4B54-9077-759533CA5D33", "82FEE285-9225-47D5-862E-1BF4251E5218", @"42efaed2-90e7-4bf4-8ca2-213da9e6c778" ); // Modification Process:Set Attributes:Set Final Approval Group:Group Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CE44E767-6211-46D3-BA0D-FD3E91C68F95", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Modification Process:Set Attributes:Set Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CE44E767-6211-46D3-BA0D-FD3E91C68F95", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"51088268-4e4a-4ae5-a2d2-841bcf72afab" ); // Modification Process:Set Attributes:Set Approval State:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CE44E767-6211-46D3-BA0D-FD3E91C68F95", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{% assign reservation =  Workflow | Attribute:'Reservation', 'object' %}{{ reservation.ApprovalState }}" ); // Modification Process:Set Attributes:Set Approval State:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C51264F9-CD22-427A-8EBA-B024ACC846B3", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign reservation = Workflow | Attribute:'Reservation','Object' %}
{% assign modifier = reservation.ModifiedByPersonAliasId | PersonByAliasId %}
{% assign campusId = reservation.CampusId %}

{% sql %}
Select *
From _com_bemaservices_RoomManagement_ReservationType rt
Join _com_bemaservices_RoomManagement_ReservationApprovalGroup ag on ag.ReservationTypeId = rt.Id
Join [Group] g on ag.ApprovalGroupId = g.Id
Join GroupMember gm on gm.GroupMemberStatus = 1 and gm.IsArchived = 0 and gm.GroupId = g.Id
Where rt.Id = {{reservation.ReservationTypeId}}
And gm.PersonId = {{modifier.Id | Default:'0' }}
And (ag.CampusId is null {% if campusId != null %} or ag.CampusId = {{campusId}}{% endif %})
{% endsql %}
{% assign resultCount = results | Size %}
{% if resultCount > 0 %}
Yes
{% else %}
No
{% endif %}" ); // Modification Process:Set Attributes:Check if Modifier is in an Approval Group:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "C51264F9-CD22-427A-8EBA-B024ACC846B3", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Check if Modifier is in an Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C51264F9-CD22-427A-8EBA-B024ACC846B3", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"20d22b1d-0394-4e1b-ad69-46041eb8466e" ); // Modification Process:Set Attributes:Check if Modifier is in an Approval Group:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C51264F9-CD22-427A-8EBA-B024ACC846B3", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"Sql" ); // Modification Process:Set Attributes:Check if Modifier is in an Approval Group:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "6ADEFA0E-A54E-47C0-A266-B00742B92A15", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Modification Process:Set Attributes:End Workflow if Modifier in Approval Group:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6ADEFA0E-A54E-47C0-A266-B00742B92A15", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Modification Process:Set Attributes:End Workflow if Modifier in Approval Group:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "22723854-CEC9-442D-BDCC-92B8D283AFA2", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% sql %}
SELECT Verb, ValueName, IsNull(OldValue,'N/A') as OldValue,IsNull(NewValue,'N/A') as NewValue
  FROM [History] h
  Join Category c on c.Id = h.CategoryId and c.Guid = '806E96DE-3744-4F56-B12F-787F36A1CEB5'
  Join [dbo].[_com_bemaservices_RoomManagement_Reservation] r on r.Id = h.EntityId
  Where h.Verb in ('MODIFY','ADD','REMOVE','DELETE')
  And h.ValueName not like '%Approval State%'
  And h.ValueName not like '%Updated by the%'
And h.CreatedDateTime > '{{Workflow | Attribute:'PreviousModifiedDateTime','RawValue' | DateAdd:2,'s'}}'
And r.Id = {{Workflow | Attribute:'Reservation','Id'}}
{% endsql %}
{{ results | ToJSON }}" ); // Modification Process:Set Attributes:Set Reservation Changes:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "22723854-CEC9-442D-BDCC-92B8D283AFA2", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set Reservation Changes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "22723854-CEC9-442D-BDCC-92B8D283AFA2", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"10f0c3b4-6a25-4491-9159-55b95772ac10" ); // Modification Process:Set Attributes:Set Reservation Changes:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "22723854-CEC9-442D-BDCC-92B8D283AFA2", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"Sql" ); // Modification Process:Set Attributes:Set Reservation Changes:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "DBA7D080-21A9-4AA3-A7AD-664E7E0CF8AB", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign initialApprovalGroupIncludedKeywords = 'Schedule,Campus' %}
{% assign finalApprovalGroupExcludedKeywords = 'Reservation,Name,Event Contact,Event Contact Phone Number,Event Contact Email,Administrative Contact,Administrative Contact Phone Number,Administrative Contact Email,Note' %}

{% assign newApprovalState = '' %}
{% assign initialApprovalGroup = Workflow | Attribute:'InitialApprovalGroup','RawValue' %}
{% assign finalApprovalGroup = Workflow | Attribute:'FinalApprovalGroup', 'RawValue' %}

{% assign reservationChanges = Workflow | Attribute:'ReservationChanges' | FromJSON %}
{% for reservationChange in reservationChanges %}
    {% assign skipApproval = false %}
    {% if newApprovalState != 'PendingInitialApproval' %}
        {% if initialApprovalGroupIncludedKeywords contains reservationChange.ValueName %}
            {% if initialApprovalGroup != '' %}
                {% assign newApprovalState = 'PendingInitialApproval' %}
            {% elseif finalApprovalGroup != '' %}
                {% assign newApprovalState = 'PendingFinalApproval' %}
            {% endif %}
        {% endif %}

        {% if newApprovalState != 'PendingInitialApproval' and newApprovalState != 'PendingSpecialApproval' %}
            {% if reservationChange.ValueName contains '[Resource]' or reservationChange.ValueName contains '[Location]' %}
                {% if reservationChange.ValueName contains 'Quantity' %}
                    {% assign newValueInt = reservationChange.NewValue | AsInteger %}
                    {% assign oldValueInt = reservationChange.OldValue | AsInteger %}
                    {% if reservationChange.OldValue == 'N/A' or newValueInt > oldValueInt %}
                        {% assign newApprovalState = 'PendingSpecialApproval' %}
                    {% else %}
                        {% assign skipApproval = true %}
                    {% endif %}
                {% else %}
                    {% assign newApprovalState = 'PendingSpecialApproval' %}
                {% endif %}
            {% endif %}

            {% if newApprovalState != 'PendingInitialApproval' and newApprovalState != 'PendingSpecialApproval' and newApprovalState != 'PendingFinalApproval' %}
                {% if finalApprovalGroupExcludedKeywords contains reservationChange.ValueName %}
                {% else %}
                    {% if finalApprovalGroup != '' and skipApproval == false %}
                        {% assign newApprovalState = 'PendingFinalApproval' %}
                    {% endif %}
                {% endif %}
            {% endif %}
        {% endif %}
    {% endif %}
{% endfor %}
{% if newApprovalState == 'PendingInitialApproval' %}
1
{% elseif newApprovalState == 'PendingSpecialApproval' %}
6
{% elseif newApprovalState == 'PendingFinalApproval' %}
5
{% endif %}" ); // Modification Process:Set Attributes:Set New Approval State:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "DBA7D080-21A9-4AA3-A7AD-664E7E0CF8AB", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set New Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DBA7D080-21A9-4AA3-A7AD-664E7E0CF8AB", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"cd7f828d-ac42-496f-b4d2-9c13a2a8f50c" ); // Modification Process:Set Attributes:Set New Approval State:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DBF9A69D-8863-48E5-B4AE-B0BB640099BD", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign newApprovalState = Workflow | Attribute:'NewApprovalState' %}
{% if newApprovalState == 'Pending Initial Approval' %}
1
{% elseif newApprovalState == 'Pending Special Approval' %}
2
{% elseif newApprovalState == 'Pending Final Approval' %}
3
{% else %}
99
{% endif %}" ); // Modification Process:Set Attributes:Set Approval Level:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "DBF9A69D-8863-48E5-B4AE-B0BB640099BD", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set Approval Level:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DBF9A69D-8863-48E5-B4AE-B0BB640099BD", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"4c2e413f-c526-4d25-95f3-85f669b3002f" ); // Modification Process:Set Attributes:Set Approval Level:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "01E55491-06A3-457B-8288-E4CFB4F17DFA", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign approvalState = Workflow | Attribute:'ApprovalState' %}
{% assign newApprovalState = Workflow | Attribute:'NewApprovalState' | Remove:' ' %}
{% if approvalState == 'Draft' or approvalState == 'Denied' or approvalState == 'Cancelled' or approvalState == 'ChangesNeed' %}
False
{% elseif approvalState == 'Approved' or approvalState == 'PendingFinalApproval' %}
True
{% elseif approvalState == 'PendingSpecialApproval' %}
    {% if newApprovalState == 'PendingFinalApproval' %}
    False
    {% else %}
    True
    {% endif %}
{% elseif approvalState == 'PendingInitialApproval' %}
    {% if newApprovalState == 'PendingFinalApproval' or newApprovalState == 'PendingSpecialApproval' %}
    False
    {% else %}
    True
    {% endif %}
{% endif %}" ); // Modification Process:Set Attributes:Set Is Status Updated:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "01E55491-06A3-457B-8288-E4CFB4F17DFA", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Modification Process:Set Attributes:Set Is Status Updated:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "01E55491-06A3-457B-8288-E4CFB4F17DFA", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"46215ab7-2064-4066-bbb9-ce9c9800ac9c" ); // Modification Process:Set Attributes:Set Is Status Updated:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6B159DC6-475D-4DEC-AEE9-D709309C6931", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Modification Process:Set Attributes:Close Workflow if No Change:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6B159DC6-475D-4DEC-AEE9-D709309C6931", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Modification Process:Set Attributes:Close Workflow if No Change:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "83AF3B2C-870C-438E-B782-EAC5B8C979B7", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Modification Process:Set Attributes:Activate Change Approval Status Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "83AF3B2C-870C-438E-B782-EAC5B8C979B7", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"8F653339-CEA7-45C0-ABE0-D5706520EB6F" ); // Modification Process:Set Attributes:Activate Change Approval Status Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C241E775-895E-4B3F-8737-F95125545A75", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Final Approval Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C241E775-895E-4B3F-8737-F95125545A75", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Final Approval Date:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "C241E775-895E-4B3F-8737-F95125545A75", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Final Approval Date:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C241E775-895E-4B3F-8737-F95125545A75", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"FinalApprovalDateTime" ); // Modification Process:Change Approval Status:Clear Final Approval Date:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C241E775-895E-4B3F-8737-F95125545A75", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Final Approval Date:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "17389CA4-A9E0-42B0-9D4C-86877A1F8D04", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Final Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "17389CA4-A9E0-42B0-9D4C-86877A1F8D04", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Final Approver:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "17389CA4-A9E0-42B0-9D4C-86877A1F8D04", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Final Approver:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "17389CA4-A9E0-42B0-9D4C-86877A1F8D04", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"FinalApproverAliasId" ); // Modification Process:Change Approval Status:Clear Final Approver:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "17389CA4-A9E0-42B0-9D4C-86877A1F8D04", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Final Approver:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "58273E8E-6291-4B70-9DF9-F9BD246DDF6E", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Special Approval Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "58273E8E-6291-4B70-9DF9-F9BD246DDF6E", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Special Approval Date:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "58273E8E-6291-4B70-9DF9-F9BD246DDF6E", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Special Approval Date:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "58273E8E-6291-4B70-9DF9-F9BD246DDF6E", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"SpecialApprovalDateTime" ); // Modification Process:Change Approval Status:Clear Special Approval Date:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "58273E8E-6291-4B70-9DF9-F9BD246DDF6E", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Special Approval Date:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "7E94AAC4-222C-45AD-BEA2-699189167DA7", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Special Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7E94AAC4-222C-45AD-BEA2-699189167DA7", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Special Approver:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "7E94AAC4-222C-45AD-BEA2-699189167DA7", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Special Approver:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7E94AAC4-222C-45AD-BEA2-699189167DA7", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"SpecialApproverAliasId" ); // Modification Process:Change Approval Status:Clear Special Approver:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7E94AAC4-222C-45AD-BEA2-699189167DA7", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Special Approver:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"InitialApprovalDateTime" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DF3AAD5B-0E3E-44E6-A694-726FE4D05C8A", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Initial Approval Date Time:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "0B2D4123-08D1-4899-A3E6-78744060798C", "FCA61786-8EC0-44D7-8A3D-152721FF2353", @"False" ); // Modification Process:Change Approval Status:Clear Initial Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0B2D4123-08D1-4899-A3E6-78744060798C", "8373A55C-E023-4DE0-B583-06FF906520FC", @"839768a3-10d6-446c-a65b-b8f9efd7808f" ); // Modification Process:Change Approval Status:Clear Initial Approver:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "0B2D4123-08D1-4899-A3E6-78744060798C", "913D7A95-BC44-4874-92F9-66DB85DF9FEF", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Clear Initial Approver:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0B2D4123-08D1-4899-A3E6-78744060798C", "EF2CEB92-D90D-4533-9A1A-3F61E0E436A5", @"InitialApproverAliasId" ); // Modification Process:Change Approval Status:Clear Initial Approver:Property Name|Property Name Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0B2D4123-08D1-4899-A3E6-78744060798C", "6C6D7AF8-4A4C-46E7-9F57-F150186B7D2C", @"NULL" ); // Modification Process:Change Approval Status:Clear Initial Approver:Empty Value Handling
            RockMigrationHelper.AddActionTypeAttributeValue( "9BD0FDFA-8432-40A6-A8B3-DFC1F242140C", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Modification Process:Change Approval Status:Set Reservation Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9BD0FDFA-8432-40A6-A8B3-DFC1F242140C", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"951f0fa1-a0ef-4996-b5e3-3aa881af7ed4" ); // Modification Process:Change Approval Status:Set Reservation Status:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9BD0FDFA-8432-40A6-A8B3-DFC1F242140C", "2E185FB5-FC8E-41BE-B7FE-702F74B47539", @"cd7f828d-ac42-496f-b4d2-9c13a2a8f50c" ); // Modification Process:Change Approval Status:Set Reservation Status:Approval State Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DDE2AEFA-D6D1-4B93-A2D2-61D80E42E03D", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Modification Process:Change Approval Status:Close Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DDE2AEFA-D6D1-4B93-A2D2-61D80E42E03D", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // Modification Process:Change Approval Status:Close Workflow:Status|Status Attribute

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
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
