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
using System.Linq;
using Rock;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class ReservationExtensionMethods.
    /// </summary>
    public static partial class ReservationExtensionMethods
    {
        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="filterStartDateTime">The filter start date time.</param>
        /// <param name="filterEndDateTime">The filter end date time.</param>
        /// <param name="roundToDay">if set to <c>true</c> [round to day].</param>
        /// <param name="includeAttributes">if set to <c>true</c> [include attributes].</param>
        /// <param name="maxOccurrences">The maximum occurrences.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<Model.ReservationSummary> GetReservationSummaries( this IQueryable<Reservation> qry, DateTime? filterStartDateTime, DateTime? filterEndDateTime, bool roundToDay = false, bool includeAttributes = false, int? maxOccurrences = null )
        {
            var reservationSummaryList = new List<Model.ReservationSummary>();

            if ( qry == null )
            {
                return reservationSummaryList;
            }

            if ( filterStartDateTime == null )
            {
                filterStartDateTime = DateTime.Now;
            }

            if ( filterEndDateTime == null )
            {
                filterEndDateTime = DateTime.Now.AddMonths( 1 );
            }

            if ( filterStartDateTime < DateTime.MinValue.AddYears( 1 ) )
            {
                filterStartDateTime = DateTime.MinValue.AddYears( 1 );
            }
            if ( filterEndDateTime > DateTime.MaxValue.AddYears( -1 ) )
            {
                filterEndDateTime = DateTime.MaxValue.AddYears( -1 );
            }

            var qryStartDateTime = filterStartDateTime.Value.AddMonths( -1 );
            var qryEndDateTime = filterEndDateTime.Value.AddMonths( 1 );
            if ( roundToDay )
            {
                filterEndDateTime = filterEndDateTime.Value.AddDays( 1 ).AddMilliseconds( -1 );
            }

            var reservations = qry
                .Where( r => r.FirstOccurrenceStartDateTime == null || r.FirstOccurrenceStartDateTime <= filterEndDateTime )
                .Where( r => r.LastOccurrenceEndDateTime == null || r.LastOccurrenceEndDateTime >= filterStartDateTime )
                .Where( r => r.Schedule.iCalendarContent.Contains( "RRULE" ) || r.Schedule.iCalendarContent.Contains( "RDATE" ) ||
                        (
                            r.Schedule.EffectiveStartDate >= qryStartDateTime &&
                            r.Schedule.EffectiveEndDate <= qryEndDateTime )
                        )
                        .ToList();

            var reservationsWithDates = reservations
                .Select( r => new ReservationDate
                {
                    Reservation = r,
                    ReservationDateTimes = r.GetReservationTimes( qryStartDateTime, qryEndDateTime )
                } )
                .Where( r => r.ReservationDateTimes.Any() )
                .ToList();

            foreach ( var reservationWithDates in reservationsWithDates )
            {
                var reservation = reservationWithDates.Reservation;

                if ( includeAttributes )
                {
                    reservation.LoadAttributes();
                }

                foreach ( var reservationDateTime in reservationWithDates.ReservationDateTimes )
                {
                    var reservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 );
                    var reservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 );

                    if (
                        ( ( reservationStartDateTime >= filterStartDateTime ) || ( reservationEndDateTime >= filterStartDateTime ) ) &&
                        ( ( reservationStartDateTime < filterEndDateTime ) || ( reservationEndDateTime < filterEndDateTime ) ) )
                    {
                        var reservationSummary = new Model.ReservationSummary
                        {
                            Id = reservation.Id,
                            ReservationType = reservation.ReservationType,
                            ApprovalState = reservation.ApprovalState,
                            ReservationName = reservation.Name,
                            ReservationLocations = reservation.ReservationLocations.ToList(),
                            ReservationResources = reservation.ReservationResources.ToList(),
                            EventStartDateTime = reservationDateTime.StartDateTime,
                            EventEndDateTime = reservationDateTime.EndDateTime,
                            ReservationStartDateTime = reservationStartDateTime,
                            ReservationEndDateTime = reservationEndDateTime,
                            EventDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime ),
                            EventTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime, false ),
                            ReservationDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ) ),
                            ReservationTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ), false ),
                            ReservationMinistry = reservation.ReservationMinistry,
                            EventContactPersonAlias = reservation.EventContactPersonAlias,
                            EventContactEmail = reservation.EventContactEmail,
                            EventContactPhoneNumber = reservation.EventContactPhone,
                            AdministrativeContactPersonAlias = reservation.AdministrativeContactPersonAlias,
                            AdministrativeContactEmail = reservation.AdministrativeContactEmail,
                            AdministrativeContactPhoneNumber = reservation.AdministrativeContactPhone,
                            SetupPhotoId = reservation.SetupPhotoId,
                            Note = reservation.Note,
                            RequesterAlias = reservation.RequesterAlias,
                            NumberAttending = reservation.NumberAttending,
                            ModifiedDateTime = reservation.ModifiedDateTime,
                            ScheduleId = reservation.ScheduleId
                        };

                        if ( includeAttributes )
                        {
                            reservationSummary.Attributes = reservation.Attributes;
                            reservationSummary.AttributeValues = reservation.AttributeValues;

                            foreach ( var reservationLocation in reservationSummary.ReservationLocations )
                            {
                                reservationLocation.LoadAttributes();
                            }

                            foreach ( var reservationResource in reservationSummary.ReservationResources )
                            {
                                reservationResource.LoadAttributes();
                            }
                        }

                        reservationSummaryList.Add( reservationSummary );

                        // Exit if the number of instance of this specific event has exceeded the occurrence limit.
                        if ( maxOccurrences != null && reservationSummaryList.Count >= maxOccurrences )
                        {
                            break;
                        }
                    }
                }
            }

            // Pass 2: Sort all of the event occurrences by date, and then apply the occurrence limit.
            if(maxOccurrences != null )
            {
                reservationSummaryList = reservationSummaryList
                    .OrderBy( x => x.ReservationStartDateTime )
                    .Take( maxOccurrences.Value )
                    .ToList();

            }

            return reservationSummaryList;
        }

        /// <summary>
        /// Valids the existing reservations.
        /// </summary>
        /// <param name="reservations">The reservations.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        public static IQueryable<Reservation> ValidExistingReservations( this IQueryable<Reservation> reservations, int? reservationId = null, bool arePotentialConflictsReturned = false )
        {
            var validReservations = reservations.Where( r => r.ApprovalState != ReservationApprovalState.Denied
                                                                     && r.ApprovalState != ReservationApprovalState.Draft
                                                                     && r.ApprovalState != ReservationApprovalState.Cancelled
                                                                     && (
                                                                         ( arePotentialConflictsReturned == false && ( !r.ReservationType.IsReservationBookedOnApproval || r.ApprovalState == ReservationApprovalState.Approved ) ) ||
                                                                         ( arePotentialConflictsReturned == true && r.ReservationType.IsReservationBookedOnApproval && r.ApprovalState != ReservationApprovalState.Approved )
                                                                         )

                                                         );
            if ( reservationId != null )
            {
                validReservations = validReservations.Where( r => r.Id != reservationId );
            }

            // Make sure communication wasn't just recently approved
            return validReservations;
        }

        /// <summary>
        /// Wheres the conflicts exist.
        /// </summary>
        /// <param name="existingReservationSummaries">The existing reservation summaries.</param>
        /// <param name="newReservationSummaries">The new reservation summaries.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<ReservationSummary> WhereConflictsExist( this List<ReservationSummary> existingReservationSummaries, List<ReservationSummary> newReservationSummaries )
        {
            var conflictingSummaries = existingReservationSummaries.Where( existingReservationSummary => existingReservationSummary.MatchingSummaries( newReservationSummaries ).Any() ).ToList();
            return conflictingSummaries;
        }

        /// <summary>
        /// Matchings the summaries.
        /// </summary>
        /// <param name="sourceReservationSummary">The source reservation summary.</param>
        /// <param name="potentialSummaryMatches">The potential summary matches.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<ReservationSummary> MatchingSummaries( this ReservationSummary sourceReservationSummary, List<ReservationSummary> potentialSummaryMatches )
        {
            var matchingSummaries = potentialSummaryMatches.Where( potentialSummaryMatch =>
                 ( sourceReservationSummary.ReservationStartDateTime > potentialSummaryMatch.ReservationStartDateTime || sourceReservationSummary.ReservationEndDateTime > potentialSummaryMatch.ReservationStartDateTime ) &&
                 ( sourceReservationSummary.ReservationStartDateTime < potentialSummaryMatch.ReservationEndDateTime || sourceReservationSummary.ReservationEndDateTime < potentialSummaryMatch.ReservationEndDateTime )
                 ).ToList();
            return matchingSummaries;
        }

        /// <summary>
        /// Reserveds the resource quantity.
        /// </summary>
        /// <param name="reservationSummaries">The reservation summaries.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>System.Int32.</returns>
        public static int ReservedResourceQuantity( this List<ReservationSummary> reservationSummaries, int resourceId )
        {
            var reservedQuantity = reservationSummaries
                .DistinctBy( reservationSummary => reservationSummary.Id )
                .Sum( reservationSummary =>
                    reservationSummary.ReservationResources
                    .Where( rr => rr.Quantity.HasValue && rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resourceId )
                    .Sum( rr => rr.Quantity.Value )
                    );
            return reservedQuantity;
        }

        /// <summary>
        /// Gets the friendly schedule description.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="showDate">if set to <c>true</c> [show date].</param>
        /// <returns>System.String.</returns>
        public static string GetFriendlyScheduleDescription( DateTime startDateTime, DateTime endDateTime, bool showDate = true )
        {
            if ( startDateTime.Date == endDateTime.Date )
            {
                if ( showDate )
                {
                    return String.Format( "{0} {1} - {2}", startDateTime.ToString( "MM/dd" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
                else
                {
                    return String.Format( "{0} - {1}", startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
            }
            else
            {
                return String.Format( "{0} {1} - {2} {3}", startDateTime.ToString( "MM/dd/yy" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "MM/dd/yy" ), endDateTime.ToString( "hh:mmt" ).ToLower() );
            }
        }

        /// <summary>
        /// Clones this Reservation object to a new Reservation object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns>Reservation.</returns>
        public static Reservation Clone( this Reservation source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as Reservation;
            }
            else
            {
                var target = new Reservation();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another Reservation object to this Reservation object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this Reservation target, Reservation source )
        {
            target.Id = source.Id;
            target.Name = source.Name;

            target.Schedule = source.Schedule;
            target.ScheduleId = source.ScheduleId;

            target.CampusId = source.CampusId;
            target.EventItemOccurrenceId = source.EventItemOccurrenceId;
            target.ReservationMinistryId = source.ReservationMinistryId;

            //target.ApprovalState = source.ApprovalState;
            target.RequesterAliasId = source.RequesterAliasId;
            //target.ApproverAliasId = source.ApproverAliasId;
            target.SetupTime = source.SetupTime;
            target.CleanupTime = source.CleanupTime;
            target.NumberAttending = source.NumberAttending;
            target.Note = source.Note;
            target.SetupPhotoId = source.SetupPhotoId;
            target.EventContactPersonAlias = source.EventContactPersonAlias;
            target.EventContactPersonAliasId = source.EventContactPersonAliasId;
            target.EventContactPhone = source.EventContactPhone;
            target.EventContactEmail = source.EventContactEmail;
            target.AdministrativeContactPersonAlias = source.AdministrativeContactPersonAlias;
            target.AdministrativeContactPersonAliasId = source.AdministrativeContactPersonAliasId;
            target.AdministrativeContactPhone = source.AdministrativeContactPhone;
            target.AdministrativeContactEmail = source.AdministrativeContactEmail;

            target.ReservationLocations = source.ReservationLocations;
            target.ReservationResources = source.ReservationResources;

            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
        }

    }
}
