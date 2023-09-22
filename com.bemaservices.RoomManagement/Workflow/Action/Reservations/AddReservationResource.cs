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
    [Description( "Adds a resource to a reservation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Add Resource" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Resource Attribute", "The attribute that contains the resource.", false, "", "", 1, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ResourceFieldType" } )]
    [WorkflowTextOrAttribute( "Quantity", "Attribute Value", "The quantity or an attribute that contains the quantity of the resource. <span class='tip tip-lava'></span>",
        false, "", "", 0, "Quantity", new string[] { "Rock.Field.Types.IntegerFieldType" } )]
    public class AddReservationResource : ActionComponent
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
            var reservationService = new ReservationService( rockContext );
            var mergeFields = GetMergeFields( action );

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = reservationService.Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get the resource
            Resource resource = null;
            Guid resourceGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ResourceAttribute" ).AsGuid() ).AsGuid();
            resource = new ResourceService( rockContext ).Get( resourceGuid );
            if ( resource == null )
            {
                errorMessages.Add( "Invalid Resource Attribute or Value!" );
                return false;
            }

            // Get the Quantity
            int? quantity = GetAttributeValue( action, "Quantity", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();

            if ( ( quantity == null && resource.Quantity.HasValue ) || ( quantity != null && !resource.Quantity.HasValue ) )
            {
                errorMessages.Add( "Invalid Quantity Value!" );
                return false;
            }

            var oldValue = reservation.ApprovalState;
            var changes = new History.HistoryChangeList();

            var reservationResource = new ReservationResource();
            reservation.ReservationResources.Add( reservationResource );
            reservationResource.Reservation = reservation;
            reservationResource.ReservationId = reservation.Id;
            reservationResource.Resource = resource;
            reservationResource.ResourceId = resource.Id;
            reservationResource.Quantity = quantity.Value;

            changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, String.Format( "[Resource] {0} {1}", reservationResource.Quantity, reservationResource.Resource.Name ) ) );

            var availableQuantity = reservationService.GetAvailableResourceQuantity( reservationResource.Resource, reservation, false );
            if ( availableQuantity.HasValue && availableQuantity - reservationResource.Quantity < 0 )
            {
                reservationResource.ApprovalState = ReservationResourceApprovalState.Denied;
                reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
            }

            if ( oldValue != reservation.ApprovalState )
            {
                History.EvaluateChange(
                    changes,
                    "Approval State",
                    oldValue.ToString(),
                    reservation.ApprovalState.ToString() );
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