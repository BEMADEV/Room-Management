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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.bemaservices.RoomManagement.Migrations;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
namespace com.bemaservices.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// Class ScheduledLocationItemPicker.
    /// Implements the <see cref="Rock.Web.UI.Controls.ItemPicker" />
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.ItemPicker" />
    [ToolboxData( "<{0}:ScheduledLocationItemPicker runat=server></{0}:ScheduledLocationItemPicker>" )]
    public class ScheduledLocationItemPicker : Rock.Web.UI.Controls.ItemPicker
    {
        #region Controls

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>The reservation type identifier.</value>
        public int? ReservationTypeId
        {
            get { return ViewState["ReservationTypeId"] as int? ?? 0; }
            set
            {
                ViewState["ReservationTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>The reservation identifier.</value>
        public int? ReservationId
        {
            get { return ViewState["ReservationId"] as int? ?? 0; }
            set
            {
                ViewState["ReservationId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the setup time.
        /// </summary>
        /// <value>The setup time.</value>
        public int? SetupTime
        {
            get { return ViewState["SetupTime"] as int? ?? 0; }
            set
            {
                ViewState["SetupTime"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the cleanup time.
        /// </summary>
        /// <value>The cleanup time.</value>
        public int? CleanupTime
        {
            get { return ViewState["CleanupTime"] as int? ?? 0; }
            set
            {
                ViewState["CleanupTime"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the content of the i calendar.
        /// </summary>
        /// <value>The content of the i calendar.</value>
        public string ICalendarContent
        {
            get { return ViewState["ICalendarContent"] as string ?? string.Empty; }
            set
            {
                ViewState["ICalendarContent"] = value;
            }
        }

        public int? AttendeeCount
        {
            get { return ViewState["AttendeeCount"] as int? ?? 0; }
            set
            {
                ViewState["AttendeeCount"] = value;
            }
        }

        #endregion


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "/0";
            this.IconCssClass = "fa fa-home";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetValue( Rock.Model.Location location )
        {
            if ( location != null )
            {
                ItemId = location.Id.ToString();
                List<int> parentLocationIds = new List<int>();
                var parentLocation = location.ParentLocation;

                while ( parentLocation != null )
                {
                    if ( parentLocationIds.Contains( parentLocation.Id ) )
                    {
                        // infinite recursion
                        break;
                    }

                    parentLocationIds.Insert( 0, parentLocation.Id );
                    ;
                    parentLocation = parentLocation.ParentLocation;
                }

                InitialItemParentIds = parentLocationIds.AsDelimited( "," );
                ItemName = location.ToString();
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="locations">The locations.</param>
        public void SetValues( IEnumerable<Location> locations )
        {
            var theLocations = locations.ToList();

            if ( theLocations.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                List<int> parentLocationIds = new List<int>();

                foreach ( var location in theLocations )
                {
                    if ( location != null )
                    {
                        ids.Add( location.Id.ToString() );
                        names.Add( location.Name );
                        var parentLocation = location.ParentLocation;

                        while ( parentLocation != null )
                        {
                            if ( parentLocationIds.Contains( parentLocation.Id ) )
                            {
                                // infinite recursion
                                break;
                            }

                            parentLocationIds.Insert( 0, parentLocation.Id );
                            ;
                            parentLocation = parentLocation.ParentLocation;
                        }
                    }
                }

                InitialItemParentIds = parentLocationIds.AsDelimited( "," );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new LocationService( new RockContext() ).Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var ids = this.SelectedValuesAsInt().ToList();
            var items = new LocationService( new RockContext() ).Queryable().Where( i => ids.Contains( i.Id ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>The item rest URL.</value>
        public override string ItemRestUrl
        {
            get { return "~/api/com_bemaservices/ScheduledLocations/GetChildren/"; }
        }

        public void SetExtraRestParams()
        {
            StringBuilder additionalParams = new StringBuilder();
            additionalParams.Append( "?getCategorizedItems=true" );
            additionalParams.Append( "&showUnnamedEntityItems=true" );
            additionalParams.Append( "&showCategoriesThatHaveNoChildren=true" );

            var locationTypeIds = new List<int>();
            int? reservedLocationEntitySetId = null;
            int? conflictedLocationEntitySetId = null;
            using ( var rockContext = new RockContext() )
            {
                var reservationService = new ReservationService( rockContext );

                var newReservation = new Reservation()
                {
                    Id = ReservationId ?? 0,
                    Schedule = ReservationService.BuildScheduleFromICalContent( ICalendarContent ),
                    SetupTime = SetupTime,
                    CleanupTime = CleanupTime
                };
                newReservation = reservationService.SetFirstLastOccurrenceDateTimes( newReservation );

                List<int> reservedLocationIds = reservationService.GetReservedLocationIds( newReservation, false, false, false );
                List<int> conflictedLocationIds = reservationService.GetReservedLocationIds( newReservation, false, true, false );

                if ( ReservationTypeId.HasValue )
                {
                    var reservationType = new ReservationTypeService( rockContext ).Get( ReservationTypeId.Value );
                    locationTypeIds = reservationType.ReservationLocationTypes.Select( rlt => rlt.LocationTypeValueId ).ToList();
                }

                if ( reservedLocationIds.Any() || conflictedLocationIds.Any() )
                {
                    var entitySetService = new EntitySetService( rockContext );

                    if ( reservedLocationIds.Any() )
                    {
                        var options = new AddEntitySetActionOptions
                        {
                            EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.LOCATION ),
                            EntityIdList = reservedLocationIds,
                            ExpiryInMinutes = 120
                        };

                        reservedLocationEntitySetId = entitySetService.AddEntitySet( options );
                    }

                    if ( conflictedLocationIds.Any() )
                    {
                        var options = new AddEntitySetActionOptions
                        {
                            EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.LOCATION ),
                            EntityIdList = conflictedLocationIds,
                            ExpiryInMinutes = 120
                        };

                        conflictedLocationEntitySetId = entitySetService.AddEntitySet( options );
                    }
                }
            }

            if ( locationTypeIds.Any() )
            {
                additionalParams.AppendFormat( "&locationTypeIds={0}", locationTypeIds.AsDelimited( "," ) );
            }

            if ( reservedLocationEntitySetId.HasValue )
            {
                additionalParams.AppendFormat( "&reservedLocationEntitySetId={0}", reservedLocationEntitySetId );
            }

            if ( conflictedLocationEntitySetId.HasValue )
            {
                additionalParams.AppendFormat( "&conflictedLocationEntitySetId={0}", conflictedLocationEntitySetId );
            }

            if ( AttendeeCount.HasValue )
            {
                additionalParams.AppendFormat( "&AttendeeCount={0}", AttendeeCount );
            }

            ItemRestUrlExtraParams = "/0" + additionalParams.ToString();
        }
    }
}