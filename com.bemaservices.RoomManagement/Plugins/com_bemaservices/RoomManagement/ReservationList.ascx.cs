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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using com.bemaservices.RoomManagement.Model;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// Block for viewing list of reservations
    /// </summary>
    [DisplayName( "Reservation List" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Block for viewing a list of reservations." )]

    [LinkedPage( "Detail Page" )]
    [TextField( "Related Entity Query String Parameter", "The query string parameter that holds id to the related entity.", false )]
    public partial class ReservationList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gReservations.DataKeyNames = new string[] { "Id" };
            gReservations.Actions.ShowAdd = false;
            gReservations.GridRebind += gReservations_GridRebind;

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the gReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gReservations_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ReservationId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReservations_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Gfs the settings display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterSetting.MINISTRY:
                    {
                        var reservationMinistryValues = e.Value.Split( ',' ).AsIntegerList();
                        if ( reservationMinistryValues.Any() )
                        {
                            var reservationMinistryService = new ReservationMinistryService( new RockContext() );
                            e.Value = reservationMinistryService.GetByIds( reservationMinistryValues ).Select( r => r.Name ).ToList().AsDelimited( "," );
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case FilterSetting.RESERVATION_TYPE:
                    {
                        var reservationTypeValues = e.Value.Split( ',' ).AsIntegerList();
                        if ( reservationTypeValues.Any() )
                        {
                            var reservationTypeService = new ReservationTypeService( new RockContext() );
                            e.Value = reservationTypeService.GetByIds( reservationTypeValues ).Select( r => r.Name ).ToList().AsDelimited( "," );
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case FilterSetting.APPROVAL_STATE:
                    {
                        var approvalValues = e.Value.Split( ',' ).Select( a => a.ConvertToEnum<ReservationApprovalState>() );
                        if ( approvalValues.Any() )
                        {

                            e.Value = approvalValues.Select( a => a.ConvertToString() ).ToList().AsDelimited( "," );

                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case FilterSetting.START_TIME:
                case FilterSetting.END_TIME:
                    {
                        break;
                    }
                case FilterSetting.CREATED_BY:
                case FilterSetting.ADMIN_CONTACT:
                case FilterSetting.EVENT_CONTACT:
                    {
                        string personName = string.Empty;

                        int? personId = e.Value.AsIntegerOrNull();
                        if ( personId.HasValue )
                        {
                            var personService = new PersonService( new RockContext() );
                            var person = personService.Get( personId.Value );
                            if ( person != null )
                            {
                                personName = person.FullName;
                            }
                        }

                        e.Value = personName;

                        break;
                    }
                case FilterSetting.RESOURCES:
                    {
                        var resourceIdList = e.Value.Split( ',' ).AsIntegerList();
                        if ( resourceIdList.Any() && rpResource.Visible )
                        {
                            var service = new ResourceService( new RockContext() );
                            var resources = service.GetByIds( resourceIdList );
                            if ( resources != null && resources.Any() )
                            {
                                e.Value = resources.Select( a => a.Name ).ToList().AsDelimited( "," );
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case FilterSetting.LOCATIONS:
                    {
                        var locationIdList = e.Value.Split( ',' ).AsIntegerList();
                        if ( locationIdList.Any() && lipLocation.Visible )
                        {
                            var service = new LocationService( new RockContext() );
                            var locations = service.GetByIds( locationIdList );
                            if ( locations != null && locations.Any() )
                            {
                                e.Value = locations.Select( a => a.Name ).ToList().AsDelimited( "," );
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( FilterSetting.RESERVATION_NAME, tbName.Text );
            gfSettings.SaveUserPreference( FilterSetting.APPROVAL_STATE, cblApproval.SelectedValues.AsDelimited( "," ) );
            gfSettings.SaveUserPreference( FilterSetting.MINISTRY, cblMinistry.SelectedValues.AsDelimited( "," ) );
            gfSettings.SaveUserPreference( FilterSetting.RESERVATION_TYPE, cblReservationType.SelectedValues.AsDelimited( "," ) );

            int creatorId = ppCreator.PersonId ?? 0;
            gfSettings.SaveUserPreference( FilterSetting.CREATED_BY, creatorId.ToString() );

            int eventContactId = ppEventContact.PersonId ?? 0;
            gfSettings.SaveUserPreference( FilterSetting.EVENT_CONTACT, eventContactId.ToString() );

            int adminContactId = ppAdminContact.PersonId ?? 0;
            gfSettings.SaveUserPreference( FilterSetting.ADMIN_CONTACT, adminContactId.ToString() );

            gfSettings.SaveUserPreference( FilterSetting.START_TIME, dtpStartDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( FilterSetting.END_TIME, dtpEndDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( FilterSetting.RESOURCES, rpResource.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            gfSettings.SaveUserPreference( FilterSetting.LOCATIONS, lipLocation.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.RESERVATION_NAME ) ) )
            {
                tbName.Text = gfSettings.GetUserPreference( FilterSetting.RESERVATION_NAME );
            }

            // Setup Ministry Filter
            cblMinistry.DataSource = new ReservationMinistryService( rockContext ).Queryable().DistinctBy( rmc => rmc.Name ).OrderBy( m => m.Name );
            cblMinistry.DataBind();

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.MINISTRY ) ) )
            {
                cblMinistry.SetValues( gfSettings.GetUserPreference( FilterSetting.MINISTRY ).SplitDelimitedValues() );
            }

            // Setup Reservation Type Filter
            cblReservationType.DataSource = new ReservationTypeService( rockContext ).Queryable().Where( rt => rt.IsActive ).ToList();
            cblReservationType.DataBind();

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.RESERVATION_TYPE ) ) )
            {
                cblReservationType.SetValues( gfSettings.GetUserPreference( FilterSetting.RESERVATION_TYPE ).SplitDelimitedValues() );
            }

            cblApproval.BindToEnum<ReservationApprovalState>();
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.APPROVAL_STATE ) ) )
            {
                cblApproval.SetValues( gfSettings.GetUserPreference( FilterSetting.APPROVAL_STATE ).SplitDelimitedValues() );
            }

            var personService = new PersonService( rockContext );
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.CREATED_BY ) ) )
            {
                int? personId = gfSettings.GetUserPreference( FilterSetting.CREATED_BY ).AsIntegerOrNull();
                if ( personId.HasValue && personId.Value != 0 )
                {
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppCreator.SetValue( person );
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.EVENT_CONTACT ) ) )
            {
                int? personId = gfSettings.GetUserPreference( FilterSetting.EVENT_CONTACT ).AsIntegerOrNull();
                if ( personId.HasValue && personId.Value != 0 )
                {
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppEventContact.SetValue( person );
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.ADMIN_CONTACT ) ) )
            {
                int? personId = gfSettings.GetUserPreference( FilterSetting.ADMIN_CONTACT ).AsIntegerOrNull();
                if ( personId.HasValue && personId.Value != 0 )
                {
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppAdminContact.SetValue( person );
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.START_TIME ) ) )
            {
                dtpStartDateTime.SelectedDateTime = gfSettings.GetUserPreference( FilterSetting.START_TIME ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.END_TIME ) ) )
            {
                dtpEndDateTime.SelectedDateTime = gfSettings.GetUserPreference( FilterSetting.END_TIME ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.RESOURCES ) ) )
            {
                rpResource.SetValues( gfSettings.GetUserPreference( FilterSetting.RESOURCES ).Split( ',' ).AsIntegerList() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.LOCATIONS ) ) )
            {
                lipLocation.SetValues( gfSettings.GetUserPreference( FilterSetting.LOCATIONS ).Split( ',' ).AsIntegerList() );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            nbMessage.Visible = false;

            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var locationService = new LocationService( rockContext );

            var reservationQueryOptions = new ReservationQueryOptions();
            reservationQueryOptions.Name = tbName.Text;
            reservationQueryOptions.CreatorPersonId = ppCreator.PersonId;
            reservationQueryOptions.EventContactPersonId = ppEventContact.PersonId;
            reservationQueryOptions.AdministrativeContactPersonId = ppAdminContact.PersonId;
            reservationQueryOptions.MinistryNames = cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Text ).ToList();
            reservationQueryOptions.ApprovalStates = cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>() ).ToList();
            reservationQueryOptions.ReservationTypeIds = cblReservationType.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            reservationQueryOptions.ResourceIds = rpResource.SelectedValuesAsInt().ToList();

            var locationIdList = lipLocation.SelectedValuesAsInt().ToList();
            foreach ( var rootLocationId in lipLocation.SelectedValuesAsInt().ToList() )
            {
                locationIdList.AddRange( locationService.GetAllDescendentIds( rootLocationId ) );
                locationIdList.AddRange( locationService.GetAllAncestorIds( rootLocationId ) );
            }
            reservationQueryOptions.LocationIds = locationIdList;

            var qry = reservationService.Queryable( reservationQueryOptions );

            // Get the related entity query string parameter if it was configured
            var relatedEntity = GetAttributeValue( "RelatedEntityQueryStringParameter" );
            int? entityId = null;
            if ( !string.IsNullOrWhiteSpace( relatedEntity ) )
            {
                entityId = PageParameter( relatedEntity ).AsIntegerOrNull();

                if ( entityId != null && RelatedEntities.EventItemOccurrenceId.ToString() == relatedEntity )
                {
                    qry = qry.Where( r => r.ReservationLinkages.Any( rl => rl.EventItemOccurrenceId == entityId ) );
                }
                else
                {
                    ShowMessage( string.Format( "Unsupported Related Entity QueryString Parameter '{0}'", relatedEntity ) );
                }
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var defaultStartDateTime = today;
            var defaultEndDateTime = today.AddMonths( 1 );
            if ( entityId.HasValue )
            {
                defaultStartDateTime = today;
                defaultEndDateTime = today.AddYears( 5 );
            }
            var filterStartDateTime = dtpStartDateTime.SelectedDateTime ?? defaultStartDateTime;
            var filterEndDateTime = dtpEndDateTime.SelectedDateTime ?? defaultEndDateTime;
            var reservationSummaryList = qry.GetReservationSummaries( filterStartDateTime, filterEndDateTime, false );

            // Bind to Grid
            gReservations.DataSource = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationType = r.ReservationType.Name,
                ReservationName = r.ReservationName,
                Locations = r.ReservationLocations.Select( rl => rl.Location.Name ).ToList().AsDelimited( ", " ),
                Resources = r.ReservationResources.Select( rr => rr.Resource.Name ).ToList().AsDelimited( ", " ),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventDateTimeDescription,
                ReservationDateTimeDescription = r.ReservationDateTimeDescription +
                    string.Format( " ({0})", ( r.ReservationStartDateTime.Date == r.ReservationEndDateTime.Date )
                        ? r.ReservationStartDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 )
                        : r.ReservationStartDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 ) + "-" + r.ReservationEndDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 ) ),
                ApprovalState = r.ApprovalState.ConvertToString()
            } )
            .OrderBy( r => r.ReservationStartDateTime ).ToList();
            gReservations.EntityTypeId = EntityTypeCache.Get<Reservation>().Id;
            gReservations.DataBind();
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMessage( string message )
        {
            nbMessage.Visible = true;
            nbMessage.Text = message;
        }

        #endregion


        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            /// <summary>
            /// The reservation name
            /// </summary>
            public const string RESERVATION_NAME = "Reservation Name";
            /// <summary>
            /// The reservation type
            /// </summary>
            public const string RESERVATION_TYPE = "Reservation Type";
            /// <summary>
            /// The ministry
            /// </summary>
            public const string MINISTRY = "Ministry";
            /// <summary>
            /// The created by
            /// </summary>
            public const string CREATED_BY = "Created By";
            /// <summary>
            /// The created by
            /// </summary>
            public const string EVENT_CONTACT = "Event Contact";
            /// <summary>
            /// The created by
            /// </summary>
            public const string ADMIN_CONTACT = "Admin Contact";
            /// <summary>
            /// The locations
            /// </summary>
            public const string LOCATIONS = "Locations";
            /// <summary>
            /// The resources
            /// </summary>
            public const string RESOURCES = "Resources";
            /// <summary>
            /// The start time
            /// </summary>
            public const string START_TIME = "Start Time";
            /// <summary>
            /// The end time
            /// </summary>
            public const string END_TIME = "End Time";
            /// <summary>
            /// The approval state
            /// </summary>
            public const string APPROVAL_STATE = "Approval State";
        }
        #endregion

        /// <summary>
        /// Enum RelatedEntities
        /// </summary>
        private enum RelatedEntities
        {
            /// <summary>
            /// The event item occurrence identifier
            /// </summary>
            EventItemOccurrenceId
        }
    }
}