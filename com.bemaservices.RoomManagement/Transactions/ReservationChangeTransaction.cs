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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace com.bemaservices.RoomManagement.Transactions
{
    /// <summary>
    /// Launches a reservation change workflow
    /// </summary>
    public class ReservationChangeTransaction : ITransaction
    {
        /// <summary>
        /// The state
        /// </summary>
        private EntityState State;
        /// <summary>
        /// The reservation unique identifier
        /// </summary>
        private Guid? ReservationGuid;
        /// <summary>
        /// The reservation type identifier
        /// </summary>
        private int ReservationTypeId;
        /// <summary>
        /// The approval state
        /// </summary>
        private ReservationApprovalState ApprovalState;
        /// <summary>
        /// The previous approval state
        /// </summary>
        private ReservationApprovalState PreviousApprovalState;
        /// <summary>
        /// The reservation's ModifiedDateTime before this save; null for new reservations or when no value was stored.
        /// </summary>
        private DateTime? PreviousModifiedDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationChangeTransaction" /> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ReservationChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a reservation, save the values
            var reservation = entry.Entity as Reservation;
            if ( reservation != null )
            {
                State = entry.State;
                ApprovalState = reservation.ApprovalState;
                ReservationTypeId = reservation.ReservationTypeId;

                // If this isn't a new reservation, get the previous state and role values
                if ( State != EntityState.Added )
                {
                    var dbStatusProperty = entry.Property( "ApprovalState" );
                    if ( dbStatusProperty != null )
                    {
                        PreviousApprovalState = ( ReservationApprovalState ) dbStatusProperty.OriginalValue;
                    }

                    var dbModifiedDateTimeProperty = entry.Property( "ModifiedDateTime" );
                    if ( dbModifiedDateTimeProperty != null )
                    {
                        PreviousModifiedDateTime = dbModifiedDateTimeProperty.OriginalValue as DateTime?;
                    }
                }

                // If this isn't a deleted reservation, get the reservation guid
                if ( State != EntityState.Deleted )
                {
                    ReservationGuid = reservation.Guid;
                }
            }
        }

        /// <summary>
        /// Launches the workflows for the reservation type's triggers that match this save. Each trigger uses its own
        /// <see cref="RockContext" /> and error handling, so one failing workflow doesn't stop the remaining triggers.
        /// Run by Rock's transaction queue after <see cref="Reservation.PostSaveChanges" /> enqueues it.
        /// </summary>
        public void Execute()
        {
            // Get all the workflows from cache
            var cachedWorkflowTriggers = ReservationWorkflowTriggerService.GetCachedTriggers();

            // If any workflows exist
            if ( cachedWorkflowTriggers != null && cachedWorkflowTriggers.Any() )
            {
                // Get the workflows associated to the reservation
                var reservationWorkflowTriggers = cachedWorkflowTriggers.Where( t => t.ReservationTypeId == ReservationTypeId ).ToList();

                // Loop through reservationWorkflows and lauch appropriate workflow
                foreach ( var reservationWorkflowTrigger in reservationWorkflowTriggers )
                {
                    try
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            switch ( reservationWorkflowTrigger.TriggerType )
                            {
                                case ReservationWorkflowTriggerType.ReservationCreated:
                                    {
                                        if ( State == EntityState.Added && QualifiersMatch( rockContext, reservationWorkflowTrigger, ApprovalState, ApprovalState ) )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "Reservation Created" );
                                        }
                                        break;
                                    }
                                case ReservationWorkflowTriggerType.ReservationUpdated:
                                    {
                                        if ( State == EntityState.Modified )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "Reservation Updated" );
                                        }
                                        break;
                                    }
                                case ReservationWorkflowTriggerType.StateChanged:
                                    {
                                        if ( State == EntityState.Modified && PreviousApprovalState != ApprovalState && QualifiersMatch( rockContext, reservationWorkflowTrigger, PreviousApprovalState, ApprovalState ) )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "State Changed" );
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( string.Format( "Room Management could not launch the workflow for reservation workflow trigger {0} (reservation {1}).", reservationWorkflowTrigger.Id, ReservationGuid ), ex ) );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether an approval-state transition satisfies the trigger's qualifier. ReservationTypeDetail saves the
        /// qualifier as <c>|From|To|</c>, the same format as Rock's ConnectionWorkflow.QualifierValue, so part [1] is the
        /// previous state and part [2] is the new state. A blank part matches any state, including Draft (0); a part that is
        /// not a valid <see cref="ReservationApprovalState" /> never matches.
        /// Used by <see cref="Execute" /> for Reservation Created triggers (with the same state for both) and State Changed triggers.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="reservationWorkflowTrigger">The reservation workflow trigger.</param>
        /// <param name="prevState">The approval state before the save.</param>
        /// <param name="state">The approval state after the save.</param>
        /// <returns><c>true</c> if both qualifier parts match; otherwise <c>false</c>.</returns>
        private bool QualifiersMatch( RockContext rockContext, ReservationWorkflowTrigger reservationWorkflowTrigger, ReservationApprovalState prevState, ReservationApprovalState state )
        {
            var qualifierParts = ( reservationWorkflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierState = qualifierParts[1].ConvertToEnumOrNull<ReservationApprovalState>();
                if ( qualifierState.HasValue )
                {
                    matches = qualifierState == prevState;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                var qualifierState = qualifierParts[2].ConvertToEnumOrNull<ReservationApprovalState>();
                if ( qualifierState.HasValue )
                {
                    matches = qualifierState == state;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        /// <summary>
        /// Activates and processes the trigger's workflow type for the saved reservation, passing PreviousModifiedDateTime (the
        /// reservation's ModifiedDateTime before this save) when known, and links a persisted workflow to the reservation with a
        /// <see cref="ReservationWorkflow" />. Skips triggers without an active workflow type and reservations that no longer exist.
        /// Called by <see cref="Execute" /> for each matching trigger.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="reservationWorkflowTrigger">The reservation workflow trigger.</param>
        /// <param name="name">The workflow name.</param>
        private void LaunchWorkflow( RockContext rockContext, ReservationWorkflowTrigger reservationWorkflowTrigger, string name )
        {
            if ( !reservationWorkflowTrigger.WorkflowTypeId.HasValue || !ReservationGuid.HasValue )
            {
                return;
            }

            var workflowType = Rock.Web.Cache.WorkflowTypeCache.Get( reservationWorkflowTrigger.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                // The reservation can be deleted between the save and this queued transaction running.
                var reservation = new ReservationService( rockContext ).Get( ReservationGuid.Value );
                if ( reservation != null )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                    // Process() saves the workflow's attribute values if the workflow is persisted, so it only needs to be set here.
                    if ( PreviousModifiedDateTime.HasValue )
                    {
                        workflow.SetAttributeValue( "PreviousModifiedDateTime", PreviousModifiedDateTime.Value );
                    }

                    List<string> workflowErrors;
                    new WorkflowService( rockContext ).Process( workflow, reservation, out workflowErrors );
                    if ( workflow.Id != 0 )
                    {
                        ReservationWorkflow reservationWorkflow = new ReservationWorkflow();
                        reservationWorkflow.ReservationId = reservation.Id;
                        reservationWorkflow.WorkflowId = workflow.Id;
                        reservationWorkflow.ReservationWorkflowTriggerId = reservationWorkflowTrigger.Id;
                        reservationWorkflow.TriggerType = reservationWorkflowTrigger.TriggerType;
                        reservationWorkflow.TriggerQualifier = reservationWorkflowTrigger.QualifierValue;
                        new ReservationWorkflowService( rockContext ).Add( reservationWorkflow );
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}