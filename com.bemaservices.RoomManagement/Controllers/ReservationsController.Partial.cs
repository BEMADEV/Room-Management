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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using static com.bemaservices.RoomManagement.Model.ReservationService;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// API Controller class for the Reservation model
    /// </summary>
    public partial class ReservationsController : Rock.Rest.ApiController<com.bemaservices.RoomManagement.Model.Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationsController" /> class.
        /// </summary>
        public ReservationsController() : base( new com.bemaservices.RoomManagement.Model.ReservationService( new Rock.Data.RockContext() ) ) { }
    }
    public partial class ReservationsController
    {
        /// <summary>
        /// Gets the reservation occurrences.
        /// </summary>
        /// <param name="startDateTime">The start date time. Defaults to current datetime.</param>
        /// <param name="endDateTime">The end date time. Defaults to current datetime plus one month.</param>
        /// <param name="reservationTypeIds">An optional parameter to filter occurrences by reservation types. Should be a list of integers separated by commas.</param>
        /// <param name="reservationIds">An optional parameter to filter occurrences by reservations. Should be a list of integers separated by commas.</param>
        /// <param name="locationIds">An optional parameter to filter occurrences by locations. Should be a list of integers separated by commas.</param>
        /// <param name="resourceIds">An optional parameter to filter occurrences by resources. Should be a list of integers separated by commas.</param>
        /// <param name="approvalStates">An optional parameter to filter occurrences by approval state. Should be a list of strings separated by commas. If this value is null, the method will only return approved reservations.</param>
        /// <param name="filterTimeBy">The filter time by.</param>
        /// <param name="includeAttributes">if set to <c>true</c> [include attributes].</param>
        /// <returns>IQueryable&lt;ReservationOccurrence&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Reservations/GetReservationOccurrences" )]
        public IQueryable<com.bemaservices.RoomManagement.Model.ReservationSummary> GetReservationOccurrences(
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            string reservationTypeIds = null,
            string reservationIds = null,
            string locationIds = null,
            string resourceIds = null,
            string approvalStates = null,
            string filterTimeBy = null,
            bool includeAttributes = false
            )
        {
            RockContext rockContext = new RockContext();
            ReservationService reservationService = new ReservationService( rockContext );

            List<ReservationApprovalState> approvalStateList = new List<ReservationApprovalState>();

            foreach ( var approvalString in approvalStates.SplitDelimitedValues() )
            {
                try
                {
                    approvalStateList.Add( approvalString.ConvertToEnum<ReservationApprovalState>() );
                }
                catch
                {

                }
            }
            if ( !approvalStateList.Any() )
            {
                approvalStateList.Add( ReservationApprovalState.Approved );
            }

            var filterTimeByEnum = filterTimeBy.ConvertToEnum<ReservationExtensionMethods.FilterTimeBy>( ReservationExtensionMethods.FilterTimeBy.Reservation );

            var reservationQueryOptions = new ReservationQueryOptions();
            reservationQueryOptions.ReservationTypeIds = reservationTypeIds.SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.ReservationIds = reservationIds.SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.LocationIds = locationIds.SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.ResourceIds = resourceIds.SplitDelimitedValues().AsIntegerList();
            reservationQueryOptions.ApprovalStates = approvalStateList;

            var reservationSummaryList = reservationService.Queryable( reservationQueryOptions )
                .GetReservationSummaries( startDateTime, endDateTime, false, includeAttributes, null, filterTimeByEnum );

            return reservationSummaryList.AsQueryable();
        }

    }
}

