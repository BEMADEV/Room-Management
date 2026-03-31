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
using System.IO;
using System.Linq;
using com.bemaservices.RoomManagement.Migrations;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Badge.Component;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that returns available locations for a given schedule or date range.
    /// Filters locations by reservation type's allowed location types and optional location IDs.
    /// Returns locations that have no conflicting reservations during the specified time period.
    /// </summary>
    public class ReservableLocations : LavaBlockBase, ILavaSecured
    {
        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "reservablelocations";

        /// <summary>
        /// The attributes markup
        /// </summary>
        private string _attributesMarkup;
        /// <summary>
        /// The render errors
        /// </summary>
        private bool _renderErrors = true;

        /// <summary>
        /// The settings
        /// </summary>
        LavaElementAttributes _settings = new LavaElementAttributes();

        #region Parameter Names

        /// <summary>
        /// The parameter reservation type identifier
        /// </summary>
        public static readonly string ParameterReservationTypeId = "reservationtypeid";

        /// <summary>
        /// The parameter schedule identifier
        /// </summary>
        public static readonly string ParameterScheduleId = "scheduleid";

        /// <summary>
        /// The parameter start date time
        /// </summary>
        public static readonly string ParameterStartDateTime = "startdatetime";

        /// <summary>
        /// The parameter end date time
        /// </summary>
        public static readonly string ParameterEndDateTime = "enddatetime";

        /// <summary>
        /// The parameter location ids
        /// </summary>
        public static readonly string ParameterLocationIds = "locationids";

        /// <summary>
        /// The parameter include descendants
        /// </summary>
        public static readonly string ParameterIncludeDescendants = "includedescendants";

        #endregion

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // first ensure that available locations are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                return;
            }

            try
            {
                _settings.ParseFromMarkup( _attributesMarkup, context );

                var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var reservationTypeService = new ReservationTypeService( rockContext );
                var reservationService = new ReservationService( rockContext );
                var locationService = new LocationService( rockContext );

                // Get parameters
                var reservationTypeId = _settings.GetInteger( ParameterReservationTypeId );
                var scheduleId = _settings.GetIntegerOrNull( ParameterScheduleId );
                var startDateTime = _settings.GetString( ParameterStartDateTime ).AsDateTime();
                var endDateTime = _settings.GetString( ParameterEndDateTime ).AsDateTime();
                var locationIds = _settings.GetString( ParameterLocationIds, string.Empty ).SplitDelimitedValues().AsIntegerList();
                var includeDescendants = _settings.GetBoolean( ParameterIncludeDescendants );

                // Validate required parameters
                var reservationType = reservationTypeService.Get( reservationTypeId );
                if ( reservationType == null )
                {
                    result.Write( "ReservationTypeId parameter is required to pull reservable location types and when booking occurs." );
                    return;
                }

                // Create a temporary reservation to check availability
                var tempReservation = new Reservation();
                tempReservation.ReservationType = reservationType;
                tempReservation.ReservationTypeId = reservationTypeId;

                // Set up the schedule
                if ( scheduleId.HasValue )
                {
                    var scheduleService = new ScheduleService( rockContext );
                    tempReservation.Schedule = scheduleService.Get( scheduleId.Value );
                    if ( tempReservation.Schedule == null )
                    {
                        throw new Exception( $"Schedule with ID {scheduleId.Value} not found" );
                    }
                }
                else if ( startDateTime.HasValue && endDateTime.HasValue )
                {
                    // Create a simple schedule from start/end datetime
                    var iCalContent = $@"BEGIN:VCALENDAR
VERSION:2.0
BEGIN:VEVENT
DTSTART:{startDateTime.Value:yyyyMMddTHHmmss}
DTEND:{endDateTime.Value:yyyyMMddTHHmmss}
END:VEVENT
END:VCALENDAR";
                    tempReservation.Schedule = ReservationService.BuildScheduleFromICalContent( iCalContent );
                }
                else
                {
                    throw new Exception( "Either scheduleid or both startdatetime and enddatetime parameters are required" );
                }

                // Update the first/last occurrence times
                tempReservation = ReservationService.UpdateFirstLastOccurrenceDateTimes( tempReservation );

                // Get the list of locations we care about
                var locationQry = locationService.Queryable().Where( l => l.IsActive );
                if ( reservationType.ReservationLocationTypes != null && reservationType.ReservationLocationTypes.Any() )
                {
                    var locationTypeIds = reservationType.ReservationLocationTypes.Select( rlt => rlt.LocationTypeValueId ).ToList();
                    locationQry = locationQry.Where( l =>
                        l.LocationTypeValueId != null &&
                        locationTypeIds.Contains( l.LocationTypeValueId.Value ) );
                }

                if ( locationIds != null && locationIds.Any() )
                {
                    locationQry = locationQry.Where( l => locationIds.Contains( l.Id ) );
                }

                var filteredLocations = locationQry.ToList();
                if ( includeDescendants )
                {
                    filteredLocations.AddRange( locationQry.Select( l => l.Id ).SelectMany( lId => locationService.GetAllDescendents( lId ) ) );
                }

                // Extract the IDs into a primitive list BEFORE using in the query
                var filteredLocationIds = filteredLocations.Select( l => l.Id ).ToList();

                // Get any Reservations containing related Locations
                var existingReservationQry = reservationService
                    .Queryable()
                    .Where( r => r.ReservationLocations.Any( rl => filteredLocationIds.Contains( rl.LocationId ) ) );

                // Check existing Reservations for conflicts
                IEnumerable<Model.ReservationSummary> conflictingReservationSummaries = reservationService.GetConflictingReservationSummaries( tempReservation, existingReservationQry, false );

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
                reservedLocationAndChildIds.AddRange( reservedLocationIds.SelectMany( l => locationService.GetAllAncestorIds( l ) ) );

                var reservableLocations = filteredLocations.Where( l => !reservedLocationAndChildIds.Contains( l.Id ) ).ToList();

                

                // Add to context
                context["ReservableLocations"] = reservableLocations;
                context["TotalReservableLocations"] = reservableLocations.Count;

                base.OnRender( context, result );
            }
            catch ( Exception ex )
            {
                var message = "Reservable Locations not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "ReservableLocations";
            }
        }

        #endregion
    }
}
