﻿// <copyright>
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
    [Description( "Adds a location to a reservation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Add Location" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Location Attribute", "The attribute that contains the location.", false, "", "", 1, null,
        new string[] { "Rock.Field.Types.LocationFieldType" } )]
    public class AddReservationLocation : ActionComponent
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

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = reservationService.Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get the location
            Location location = null;
            Guid locationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "LocationAttribute" ).AsGuid() ).AsGuid();
            location = new LocationService( rockContext ).Get( locationGuid );
            if ( location == null )
            {
                errorMessages.Add( "Invalid Location Attribute or Value!" );
                return false;
            }

            var oldValue = reservation.ApprovalState;
            var changes = new History.HistoryChangeList();

            var reservationLocation = new ReservationLocation();
            reservation.ReservationLocations.Add( reservationLocation );
            reservationLocation.Reservation = reservation;
            reservationLocation.ReservationId = reservation.Id;
            reservationLocation.Location = location;
            reservationLocation.LocationId = location.Id;

            changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, String.Format( "[Location] {0}", reservationLocation.Location.Name ) ) );

            var reservedLocationIds = reservationService.GetReservedLocationIds( reservation, true, false );
            if ( reservedLocationIds.Contains( location.Id ) )
            {
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Denied;
                reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
            }
            else
            {
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
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