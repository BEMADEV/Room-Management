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
using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.UI.Controls;
using TimeZoneConverter;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class ReservationService.
    /// Implements the <see cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.Reservation}" />
    /// </summary>
    /// <seealso cref="Rock.Data.Service{com.bemaservices.RoomManagement.Model.Reservation}" />
    public class ReservationService : Service<Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationService( RockContext context ) : base( context ) { }

        #region Reservation Methods

        /// <summary>
        /// Gets an <see cref="T:System.Linq.IQueryable`1" /> list of all models
        /// with eager loading of the comma-delimited properties specified in includes
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        public override IQueryable<Reservation> Queryable( string includes )
        {
            return Queryable( includes, new ReservationQueryOptions() );
        }

        /// <summary>
        /// Gets an <see cref="T:System.Linq.IQueryable`1" /> list of all models
        /// Note: You can sometimes improve performance by using Queryable().AsNoTracking(), but be careful. Lazy-Loading doesn't always work with AsNoTracking  https://stackoverflow.com/a/20290275/1755417
        /// </summary>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        public override IQueryable<Reservation> Queryable()
        {
            return Queryable( new ReservationQueryOptions() );
        }

        /// <summary>
        /// Queryables the specified reservation query options.
        /// </summary>
        /// <param name="reservationQueryOptions">The reservation query options.</param>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        public IQueryable<Reservation> Queryable( ReservationQueryOptions reservationQueryOptions )
        {
            return this.Queryable( null, reservationQueryOptions );
        }

        /// <summary>
        /// Queryables the specified includes.
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="reservationQueryOptions">The reservation query options.</param>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        private IQueryable<Reservation> Queryable( string includes, ReservationQueryOptions reservationQueryOptions )
        {
            var qry = base.Queryable( includes );

            reservationQueryOptions = reservationQueryOptions ?? new ReservationQueryOptions();

            if ( reservationQueryOptions.Name.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( r => r.Name.Contains( reservationQueryOptions.Name ) );
            }

            if ( reservationQueryOptions.ReservationTypeIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry.Where( r => reservationQueryOptions.ReservationTypeIds.Contains( r.ReservationTypeId ) );
            }

            if ( reservationQueryOptions.ReservationIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry.Where( r => reservationQueryOptions.ReservationIds.Contains( r.Id ) );
            }

            if ( reservationQueryOptions.CampusIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry
                   .Where( r =>
                       !r.CampusId.HasValue ||    // All
                       reservationQueryOptions.CampusIds.Contains( r.CampusId.Value ) );
            }

            if ( reservationQueryOptions.MinistryIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.ReservationMinistryId.HasValue ||    // All
                        reservationQueryOptions.MinistryIds.Contains( r.ReservationMinistry.Id ) );
            }

            if ( reservationQueryOptions.MinistryNames.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.ReservationMinistryId.HasValue ||    // All
                        reservationQueryOptions.MinistryNames.Contains( r.ReservationMinistry.Name ) );
            }

            if ( reservationQueryOptions.LocationIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationLocations.Any( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && reservationQueryOptions.LocationIds.Contains( rl.LocationId ) ) );
            }

            if ( reservationQueryOptions.ResourceIds.Where( Id => Id != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationResources.Any( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && reservationQueryOptions.ResourceIds.Contains( rr.ResourceId ) ) );
            }

            if ( reservationQueryOptions.ApprovalStates.Any() )
            {
                qry = qry.Where( r => reservationQueryOptions.ApprovalStates.Contains( r.ApprovalState ) );
            }

            if ( reservationQueryOptions.ApprovalsByPersonId != null )
            {
                var approvalPersonId = reservationQueryOptions.ApprovalsByPersonId.Value;

                // NICK TODO: GetLocationsByApprovalGroupMembership is not returning the locations correctly, I'll probably will need to re-write it
                var myLocationsToApproveIds = GetLocationIdsByApprovalGroupMembership( approvalPersonId );

                var myResourcesToApproveIds = GetResourceIdsByApprovalGroupMembership( approvalPersonId );

                qry = qry.Where( r =>
                                    (
                                        r.ApprovalState == ReservationApprovalState.PendingInitialApproval &&
                                        r.ReservationType.ReservationApprovalGroups
                                            .Where( ag =>
                                                ag.ApprovalGroupType == ApprovalGroupType.InitialApprovalGroup &&
                                                ( ag.CampusId == null || ag.CampusId == r.CampusId )
                                                )
                                            .SelectMany( ag => ag.ApprovalGroup.Members )
                                            .Any( m => m.PersonId == approvalPersonId && m.GroupMemberStatus == GroupMemberStatus.Active )
                                    ) ||
                                    (
                                        r.ApprovalState == ReservationApprovalState.PendingFinalApproval &&
                                        r.ReservationType.ReservationApprovalGroups
                                            .Where( ag =>
                                                ag.ApprovalGroupType == ApprovalGroupType.FinalApprovalGroup &&
                                                ( ag.CampusId == null || ag.CampusId == r.CampusId )
                                                )
                                            .SelectMany( ag => ag.ApprovalGroup.Members )
                                            .Any( m => m.PersonId == approvalPersonId && m.GroupMemberStatus == GroupMemberStatus.Active )
                                    ) ||
                                    (
                                        r.ApprovalState == ReservationApprovalState.PendingSpecialApproval &&
                                        (
                                            r.ReservationLocations
                                                .Where( rl => rl.ApprovalState == ReservationLocationApprovalState.Unapproved )
                                                .Any( rl => ( myLocationsToApproveIds.Contains( rl.LocationId ) ) ) ||
                                            r.ReservationResources
                                                .Where( rr => rr.ApprovalState == ReservationResourceApprovalState.Unapproved )
                                                .Any( rr => ( myResourcesToApproveIds.Contains( rr.ResourceId ) ) )
                                        )

                                    )
                                );
            }

            if ( reservationQueryOptions.CreatorPersonId.HasValue )
            {
                qry = qry
                    .Where( r =>
                        r.CreatedByPersonAlias != null &&
                        r.CreatedByPersonAlias.PersonId != null &&
                        r.CreatedByPersonAlias.PersonId == reservationQueryOptions.CreatorPersonId.Value );
            }

            if ( reservationQueryOptions.EventContactPersonId.HasValue )
            {
                qry = qry
                    .Where( r =>
                        r.EventContactPersonAlias != null &&
                        r.EventContactPersonAlias.PersonId != null &&
                        r.EventContactPersonAlias.PersonId == reservationQueryOptions.EventContactPersonId.Value );
            }

            if ( reservationQueryOptions.AdministrativeContactPersonId.HasValue )
            {
                qry = qry
                    .Where( r =>
                        r.AdministrativeContactPersonAlias != null &&
                        r.AdministrativeContactPersonAlias.PersonId != null &&
                        r.AdministrativeContactPersonAlias.PersonId == reservationQueryOptions.AdministrativeContactPersonId.Value );
            }

            if ( reservationQueryOptions.ReservationsByPersonId != null )
            {
                qry = qry.Where( r =>
                    r.AdministrativeContactPersonAlias.PersonId == reservationQueryOptions.ReservationsByPersonId.Value ||
                    r.EventContactPersonAlias.PersonId == reservationQueryOptions.ReservationsByPersonId.Value
                    );
            }

            return qry;
        }

        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="filterStartDateTime">The filter start date time.</param>
        /// <param name="filterEndDateTime">The filter end date time.</param>
        /// <param name="roundToDay">if set to <c>true</c> [round to day].</param>
        /// <param name="includeAttributes">if set to <c>true</c> [include attributes].</param>
        /// <returns>List&lt;Model.ReservationSummary&gt;.</returns>
        public List<Model.ReservationSummary> GetReservationSummaries( IQueryable<Reservation> qry, DateTime filterStartDateTime, DateTime filterEndDateTime, bool roundToDay = false, bool includeAttributes = false )
        {
            return qry.GetReservationSummaries( filterStartDateTime, filterEndDateTime, roundToDay, includeAttributes );
        }

        /// <summary>
        /// Gets the conflicting reservation summaries.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IEnumerable&lt;ReservationSummary&gt;.</returns>
        private IEnumerable<Model.ReservationSummary> GetConflictingReservationSummaries( Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            return GetConflictingReservationSummaries( newReservation, Queryable(), arePotentialConflictsReturned );
        }

        /// <summary>
        /// Gets the conflicting reservation summaries.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="existingReservationQry">The existing reservation qry.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IEnumerable&lt;ReservationSummary&gt;.</returns>
        private List<Model.ReservationSummary> GetConflictingReservationSummaries( Reservation newReservation, IQueryable<Reservation> existingReservationQry, bool arePotentialConflictsReturned = false )
        {
            var newReservationQry = new List<Reservation>() { newReservation }.AsQueryable();
            var filteredExistingReservationQry = existingReservationQry.AsNoTracking().ValidExistingReservations( newReservation.Id, arePotentialConflictsReturned );

            var qryStartTime = RockDateTime.Now;
            if ( newReservation.FirstOccurrenceStartDateTime.HasValue && newReservation.FirstOccurrenceStartDateTime > qryStartTime )
            {
                qryStartTime = newReservation.FirstOccurrenceStartDateTime.Value;
            }

            var qryEndTime = newReservation.LastOccurrenceEndDateTime ?? RockDateTime.Now.AddYears( 1 );
            var newReservationSummaries = newReservationQry.GetReservationSummaries( qryStartTime, qryEndTime );
            var existingReservationSummaries = filteredExistingReservationQry.GetReservationSummaries( qryStartTime, qryEndTime );

            return existingReservationSummaries.WhereConflictsExist( newReservationSummaries );
        }

        /// <summary>
        /// Generates the conflict information.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>System.String.</returns>
        public string GenerateConflictInfo( Reservation reservation, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            // Check to make sure that nothing has a scheduling conflict.
            bool hasConflict = false;
            StringBuilder sb = new StringBuilder();

            if ( arePotentialConflictsReturned )
            {
                sb.Append( "<b>The following items can be reserved, but have also been requested for the scheduled times:</b><br><ul>" );
            }
            else
            {
                sb.Append( "<b>The following items can not be reserved, as they are already reserved for the scheduled times:</b><br><ul>" );
            }

            var reservedLocationIds = GetReservedLocationIds( reservation, true, arePotentialConflictsReturned );

            // Check self
            string message = string.Empty;
            foreach ( var location in reservation.ReservationLocations.Where( l => reservedLocationIds.Contains( l.LocationId ) ) )
            {
                //sb.AppendFormat( "<li>{0}</li>", location.Location.Name );
                message = BuildLocationConflictHtmlList( reservation, location.Location.Id, detailPageRoute, arePotentialConflictsReturned );
                if ( message != null )
                {
                    sb.AppendFormat( "<li>{0} due to:<ul>{1}</ul></li>", location.Location.Name, message );
                }
                else
                {
                    sb.AppendFormat( "<li>{0}</li>", location.Location.Name );
                }
                hasConflict = true;
            }

            // Check resources...
            var resources = reservation.ReservationResources.Select( rr => rr.Resource ).Distinct().ToList();
            foreach ( var resource in resources )
            {
                var reservationQuantity = reservation.ReservationResources.Where( rr => rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity );
                var availableQuantity = GetAvailableResourceQuantity( resource, reservation, arePotentialConflictsReturned );
                if ( availableQuantity.HasValue && availableQuantity - reservationQuantity < 0 )
                {
                    message = BuildResourceConflictHtmlList( reservation, resource.Id, detailPageRoute, arePotentialConflictsReturned );
                    sb.AppendFormat( "<li>{0} [note: only {1} available] due to:<ul>{2}</ul></li>", resource.Name, availableQuantity, message );
                    hasConflict = true;
                }
            }

            if ( hasConflict )
            {
                sb.Append( "</ul>" );
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Updates the approval.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="currentReservationApprovalState">State of the current reservation approval.</param>
        /// <param name="isOverride">if set to <c>true</c> [is override].</param>
        /// <returns>Reservation.</returns>
        [Obsolete]
        public Reservation UpdateApproval( Reservation reservation, ReservationApprovalState currentReservationApprovalState, bool isOverride = false )
        {
            reservation.ApprovalState = currentReservationApprovalState;

            Group finalApprovalGroup = null;
            finalApprovalGroup = reservation.ReservationType.GetApprovalGroup( ApprovalGroupType.FinalApprovalGroup, reservation.CampusId );


            foreach ( var reservationResource in reservation.ReservationResources )
            {
                bool isAutoApprove = ( reservationResource.Resource.ApprovalGroupId == null );

                if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Unapproved || isOverride )
                {
                    if ( isAutoApprove || isOverride )
                    {
                        reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                    }
                    else
                    {
                        reservation.ApprovalState = ReservationApprovalState.PendingInitialApproval;
                    }
                }
                else if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Denied )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                }
            }

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                reservationLocation.Location.LoadAttributes();
                var approvalGroupGuid = reservationLocation.Location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();

                bool isAutoApprove = ( approvalGroupGuid == null );

                if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Unapproved || isOverride )
                {
                    if ( isAutoApprove || isOverride )
                    {
                        reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                    }
                    else
                    {
                        reservation.ApprovalState = ReservationApprovalState.PendingInitialApproval;
                    }
                }
                else if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Denied )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                }
            }



            if ( reservation.ApprovalState == ReservationApprovalState.PendingInitialApproval || reservation.ApprovalState == ReservationApprovalState.PendingFinalApproval || reservation.ApprovalState == ReservationApprovalState.ChangesNeeded )
            {
                if ( reservation.ReservationLocations.All( rl => rl.ApprovalState == ReservationLocationApprovalState.Approved ) && reservation.ReservationResources.All( rr => rr.ApprovalState == ReservationResourceApprovalState.Approved ) )
                {
                    if ( finalApprovalGroup == null )
                    {
                        reservation.ApprovalState = ReservationApprovalState.Approved;
                    }
                    else
                    {
                        reservation.ApprovalState = ReservationApprovalState.PendingFinalApproval;
                    }
                }
                else
                {
                    if ( reservation.ReservationLocations.Any( rl => rl.ApprovalState == ReservationLocationApprovalState.Denied ) || reservation.ReservationResources.Any( rr => rr.ApprovalState == ReservationResourceApprovalState.Denied ) )
                    {
                        reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                    }
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Denied )
            {
                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Denied;
                }

                foreach ( var reservationResource in reservation.ReservationResources )
                {
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Denied;
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Approved )
            {
                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                }

                foreach ( var reservationResource in reservation.ReservationResources )
                {
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                }
            }

            return reservation;
        }

        /// <summary>
        /// Are all locations and resources approved.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <returns><c>true</c> if all locations and resources are approved, <c>false</c> otherwise.</returns>
        public bool AreAllLocationsAndResourcesApproved( Reservation reservation )
        {
            var areAllItemsApproved = true;

            foreach ( var reservationResource in reservation.ReservationResources )
            {
                if ( reservationResource.ApprovalState != ReservationResourceApprovalState.Approved )
                {
                    areAllItemsApproved = false;
                }
            }

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                if ( reservationLocation.ApprovalState != ReservationLocationApprovalState.Approved )
                {
                    areAllItemsApproved = false;
                }
            }

            return areAllItemsApproved;
        }

        /// <summary>
        /// Are the location or resource changes needed.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <returns><c>true</c> if changes are needed, <c>false</c> otherwise.</returns>
        public bool AreLocationOrResourceChangesNeeded( Reservation reservation )
        {
            var areChangesNeeded = false;
            foreach ( var reservationResource in reservation.ReservationResources )
            {
                if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Denied )
                {
                    areChangesNeeded = true;
                }
            }

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Denied )
                {
                    areChangesNeeded = true;
                }
            }

            return areChangesNeeded;
        }

        /// <summary>
        /// Determines whether this instance [can person approve reservation resource] the specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="hasOverrideApproval">if set to <c>true</c> [has override approval].</param>
        /// <param name="reservationResource">The reservation resource.</param>
        /// <returns><c>true</c> if this instance [can person approve reservation resource] the specified person; otherwise, <c>false</c>.</returns>
        public static bool CanPersonApproveReservationResource( Person person, bool hasOverrideApproval, ReservationResource reservationResource )
        {
            bool canApprove = false;

            if ( hasOverrideApproval )
            {
                canApprove = true;
            }
            else
            {
                if ( reservationResource.Resource.ApprovalGroupId == null )
                {
                    canApprove = true;
                }
                else
                {
                    if ( ReservationTypeService.IsPersonInGroupWithId( person, reservationResource.Resource.ApprovalGroupId ) )
                    {
                        canApprove = true;
                    }
                }
            }

            return canApprove;
        }

        /// <summary>
        /// Determines whether this instance [can person approve reservation location] the specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="isSuperAdmin">if set to <c>true</c> [is super admin].</param>
        /// <param name="reservationLocation">The reservation location.</param>
        /// <returns><c>true</c> if this instance [can person approve reservation location] the specified person; otherwise, <c>false</c>.</returns>
        public static bool CanPersonApproveReservationLocation( Person person, bool isSuperAdmin, ReservationLocation reservationLocation )
        {
            bool canApprove = false;

            if ( isSuperAdmin )
            {
                canApprove = true;
            }
            else
            {
                reservationLocation.Location.LoadAttributes();
                var approvalGroupGuid = reservationLocation.Location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();

                if ( approvalGroupGuid == null )
                {
                    canApprove = true;
                }
                else
                {
                    if ( ReservationTypeService.IsPersonInGroupWithGuid( person, approvalGroupGuid ) )
                    {
                        canApprove = true;
                    }
                }
            }

            return canApprove;
        }

        /// <summary>
        /// Builds the content of the schedule from i cal.
        /// </summary>
        /// <param name="filterICalContent">Content of the filter i cal.</param>
        /// <returns>Schedule.</returns>
        public static Schedule BuildScheduleFromICalContent( string filterICalContent )
        {
            var filterSchedule = new Schedule();
            var calEvent = InetCalendarHelper.CreateCalendarEvent( filterICalContent );
            if ( calEvent != null )
            {
                filterSchedule.EffectiveStartDate = calEvent.DtStart != null ? calEvent.DtStart.Value.Date : ( DateTime? ) null;
                filterSchedule.EffectiveEndDate = calEvent.DtEnd != null ? calEvent.DtEnd.Value.Date : ( DateTime? ) null;
            }
            filterSchedule.iCalendarContent = filterICalContent;


            return filterSchedule;
        }

        /// <summary>
        /// Updates the schedule with maximum end date.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="reservationType">Type of the reservation.</param>
        /// <param name="scheduleErrorMessage">The schedule error message.</param>
        /// <returns>Schedule.</returns>
        public static Schedule UpdateScheduleWithMaxEndDate( Schedule schedule, ReservationType reservationType, out string scheduleErrorMessage )
        {
            scheduleErrorMessage = String.Empty;

            /* Enforce maximum duration if specified */
            var maxDuration = reservationType.MaximumReservationDuration;
            if ( maxDuration.HasValue && maxDuration > 0 )
            {
                var scheduledStartTimesOverMax = schedule.GetScheduledStartTimes( DateTime.Now.AddDays( maxDuration.Value ), DateTime.Now.AddDays( maxDuration.Value + 366 ) );

                if ( ( maxDuration > 0 && scheduledStartTimesOverMax != null && scheduledStartTimesOverMax.Count() > 0 ) || schedule.EffectiveEndDate > DateTime.Now.AddDays( maxDuration.Value ) )
                {
                    var icalEvent = schedule.GetICalEvent();

                    var countRules = icalEvent.RecurrenceRules.Where( rule => rule.Count > 0 );
                    if ( countRules.Any() )
                    {
                        var scheduledStartTimesWithinMax = schedule.GetScheduledStartTimes( DateTime.Now, DateTime.Now.AddDays( maxDuration.Value ) );
                        icalEvent.RecurrenceRules.FirstOrDefault().Count = scheduledStartTimesWithinMax.Count;

                        scheduleErrorMessage = "Reservations can only be submitted for " + maxDuration.ToString() + " days.  The recurrence has been adjusted accordingly.";
                    }

                    if ( !countRules.Any() && icalEvent.RecurrenceRules.Count > 0 )
                    {
                        icalEvent.RecurrenceRules[0].Until = DateTime.Now.AddDays( maxDuration.Value );

                        scheduleErrorMessage = "Reservations can only be submitted for " + maxDuration.ToString() + " days.  The recurrence has been adjusted accordingly.";
                    }

                    if ( !countRules.Any() && icalEvent.RecurrenceRules.Count <= 0 )
                    {
                        // single date event beyond the threshold
                        scheduleErrorMessage = "Reservations can only be submitted for " + maxDuration.ToString() + " days.";
                    }

                    Ical.Net.Calendar calendar = new Calendar();
                    calendar.Events.Add( icalEvent );
                    var iCalendarSerializer = new CalendarSerializer( calendar );
                    schedule = BuildScheduleFromICalContent( iCalendarSerializer.SerializeToString( calendar ) );
                }
            }

            return schedule;
        }

        /// <summary>
        /// Sets the first last occurrence date times.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <returns>Reservation.</returns>
        public Reservation SetFirstLastOccurrenceDateTimes( Reservation reservation )
        {

            var beginDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;

            var maximumReservationDate = RockDateTime.Now.AddDays( reservation?.ReservationType?.DefaultReservationDuration ?? 7305 );

            var calEvent = reservation.Schedule.GetICalEvent();
            if ( calEvent != null )
            {
                var countRules = calEvent.RecurrenceRules.Where( rule => rule.Count > 0 );

                DateTime dt1 = new DateTime();

                if ( !countRules.Any() && calEvent.RecurrenceRules.Count > 0 && calEvent.RecurrenceRules[0].Until.Date == dt1 )
                {
                    calEvent.RecurrenceRules[0].Until = maximumReservationDate;
                    Ical.Net.Calendar calendar = new Calendar();
                    calendar.Events.Add( calEvent );
                    var iCalendarSerializer = new CalendarSerializer( calendar );
                    reservation.Schedule = ReservationService.BuildScheduleFromICalContent( iCalendarSerializer.SerializeToString( calendar ) );
                }

                if ( !calEvent.RecurrenceRules.Any( r => ( r.Until != null && r.Until != DateTime.MinValue ) || ( r.Count != null && r.Count != -2147483648 ) ) )
                {
                    endDateTime = maximumReservationDate;
                }

                var occurrences = reservation.GetReservationTimes( beginDateTime, endDateTime ).ToList();
                if ( occurrences.Count > 0 )
                {
                    var firstReservationOccurrence = occurrences.First();
                    var lastReservationOccurrence = occurrences.Last();

                    try
                    {
                        reservation.FirstOccurrenceStartDateTime = firstReservationOccurrence.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 );
                    }
                    catch
                    {
                        reservation.FirstOccurrenceStartDateTime = firstReservationOccurrence.StartDateTime;
                    }

                    try
                    {
                        reservation.LastOccurrenceEndDateTime = lastReservationOccurrence.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 );
                    }
                    catch
                    {
                        reservation.LastOccurrenceEndDateTime = lastReservationOccurrence.EndDateTime;
                    }
                }
            }

            return reservation;
        }

        #endregion

        #region Location Conflict Methods

        /// <summary>
        /// Gets the  location ids for any existing non-denied reservations that have the a location as the ones in the given newReservation object.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="filterByLocations">if set to <c>true</c> [filter by locations].</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <param name="areAncestorsReturned">if set to <c>true</c> [are ancestors returned].</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        public List<int> GetReservedLocationIds( Reservation newReservation, bool filterByLocations = true, bool arePotentialConflictsReturned = false, bool areAncestorsReturned = true )
        {
            var locationService = new LocationService( new RockContext() );

            // Get any Locations related to those reserved by the new Reservation
            var newReservationLocationIds = newReservation.ReservationLocations.Select( rl => rl.LocationId ).ToList();
            var relevantLocationIds = new List<int>();
            relevantLocationIds.AddRange( newReservationLocationIds );
            relevantLocationIds.AddRange( newReservationLocationIds.SelectMany( l => locationService.GetAllAncestorIds( l ) ) );
            relevantLocationIds.AddRange( newReservationLocationIds.SelectMany( l => locationService.GetAllDescendentIds( l ) ) );

            // Get any Reservations containing related Locations
            var existingReservationQry = Queryable();
            if ( filterByLocations )
            {
                existingReservationQry = existingReservationQry.Where( r => r.ReservationLocations.Any( rl => relevantLocationIds.Contains( rl.LocationId ) ) );
            }

            // Check existing Reservations for conflicts
            IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );

            // Grab any locations booked by conflicting Reservations
            var reservedLocationIds = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationLocations.Where( rl =>
                        rl.ApprovalState != ReservationLocationApprovalState.Denied )
                        .Select( rl => rl.LocationId )
                        )
                  .Distinct();

            var reservedLocationAndChildIds = new List<int>();
            reservedLocationAndChildIds.AddRange( reservedLocationIds );
            reservedLocationAndChildIds.AddRange( reservedLocationIds.SelectMany( l => locationService.GetAllDescendentIds( l ) ) );

            if ( areAncestorsReturned )
            {
                reservedLocationAndChildIds.AddRange( reservedLocationIds.SelectMany( l => locationService.GetAllAncestorIds( l ) ) );
            }

            return reservedLocationAndChildIds;
        }

        /// <summary>
        /// Gets the conflicts for location identifier.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>List&lt;ReservationConflict&gt;.</returns>
        public List<ReservationConflict> GetConflictsForLocationId( int locationId, Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            var locationService = new LocationService( new RockContext() );

            var relevantLocationIds = new List<int>();
            relevantLocationIds.Add( locationId );
            relevantLocationIds.AddRange( locationService.GetAllAncestorIds( locationId ) );
            relevantLocationIds.AddRange( locationService.GetAllDescendentIds( locationId ) );

            // Get any Reservations containing related Locations
            var existingReservationQry = Queryable().Where( r => r.ReservationLocations.Any( rl => relevantLocationIds.Contains( rl.LocationId ) ) );

            // Check existing Reservations for conflicts
            IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );
            var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationLocations.Where( rl =>
                        rl.ApprovalState != ReservationLocationApprovalState.Denied &&
                        relevantLocationIds.Contains( rl.LocationId ) )
                     .Select( rl => new ReservationConflict
                     {
                         LocationId = rl.LocationId,
                         LocationName = rl.Location.Name,
                         ReservationId = rl.ReservationId,
                         ReservationName = rl.Reservation.Name,
                         ReservationSchedule = rl.Reservation.Schedule.ToFriendlyScheduleText()
                     } ) )
                 .Distinct()
                 .ToList();

            return locationConflicts;
        }

        /// <summary>
        /// Builds a conflict message string (as HTML List) and returns it if there are location conflicts.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>an HTML List if conflicts exists; null otherwise.</returns>
        public string BuildLocationConflictHtmlList( Reservation newReservation, int locationId, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            var conflicts = GetConflictsForLocationId( locationId, newReservation, arePotentialConflictsReturned );

            if ( conflicts.Any() )
            {
                StringBuilder sb = new StringBuilder();
                detailPageRoute = detailPageRoute.StartsWith( "/" ) ? detailPageRoute : "/" + detailPageRoute;

                foreach ( var conflict in conflicts )
                {
                    sb.AppendFormat( "<li>{0} [on {1} via <a href='{4}?ReservationId={2}' target='_blank'>'{3}'</a>]</li>",
                        conflict.LocationName,
                        conflict.ReservationSchedule,
                        conflict.ReservationId,
                        conflict.ReservationName,
                        detailPageRoute
                        );
                }
                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds a conflict message string (as HTML List) and returns it if there are resource conflicts.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="detailPageRoute">The detail page route.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>an HTML List if conflicts exists; null otherwise.</returns>
        public string BuildResourceConflictHtmlList( Reservation newReservation, int resourceId, string detailPageRoute, bool arePotentialConflictsReturned = false )
        {
            var conflicts = GetConflictsForResourceId( resourceId, newReservation, arePotentialConflictsReturned );

            if ( conflicts.Any() )
            {
                StringBuilder sb = new StringBuilder();
                detailPageRoute = detailPageRoute.StartsWith( "/" ) ? detailPageRoute : "/" + detailPageRoute;

                foreach ( var conflict in conflicts )
                {
                    sb.AppendFormat( "<li>{0} ({5}) [on {1} via <a href='{4}?ReservationId={2}' target='_blank'>'{3}'</a>]</li>",
                        conflict.ResourceName,
                        conflict.ReservationSchedule,
                        conflict.ReservationId,
                        conflict.ReservationName,
                        detailPageRoute,
                        conflict.ResourceQuantity
                        );
                }
                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Resource Conflict Methods

        /// <summary>
        /// Gets the available resource quantity for the given resource, during the time (schedule)
        /// of the given resource -- but excluding the ones used by the given resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>a quantity of available resources at</returns>
        public int? GetAvailableResourceQuantity( Resource resource, Reservation reservation, bool arePotentialConflictsReturned = false )
        {
            // For each new reservation summary, make sure that the quantities of existing summaries that come into contact with it
            // do not exceed the resource's quantity

            if ( !resource.Quantity.HasValue )
            {
                return null;
            }
            else
            {
                var qryStartTime = reservation.FirstOccurrenceStartDateTime ?? RockDateTime.Now.AddMonths( -1 );
                var qryEndTime = reservation.LastOccurrenceEndDateTime ?? RockDateTime.Now.AddYears( 1 );

                // Get all existing non-denied reservations (for a huge time period; a month before now and a year after
                // now) which have the given resource in them.
                var existingValidReservations = Queryable().AsNoTracking().ValidExistingReservations( reservation.Id, arePotentialConflictsReturned ).Where( r => r.ReservationResources.Any( rr => resource.Id == rr.ResourceId ) );
                var existingReservationSummaries = existingValidReservations.GetReservationSummaries( qryStartTime, qryEndTime );

                // Now narrow the reservations down to only the ones in the matching/overlapping time frame
                var newReservationList = new List<Reservation>() { reservation }.AsQueryable();
                var newReservationSummaries = newReservationList.GetReservationSummaries( qryStartTime, qryEndTime );
                var reservedQuantities = newReservationSummaries
                    .Select( newReservationSummary =>
                        newReservationSummary.MatchingSummaries( existingReservationSummaries ).ReservedResourceQuantity( resource.Id )
                   );

                var maxReservedQuantity = reservedQuantities.Count() > 0 ? reservedQuantities.Max() : 0;
                return resource.Quantity - maxReservedQuantity;
            }
        }

        /// <summary>
        /// Gets the conflicts for resource identifier.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="newReservation">The new reservation.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>List&lt;ReservationConflict&gt;.</returns>
        public List<ReservationConflict> GetConflictsForResourceId( int resourceId, Reservation newReservation, bool arePotentialConflictsReturned = false )
        {
            // Get any Reservations containing related Locations
            var existingReservationQry = Queryable().Where( r => r.ReservationResources.Any( rl => rl.ResourceId == resourceId ) );

            // Check existing Reservations for conflicts
            IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation, existingReservationQry, arePotentialConflictsReturned );
            var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationResources.Where( rr =>
                        rr.ApprovalState != ReservationResourceApprovalState.Denied &&
                        rr.ResourceId == resourceId &&
                        rr.Quantity.HasValue )
                     .Select( rr => new ReservationConflict
                     {
                         ResourceId = rr.ResourceId,
                         ResourceName = rr.Resource.Name,
                         ResourceQuantity = rr.Quantity.Value,
                         ReservationId = rr.ReservationId,
                         ReservationName = rr.Reservation.Name,
                         ReservationSchedule = rr.Reservation.Schedule.ToFriendlyScheduleText()
                     } ) )
                 .Distinct()
                 .ToList();
            return locationConflicts;
        }

        #endregion

        #region Location & Resource Approval Helper Methods

        /// <summary>
        /// Gets the locations where the given person is in the location's approval group.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>IEnumerable&lt;Location&gt;.</returns>
        public IEnumerable<int> GetLocationIdsByApprovalGroupMembership( int personId )
        {
            // select location where location.approvalgroup in (select group where group.groupmember contains person ) 
            var rockContext = new RockContext();
            var results = rockContext.Database.SqlQuery<int>(
                $@"
                Declare @PersonId int = {personId}
                SELECT l.Id
                FROM [Location] l
                INNER JOIN [AttributeValue] av ON av.[EntityId] = l.[Id]
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId] AND a.[Guid] = '96C07909-E34A-4379-854F-C05E79F772E4'
                INNER JOIN [Group] g on g.Guid = Try_Cast(av.Value as uniqueidentifier) and g.IsActive = 1 and g.IsArchived = 0
                INNER JOIN [GroupMember] gm ON gm.[GroupId] = g.[Id] and gm.IsArchived = 0 and gm.GroupMemberStatus = 1
                Inner Join Person p on p.Id = gm.PersonId and p.Id = @PersonId
                Group By l.Id
                " ).ToList<int>();

            return results;
        }

        /// <summary>
        /// Gets the resources where the given person is in the resources's approval group.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>IEnumerable&lt;Resource&gt;.</returns>
        public IEnumerable<int> GetResourceIdsByApprovalGroupMembership( int personId )
        {
            //// select location where location.approvalgroup in (select group where group.groupmember contains person ) 
            var rockContext = new RockContext();
            var results = rockContext.Database.SqlQuery<int>(
                $@"
                Declare @PersonId int = {personId}
                SELECT r.Id 
                FROM [_com_bemaservices_RoomManagement_Resource] r
                INNER JOIN [Group] g ON g.[Id] = r.[ApprovalGroupId] and g.IsActive = 1 and g.IsArchived = 0
                INNER JOIN [GroupMember] gm ON gm.[GroupId] = g.[Id] and gm.IsArchived = 0 and gm.GroupMemberStatus = 1
                Inner Join Person p on p.Id = gm.PersonId and p.Id = @PersonId
                Group By r.Id
                " ).ToList<int>();

            return results;
        }

        #endregion

        #region GetReservationCalendarFeed

        /// <summary>
        /// Creates the i calendar.
        /// </summary>
        /// <param name="reservationCalendarOptions">The reservation calendar options.</param>
        /// <returns>System.String.</returns>
        public string CreateICalendar( ReservationCalendarOptions reservationCalendarOptions )
        {
            // Get a list of Rock Reservations that match the specified filter.
            var reservations = this.Queryable( reservationCalendarOptions )
                .Where( r => r.Schedule.EffectiveStartDate <= reservationCalendarOptions.EndDate && reservationCalendarOptions.StartDate <= r.Schedule.EffectiveEndDate )
                .ToList();

            // Create the iCalendar.
            var iCalendar = new Calendar();

            // Specify the calendar timezone using the Internet Assigned Numbers Authority (IANA) identifier, because most third-party applications
            // require this to interpret event times correctly.
            var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );

            // If the client is Outlook, do not set the basic Event Description property.
            var setEventDescription = ( reservationCalendarOptions.ClientDeviceType != "Outlook" );

            // Keep track of the earliest event date/time, so we can use it to set the calendar's time zone info below.
            var earliestEventDateTime = RockDateTime.Now;

            foreach ( var reservation in reservations )
            {
                var reservationSequenceNo = GetSequenceNumber( reservation.CreatedDateTime, reservation.ModifiedDateTime );

                if ( reservation.Schedule == null )
                {
                    continue;
                }

                // Calculate a sequence number for the event item occurrence.
                //
                // The `CalendarEvent.Sequence` represents the revision number for a specific event occurrence. Many
                // calendaring applications will not update an existing event with the same `CalendarEvent.Uid`
                // unless the sequence number is greater than the last time an event with the same unique ID was
                // sent. We assign a sequence number based on the number of seconds difference between the dates on
                // which the Rock event component instances were first created and last modified. Furthermore, there
                // are multiple Rock components within a given event object graph to consider; the final sequence
                // number will be the highest of all sequence numbers calculated within a given event object graph.
                // For more information, refer to https://icalendar.org/iCalendar-RFC-5545/3-8-7-4-sequence-number.html.

                var sequenceNo = reservationSequenceNo;

                var occurrenceSequenceNo = GetSequenceNumber( reservation.CreatedDateTime, reservation.ModifiedDateTime );
                if ( sequenceNo < occurrenceSequenceNo )
                {
                    sequenceNo = occurrenceSequenceNo;
                }

                var scheduleSequenceNo = GetSequenceNumber( reservation.Schedule.CreatedDateTime, reservation.Schedule.ModifiedDateTime );
                if ( sequenceNo < scheduleSequenceNo )
                {
                    sequenceNo = scheduleSequenceNo;
                }

                var startDateTimesAccordingToRock = reservation
                    .GetReservationTimes( reservationCalendarOptions.StartDate, reservationCalendarOptions.EndDate )
                    .Select( rdt => rdt.StartDateTime )
                    .ToList();
                if ( !startDateTimesAccordingToRock.Any() )
                {
                    continue;
                }

                var firstStartDateTime = startDateTimesAccordingToRock.OrderBy( dt => dt ).First();
                if ( firstStartDateTime < earliestEventDateTime )
                {
                    earliestEventDateTime = firstStartDateTime;
                }

                var calendarEvent = InetCalendarHelper.CreateCalendarEvent( reservation.Schedule.iCalendarContent );
                if ( calendarEvent?.Start == null )
                {
                    continue;
                }

                calendarEvent.Sequence = sequenceNo;

                // Create a new calendar event copy to prevent thread-safety issues. This might not be a legitimate
                // concern, but we've historically done this, so it doesn't hurt to leave this behavior in place.
                calendarEvent = CopyCalendarEvent( calendarEvent );

                // Fill out the calendar event's details from this reservation.
                SetCalendarEventDetailsFromReservation( calendarEvent, reservation, setEventDescription );

                // In many cases, we can simply use a schedule's iCalendar (RFC 5545) definition to export a given
                // event to the external calendar apps. Testing has proven, however, that some recurring event
                // definitions are not handled consistently among the calendar apps supported by Rock. In these
                // cases, we need to take more of a manual approach to ensure each all calendar apps ultimately
                // reflects Rock's internal event calendar behavior.

                if ( calendarEvent.RecurrenceDates?.Any() == true )
                {
                    // Outlook (Web + Desktop) & Apple calendars don't properly add iCalendar events having specific
                    // recurrence dates; we'll add these recurrences manually to play it safe.
                    calendarEvent.RecurrenceDates.Clear();

                    // Rock's schedule builder only allows EITHER recurrence dates OR recurrence rules to be set;
                    // Let's clear out the recurrence rules just to be sure we know what we're working with.
                    calendarEvent.RecurrenceRules.Clear();

                    foreach ( var startDateTime in startDateTimesAccordingToRock )
                    {
                        var recurrenceCalendarEvent = CopyCalendarEvent( calendarEvent );
                        recurrenceCalendarEvent.Uid = $"{calendarEvent.Uid}_{startDateTime:s}";
                        recurrenceCalendarEvent.DtStart = ConvertToCalDateTime( startDateTime, timeZoneId );

                        SetCalendarEventDateTimeInfo( recurrenceCalendarEvent, timeZoneId );
                        iCalendar.Events.Add( recurrenceCalendarEvent );
                    }

                    // This event item occurrence has been manually added; move on to the next one.
                    continue;
                }

                if ( calendarEvent.RecurrenceRules?.Any() == true )
                {
                    // The various calendar apps supported by Rock all handle recurrence rule-based iCalendar events
                    // slightly differently, when the event's start date itself doesn't follow the recurrence rules
                    // (i.e. a Rock schedule whose start date is on a Friday, but is scheduled to repeat every
                    // Saturday thereafter).
                    //
                    // To determine if this particular event is one of these scenarios, check to see if the schedule's
                    // start date follows the recurrence rules (by checking for an occurrence on the start date).
                    var startDateTime = calendarEvent.Start.Value;
                    var startDateOccurrences = GetOccurrencesExcludingStartDate(
                        reservation.Schedule.iCalendarContent,
                        startDateTime.StartOfDay(),
                        startDateTime.EndOfDay()
                    );

                    if ( !startDateOccurrences.Any() )
                    {
                        // Add the start date as a one-time event (it will be disconnected from the rest of the series).
                        var startDateCalendarEvent = CopyCalendarEvent( calendarEvent );
                        startDateCalendarEvent.Uid = $"{calendarEvent.Uid}_{startDateTime:s}";
                        startDateCalendarEvent.RecurrenceRules.Clear();

                        SetCalendarEventDateTimeInfo( startDateCalendarEvent, timeZoneId );
                        iCalendar.Events.Add( startDateCalendarEvent );

                        // If - for some reason - the start date was the only recurrence (should never happen),
                        // there are no more recurrences to add, so move on to the next event item occurrence.
                        if ( startDateTimesAccordingToRock.Count < 2 )
                        {
                            continue;
                        }

                        // Reassign the calendar event's start date to that of the first recurrence that matches the
                        // recurrence rules (bypass the original start date).
                        calendarEvent.DtStart = ConvertToCalDateTime( startDateTimesAccordingToRock[1], timeZoneId );

                        // Reduce any recurrence rule counts by 1, to account for the start date recurrence we already
                        // added manually, above.
                        var rulesWithCounts = calendarEvent.RecurrenceRules.Where( rr => rr.Count > 0 ).ToList();
                        foreach ( var rule in rulesWithCounts )
                        {
                            rule.Count--;

                            // If this rule's count is now zero, continue to the next event item occurrence.
                            // This would be indicative of a poorly-written recurring schedule, with a
                            // "End after 1 occurrences" rule.
                            if ( rule.Count == 0 )
                            {
                                continue;
                            }
                        }

                        SetCalendarEventDateTimeInfo( calendarEvent, timeZoneId );
                        iCalendar.Events.Add( calendarEvent );

                        // This event item occurrence has been manually added; move on to the next one.
                        continue;
                    }

                    // Else, this event is safe to add as a standard iCalendar event below, since its start date
                    // follows the recurrence rules.
                }

                // One-time events and recurrence rule-based events whose start date follows the recurrence rules
                // can be added as a standard iCalendar event, as all supported calendar apps handle such events in
                // a manner that matches Rock's internal event calendar behavior.
                SetCalendarEventDateTimeInfo( calendarEvent, timeZoneId );
                iCalendar.Events.Add( calendarEvent );
            }

            // Find a non-DST date to use as the earliest supported timezone date, also ensuring that it is not a leap-day.
            // This is necessary to work around a bug in the iCal.Net framework (v4.2.0).
            // See https://github.com/rianjs/ical.net/issues/439.
            var tzInfo = TZConvert.GetTimeZoneInfo( timeZoneId );
            if ( tzInfo.SupportsDaylightSavingTime )
            {
                for ( var i = 0; i < 365; i++ )
                {
                    if ( !tzInfo.IsDaylightSavingTime( earliestEventDateTime ) )
                    {
                        break;
                    }
                    earliestEventDateTime = earliestEventDateTime.AddDays( -1 );
                };
            }

            // Ensure that the target date is not a leap-day.
            // This is necessary to work around a bug in the iCal.Net framework (v4.2.0).
            if ( earliestEventDateTime.Month == 2 && earliestEventDateTime.Day == 29 )
            {
                earliestEventDateTime = earliestEventDateTime.AddDays( -1 );
            }

            iCalendar.AddTimeZone( VTimeZone.FromDateTimeZone( timeZoneId, earliestEventDateTime, includeHistoricalData: true ) );

            // Return a serialized iCalendar.
            var serializer = new CalendarSerializer();
            var calendarString = serializer.SerializeToString( iCalendar );

            return calendarString;
        }

        /// <summary>
        /// Loads the occurrences for the specified iCalendar content string, excluding the occurrence that represents
        /// the calendar event's start date/time, if it doesn't match the specified recurrence dates or recurrence rules.
        /// </summary>
        /// <remarks>
        /// For example, this is helpful when considering a group member's preferred schedule template, with respect to
        /// group scheduling. If we don't exclude start date/times that don't match the recurrence dates or rules in
        /// such scenarios, this can lead to false positives and people being scheduled when they shouldn't be.
        /// </remarks>
        /// <param name="iCalendarContent">The RFC 5545 iCalendar content string.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns>The occurrences.</returns>
        internal static Occurrence[] GetOccurrencesExcludingStartDate( string iCalendarContent, DateTime startDateTime, DateTime? endDateTime )
        {
            var iCalEvent = InetCalendarHelper.CreateCalendarEvent( iCalendarContent );
            if ( iCalEvent == null )
            {
                return new Occurrence[0];
            }

            /*
                8/26/2024 - JPH

                Issue #1:
                ---------
                The iCal.NET library we use to manage iCalendar events has a known issue where recurring events can
                incorrectly-generate an extra occurrence for the specified `DtStart` date/time value, even if:

                    1) `DtStart` doesn't qualify as a recurrence (for events with recurrence rules)
                    2) `DtStart` doesn't appear in the list of specific dates (for events with recurrence dates)

                https://github.com/rianjs/ical.net/issues/431

                Furthermore, the iCal.NET library is no longer maintained, so it's clear we cannot count on this
                issue being fixed on their end any time soon.

                Workaround #1:
                --------------
                The `CalendarEvent` Type is an implementation of the `RecurringComponent` Type, which has a
                `EvaluationIncludesReferenceDate` property that dictates whether to add the `DtStart` date/time
                value as an `Occurrence` in the returned set of occurrences. Simply put, we ultimately need to set
                this property to `false`. But the problem is that it's a readonly property, with the `CalendarEvent`
                Type setting it to `true` upon instantiation. Luckily, the base `RecurringComponent` Type sets this
                property to `false` upon instantiation.

                All we need to do is create a new instance of `RecurringComponent`, copy all of the `CalendarEvent`
                instance's property values over (using their handy `CopyFrom()` method) and get a new, refined set
                of occurrences. We can then compare the two `Occurrence` sets and remove those occurrences from the
                former set that don't appear in the latter. The reason we don't want to simply RETURN the latter
                occurrence set directly, is because the iCal.NET library attaches the underlying Type instance to
                each occurrence's `Source` property, and in local testing, returning an occurrence set with
                `RecurringComponent` source objects can lead to unhandled exceptions being thrown by some downstream
                Rock processes; better to play it safe and return the same object graph structure that's
                historically been returned from this method.

                Issue #2:
                ---------
                Related to Issue #1, events having recurrence rules that include a specified count (i.e. "End after n
                occurrences"), will incorrectly count the `DtStart` occurrence as one of the "n" occurrences, which can
                lead to the final, correct occurrence being excluded from the returned set (because the library thinks
                it has already returned enough occurrences, since it incorrectly included the `DtStart` date/time value
                as an occurrence.

                Workaround #2:
                --------------
                We'll simply increase each recurrence rule count by 1 before getting the first ['CalendarEvent`] set of
                occurrences, then reduce each recurrence rule count back to the original value before getting the second
                [`RecurringComponent`] set of occurrences. This will ensure that the final, correct occurrence doesn't
                get chopped from the set, and will be returned from this method.

                Reason: Recurring Schedules sometimes return incorrect occurrences.
                https://github.com/SparkDevNetwork/Rock/issues/5980
            */

            // Temporarily increase each recurrence rule count by 1, so we don't accidentally exclude the last, correct
            // occurrence from the final set.
            var rulesWithCounts = iCalEvent.RecurrenceRules?.Where( rr => rr.Count > 0 ).ToList();
            if ( rulesWithCounts?.Any() == true )
            {
                rulesWithCounts.ForEach( rr => rr.Count++ );
            }

            // Get the original set of occurrences (which might incorrectly include the `DtStart` date/time value).
            var occurrenceSet = endDateTime.HasValue
                ? iCalEvent.GetOccurrences( startDateTime, endDateTime.Value )
                : iCalEvent.GetOccurrences( startDateTime );

            if ( iCalEvent.RecurrenceRules?.Any() == true || iCalEvent.RecurrenceDates?.Any() == true )
            {
                // Copy the `CalendarEvent` property values into a new `RecurringComponent` instance.
                var recurringComponent = new RecurringComponent();
                recurringComponent.CopyFrom( iCalEvent );

                // Return each recurrence rule count to its original value, since the `RecurringComponent` object won't
                // incorrectly include a non-matching `DtStart` date/time value as an occurrence.
                rulesWithCounts = recurringComponent.RecurrenceRules?.Where( rr => rr.Count > 1 ).ToList();
                if ( rulesWithCounts?.Any() == true )
                {
                    rulesWithCounts.ForEach( rr => rr.Count-- );
                }

                // Get the 2nd set of occurrences (which will not incorrectly include the `DtStart` date/time value).
                var recurringOccurrences = endDateTime.HasValue
                    ? recurringComponent.GetOccurrences( startDateTime, endDateTime.Value )
                    : recurringComponent.GetOccurrences( startDateTime );

                // Refine the final set of occurrences to only those that appear in both sets.
                occurrenceSet = occurrenceSet
                    .Where( o =>
                        o.Period?.StartTime?.Value != null
                        && recurringOccurrences.Any( ro => ro.Period?.StartTime?.Value == o.Period.StartTime.Value )
                    )
                    .OrderBy( o => o.Period.StartTime.Value )
                    .ToHashSet();
            }

            var occurrences = occurrenceSet.ToArray();
            return occurrences;
        }

        /// <summary>
        /// Adjust the date and time information for this event to ensure that the serialized iCalendar data can be
        /// processed by calendaring applications such as Microsoft Outlook Web, Google Calendar and Apple Calendar.
        /// These applications require specific date/time formats and value combinations for a valid import format.
        /// </summary>
        /// <param name="iCalEvent">The iCal.NET calendar event.</param>
        /// <param name="timeZoneId">The IANA time zone identifier.</param>
        private void SetCalendarEventDateTimeInfo( CalendarEvent iCalEvent, string timeZoneId = null )
        {
            // Determine the start and end time for the event.
            // For an all-day event, omit the End date.
            // see https://stackoverflow.com/questions/1716237/single-day-all-day-appointments-in-ics-files
            var start = iCalEvent.Start;

            timeZoneId = timeZoneId ?? iCalEvent.Start.TzId;

            iCalEvent.Start = ConvertToCalDateTime( start, timeZoneId );

            // Determine if this is an all-day event. The Rock ScheduleBuilder component adopts a convention of
            // assigning a 1 second duration to an event if the duration was not specified as part of the input.
            // Therefore, if the event starts at midnight and has a duration of <= 1s, assume it is an all day event.
            var startTime = new TimeSpan( start.Hour, start.Minute, start.Second );
            if ( startTime.TotalSeconds == 0 && ( iCalEvent.Duration == null || iCalEvent.Duration.TotalSeconds <= 1 ) )
            {
                iCalEvent.IsAllDay = true;
            }

            if ( iCalEvent.IsAllDay )
            {
                iCalEvent.End = null;
            }
            else
            {
                iCalEvent.End = ConvertToCalDateTime( iCalEvent.Start.Add( iCalEvent.Duration ), timeZoneId );
            }
        }

        /// <summary>
        /// Sets the calendar event details from reservation.
        /// </summary>
        /// <param name="iCalEvent">The i cal event.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="setEventDescription">if set to <c>true</c> [set event description].</param>
        /// <returns>CalendarEvent.</returns>
        private CalendarEvent SetCalendarEventDetailsFromReservation( CalendarEvent iCalEvent, Reservation reservation, bool setEventDescription )
        {
            string locations = null;
            if ( reservation.ReservationLocations.Any() )
            {
                locations = reservation.ReservationLocations.Select( x => x.Location.Name ).JoinStrings( ", " );
            }

            // We get all of the schedule info from Schedule.iCalendarContent
            iCalEvent.Summary = !string.IsNullOrEmpty( reservation.Name ) ? reservation.Name : string.Empty;
            iCalEvent.Location = !string.IsNullOrEmpty( locations ) ? locations : string.Empty;
            iCalEvent.Uid = reservation.Guid.ToString();

            // Rock has more descriptions than iCal so lets concatenate them
            string description = reservation.Note;

            // Don't set the description prop for outlook to force it to use the X-ALT-DESC property which can have markup.
            if ( setEventDescription )
            {
                iCalEvent.Description = description.ConvertBrToCrLf()
                                                    .Replace( "</P>", "" )
                                                    .Replace( "</p>", "" )
                                                    .Replace( "<P>", Environment.NewLine )
                                                    .Replace( "<p>", Environment.NewLine )
                                                    .Replace( "&nbsp;", " " )
                                                    .SanitizeHtml();
            }

            // HTML version of the description for outlook
            iCalEvent.AddProperty( "X-ALT-DESC;FMTTYPE=text/html", "<html>" + description + "</html>" );

            // classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL"
            iCalEvent.Class = "PUBLIC";

            // add contact info if it exists
            if ( reservation.EventContactPersonAliasId != null )
            {
                iCalEvent.Organizer = new Organizer( string.Format( "MAILTO:{0}", reservation.EventContactPersonAlias.Person.Email ) )
                {
                    CommonName = reservation.EventContactPersonAlias.Person.FullName
                };

                // Outlook doesn't seems to use Contacts or Comments
                string contactName = !string.IsNullOrEmpty( reservation.EventContactPersonAlias.Person.FullName ) ? "Name: " + reservation.EventContactPersonAlias.Person.FullName : string.Empty;
                string contactEmail = !string.IsNullOrEmpty( reservation.EventContactEmail ) ? ", Email: " + reservation.EventContactEmail : string.Empty;
                string contactPhone = !string.IsNullOrEmpty( reservation.EventContactPhone ) ? ", Phone: " + reservation.EventContactPhone : string.Empty;
                string contactInfo = contactName + contactEmail + contactPhone;

                iCalEvent.Contacts.Add( contactInfo );
                iCalEvent.Comments.Add( contactInfo );
            }

            return iCalEvent;
        }

        /// <summary>
        /// Converts to cal date time.
        /// </summary>
        /// <param name="newDateTime">The new date time.</param>
        /// <param name="tzId">The tz identifier.</param>
        /// <returns>CalDateTime.</returns>
        private CalDateTime ConvertToCalDateTime( IDateTime newDateTime, string tzId )
        {
            if ( newDateTime is CalDateTime cdt )
            {
                if ( tzId != null )
                {
                    cdt.TzId = tzId;
                }
                return cdt;
            }

            var dateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

            var newDate = ConvertToCalDateTime( dateTime, tzId );

            return newDate;
        }

        /// <summary>
        /// Converts to cal date time.
        /// </summary>
        /// <param name="newDateTime">The new date time.</param>
        /// <param name="tzId">The tz identifier.</param>
        /// <returns>CalDateTime.</returns>
        private CalDateTime ConvertToCalDateTime( DateTime newDateTime, string tzId )
        {
            var newDate = new CalDateTime( newDateTime );
            if ( tzId != null )
            {
                newDate.TzId = tzId;
            }

            // Set the HasTime property to ensure that iCal.Net serializes the date value as an iCalendar "DATE" rather than a "PERIOD".
            // Microsoft Outlook ignores date values that are expressed using the iCalendar "PERIOD" type.
            // (see: MS-STANOICAL - v20210817 - 2.2.86)
            newDate.HasTime = true;

            return newDate;
        }

        /// <summary>
        /// Gets the sequence number.
        /// </summary>
        /// <param name="createdDateTime">The created date time.</param>
        /// <param name="modifiedDateTime">The modified date time.</param>
        /// <returns>System.Int32.</returns>
        private int GetSequenceNumber( DateTime? createdDateTime, DateTime? modifiedDateTime )
        {
            var minCreatedDateTime = RockDateTime.New( 2020, 1, 1 ).Value;

            createdDateTime = createdDateTime ?? minCreatedDateTime;
            if ( createdDateTime < minCreatedDateTime )
            {
                createdDateTime = minCreatedDateTime;
            }

            modifiedDateTime = modifiedDateTime ?? createdDateTime;
            if ( modifiedDateTime < createdDateTime )
            {
                modifiedDateTime = createdDateTime;
            }

            var sequenceNo = ( int ) modifiedDateTime.Value.Subtract( createdDateTime.Value ).TotalSeconds;
            return sequenceNo;
        }

        /// <summary>
        /// Copies the calendar event.
        /// </summary>
        /// <param name="iCalEvent">The i cal event.</param>
        /// <returns>CalendarEvent.</returns>
        private CalendarEvent CopyCalendarEvent( CalendarEvent iCalEvent )
        {
            // The iCal.Net serializer is not thread-safe, so we need to create a new instance for each serialization.
            // See https://github.com/rianjs/ical.net/issues/553.
            var serializer = new CalendarSerializer();
            var iCalString = serializer.SerializeToString( iCalEvent );

            var eventCopy = Calendar.Load<CalendarEvent>( iCalString )
                .FirstOrDefault();

            return eventCopy;
        }

        #endregion

        /// <summary>
        /// Create a new non-persisted reservation using an existing reservation as a template.
        /// </summary>
        /// <param name="reservationId">The identifier of a reservation to use as a template for the new reservation.</param>
        /// <returns>Reservation.</returns>
        /// <exception cref="Rock.Lava.LavaEngineExceptionEventArgs.Exception"></exception>
        /// <exception cref="System.Exception"></exception>
        public Reservation GetNewFromTemplate( int reservationId )
        {
            var item = this.Queryable()
                           .AsNoTracking()
                           .FirstOrDefault( x => x.Id == reservationId );

            if ( item == null )
            {
                throw new Exception( string.Format( "GetNewFromTemplate method failed. Reservation ID \"{0}\" could not be found.", reservationId ) );
            }

            // Deep-clone the Reservation and reset the properties that connect it to the permanent store.
            var newItem = item.Clone( false );

            newItem.Id = 0;
            newItem.Guid = Guid.NewGuid();
            newItem.ForeignId = null;
            newItem.ForeignGuid = null;
            newItem.ForeignKey = null;

            newItem.CreatedByPersonAlias = null;
            newItem.CreatedByPersonAliasId = null;
            newItem.CreatedDateTime = RockDateTime.Now;
            newItem.ModifiedByPersonAlias = null;
            newItem.ModifiedByPersonAliasId = null;
            newItem.ModifiedDateTime = RockDateTime.Now;

            newItem.ReservationLinkages = new List<ReservationLinkage>();

            // Clear the approval state since that would not be fair otherwise...
            newItem.ApprovalState = ReservationApprovalState.PendingInitialApproval;
            foreach ( var rl in newItem.ReservationLocations )
            {
                rl.ApprovalState = ReservationLocationApprovalState.Unapproved;
            }

            foreach ( var rr in newItem.ReservationResources )
            {
                rr.ApprovalState = ReservationResourceApprovalState.Unapproved;
            }

            if ( item.SetupPhoto != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var newPhoto = item.SetupPhoto.CloneWithoutIdentity();
                    newPhoto.DatabaseData = item.SetupPhoto.DatabaseData.CloneWithoutIdentity();
                    newPhoto.IsTemporary = true;
                    var binaryFileService = new BinaryFileService( rockContext );
                    binaryFileService.Add( newPhoto );
                    rockContext.SaveChanges();
                    newItem.SetupPhoto = newPhoto;
                    newItem.SetupPhotoId = newPhoto.Id;
                }
            }

            return newItem;
        }

        #region Helper Classes

        /// <summary>
        /// Holds the view model for a Reservation Summary
        /// </summary>
        public class ReservationSummary : com.bemaservices.RoomManagement.Model.ReservationSummary
        {
        }


        #endregion
    }
}
