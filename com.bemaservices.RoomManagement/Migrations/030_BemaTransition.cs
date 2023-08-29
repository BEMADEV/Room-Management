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
using System.Data.SqlClient;
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
    [MigrationNumber( 30, "1.9.4" )]
    public class BemaTransition : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            try
            {
                UpdateTablesV56();
            }
            catch ( Exception ex )
            {
                try
                {
                    UpdateTablesV55();
                }
                catch ( Exception ex1 )
                {
                    try
                    {
                        UpdateTablesV41();
                    }
                    catch ( Exception ex2 )
                    {
                        UpdateTablesV33();
                    }
                }

                SqlWithLongTimeout( @"
                    INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
                               ([LocationTypeValueId]
                               ,[Guid]
                               ,[ReservationTypeId])
                         Select dv.Id
                               ,newId()
                               ,rt.Id
		                       From [dbo].[_com_bemaservices_RoomManagement_ReservationType] rt
                    Outer Apply DefinedValue dv where dv.DefinedTypeId = (Select Top 1 Id From DefinedType Where Guid = '3285DCEF-FAA4-43B9-9338-983F4A384ABA')" );
            }

            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.LocationLayout", "Location Layout", "com.centralaz.RoomManagement.Model.LocationLayout, com.centralaz.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "79991D84-88EB-4384-8D09-1E514BC3B2BD" );
            RoomManagementMigrationHelper.UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.LocationLayout", "Location Layout", "com.bemaservices.RoomManagement.Model.LocationLayout, com.bemaservices.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "79991D84-88EB-4384-8D09-1E514BC3B2BD" );

            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.DataFilter.Reservation.ReservationInDateRangeFilter", "Reservation In Date Range Filter", "com.centralaz.RoomManagement.DataFilter.Reservation.ReservationInDateRangeFilter, com.centralaz.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "9B2E908E-824D-4C5D-9975-A5E2B72ACC8F" );
            RoomManagementMigrationHelper.UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.DataFilter.Reservation.ReservationInDateRangeFilter", "Reservation In Date Range Filter", "com.bemaservices.RoomManagement.DataFilter.Reservation.ReservationInDateRangeFilter, com.bemaservices.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "9B2E908E-824D-4C5D-9975-A5E2B72ACC8F" );

            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Web.Cache.ReservationMinistryCache", "Reservation Ministry Cache", "com.centralaz.RoomManagement.Web.Cache.ReservationMinistryCache, com.centralaz.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "4F4C8820-BF5E-404F-8120-0FCB37B184F0" );
            RoomManagementMigrationHelper.UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Web.Cache.ReservationMinistryCache", "Reservation Ministry Cache", "com.bemaservices.RoomManagement.Web.Cache.ReservationMinistryCache, com.bemaservices.RoomManagement, Version = 1.2.2.0, Culture = neutral, PublicKeyToken = null", false, false, "4F4C8820-BF5E-404F-8120-0FCB37B184F0" );

            var sqlQuery = @"
                Update Page
                Set [Order] = 2
                Where Guid = 'CFF84B6D-C852-4FC4-B602-9F045EDC8854'

                Update [dbo].[_com_bemaservices_RoomManagement_ReservationType]
                Set DefaultCleanupTime = DefaultSetupTime
                Where DefaultCleanupTime is null

                Update [dbo].[_com_bemaservices_RoomManagement_Reservation]
                Set [ApprovalState] = 6
                Where ApprovalState = 1
            ";

            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates the tables V56.
        /// </summary>
        private void UpdateTablesV56()
        {
            var sqlQuery = @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
BEGIN
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId])
	Select Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId]
	From [dbo].[_com_centralaz_RoomManagement_Resource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_Question]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime])
	Select Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime]
	From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId])
	Select Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId]
	From [dbo].[_com_centralaz_RoomManagement_Reservation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState])
	Select Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
	From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId])
	Select Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
END


";

            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates the tables V55.
        /// </summary>
        private void UpdateTablesV55()
        {
            var sqlQuery = @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
BEGIN
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId])
	Select Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId]
	From [dbo].[_com_centralaz_RoomManagement_Resource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_Question]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime])
	Select Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime]
	From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId])
	Select Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId]
	From [dbo].[_com_centralaz_RoomManagement_Reservation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState])
	Select Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
	From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId])
	Select Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
