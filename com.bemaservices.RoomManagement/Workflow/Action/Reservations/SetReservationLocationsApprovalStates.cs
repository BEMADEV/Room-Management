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
    /// Sets the approval state of a reservation.
    /// </summary>
    [ActionCategory( "Room Management" )]
    [Description( "Sets the states of a reservation's locations." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Locations Set States" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Approval State Attribute", "The attribute that contains the reservation locations' approval state.", false, "", "", 1, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationLocationApprovalStateFieldType" } )]
    [ReservationLocationApprovalStateField( "Approval State", "The approval state to use (if Approval State Attribute is not specified).", false, "", "", 2 )]
    [BooleanField( "Ignore Locations With Approval Groups", "Whether to skip updating the statuses of locations with approval groups", true, "", 3 )]
    public class SetReservationLocationsApprovalStates : ActionComponent
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

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = new ReservationService( rockContext ).Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get reservation approval state
            ReservationLocationApprovalState? approvalState = null;
            Guid? approvalStateAttributeGuid = GetAttributeValue( action, "ApprovalStateAttribute" ).AsGuidOrNull();
            if ( approvalStateAttributeGuid.HasValue )
            {
                approvalState = action.GetWorkflowAttributeValue( approvalStateAttributeGuid.Value ).ConvertToEnumOrNull<ReservationLocationApprovalState>();
            }

            if ( approvalState == null )
            {
                approvalState = GetAttributeValue( action, "ApprovalState" ).ConvertToEnumOrNull<ReservationLocationApprovalState>();
            }

            if ( approvalState == null )
            {
                errorMessages.Add( "Invalid Approval State Attribute or Value!" );
                return false;
            }

            // Get ignore locations with approval groups value
            bool ignoreLocationsWithApprovalGroups = true;
            ignoreLocationsWithApprovalGroups = GetAttributeValue( action, "IgnoreLocationsWithApprovalGroups" ).AsBoolean( true );

            var changes = new History.HistoryChangeList();
            var groupService = new GroupService( rockContext );
            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                Group approvalGroup = null;
                var location = reservationLocation.Location;
                location.LoadAttributes();
                var approvalGroupGuid = location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();
                if ( approvalGroupGuid.HasValue )
                {
                    approvalGroup = groupService.Get( approvalGroupGuid.Value );
                }

                if ( approvalGroup == null || !ignoreLocationsWithApprovalGroups )
                {
                    var oldValue = reservationLocation.ApprovalState;

                    reservationLocation.ApprovalState = approvalState.Value;

                    if ( oldValue != reservationLocation.ApprovalState )
                    {
                        History.EvaluateChange( changes, String.Format( "{0} Approval State", reservationLocation.Location.Name ), oldValue.ToString(), reservationLocation.ApprovalState.ToString() );
                    }
                }
            }

            if ( changes.Any() )
            {
                changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format( "Updated by the '{0}' workflow", action.ActionTypeCache.ActivityType.WorkflowType.Name ) ) );
                HistoryService.SaveChanges( rockContext, typeof( Reservation ), com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(), reservation.Id, changes, false );
            }

            rockContext.SaveChanges();

            return true;
        }
    }
}