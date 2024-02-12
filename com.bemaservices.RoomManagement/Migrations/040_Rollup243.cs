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
    [MigrationNumber( 040, "1.14.0" )]
    public class Rollup243 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            UpdateApprovalProcess();
            UpdateSpecialApprovalNotification();
        }

        /// <summary>
        /// Updates the special approval notification.
        /// </summary>
        private void UpdateSpecialApprovalNotification()
        {
            RockMigrationHelper.UpdateWorkflowActionType( "E92F7E39-7C7B-45E4-97BA-7ECD197E7642", "Decrypt Relevant Items", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "09C99356-6585-40BA-AAF5-89BCDD265118" ); // Special Approval Notification:Start:Decrypt Relevant Items
            RockMigrationHelper.UpdateWorkflowActionType( "E92F7E39-7C7B-45E4-97BA-7ECD197E7642", "Send Notification Email", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "F07AA85B-B666-4D8C-B770-7B5377A9EF34" ); // Special Approval Notification:Start:Send Notification Email
            RockMigrationHelper.AddActionTypeAttributeValue( "09C99356-6585-40BA-AAF5-89BCDD265118", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ Workflow | Attribute:'relevantitem' | UrlDecode }}" ); // Special Approval Notification:Start:Decrypt Relevant Items:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "09C99356-6585-40BA-AAF5-89BCDD265118", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Special Approval Notification:Start:Decrypt Relevant Items:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "09C99356-6585-40BA-AAF5-89BCDD265118", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"b8f2580d-64f1-4610-9e93-6f0de2cefafb" ); // Special Approval Notification:Start:Decrypt Relevant Items:Attribute
          
        }

        /// <summary>
        /// Updates the approval process.
        /// </summary>
        private void UpdateApprovalProcess()
        {
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign groupsNotified = 0 %}
{% assign reservationId = Workflow | Attribute:'Reservation','Id' %}
{% reservation id:'{{reservationId}}' securityenabled:'false' %}
    {% if reservation.ApprovalState == 'PendingSpecialApproval' %}
        {% assign approvalGroups = '' %}
    
        {% for reservationLocation in reservation.ReservationLocations %}
            {% if reservationLocation.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationLocation.Location | Attribute:'ApprovalGroup','Object' %}
                {% if approvalGroup != empty %}
                    {% assign existingItems = approvalGroups[approvalGroup.Guid] | Default:'' %}
                    {% assign approvalGroups = approvalGroups | RemoveFromDictionary:approvalGroup.Guid %}
                    {% assign relevantItems = existingItems | Append:reservationLocation.Location.Name | Append:', ' %}
                    {% assign approvalGroups = approvalGroups | AddToDictionary:approvalGroup.Guid, relevantItems %}
                {% endif %}
            {% endif %}
        {% endfor %}
        
        {% for reservationResource in reservation.ReservationResources %}
            {% if reservationResource.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationResource.Resource.ApprovalGroup %}
                {% if approvalGroup != null %}
                    {% assign existingItems = approvalGroups[approvalGroup.Guid] | Default:'' %}
                    {% assign approvalGroups = approvalGroups | RemoveFromDictionary:approvalGroup.Guid %}
                    {% assign relevantItems = existingItems | Append:reservationResource.Resource.Name | Append:', ' %}
                    {% assign approvalGroups = approvalGroups | AddToDictionary:approvalGroup.Guid, relevantItems %}
                {% endif %}
            {% endif %}
        {% endfor %}
        
        {% assign keys = approvalGroups | AllKeysFromDictionary %}
        {% for key in keys %}
            {% assign relevantItems = approvalGroups[key] | ReplaceLast:', ','' | ReplaceLast:', ',' and ' | UrlEncode %}
            {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' approvalgroup:'{{key}}' reservation:'{{ reservation.Guid }}' relevantitem:'{{relevantItems}}' %}
            {% endworkflowactivate %}
            {% assign groupsNotified = groupsNotified | Plus:1 %}
        {% endfor %}
    {% else %}
        {% assign groupsNotified = -1 %}
    {% endif %}
{% endreservation %}

{{groupsNotified}}" ); // Approval Process:Pending Special Approval:Send Notifications to Any Special Approval Groups:Lava
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }
    }
}
