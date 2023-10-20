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
    [MigrationNumber( 39, "1.13.0" )]
    public class RockWorkflowCompliance : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            UpdateApprovalWorkflowLava();
            UpdateSpecialApprovalAttributes();
        }

        private void UpdateSpecialApprovalAttributes()
        {
            Sql( @"
                -- Updates the three keys in the Special Approval Notification
                -- workflow to be lowercase
                UPDATE [Attribute] 
                SET [Key] = LOWER([Key]) 
                WHERE Id IN (
                    SELECT Id
                    FROM Attribute a
                    WHERE a.EntityTypeQualifierColumn = 'WorkflowTypeId'
                    AND a.EntityTypeQualifierValue in (
                        SELECT convert(nvarchar(max),wt.Id)
                        FROM WorkflowType wt
                        WHERE wt.[Guid] = '66899922-D665-4839-8742-BD8556D7FB61'
                    )
                )
                " );
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Special Approval Needed: {{ Workflow | Attribute:'relevantitem'}} for {{Workflow | Attribute:'reservation'}}" ); // Special Approval Notification:Start:Send Notification Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "F07AA85B-B666-4D8C-B770-7B5377A9EF34", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'reservation','Object' %} 
<p>
A new reservation requires your special approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/>
Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/>
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
            
        }

        private void UpdateApprovalWorkflowLava()
        {
            RockMigrationHelper.AddActionTypeAttributeValue( "DB6E99A9-D680-48B7-BCFF-5DDF88D3DEB5", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign groupsNotified = 0 %}
{% assign reservationId = Workflow | Attribute:'Reservation','Id' %}
{% reservation id:'{{reservationId}}' %}
    {% if reservation.ApprovalState == 'PendingSpecialApproval' %}
        {% for reservationLocation in reservation.ReservationLocations %}
            {% if reservationLocation.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationLocation.Location | Attribute:'ApprovalGroup','Object' %}
                {% if approvalGroup != empty %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' approvalgroup:'{{approvalGroup.Guid}}' reservation:'{{ reservation.Guid }}' relevantitem:'{{reservationLocation.Location.Name}}' %}                      
                    {% endworkflowactivate %}
                    {% assign groupsNotified = groupsNotified | Plus:1 %}
                {% endif %}
            {% endif %}
        {% endfor %}
        {% for reservationResource in reservation.ReservationResources %}
            {% if reservationResource.ApprovalState == 'Unapproved' %}
                {% assign approvalGroup = null %}
                {% assign approvalGroup = reservationResource.Resource.ApprovalGroup %}
                {% if approvalGroup != null %}
                    {% workflowactivate workflowtype:'66899922-D665-4839-8742-BD8556D7FB61' approvalgroup:'{{approvalGroup.Guid}}' reservation:'{{ reservation.Guid }}' relevantitem:'{{reservationResource.Resource.Name}}' %}
                    {% endworkflowactivate %}
                    {% assign groupsNotified = groupsNotified | Plus:1 %}
                {% endif %}
            {% endif %}
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
