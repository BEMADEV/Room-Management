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
using System.Linq;

using com.bemaservices.RoomManagement.Attribute;
using com.bemaservices.RoomManagement.Model;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using com.bemaservices.RoomManagement.Migrations;
using System.Text;
using System.Text.RegularExpressions;

namespace com.bemaservices.RoomManagement.Jobs
{
    /// <summary>
    /// Launches a workflow for any reservations that match the configured criteria and sets the 'ReservationId' into
    /// the workflow's attribute.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [SlidingDateRangeField( "Date Range",
        Description = "The range of reservations to fire a workflow for.",
        Key = AttributeKey.DateRange,
        IsRequired = false,
        Order = 0
        )]

    [BooleanField( "Include only reservations that start in date range",
        Key = AttributeKey.StartsInDateRange,
        Order = 1
        )]

    [CustomCheckboxListField( "Reservation States",
        Description = "The Reservation States to kickstart",
        Key = AttributeKey.ReservationStates,
        ListSource = "Pending Initial Approval, Pending Special Approval, Pending Final Approval",
        IsRequired = false,
        Order = 2
        )]

    [WorkflowTypeField( "Workflow Type",
        Description = "The workflow type to fire for eligible reservations.",
        Key = AttributeKey.WorkflowType,
        IsRequired = true,
        Order = 3 )]

    [DisallowConcurrentExecution]
    public class KickstartReservationsWithRemovedApprovalGroups : RockJob
    {

        private static class AttributeKey
        {
            public const string DateRange = "DateRange";
            public const string StartsInDateRange = "StartsInDateRange";
            public const string ReservationStates = "ReservationStates";
            public const string WorkflowType = "WorkflowType";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KickstartReservationsWithRemovedApprovalGroups" /> class.
        /// </summary>
        public KickstartReservationsWithRemovedApprovalGroups()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        public override void Execute()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.DateRange ) ?? "-1||" );
            var startsInDateRange = GetAttributeValue( AttributeKey.StartsInDateRange )
                .ToStringSafe()
                .AsBoolean();
            var reservationStates = GetAttributeValue( AttributeKey.ReservationStates )
                .ToStringSafe()
                .RemoveSpaces()
                .Split( ',' )
                .AsEnumList<ReservationApprovalState>();

            int reservationsProcessed = 0;
            List<string> workflowErrors = new List<string>();

            var rockContext = new RockContext();
            WorkflowTypeCache workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( "WorkflowType" ).ToStringSafe().AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
            }

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var startDate = dateRange.Start ?? DateTime.MinValue;
                var endDate = dateRange.End ?? DateTime.MaxValue;

                var reservationQueryOptions = new ReservationQueryOptions();
                reservationQueryOptions.ApprovalStates = reservationStates;
                var reservationService = new ReservationService( rockContext );
                var reservationIds = reservationService
                    .Queryable( reservationQueryOptions )
                    .AsNoTracking()
                    .GetReservationSummaries( startDate, endDate )
                    .Select( r => r.ReservationId )
                    .Distinct()
                    .ToList();

                var reservationQuery = reservationService.GetByIds( reservationIds );

                var reservationList = new List<Reservation>();

                if ( !reservationStates.Any() || reservationStates.Contains( ReservationApprovalState.PendingInitialApproval ) )
                {
                    var initialApprovalReservations = reservationQuery.Where( r =>
                        r.ApprovalState == ReservationApprovalState.PendingInitialApproval &&
                        r.ReservationType.ReservationApprovalGroups
                            .Where( rag =>
                                rag.ApprovalGroupType == ApprovalGroupType.InitialApprovalGroup &&
                                ( !rag.CampusId.HasValue || rag.CampusId == r.CampusId )
                            )
                            .Any() == false
                        );
                    reservationList.AddRange( initialApprovalReservations );
                }

                if ( !reservationStates.Any() || reservationStates.Contains( ReservationApprovalState.PendingFinalApproval ) )
                {
                    var finalApprovalReservations = reservationQuery.Where( r =>
                        r.ApprovalState == ReservationApprovalState.PendingFinalApproval &&
                        r.ReservationType.ReservationApprovalGroups
                            .Where( rag =>
                                rag.ApprovalGroupType == ApprovalGroupType.FinalApprovalGroup &&
                                ( !rag.CampusId.HasValue || rag.CampusId == r.CampusId )
                            )
                            .Any() == false
                        );
                    reservationList.AddRange( finalApprovalReservations );
                }

                if ( !reservationStates.Any() || reservationStates.Contains( ReservationApprovalState.PendingSpecialApproval ) )
                {
                    var locationApprovalGroupAttributeGuid = "96C07909-E34A-4379-854F-C05E79F772E4".AsGuid();

                    List<int> locationIdsWithApprovalGroups = new List<int>();

                    var attributeValueList = new AttributeValueService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( av => av.Attribute.Guid == locationApprovalGroupAttributeGuid && av.EntityId != null )
                        .ToList();

                    var groupService = new GroupService( rockContext );
                    var validGroupGuidList = new List<Guid>();
                    foreach ( var attributeValue in attributeValueList )
                    {
                        Guid? groupGuid = attributeValue.Value.AsGuidOrNull();
                        if ( groupGuid != null )
                        {
                            if ( validGroupGuidList.Contains( groupGuid.Value ) )
                            {
                                locationIdsWithApprovalGroups.Add( attributeValue.EntityId.Value );
                            }
                            else
                            {
                                var group = groupService.Get( groupGuid.Value );
                                if ( group != null )
                                {
                                    validGroupGuidList.Add( group.Guid );
                                    locationIdsWithApprovalGroups.Add( attributeValue.EntityId.Value );
                                }
                            }
                        }
                    }

                    var specialApprovalReservations = reservationQuery.Where( r =>
                        r.ApprovalState == ReservationApprovalState.PendingSpecialApproval &&
                        r.ReservationResources
                            .Where( rr =>
                                rr.ApprovalState == ReservationResourceApprovalState.Unapproved &&
                                rr.Resource.ApprovalGroupId.HasValue
                            )
                            .Any() == false &&
                        r.ReservationLocations
                            .Where( rl =>
                                rl.ApprovalState == ReservationLocationApprovalState.Unapproved &&
                                locationIdsWithApprovalGroups.Contains( rl.LocationId )
                            )
                            .Any() == false
                        );
                    reservationList.AddRange( specialApprovalReservations );
                }

                foreach ( var reservation in reservationList )
                {
                    try
                    {
                        var workflowService = new WorkflowService( rockContext );
                        var workflow = Rock.Model.Workflow.Activate( workflowType, reservation.Name );

                        // launch workflow
                        workflowService.Process( workflow, reservation, out workflowErrors );

                        reservationsProcessed++;
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                        this.Result += "Exception(s) occurred trying to launch a workflow. ";
                    }
                }
            }

            this.Result += string.Format( "{0} workflows launched{1}", reservationsProcessed, ( workflowErrors.Count > 0 ) ? ", but " + workflowErrors.Count + " workflow errors were reported" : string.Empty );
        }
    }
}