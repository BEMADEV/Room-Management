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
using Ical.Net.Serialization;
using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

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
        /// Returns a queryable collection of all <see cref="Rock.Model.Person"/> entities with eager loading of the properties that are included in the includes parameter.
        /// </summary>
        /// <param name="includes">A <see cref="System.String"/> containing a comma delimited list of properties that should support eager loading.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities with properties that support eager loading.</returns>
        public override IQueryable<Reservation> Queryable( string includes )
        {
            return Queryable( includes, new ReservationQueryOptions() );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities (not including Deceased or Nameless records)
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.Person"/> entities.</returns>
        public override IQueryable<Reservation> Queryable()
        {
            return Queryable( new ReservationQueryOptions() );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities 
        /// using the options specified the <see cref="PersonQueryOptions"/> (default is to exclude deceased people and nameless person records)
        /// </summary>
        /// <param name="personQueryOptions">The person query options.</param>
        /// <returns></returns>
        public IQueryable<Reservation> Queryable( ReservationQueryOptions reservationQueryOptions )
        {
            return this.Queryable( null, reservationQueryOptions );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Person"/> entities with eager loading of properties that are included in the includes parameter.
        /// using the option specified the <see cref="PersonQueryOptions"/> (default is to exclude deceased people, nameless person records and the anonymous visitor. )
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <param name="personQueryOptions">The person query options.</param>
        /// <returns></returns>
        private IQueryable<Reservation> Queryable( string includes, ReservationQueryOptions reservationQueryOptions )
        {
            var qry = base.Queryable( includes );

            reservationQueryOptions = reservationQueryOptions ?? new ReservationQueryOptions();

            if ( reservationQueryOptions.ReservationTypeIds.Any() )
            {
                qry = qry.Where( r => reservationQueryOptions.ReservationTypeIds.Contains( r.ReservationTypeId ) );
            }

            if ( reservationQueryOptions.ReservationIds.Any() )
            {
                qry = qry.Where( r => reservationQueryOptions.ReservationIds.Contains( r.Id ) );
            }

            if ( reservationQueryOptions.LocationIds.Any() )
            {
                qry = qry.Where( r => r.ReservationLocations.Any( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && reservationQueryOptions.LocationIds.Contains( rl.LocationId ) ) );
            }

            if ( reservationQueryOptions.ResourceIds.Any() )
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
            var filteredExistingReservationQry = existingReservationQry.AsNoTracking().ValidExistingReservations( arePotentialConflictsReturned: arePotentialConflictsReturned );

            var newReservationSummaries = newReservationQry.GetReservationSummaries( RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );
            var existingReservationSummaries = filteredExistingReservationQry.GetReservationSummaries( RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );

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
                sb.Append( "<b>The following items can be reserved, but have also been requested for the scheduled times:<br><ul>" );
            }
            else
            {
                sb.Append( "<b>The following items can not be reserved, as they are already reserved for the scheduled times:<br><ul>" );
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
            foreach ( var resource in reservation.ReservationResources )
            {
                var availableQuantity = GetAvailableResourceQuantity( resource.Resource, reservation, arePotentialConflictsReturned );
                if ( availableQuantity.HasValue && availableQuantity - resource.Quantity < 0 )
                {
                    message = BuildResourceConflictHtmlList( reservation, resource.Resource.Id, detailPageRoute, arePotentialConflictsReturned );
                    sb.AppendFormat( "<li>{0} [note: only {1} available] due to:<ul>{2}</ul></li>", resource.Resource.Name, availableQuantity, message );
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

            var maximumReservationDate = RockDateTime.Now.AddDays( reservation.ReservationType.DefaultReservationDuration );

            var calEvent = reservation.Schedule.GetICalEvent();
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
                         Location = rl.Location,
                         ReservationId = rl.ReservationId,
                         Reservation = rl.Reservation
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
                        conflict.Location.Name,
                        conflict.Reservation.Schedule.ToFriendlyScheduleText(),
                        conflict.ReservationId,
                        conflict.Reservation.Name,
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
                        conflict.Resource.Name,
                        conflict.Reservation.Schedule.ToFriendlyScheduleText(),
                        conflict.ReservationId,
                        conflict.Reservation.Name,
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
                // Get all existing non-denied reservations (for a huge time period; a month before now and a year after
                // now) which have the given resource in them.
                var existingValidReservations = Queryable().AsNoTracking().ValidExistingReservations(arePotentialConflictsReturned: arePotentialConflictsReturned).Where( r => r.ReservationResources.Any( rr => resource.Id == rr.ResourceId ) );
                var existingReservationSummaries = existingValidReservations.GetReservationSummaries( RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );

                // Now narrow the reservations down to only the ones in the matching/overlapping time frame
                var newReservationList = new List<Reservation>() { reservation }.AsQueryable();
                var newReservationSummaries = newReservationList.GetReservationSummaries( RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );
                var reservedQuantities = newReservationSummaries
                    .Select( newReservationSummary =>
                        newReservationSummary.MatchingSummaries(existingReservationSummaries).ReservedResourceQuantity(resource.Id)
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
                         Resource = rr.Resource,
                         ResourceQuantity = rr.Quantity.Value,
                         ReservationId = rr.ReservationId,
                         Reservation = rr.Reservation
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

        /// <summary>
        /// Create a new non-persisted reservation using an existing reservation as a template.
        /// </summary>
        /// <param name="reservationId">The identifier of a reservation to use as a template for the new reservation.</param>
        /// <returns>Reservation.</returns>
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
