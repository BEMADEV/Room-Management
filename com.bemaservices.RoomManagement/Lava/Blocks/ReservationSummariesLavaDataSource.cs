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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Ical.Net.DataTypes;
using Rock.Utility;
using Rock.Lava;
using Rock;
using com.bemaservices.RoomManagement.Model;

namespace com.bemaservices.RoomManagement.Lava.Blocks
{
    /// <summary>
    /// Creates filtered sets of reservations as Lava data objects suitable for use in a Lava template.
    /// </summary>
    public class ReservationSummariesLavaDataSource
    {
        #region Filter Parameter Names

        /// <summary>
        /// The parameter name
        /// </summary>
        public static readonly string ParameterName = "name";

        /// <summary>
        /// The parameter approvals by person identifier
        /// </summary>
        public static readonly string ParameterApprovalsByPersonId = "approvalsbypersonid";

        /// <summary>
        /// The parameter reservations by person identifier
        /// </summary>
        public static readonly string ParameterReservationsByPersonId = "reservationsbypersonid";

        /// <summary>
        /// The parameter creator person identifier
        /// </summary>
        public static readonly string ParameterCreatorPersonId = "creatorpersonid";

        /// <summary>
        /// The parameter event contact person identifier
        /// </summary>
        public static readonly string ParameterEventContactPersonId = "eventcontactpersonid";

        /// <summary>
        /// The parameter administrative contact person identifier
        /// </summary>
        public static readonly string ParameterAdministrativeContactPersonId = "administrativecontactpersonid";

        /// <summary>
        /// The parameter reservation type ids
        /// </summary>
        public static readonly string ParameterReservationTypeIds = "reservationtypeids";

        /// <summary>
        /// The parameter reservation ids
        /// </summary>
        public static readonly string ParameterReservationIds = "reservationids";

        /// <summary>
        /// The parameter location ids
        /// </summary>
        public static readonly string ParameterLocationIds = "locationids";

        /// <summary>
        /// The parameter resource ids
        /// </summary>
        public static readonly string ParameterResourceIds = "resourceids";

        /// <summary>
        /// The parameter campus ids
        /// </summary>
        public static readonly string ParameterCampusIds = "campusids";

        /// <summary>
        /// The parameter ministry ids
        /// </summary>
        public static readonly string ParameterMinistryIds = "ministryids";

        /// <summary>
        /// The parameter ministry names
        /// </summary>
        public static readonly string ParameterMinistryNames = "ministrynames";

        /// <summary>
        /// The parameter approval states
        /// </summary>
        public static readonly string ParameterApprovalStates = "approvalstates";

        /// <summary>
        /// The parameter start date time
        /// </summary>
        public static readonly string ParameterStartDateTime = "startdatetime";

        /// <summary>
        /// The parameter end date time
        /// </summary>
        public static readonly string ParameterEndDateTime = "enddatetime";

        /// <summary>
        /// The parameter maximum summaries
        /// </summary>
        public static readonly string ParameterMaxSummaries = "maxsummaries";

        #endregion

        /// <summary>
        /// The maximum number of events that can be retrieved for this data source, regardless of parameter settings.
        /// </summary>
        public static readonly int MaximumResultSetSize = 10000;

        /// <summary>
        /// Get a filtered set of occurrences for a specific calendar.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public List<ReservationSummary> GetReservationSummaries( LavaElementAttributes settings )
        {
            return this.GetReservationSummaries( settings, null );
        }

        /// <summary>
        /// Gets the event occurrences for calendar.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        /// <exception cref="System.Exception">Invalid configuration setting \"{unknownNames.AsDelimited( "," )}\".</exception>
        /// <exception cref="System.Exception">Invalid configuration setting \"maxoccurrences\".</exception>
        public List<ReservationSummary> GetReservationSummaries( LavaElementAttributes settings, RockContext rockContext )
        {
            // Check for invalid parameters.
            var unknownNames = settings.GetUnmatchedAttributes( new List<string> {
                    ParameterName
                    ,ParameterApprovalsByPersonId
                    ,ParameterReservationsByPersonId
                    ,ParameterCreatorPersonId
                    ,ParameterEventContactPersonId
                    ,ParameterAdministrativeContactPersonId
                    ,ParameterReservationTypeIds
                    ,ParameterReservationIds
                    ,ParameterLocationIds
                    ,ParameterResourceIds
                    ,ParameterCampusIds
                    ,ParameterMinistryIds
                    ,ParameterMinistryNames
                    ,ParameterApprovalStates
                    ,ParameterStartDateTime
                    ,ParameterEndDateTime
                    ,ParameterMaxSummaries
            } );

            if ( unknownNames.Any() )
            {
                throw new Exception( $"Invalid configuration setting \"{unknownNames.AsDelimited( "," )}\"." );
            }

            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var reservationQueryOptions = new ReservationQueryOptions();
            reservationQueryOptions.Name = settings.GetStringValue( ParameterName );
            reservationQueryOptions.ApprovalsByPersonId = settings.GetIntegerValue( ParameterApprovalsByPersonId );
            reservationQueryOptions.ReservationsByPersonId = settings.GetIntegerValue( ParameterReservationsByPersonId );
            reservationQueryOptions.CreatorPersonId = settings.GetIntegerValue( ParameterCreatorPersonId );
            reservationQueryOptions.EventContactPersonId = settings.GetIntegerValue( ParameterEventContactPersonId );
            reservationQueryOptions.AdministrativeContactPersonId = settings.GetIntegerValue( ParameterAdministrativeContactPersonId );

            reservationQueryOptions.ReservationTypeIds = settings.GetStringValue( ParameterReservationTypeIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.ReservationIds = settings.GetStringValue( ParameterReservationIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.LocationIds = settings.GetStringValue( ParameterLocationIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.ResourceIds = settings.GetStringValue( ParameterResourceIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.CampusIds = settings.GetStringValue( ParameterCampusIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.MinistryIds = settings.GetStringValue( ParameterMinistryIds, string.Empty ).SplitDelimitedValues().AsIntegerList();

            reservationQueryOptions.MinistryNames = settings.GetStringValue( ParameterMinistryNames, string.Empty ).SplitDelimitedValues().Where( s => s.IsNotNullOrWhiteSpace() ).ToList();

            reservationQueryOptions.ApprovalStates = settings.GetStringValue( ParameterApprovalStates, string.Empty ).SplitDelimitedValues().AsEnumList<ReservationApprovalState>();

            // Get the Maximum Summaries.
            int maxSummaries = 100;

            if ( settings.HasValue( ParameterMaxSummaries ) )
            {
                maxSummaries = settings.GetIntegerValue( ParameterMaxSummaries, null ) ?? 0;

                if ( maxSummaries == 0 )
                {
                    throw new Exception( $"Invalid configuration setting \"maxoccurrences\"." );
                }
            }

            // Get the Date Range.
            var startDate = settings.GetDateTimeValue( ParameterStartDateTime );
            var endDate = settings.GetDateTimeValue( ParameterEndDateTime );

            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable( reservationQueryOptions );
            var qryList = qry.ToList();

            var summaries = qry.GetReservationSummaries( startDate, endDate, false, false, maxSummaries );

            return summaries;
        }
    }
}