END
";

            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates the tables V41.
        /// </summary>
        private void UpdateTablesV41()
        {
            var sqlQuery = @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
            BEGIN
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
                       ,[LocationId]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IsDefault]
                       ,[LayoutPhotoId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[IsSystem]
                       ,[LocationId]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IsDefault]
                       ,[LayoutPhotoId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId])
	            Select Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId]
	            From [dbo].[_com_centralaz_RoomManagement_Resource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_Question]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IconCssClass]
                       ,[FinalApprovalGroupId]
                       ,[SuperAdminGroupId]
                       ,[NotificationEmailId]
                       ,[DefaultSetupTime]
                       ,[IsCommunicationHistorySaved]
                       ,[IsNumberAttendingRequired]
                       ,[IsContactDetailsRequired]
                       ,[IsSetupTimeRequired]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[IsSystem]
                           ,[Name]
                           ,[Description]
                           ,[IsActive]
                           ,[IconCssClass]
                           ,[FinalApprovalGroupId]
                           ,[SuperAdminGroupId]
                           ,[NotificationEmailId]
                           ,[DefaultSetupTime]
                           ,[IsCommunicationHistorySaved]
                           ,[IsNumberAttendingRequired]
                           ,[IsContactDetailsRequired]
                           ,[IsSetupTimeRequired]
                           ,[Guid]
                           ,[CreatedDateTime]
                           ,[ModifiedDateTime]
                           ,[CreatedByPersonAliasId]
                           ,[ModifiedByPersonAliasId]
                           ,[ForeignKey]
                           ,[ForeignGuid]
                           ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId])
	            Select Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
                       ,[ScheduleId]
                       ,[CampusId]
                       ,[ReservationMinistryId]
                       ,[ReservationStatusId]
                       ,[RequesterAliasId]
                       ,[ApproverAliasId]
                       ,[SetupTime]
                       ,[CleanupTime]
                       ,[NumberAttending]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[SetupPhotoId]
                       ,[EventContactPersonAliasId]
                       ,[EventContactPhone]
                       ,[EventContactEmail]
                       ,[AdministrativeContactPersonAliasId]
                       ,[AdministrativeContactPhone]
                       ,[AdministrativeContactEmail]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                        ,[ScheduleId]
                        ,[CampusId]
                        ,[ReservationMinistryId]
                        ,[ReservationStatusId]
                        ,[RequesterAliasId]
                        ,[ApproverAliasId]
                        ,[SetupTime]
                        ,[CleanupTime]
                        ,[NumberAttending]
                        ,[Note]
                        ,[Guid]
                        ,[CreatedDateTime]
                        ,[ModifiedDateTime]
                        ,[CreatedByPersonAliasId]
                        ,[ModifiedByPersonAliasId]
                        ,[ForeignKey]
                        ,[ForeignGuid]
                        ,[ForeignId]
                        ,[ApprovalState]
                        ,[SetupPhotoId]
                        ,[EventContactPersonAliasId]
                        ,[EventContactPhone]
                        ,[EventContactEmail]
                        ,[AdministrativeContactPersonAliasId]
                        ,[AdministrativeContactPhone]
                        ,[AdministrativeContactEmail]
                        ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_Reservation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState])
	            Select Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[LocationLayoutId])
	            Select Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[LocationLayoutId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
            END
            ";

            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates the tables V33.
        /// </summary>
        private void UpdateTablesV33()
        {
            var sqlQuery = @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
            BEGIN
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId])
	            Select Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId]
	            From [dbo].[_com_centralaz_RoomManagement_Resource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_Question]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IconCssClass]
                       ,[FinalApprovalGroupId]
                       ,[SuperAdminGroupId]
                       ,[NotificationEmailId]
                       ,[DefaultSetupTime]
                       ,[IsCommunicationHistorySaved]
                       ,[IsNumberAttendingRequired]
                       ,[IsContactDetailsRequired]
                       ,[IsSetupTimeRequired]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[IsSystem]
                           ,[Name]
                           ,[Description]
                           ,[IsActive]
                           ,[IconCssClass]
                           ,[FinalApprovalGroupId]
                           ,[SuperAdminGroupId]
                           ,[NotificationEmailId]
                           ,[DefaultSetupTime]
                           ,[IsCommunicationHistorySaved]
                           ,[IsNumberAttendingRequired]
                           ,[IsContactDetailsRequired]
                           ,[IsSetupTimeRequired]
                           ,[Guid]
                           ,[CreatedDateTime]
                           ,[ModifiedDateTime]
                           ,[CreatedByPersonAliasId]
                           ,[ModifiedByPersonAliasId]
                           ,[ForeignKey]
                           ,[ForeignGuid]
                           ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId] 
                       ,[ReservationTypeId])
	            Select Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id
                       ,[Name]
                       ,[ScheduleId]
                       ,[CampusId]
                       ,[ReservationMinistryId]
                       ,[ReservationStatusId]
                       ,[RequesterAliasId]
                       ,[ApproverAliasId]
                       ,[SetupTime]
                       ,[CleanupTime]
                       ,[NumberAttending]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[SetupPhotoId]
                       ,[EventContactPersonAliasId]
                       ,[EventContactPhone]
                       ,[EventContactEmail]
                       ,[AdministrativeContactPersonAliasId]
                       ,[AdministrativeContactPhone]
                       ,[AdministrativeContactEmail]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                        ,[ScheduleId]
                        ,[CampusId]
                        ,[ReservationMinistryId]
                        ,[ReservationStatusId]
                        ,[RequesterAliasId]
                        ,[ApproverAliasId]
                        ,[SetupTime]
                        ,[CleanupTime]
                        ,[NumberAttending]
                        ,[Note]
                        ,[Guid]
                        ,[CreatedDateTime]
                        ,[ModifiedDateTime]
                        ,[CreatedByPersonAliasId]
                        ,[ModifiedByPersonAliasId]
                        ,[ForeignKey]
                        ,[ForeignGuid]
                        ,[ForeignId]
                        ,[ApprovalState]
                        ,[SetupPhotoId]
                        ,[EventContactPersonAliasId]
                        ,[EventContactPhone]
                        ,[EventContactEmail]
                        ,[AdministrativeContactPersonAliasId]
                        ,[AdministrativeContactPhone]
                        ,[AdministrativeContactEmail]
                        ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_Reservation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState])
	            Select Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[LocationLayoutId])
	            Select Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,null
	            From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
            END
            ";

            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// SQLs the with long timeout.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        private void SqlWithLongTimeout( string sqlQuery )
        {
            using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandTimeout = 18000;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}