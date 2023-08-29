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
    [Description( "Creates a reservation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Create" )]

    // Reservation Property Fields
    [WorkflowTextOrAttribute( "Name", "Attribute Value", "The name or an attribute that contains the name of the reservation. <span class='tip tip-lava'></span>",
        false, "", "", 0, "Name", new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Reservation Type Attribute", "The attribute that contains the reservation type of the reservation.",
        false, "", "", 1, null, new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationTypeFieldType" } )]
    [ReservationTypeField( "Reservation Type", "The reservation type to use (if Reservation Type Attribute is not specified).", false, "", "", 2 )]

    [WorkflowAttribute( "Approval State Attribute", "The attribute that contains the reservation approval state.", false, "", "", 3, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationApprovalStateFieldType" } )]
    [ReservationApprovalStateField( "Approval State", "The approval state to use (if Approval State Attribute is not specified).", false, "", "", 4 )]

    [WorkflowAttribute( "Schedule Attribute", "The attribute that contains the reservation schedule.", true, "", "", 5, null,
        new string[] { "Rock.Field.Types.ScheduleFieldType" } )]

    // New Reservation Attribute
    [WorkflowAttribute( "Reservation Attribute", "The reservation attribute to set the value to the reservation created.", true, "", "", 6, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    public class CreateReservation : ActionComponent
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
            var attribute = AttributeCache.Get( GetAttributeValue( action, "ReservationAttribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                var mergeFields = GetMergeFields( action );

                // Get Name
                string name = GetAttributeValue( action, "Name", true ).ResolveMergeFields( mergeFields );

                if ( name.IsNullOrWhiteSpace() )
                {
                    errorMessages.Add( "Invalid Name Value!" );
                    return false;
                }

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

                // Get reservation approval state
                ReservationApprovalState? approvalState = null;
                Guid? approvalStateAttributeGuid = GetAttributeValue( action, "ApprovalStateAttribute" ).AsGuidOrNull();
                if ( approvalStateAttributeGuid.HasValue )
                {
                    approvalState = action.GetWorkflowAttributeValue( approvalStateAttributeGuid.Value ).ConvertToEnumOrNull<ReservationApprovalState>();
                }

                if ( approvalState == null )
                {
                    approvalState = GetAttributeValue( action, "ApprovalState" ).ConvertToEnumOrNull<ReservationApprovalState>();
                }

                if ( approvalState == null )
                {
                    errorMessages.Add( "Invalid Approval State Attribute or Value!" );
                    return false;
                }

                // Get reservation schedule
                Schedule schedule = null;
                Guid? scheduleAttributeGuid = GetAttributeValue( action, "ScheduleAttribute" ).AsGuidOrNull();
                if ( scheduleAttributeGuid.HasValue )
                {
                    schedule = new ScheduleService( rockContext ).Get( action.GetWorkflowAttributeValue( scheduleAttributeGuid.Value ).AsGuid() );
                }

                if ( schedule == null )
                {
                    errorMessages.Add( "Invalid Schedule Attribute!" );
                    return false;
                }

                var changes = new History.HistoryChangeList();

                var reservationService = new ReservationService( rockContext );

                var reservation = new Reservation { Id = 0 };
                reservation.ApprovalState = ReservationApprovalState.Draft;
                changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Reservation" ) );

                reservation.ReservationType = reservationType;
                reservation.ReservationTypeId = reservationType.Id;

                History.EvaluateChange( changes, "Name", reservation.Name, name );
                reservation.Name = name;

                var reservationSchedule = ReservationService.BuildScheduleFromICalContent( schedule.iCalendarContent );
                var scheduleErrorMessage = String.Empty;
                reservation.Schedule = ReservationService.UpdateScheduleWithMaxEndDate( reservationSchedule, reservationType, out scheduleErrorMessage );
                if ( scheduleErrorMessage.IsNotNullOrWhiteSpace() )
                {
                    errorMessages.Add( scheduleErrorMessage );
                    if ( approvalState != ReservationApprovalState.Denied )
                    {
                        approvalState = ReservationApprovalState.ChangesNeeded;
                    }
                }
                History.EvaluateChange( changes, "Schedule", "", reservation.GetFriendlyReservationScheduleText() );

                History.EvaluateChange( changes, "Approval State", reservation.ApprovalState.ToString(), approvalState.ToString() );
                reservation.ApprovalState = approvalState.Value;

                reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );

                reservationService.Add( reservation );
                rockContext.SaveChanges();

                if ( changes.Any() )
                {
                    changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format( "Updated by the '{0}' workflow", action.ActionTypeCache.ActivityType.WorkflowType.Name ) ) );
                    HistoryService.SaveChanges( rockContext, typeof( Reservation ), com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(), reservation.Id, changes, false );
                }


                if ( reservation != null )
                {
                    SetWorkflowAttributeValue( action, attribute.Guid, reservation.Guid.ToString() );
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, reservation.Name ) );
                    return true;
                }
                else
                {
                    errorMessages.Add( "Reservation could not be determined!" );
                }
            }
            else
            {
                errorMessages.Add( "Reservation Attribute could not be found!" );
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