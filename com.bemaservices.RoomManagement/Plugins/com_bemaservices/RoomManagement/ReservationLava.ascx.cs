﻿// <copyright>
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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Attribute;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using com.bemaservices.RoomManagement.ReportTemplates;
using System.Web;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// Class ReservationLava.
    /// Implements the <see cref="Rock.Web.UI.RockBlock" />
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Reservation Lava" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Renders a list of reservations in lava." )]

    [CustomRadioListField( "Location Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 1, category: "Filter Settings" )]
    [CustomRadioListField( "Resource Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 2, category: "Filter Settings" )]
    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 3, category: "Filter Settings" )]
    [CustomRadioListField( "Ministry Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "MinistryFilterDisplayMode", order: 4, category: "Filter Settings" )]
    [CustomRadioListField( "Approval Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "ApprovalFilterDisplayMode", order: 5, category: "Filter Settings" )]
    [CustomRadioListField( "Reservation Type Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "ReservationTypeFilterDisplayMode", order: 6, category: "Filter Settings" )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 7, category: "Filter Settings" )]

    [LinkedPage( "Details Page", "Detail page for events", order: 8, category: "Lava Settings" )]
    [DefinedValueField( "13B169EA-A090-45FF-8B11-A9E02776E35E", "Visible Printable Report Options", "The Printable Reports that the user is able to select", true, true, "5D53E2F0-BA82-4154-B996-085C979FACB0,46C855B0-E50E-49E7-8B99-74561AFB3DD2", "Lava Settings", 9 )]
    [DefinedValueField( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Visible Reservation View Options", "The Reservation Views that the user is able to select", true, true, "67EA36B0-D861-4399-998E-3B69F7700DC0", "Lava Settings", 10 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "Lava Settings", 11 )]

    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week", order: 12, category: "View Settings" )]
    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 13, category: "View Settings" )]
    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 14, category: "View Settings" )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 15, category: "View Settings" )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 16, category: "View Settings" )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 17, category: "View Settings" )]
    [BooleanField( "Show Year View", "Determines whether the year view option is shown", false, order: 18, category: "View Settings" )]
    [BooleanField( "Download Reports", "When printing a report should it default to an attachment to download or inline response to the browser display?", true, order: 19, category: "View Settings" )]

    public partial class ReservationLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// The first day of week
        /// </summary>
        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        /// <summary>
        /// Gets or sets a value indicating whether [location panel open].
        /// </summary>
        /// <value><c>true</c> if [location panel open]; otherwise, <c>false</c>.</value>
        protected bool LocationPanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [location panel closed].
        /// </summary>
        /// <value><c>true</c> if [location panel closed]; otherwise, <c>false</c>.</value>
        protected bool LocationPanelClosed { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [resource panel open].
        /// </summary>
        /// <value><c>true</c> if [resource panel open]; otherwise, <c>false</c>.</value>
        protected bool ResourcePanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [resource panel closed].
        /// </summary>
        /// <value><c>true</c> if [resource panel closed]; otherwise, <c>false</c>.</value>
        protected bool ResourcePanelClosed { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [campus panel open].
        /// </summary>
        /// <value><c>true</c> if [campus panel open]; otherwise, <c>false</c>.</value>
        protected bool CampusPanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [campus panel closed].
        /// </summary>
        /// <value><c>true</c> if [campus panel closed]; otherwise, <c>false</c>.</value>
        protected bool CampusPanelClosed { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [ministry panel open].
        /// </summary>
        /// <value><c>true</c> if [ministry panel open]; otherwise, <c>false</c>.</value>
        protected bool MinistryPanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [ministry panel closed].
        /// </summary>
        /// <value><c>true</c> if [ministry panel closed]; otherwise, <c>false</c>.</value>
        protected bool MinistryPanelClosed { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [approval panel open].
        /// </summary>
        /// <value><c>true</c> if [approval panel open]; otherwise, <c>false</c>.</value>
        protected bool ApprovalPanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [approval panel closed].
        /// </summary>
        /// <value><c>true</c> if [approval panel closed]; otherwise, <c>false</c>.</value>
        protected bool ApprovalPanelClosed { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [reservation type panel open].
        /// </summary>
        /// <value><c>true</c> if [reservation type panel open]; otherwise, <c>false</c>.</value>
        protected bool ReservationTypePanelOpen { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [reservation type panel closed].
        /// </summary>
        /// <value><c>true</c> if [reservation type panel closed]; otherwise, <c>false</c>.</value>
        protected bool ReservationTypePanelClosed { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the view mode.
        /// </summary>
        /// <value>The view mode.</value>
        private String ViewMode { get; set; }

        /// <summary>
        /// Gets or sets the reservation view identifier.
        /// </summary>
        /// <value>The reservation view identifier.</value>
        private int? ReservationViewId { get; set; }
        /// <summary>
        /// Gets or sets the filter start date.
        /// </summary>
        /// <value>The filter start date.</value>
        private DateTime? FilterStartDate { get; set; }
        /// <summary>
        /// Gets or sets the filter end date.
        /// </summary>
        /// <value>The filter end date.</value>
        private DateTime? FilterEndDate { get; set; }
        /// <summary>
        /// Gets or sets the reservation dates.
        /// </summary>
        /// <value>The reservation dates.</value>
        private List<DateTime> ReservationDates { get; set; }

        /// <summary>
        /// Gets the preference key.
        /// </summary>
        /// <value>The preference key.</value>
        private String PreferenceKey
        {
            get
            {
                return string.Format( "reservation-lava-{0}-", this.BlockId );
            }
        }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ViewMode = ViewState["ViewMode"] as String;
            ReservationViewId = ViewState["ReservationViewId"] as int?;
            FilterStartDate = ViewState["FilterStartDate"] as DateTime?;
            FilterEndDate = ViewState["FilterEndDate"] as DateTime?;
            ReservationDates = ViewState["ReservationDates"] as List<DateTime>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _firstDayOfWeek = GetAttributeValue( "StartofWeekDay" ).ConvertToEnum<DayOfWeek>();

            lVersionText.Text = com.bemaservices.RoomManagement.VersionInfo.GetPluginProductVersionNumber();

            LocationPanelOpen = GetAttributeValue( "LocationFilterDisplayMode" ) == "3";
            LocationPanelClosed = GetAttributeValue( "LocationFilterDisplayMode" ) == "4";

            ResourcePanelOpen = GetAttributeValue( "ResourceFilterDisplayMode" ) == "3";
            ResourcePanelClosed = GetAttributeValue( "ResourceFilterDisplayMode" ) == "4";

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";

            MinistryPanelOpen = GetAttributeValue( "MinistryFilterDisplayMode" ) == "3";
            MinistryPanelClosed = GetAttributeValue( "MinistryFilterDisplayMode" ) == "4";

            ApprovalPanelOpen = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "3";
            ApprovalPanelClosed = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "4";

            ReservationTypePanelOpen = GetAttributeValue( "ReservationTypeFilterDisplayMode" ) == "3";
            ReservationTypePanelClosed = GetAttributeValue( "ReservationTypeFilterDisplayMode" ) == "4";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // register lbPrint as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( rptReports );

            RockPage.AddScriptLink( "~/Plugins/com_bemaservices/RoomManagement/Assets/Scripts/circle-progress.js", fingerprint: false );
            RockPage.AddScriptLink( "~/Plugins/com_bemaservices/RoomManagement/Assets/Scripts/event-calendar.js", fingerprint: false );
            RockPage.AddScriptLink( "~/Plugins/com_bemaservices/RoomManagement/Assets/Scripts/moment.js", fingerprint: false );
            RockPage.AddCSSLink( "~/Plugins/com_bemaservices/RoomManagement/Assets/Styles/event-calendar.css", fingerprint: false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            var reportCache = DefinedTypeCache.Get( "13B169EA-A090-45FF-8B11-A9E02776E35E" );
            var selectedReports = GetAttributeValue( "VisiblePrintableReportOptions" ).SplitDelimitedValues().AsGuidList();
            rptReports.DataSource = reportCache.DefinedValues.Where( dv => selectedReports.Contains( dv.Guid ) ).ToList();
            rptReports.DataBind();

            var viewCache = DefinedTypeCache.Get( "32EC3B34-01CF-4513-BC2E-58ECFA91D010" );
            var selectedViews = GetAttributeValue( "VisibleReservationViewOptions" ).SplitDelimitedValues().AsGuidList();
            var definedValueList = viewCache.DefinedValues.Where( dv => selectedViews.Contains( dv.Guid ) ).ToList();
            rptViews.DataSource = definedValueList;
            rptViews.DataBind();

            // Set User Preference
            ReservationViewId = this.GetUserPreference( PreferenceKey + "ReservationViewId" ).AsIntegerOrNull();
            if ( ReservationViewId == null && definedValueList.FirstOrDefault() != null )
            {
                ReservationViewId = definedValueList.FirstOrDefault().Id;
            }

            if ( !Page.IsPostBack )
            {
                if ( SetFilterControls() )
                {
                    if ( ReservationViewId != null && hfSelectedView.ValueAsInt() == 0 )
                    {
                        hfSelectedView.Value = ReservationViewId.ToString();
                    }

                    if ( definedValueList.Count > 1 )
                    {
                        var selectedValue = DefinedValueCache.Get( hfSelectedView.ValueAsInt() );
                        lSelectedView.Text = string.Format( "View As: {0}", selectedValue.Value );
                        divViewDropDown.Visible = true;
                    }
                    else
                    {
                        divViewDropDown.Visible = false;
                    }

                    pnlDetails.Visible = true;
                    BindData();
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns null.</returns>
        protected override object SaveViewState()
        {
            ViewState["ViewMode"] = ViewMode;
            ViewState["ReservationViewId"] = ReservationViewId;
            ViewState["FilterStartDate"] = FilterStartDate;
            ViewState["FilterEndDate"] = FilterEndDate;
            ViewState["ReservationDates"] = ReservationDates;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            btnDay.RemoveCssClass( "active" );
            btnWeek.RemoveCssClass( "active" );
            btnMonth.RemoveCssClass( "active" );
            btnYear.RemoveCssClass( "active" );
            pnlCalendar.RemoveCssClass( "hidden" );
            ypYearPicker.Visible = false;

            switch ( ViewMode )
            {
                case "Day":
                    btnDay.AddCssClass( "active" );
                    break;

                case "Week":
                    btnWeek.AddCssClass( "active" );
                    break;

                case "Month":
                    btnMonth.AddCssClass( "active" );
                    break;

                case "Year":
                    btnYear.AddCssClass( "active" );
                    pnlCalendar.AddCssClass( "hidden" );
                    ypYearPicker.Visible = true;

                    if ( ypYearPicker.SelectedYear.HasValue )
                    {
                        dpEndDate.SelectedDate = new DateTime( ypYearPicker.SelectedYear.Value, 12, 31 );
                        // Start at the current date if they have the current year selected.
                        if ( ypYearPicker.SelectedYear.Value == RockDateTime.Today.Year )
                        {
                            FilterStartDate = RockDateTime.Today;
                        }
                        else
                        {
                            FilterStartDate = new DateTime( ypYearPicker.SelectedYear.Value, 1, 1 );
                        }
                        FilterEndDate = dpEndDate.SelectedDate;
                        BindData();
                    }
                    else
                    {
                        ypYearPicker.SelectedYear = RockDateTime.Now.Year;
                    }

                    break;

                default:
                    break;
            }

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( SetFilterControls() )
            {
                pnlDetails.Visible = true;
                BindData();
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void calReservationCalendar_SelectionChanged( object sender, EventArgs e )
        {
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the DayRender event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs" /> instance containing the event data.</param>
        protected void calReservationCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( ReservationDates != null && ReservationDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.AddCssClass( "calendar-hasevent" );
            }
        }

        /// <summary>
        /// Handles the VisibleMonthChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MonthChangedEventArgs" /> instance containing the event data.</param>
        protected void calReservationCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            calReservationCalendar.SelectedDate = e.NewDate;
            Session["CalendarVisibleDate"] = e.NewDate;
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the SelectItem event of the lipLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lipLocation_SelectItem( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Locations", lipLocation.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectItem event of the rpResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rpResource_SelectItem( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Resources", rpResource.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Campuses", cblCampus.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cblMinistry_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Ministries", cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblApproval control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cblApproval_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Approval State", cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>().ConvertToInt() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblReservationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cblReservationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Reservation Type", cblReservationType.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the TextChanged event of the dpStartDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void dpStartDate_TextChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Start Date", dpStartDate.SelectedDate.ToString() );
            FilterStartDate = dpStartDate.SelectedDate;
            BindData();
        }

        /// <summary>
        /// Handles the TextChanged event of the dpEndDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void dpEndDate_TextChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "End Date", dpEndDate.SelectedDate.ToString() );
            FilterEndDate = dpEndDate.SelectedDate;
            BindData();
        }

        /// <summary>
        /// Handles the Click event of the btnViewMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                this.SetUserPreference( PreferenceKey + "ViewMode", btnViewMode.Text );
                ViewMode = btnViewMode.Text;
                ResetCalendarSelection();
                BindData();
            }
        }

        protected void btnToday_Click( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "ViewMode", "Day" );
            ViewMode = "Day";
            calReservationCalendar.SelectedDate = RockDateTime.Now.Date;

            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the Click event of the btnAllReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAllReservations_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.All ).ToString();
            this.SetUserPreference( PreferenceKey + "ShowBy", hfShowBy.Value );
            BindData( ShowBy.All );
        }

        /// <summary>
        /// Handles the Click event of the btnMyReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnMyReservations_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.MyReservations ).ToString();
            this.SetUserPreference( PreferenceKey + "ShowBy", hfShowBy.Value );
            BindData( ShowBy.MyReservations );
        }

        /// <summary>
        /// Handles the Click event of the btnMyApprovals control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnMyApprovals_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.MyApprovals ).ToString();
            this.SetUserPreference( PreferenceKey + "ShowBy", hfShowBy.Value );
            BindData( ShowBy.MyApprovals );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptReports control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptReports_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var definedValueId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                PrintReport( definedValueId.Value );
            }
        }
        /// <summary>
        /// Handles the ItemCommand event of the rptViews control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptViews_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var definedValueId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                var selectedValue = DefinedValueCache.Get( definedValueId.Value );
                this.SetUserPreference( PreferenceKey + "ReservationViewId", selectedValue.Id.ToString() );
                lSelectedView.Text = string.Format( "View As: {0}", selectedValue.Value );
                hfSelectedView.Value = definedValueId.ToString();
                BindData();
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            var showBy = ( ShowBy ) hfShowBy.ValueAsInt();
            BindData( showBy );
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        private void BindData( ShowBy showBy )
        {
            HighlightActionButtons( showBy );

            List<ReservationSummary> reservationSummaryList = GetReservationSummaries( showBy );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                ReservationType = r.ReservationType,
                ApprovalState = r.ApprovalState.ConvertToString(),
                ApprovalStateInt = r.ApprovalState.ConvertToInt(),
                Locations = r.ReservationLocations.OrderBy(rl=> rl.Location.Name).ToList(),
                Resources = r.ReservationResources.OrderBy( rr => rr.Resource.Name ).ToList(),
                UnassignedResources = r.UnassignedReservationResources.OrderBy( rr => rr.Resource.Name ).ToList(),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventTimeDescription = r.EventTimeDescription,
                EventDateTimeDescription = r.EventDateTimeDescription,
                ReservationTimeDescription = r.ReservationTimeDescription,
                ReservationDateTimeDescription = r.ReservationDateTimeDescription,
                SetupPhotoId = r.SetupPhotoId,
                SetupPhotoLink = ResolveRockUrl( String.Format( "~/GetImage.ashx?id={0}", r.SetupPhotoId ?? 0 ) ),
                Note = r.Note,
                RequesterAlias = r.RequesterAlias,
                EventContactPersonAlias = r.EventContactPersonAlias,
                EventContactEmail = r.EventContactEmail,
                EventContactPhoneNumber = r.EventContactPhoneNumber,
                ReservationMinistry = r.ReservationMinistry,
                MinistryName = r.ReservationMinistry != null ? r.ReservationMinistry.Name : string.Empty,
            } )
                .ToList();

            var lavaReservationSummaries = reservationSummaries
                .OrderBy( r => r.EventStartDateTime )
                .GroupBy( r => r.EventStartDateTime.Date )
                .Select( r => r.ToList() )
                .ToList();

            // Build a list of dates and then all the reservations on those dates.
            // Each date contains a few useful details depending on how you want
            // to present the data.
            // Date = The date containing these reservations.
            // Reservations = The ordered list of reservations for this day.
            // Locations = The ordered list of reservation locations being used (for example with a room setup sheet).
            // Resources = The ordered list of resources being used (for example to easily see where resources are supposed to go).
            var lavaReservationDates = reservationSummaries
                .OrderBy( r => r.EventStartDateTime )
                .GroupBy( r => r.EventStartDateTime.Date )
                .Select( r => new
                {
                    Date = r.Key,
                    Reservations = r.ToList(),
                    Locations = r
                        .SelectMany( a => a.Locations, ( a, b ) => new
                        {
                            Name = b.Location.Name,
                            Reservation = a,
                            Location = b
                        } )
                        .OrderBy( a => a.Reservation.EventStartDateTime )
                        .ThenBy( a => a.Name )
                        .ToList(),
                    Resources = r
                        .SelectMany( a => a.Resources, ( a, b ) => new
                        {
                            Name = b.Resource.Name,
                            Reservation = a,
                            Resource = b
                        } )
                        .OrderBy( a => a.Reservation.EventStartDateTime )
                        .ThenBy( a => a.Name )
                        .ToList()
                } )
                .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "FilterStartDate", FilterStartDate );
            mergeFields.Add( "FilterEndDate", FilterEndDate );
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "ReservationSummaries", lavaReservationSummaries );
            mergeFields.Add( "ReservationDates", lavaReservationDates );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            var definedValue = new DefinedValueService( new RockContext() ).Get( hfSelectedView.ValueAsInt() );
            definedValue.LoadAttributes();

            var lavaTemplate = definedValue.GetAttributeValue( "Lava" );
            var lavaCommands = definedValue.GetAttributeValue( "LavaCommands" );

            lOutput.Text = lavaTemplate.ResolveMergeFields( mergeFields, lavaCommands );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }

        }

        /// <summary>
        /// Prints the report.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        protected void PrintReport( int definedValueId )
        {
            var definedValue = new DefinedValueService( new RockContext() ).Get( definedValueId );
            definedValue.LoadAttributes();

            var logoFileUrl = definedValue.GetAttributeValue( "ReportLogo" );
            var reportTemplateGuid = definedValue.GetAttributeValue( "ReportTemplate" ).AsGuidOrNull();
            var reportFont = definedValue.GetAttributeValue( "ReportFont" );
            var reportLava = definedValue.GetAttributeValue( "Lava" );

            var showBy = ( ShowBy ) hfShowBy.ValueAsInt();
            List<ReservationSummary> reservationSummaryList = GetReservationSummaries( showBy );

            if ( !logoFileUrl.ToLower().StartsWith( "http" ) )
            {
                logoFileUrl = Server.MapPath( ResolveRockUrl( logoFileUrl ) );
            }

            var reportTemplate = GetReportTemplate( reportTemplateGuid );

            var outputArray = reportTemplate.GenerateReport( reservationSummaryList, logoFileUrl, reportFont, FilterStartDate, FilterEndDate, reportLava );

            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();
            Response.ContentType = "application/pdf";
            var download = GetAttributeValue( "DownloadReports" ).AsBoolean();
            if ( !download )
            {
                Response.AddHeader( "Content-Disposition", string.Format( "inline;filename=Reservation Schedule for {0} - {1}.pdf", FilterStartDate.Value.ToString( "MMMM d" ), FilterEndDate.Value.ToString( "MMMM d" ) ) );
            }
            else
            {
                Response.AddHeader( "Content-Disposition", string.Format( "attachment;filename=Reservation Schedule for {0} - {1}.pdf", FilterStartDate.Value.ToString( "MMMM d" ), FilterEndDate.Value.ToString( "MMMM d" ) ) );
            }
            Response.BinaryWrite( outputArray );
            Response.Flush();
            Response.End();
            return;
        }

        /// <summary>
        /// Highlights the action buttons.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        private void HighlightActionButtons( ShowBy showBy )
        {
            switch ( showBy )
            {
                case ShowBy.All:
                    btnAllReservations.AddCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
                case ShowBy.MyReservations:
                    btnAllReservations.RemoveCssClass( "active btn-primary" );
                    btnMyReservations.AddCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
                case ShowBy.MyApprovals:
                    btnAllReservations.RemoveCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.AddCssClass( "active btn-primary" );
                    break;
                default:
                    btnAllReservations.AddCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
            }
        }
        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <returns>List&lt;ReservationService.ReservationSummary&gt;.</returns>
        private List<ReservationSummary> GetReservationSummaries()
        {
            return GetReservationSummaries( ShowBy.All );
        }

        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        /// <returns>List&lt;ReservationService.ReservationSummary&gt;.</returns>
        private List<ReservationSummary> GetReservationSummaries( ShowBy showBy )
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var locationService = new LocationService( rockContext );

            var reservationQueryOptions = new ReservationQueryOptions();

            // Do additional filtering based on the ShowBy selection (My Reservations, My Approvals)
            switch ( showBy )
            {
                case ShowBy.MyReservations:
                    reservationQueryOptions.ReservationsByPersonId = CurrentPerson.Id;
                    break;
                case ShowBy.MyApprovals:
                    if ( CurrentPersonId.HasValue )
                    {
                        reservationQueryOptions.ApprovalsByPersonId = CurrentPerson.Id;
                    }
                    break;
                default:
                    break;
            }

            // Filter by Resources
            if ( rpResource.Visible )
            {
                reservationQueryOptions.ResourceIds = rpResource.SelectedValuesAsInt().ToList();
            }

            // Filter by Locations
            if ( lipLocation.Visible )
            {
                var locationIdList = lipLocation.SelectedValuesAsInt().ToList();
                foreach ( var rootLocationId in lipLocation.SelectedValuesAsInt().ToList() )
                {
                    locationIdList.AddRange( locationService.GetAllDescendentIds( rootLocationId ) );
                    locationIdList.AddRange( locationService.GetAllAncestorIds( rootLocationId ) );
                }
                reservationQueryOptions.LocationIds = locationIdList;
            }

            // Filter by campus
            if ( cblCampus.Visible )
            {
                reservationQueryOptions.CampusIds = cblCampus.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            }

            // Filter by Ministry
            if ( cblMinistry.Visible )
            {
                reservationQueryOptions.MinistryNames = cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Text ).ToList();
            }

            // Filter by Approval
            if ( cblApproval.Visible )
            {
                reservationQueryOptions.ApprovalStates = cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>() ).ToList();
            }

            // Filter by Reservation Type
            if ( cblReservationType.Visible )
            {
                reservationQueryOptions.ReservationTypeIds = cblReservationType.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            }

            var qry = reservationService.Queryable( reservationQueryOptions );

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = FilterStartDate.HasValue ? FilterStartDate.Value : today;
            var filterEndDateTime = FilterEndDate.HasValue ? FilterEndDate.Value : today.AddMonths( 1 );
            var reservationSummaryList = qry.GetReservationSummaries( filterStartDateTime, filterEndDateTime, true );
            return reservationSummaryList;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool SetFilterControls()
        {
            var rockContext = new RockContext();
            // Get and verify the view mode
            ViewMode = this.GetUserPreference( PreferenceKey + "ViewMode" );
            if ( string.IsNullOrWhiteSpace( ViewMode ) )
            {
                ViewMode = GetAttributeValue( "DefaultViewOption" );
            }

            if ( !GetAttributeValue( string.Format( "Show{0}View", ViewMode ) ).AsBoolean() )
            {
                ViewMode = GetAttributeValue( "DefaultViewOption" );
                //ShowError( "Configuration Error", string.Format( "The Default View Option setting has been set to '{0}', but the Show {0} View setting has not been enabled.", ViewMode ) );
                //return false;
            }

            // Show/Hide calendar control
            pnlCalendar.Visible = GetAttributeValue( "ShowSmallCalendar" ).AsBoolean();

            // Get the first/last dates based on today's date and the viewmode setting
            var today = RockDateTime.Today;

            if ( PageParameter( "SelectedDate" ).AsDateTime() != null )
            {
                today = PageParameter( "SelectedDate" ).AsDateTime().Value;
                calReservationCalendar.VisibleDate = today;
            }

            // Use the CalendarVisibleDate if it's in session.
            if ( Session["CalendarVisibleDate"] != null )
            {
                today = ( DateTime ) Session["CalendarVisibleDate"];
                calReservationCalendar.VisibleDate = today;
            }

            FilterStartDate = today;
            FilterEndDate = today;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = today.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = today.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( RockDateTime.Today.Year, RockDateTime.Today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 12 );
            }

            // Setup small calendar Filter
            calReservationCalendar.FirstDayOfWeek = _firstDayOfWeek.ConvertToInt().ToString().ConvertToEnum<FirstDayOfWeek>();
            calReservationCalendar.SelectedDates.Clear();
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );

            // Setup Location Filter
            lipLocation.Visible = GetAttributeValue( "LocationFilterDisplayMode" ).AsInteger() > 1;
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Locations" ) ) )
            {
                lipLocation.SetValues( this.GetUserPreference( PreferenceKey + "Locations" ).Split( ',' ).AsIntegerList() );
            }

            // Setup Resource Filter
            rpResource.Visible = GetAttributeValue( "ResourceFilterDisplayMode" ).AsInteger() > 1;
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Resources" ) ) )
            {
                rpResource.SetValues( this.GetUserPreference( PreferenceKey + "Resources" ).Split( ',' ).AsIntegerList() );
            }

            // Setup Campus Filter
            rcwCampus.Visible = GetAttributeValue( "CampusFilterDisplayMode" ).AsInteger() > 1;
            cblCampus.DataSource = CampusCache.All( false );
            cblCampus.DataBind();
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Campuses" ) ) )
            {
                cblCampus.SetValues( this.GetUserPreference( PreferenceKey + "Campuses" ).SplitDelimitedValues() );
            }
            else
            {
                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus;
                    if ( contextCampus != null )
                    {
                        cblCampus.SetValue( contextCampus.Id );
                    }
                }
            }

            // Setup Ministry Filter
            rcwMinistry.Visible = GetAttributeValue( "MinistryFilterDisplayMode" ).AsInteger() > 1;
            cblMinistry.DataSource = new ReservationMinistryService( rockContext ).Queryable().DistinctBy( rmc => rmc.Name ).OrderBy( m => m.Name );
            cblMinistry.DataBind();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Ministries" ) ) )
            {
                cblMinistry.SetValues( this.GetUserPreference( PreferenceKey + "Ministries" ).SplitDelimitedValues() );
            }

            // Setup Approval Filter
            rcwApproval.Visible = GetAttributeValue( "ApprovalFilterDisplayMode" ).AsInteger() > 1;
            cblApproval.BindToEnum<ReservationApprovalState>();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Approval State" ) ) )
            {
                cblApproval.SetValues( this.GetUserPreference( PreferenceKey + "Approval State" ).SplitDelimitedValues() );
            }

            // Setup Reservation Type Filter
            rcwReservationType.Visible = GetAttributeValue( "ReservationTypeFilterDisplayMode" ).AsInteger() > 1;
            cblReservationType.DataSource = new ReservationTypeService( rockContext ).Queryable().AsNoTracking().Where( rt => rt.IsActive ).ToList();
            cblReservationType.DataBind();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Reservation Type" ) ) )
            {
                cblReservationType.SetValues( this.GetUserPreference( PreferenceKey + "Reservation Type" ).SplitDelimitedValues() );
            }

            // Date Range Filter
            dpStartDate.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            if ( dpStartDate.Visible && !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Start Date" ) ) )
            {
                dpStartDate.SelectedDate = this.GetUserPreference( PreferenceKey + "Start Date" ).AsDateTime();
                if ( dpStartDate.SelectedDate.HasValue )
                {
                    FilterStartDate = dpStartDate.SelectedDate;
                }
            }

            dpEndDate.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            if ( dpEndDate.Visible && !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "End Date" ) ) )
            {
                dpEndDate.SelectedDate = this.GetUserPreference( PreferenceKey + "End Date" ).AsDateTime();
                if ( dpEndDate.SelectedDate.HasValue )
                {
                    FilterEndDate = dpEndDate.SelectedDate;
                }
            }

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean(),
                GetAttributeValue( "ShowYearView" ).AsBoolean()
            };

            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = btnToday.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];
            btnYear.Visible = howManyVisible > 1 && viewsVisible[3];

            var showByPreference = this.GetUserPreference( PreferenceKey + "ShowBy" );
            if ( showByPreference.IsNotNullOrWhiteSpace() )
            {
                hfShowBy.Value = showByPreference;
            }

            // Set filter visibility
            bool showFilter = ( pnlCalendar.Visible || lipLocation.Visible || rpResource.Visible || rcwCampus.Visible || rcwMinistry.Visible || rcwApproval.Visible || dpStartDate.Visible || dpEndDate.Visible );
            pnlFilters.Visible = showFilter;
            pnlList.CssClass = showFilter ? "col-md-9" : "col-md-12";

            return true;
        }

        /// <summary>
        /// Resets the calendar selection. The control is configured for day selection, but selection will be changed to the week or month if that is the viewmode being used
        /// </summary>
        private void ResetCalendarSelection()
        {
            // Even though selection will be a single date due to calendar's selection mode, set the appropriate days
            var selectedDate = calReservationCalendar.SelectedDate;
            calReservationCalendar.VisibleDate = selectedDate;
            Session["CalendarVisibleDate"] = selectedDate;
            FilterStartDate = selectedDate;
            FilterEndDate = selectedDate;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = selectedDate.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = selectedDate.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( selectedDate.Year, selectedDate.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( RockDateTime.Today.Year, RockDateTime.Today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 12 );
            }

            dpStartDate.SelectedDate = dpEndDate.SelectedDate = null;
            this.SetUserPreference( PreferenceKey + "Start Date", null );
            this.SetUserPreference( PreferenceKey + "End Date", null );

            // Reset the selection
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );
        }

        /// <summary>
        /// Sets the calendar filter dates.
        /// </summary>
        private void SetCalendarFilterDates()
        {
            FilterStartDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[0] : ( DateTime? ) null;
            FilterEndDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[calReservationCalendar.SelectedDates.Count - 1] : ( DateTime? ) null;
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }


        /// <summary>
        /// Gets the report template.
        /// </summary>
        /// <param name="reportTemplateGuid">The report template unique identifier.</param>
        /// <returns>ReportTemplate.</returns>
        private ReportTemplate GetReportTemplate( Guid? reportTemplateGuid )
        {
            if ( reportTemplateGuid.HasValue )
            {
                var reportTemplateEntityType = EntityTypeCache.Get( reportTemplateGuid.Value );
                if ( reportTemplateEntityType == null )
                {
                    return null;
                }
                else
                {
                    return ReportTemplateContainer.GetComponent( reportTemplateEntityType.Name );
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Helper Classes, etc.

        /// <summary>
        /// Enum ShowBy
        /// </summary>
        private enum ShowBy
        {
            /// <summary>
            /// All reservations
            /// </summary>
            All = 0,

            /// <summary>
            /// Only my reservations
            /// </summary>
            MyReservations = 1,

            /// <summary>
            /// Only resevations that need my approval
            /// </summary>
            MyApprovals = 2

        }
        #endregion
    }
}
