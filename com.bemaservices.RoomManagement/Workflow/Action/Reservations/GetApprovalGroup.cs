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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Attribute;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.RoomManagement.Workflow.Actions.Reservations
{
    /// <summary>
    /// Creates a reservation.
    /// </summary>
    [ActionCategory( "Room Management" )]
    [Description( "Gets an approval group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Approval Group" )]

    [WorkflowAttribute( "Reservation Type Attribute", "The attribute that contains the reservation type to pull the approval group for",
        false, "", "", 1, null, new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationTypeFieldType" } )]
    [ReservationTypeField( "Reservation Type", "The reservation type to pull the approval group for.", false, "", "", 2 )]

    [WorkflowAttribute( "Approval Group Type Attribute", "The attribute that contains the approval group type.", false, "", "", 3, null,
        new string[] { "Rock.Field.Types.EnumFieldType" } )]
    [EnumField( "Approval Group Type", "The approval group type to pull.", typeof( Model.ApprovalGroupType ), false, "", "", 4 )]

    [WorkflowAttribute( "Campus Attribute", "The attribute that contains the campus.", false, "", "", 5, null,
        new string[] { "Rock.Field.Types.CampusFieldType" } )]
    [CampusField( "Campus", "The campus to pull the approval group type for.", false, "", "", 6 )]

    [WorkflowAttribute( "Group Attribute", "The attribute to set the matching group to.", true, "", "", 7, "GroupAttribute",
        new string[] { "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    public class GetApprovalGroup : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var attribute = AttributeCache.Get( GetAttributeValue( action, "GroupAttribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                var mergeFields = GetMergeFields( action );

                // Get reservation type
                ReservationType reservationType = null;
                var reservationTypeService = new ReservationTypeService( rockContext );
                Guid? reservationTypeAttributeGuid = GetAttributeValue( action, "ReservationTypeAttribute" ).AsGuidOrNull();
                if ( reservationTypeAttributeGuid.HasValue )
                {
                    reservationType = reservationTypeService.Get( action.GetWorkflowAttributeValue( reservationTypeAttributeGuid.Value ).AsGuid() );
                }

                if ( reservationType == null )
                {
                    reservationType = reservationTypeService.Get( GetAttributeValue( action, "ReservationType" ).AsGuid() );
                }

                if ( reservationType == null )
                {
                    errorMessages.Add( "Invalid Reservation Type Attribute or Value!" );
                    return false;
                }

                // Get approval group type
                ApprovalGroupType? approvalGroupType = null;
                Guid? approvalGroupTypeAttributeGuid = GetAttributeValue( action, "ApprovalGroupTypeAttribute" ).AsGuidOrNull();
                if ( approvalGroupTypeAttributeGuid.HasValue )
                {
                    approvalGroupType = action.GetWorkflowAttributeValue( approvalGroupTypeAttributeGuid.Value ).ConvertToEnumOrNull<ApprovalGroupType>();
                }

                if ( approvalGroupType == null )
                {
                    approvalGroupType = GetAttributeValue( action, "ApprovalGroupType" ).ConvertToEnumOrNull<ApprovalGroupType>();
                }

                if ( approvalGroupType == null )
                {
                    errorMessages.Add( "Invalid Approval State Attribute or Value!" );
                    return false;
                }

                // Get Campus
                Campus campus = null;
                var campusService = new CampusService( rockContext );
                Guid? campusAttributeGuid = GetAttributeValue( action, "CampusAttribute" ).AsGuidOrNull();
                if ( campusAttributeGuid.HasValue )
                {
                    campus = campusService.Get( action.GetWorkflowAttributeValue( campusAttributeGuid.Value ).AsGuid() );
                }

                if ( campus == null )
                {
                    campus = campusService.Get( GetAttributeValue( action, "Campus" ).AsGuid() );
                }

                Group group = null;

                var approvalGroupService = new ReservationApprovalGroupService( rockContext );
                var approvalGroupQry = approvalGroupService.Queryable().Where( ag =>
                    ag.ReservationTypeId == reservationType.Id &&
                    ag.ApprovalGroupType == approvalGroupType
                    );

                if ( campus != null )
                {
                    approvalGroupQry = approvalGroupQry.Where( ag => ag.CampusId == null || ag.CampusId == campus.Id );
                }
                else
                {
                    approvalGroupQry = approvalGroupQry.Where( ag => ag.CampusId == null );
                }

                var approvalGroup = approvalGroupQry.OrderBy( a => a.ApprovalGroupType ).ThenBy( a => a.CampusId.HasValue ).ThenBy( a => a.Campus.Name ).FirstOrDefault();

                if ( approvalGroup != null )
                {
                    group = approvalGroup.ApprovalGroup;
                }

                if ( group != null )
                {
                    SetWorkflowAttributeValue( action, attribute.Guid, group.Guid.ToString() );
                    return true;
                }
            }
            else
            {
                errorMessages.Add( "Approval Group Attribute could not be found!" );
            }

            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            return true;
        }
    }
}