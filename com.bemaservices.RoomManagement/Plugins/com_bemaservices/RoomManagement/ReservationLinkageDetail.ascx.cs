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
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using com.bemaservices.RoomManagement.Model;
using DDay.iCal;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Newtonsoft.Json;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.Communication;
using System.Web;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using com.bemaservices.RoomManagement.Attribute;
using Rock.Constants;
using Rock.Lava;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// Class ReservationLinkageDetail.
    /// Implements the <see cref="Rock.Web.UI.RockBlock" />
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Reservation Linkage Detail" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Block for creating a Reservation Linkage" )]

    [EventCalendarField(
        "Default Calendar",
        Key = AttributeKey.DefaultCalendar,
        Description = "The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 0 )]

    [BooleanField(
        "Allow Creating New Calendar Events",
        Key = AttributeKey.AllowCreatingNewCalendarEvents,
        Description = "If set to \"Yes\", the staff person will be offered the \"New Event\" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.",
        Category = "",
        IsRequired = true,
        DefaultValue = "true",
        Order = 1 )]

    [BooleanField(
        "Include Inactive Calendar Items",
        Key = AttributeKey.IncludeInactiveCalendarItems,
        Description = "Check this box to hide inactive calendar items.",
        Category = "",
        IsRequired = false,
        DefaultValue = "true",
        Order = 2 )]

    [WorkflowTypeField(
        "Completion Workflow",
        Key = AttributeKey.CompletionWorkflow,
        Description = "A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:\r\n + Reservation\r\n + EventItemOccurrenceGuid",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [BooleanField(
        "Display Link to Event Details Page on Confirmation Screen",
        Key = AttributeKey.DisplayEventDetailsLink,
        Description = "Check this box to show the link to the event details page in the wizard confirmation screen.",
        Category = "",
        IsRequired = false,
        DefaultValue = "true",
        Order = 4 )]

    [LinkedPage(
        "External Event Details Page",
        Key = AttributeKey.EventDetailsPage,
        Description = "Determines which page the link in the final confirmation screen will take you to (if \"Display Link to Event Details ... \" is selected).",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAILS,
        Order = 5 )]

    #region Advanced Block Attribute Settings 

    [MemoField(
        "Event Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Event,
        Description = "Instructions here will show up on the fourth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 4 )]

    [MemoField(
        "Event Occurrence Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_EventOccurrence,
        Description = "Instructions here will show up on the fifth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 5 )]

    [MemoField(
        "Summary Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Summary,
        Description = "Instructions here will show up on the sixth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 6 )]

    [MemoField(
        "Wizard Finished Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Finished,
        Description = "Instructions here will show up on the final panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 7 )]

    #endregion Advanced Block Attribute Settings
    public partial class ReservationLinkageDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// Class AttributeKey.
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The default calendar
            /// </summary>
            public const string DefaultCalendar = "DefaultCalendar";
            /// <summary>
            /// The allow creating new calendar events
            /// </summary>
            public const string AllowCreatingNewCalendarEvents = "AllowCreatingNewCalendarEvents";
            /// <summary>
            /// The include inactive calendar items
            /// </summary>
            public const string IncludeInactiveCalendarItems = "IncludeInactiveCalendarItems";
            /// <summary>
            /// The completion workflow
            /// </summary>
            public const string CompletionWorkflow = "CompletionWorkflow";
            /// <summary>
            /// The display event details link
            /// </summary>
            public const string DisplayEventDetailsLink = "DisplayEventDetailsLink";
            /// <summary>
            /// The event details page
            /// </summary>
            public const string EventDetailsPage = "EventDetailsPage";

            /// <summary>
            /// The lava instruction event
            /// </summary>
            public const string LavaInstruction_Event = "LavaInstruction_Event";
            /// <summary>
            /// The lava instruction event occurrence
            /// </summary>
            public const string LavaInstruction_EventOccurrence = "LavaInstruction_EventOccurrence";
            /// <summary>
            /// The lava instruction summary
            /// </summary>
            public const string LavaInstruction_Summary = "LavaInstruction_Summary";
            /// <summary>
            /// The lava instruction finished
            /// </summary>
            public const string LavaInstruction_Finished = "LavaInstruction_Finished";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the event item occurrence.
        /// </summary>
        /// <value>The event item occurrence.</value>
        private EventItemOccurrence EventItemOccurrence { get; set; }
        /// <summary>
        /// Gets or sets the event item.
        /// </summary>
        /// <value>The event item.</value>
        private EventItem EventItem { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["EventItemOccurrence"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItemOccurrence = null;
            }
            else
            {
                EventItemOccurrence = JsonConvert.DeserializeObject<EventItemOccurrence>( json );
            }

            json = ViewState["EventItem"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItem = null;
            }
            else
            {
                EventItem = JsonConvert.DeserializeObject<EventItem>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Tell the browsers to not cache. This will help prevent browser using stale wizard stuff after navigating away from this page
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            // Hide inactive events if the option has been selected.
            eipSelectedEvent.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCalendarItems ).AsBoolean();

            Init_SetupAudienceControls();

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            Page.Header.Controls.Add( new LiteralControl( "<link rel=\"stylesheet\" type=\"text/css\" media=\"print\" href=\"/Plugins/com_bemaservices/RoomManagement/Assets/Styles/print.css\" />" ) );


            cblCalendars.SelectedIndexChanged += cblCalendars_SelectedIndexChanged;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Build calendar item attributes on every Init event to ensure they are populated by ViewState.
            ShowItemAttributes();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                hfReservationId.Value = PageParameter( "ReservationId" );
                using ( var rockContext = new RockContext() )
                {
                    Init_SetValuesFromReservation();
                    Init_SetCampusAndEventSelectionOption();
                    Init_SetDefaultCalendar( rockContext );
                    Init_SetCalendarEventRequired();
                }
                SetActiveWizardStep( ActiveWizardStep.Event );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns null.</returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["EventItemOccurrence"] = JsonConvert.SerializeObject( EventItemOccurrence, Formatting.None, jsonSetting );
            ViewState["EventItem"] = JsonConvert.SerializeObject( EventItem, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.</returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? reservationId = PageParameter( pageReference, "ReservationId" ).AsIntegerOrNull();
            if ( reservationId != null )
            {
                Reservation reservation = new ReservationService( new RockContext() ).Get( reservationId.Value );
                if ( reservation != null )
                {
                    breadCrumbs.Add( new BreadCrumb( reservation.Name, pageReference ) );
                    RockPage.Title = reservation.Name;
                    RockPage.BrowserTitle = reservation.Name;
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Reservation", pageReference ) );
                }
            }
            else
            {
                breadCrumbs.Add( new BreadCrumb( "New Reservation", pageReference ) );
            }

            return breadCrumbs;
        }

        #endregion

        #region Reservation Event Occurrence Events

        #region Wizard LinkButton Event Handlers

        /// <summary>
        /// Handles the Click event of the lbEvent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEvent_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        /// <summary>
        /// Handles the Click event of the lbEventOccurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
        }

        /// <summary>
        /// Handles the Click event of the lbPrev_Event control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrev_Event_Click( object sender, EventArgs e )
        {
            ExitWizard();
        }

        /// <summary>
        /// Exits the wizard.
        /// </summary>
        private void ExitWizard()
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "ReservationId", PageParameter( "ReservationId" ) );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbNext_Event control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Event_Click( object sender, EventArgs e )
        {
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Summary );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPrev_EventOccurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrev_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        /// <summary>
        /// Handles the Click event of the lbNext_EventOccurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Summary );
        }

        /// <summary>
        /// Handles the Click event of the lbPrev_Summary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrev_Summary_Click( object sender, EventArgs e )
        {
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Event );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbNext_Summary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Summary_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Finished );
        }

        #endregion Wizard LinkButton Event Handlers

        /// <summary>
        /// Handles the CheckedChanged event of the tglEventSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglEventSelection_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglEventSelection.Checked )
            {
                pnlExistingEvent.Visible = false;
                pnlNewEvent.Visible = true;
            }
            else
            {
                pnlExistingEvent.Visible = true;
                pnlNewEvent.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbEventOccurrenceSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbEventOccurrenceSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent };
            lEventOccurrenceScheduleText.Text = schedule.FriendlyScheduleText;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCalendars control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
            ShowItemAttributes();
        }

        #region Audience Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !audiencesState.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ViewState["AudiencesState"] = audiencesState;

            ShowDialog( "EventItemAudience", true );
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            Guid guid = ( Guid ) e.RowKeyValue;
            var audience = DefinedValueCache.Get( guid );
            if ( audience != null )
            {
                audiencesState.Remove( audience.Id );
            }
            ViewState["AudiencesState"] = audiencesState;

            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAudience_Click( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            int? definedValueId = ddlAudience.SelectedValueAsInt();
            if ( definedValueId.HasValue )
            {
                audiencesState.Add( definedValueId.Value );
            }

            ViewState["AudiencesState"] = audiencesState;

            BindAudienceGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gAudiences_GridRebind( object sender, EventArgs e )
        {
            BindAudienceGrid();
        }

        /// <summary>
        /// Binds the audience grid.
        /// </summary>
        private void BindAudienceGrid()
        {
            var values = new List<DefinedValueCache>();
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            audiencesState.ForEach( a => values.Add( DefinedValueCache.Get( a ) ) );

            gAudiences.DataSource = values
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();
            gAudiences.DataBind();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;
            }
        }

        #endregion

        #endregion

        #region Reservation Event Occurrence  Methods

        /// <summary>
        /// Starts the workflow.
        /// </summary>
        /// <param name="rockContext">The workflow service.</param>
        /// <param name="reservation">The reservation.</param>
        protected void LaunchPostWizardWorkflow( RockContext rockContext, Reservation reservation )
        {
            //Set Completion Workflow
            var workFlowGuid = GetAttributeValue( AttributeKey.CompletionWorkflow ).AsGuidOrNull();
            if ( workFlowGuid != null )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflowType = WorkflowTypeCache.Get( workFlowGuid.Value );

                //launch workflow if configured
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    // set workflow name
                    string workflowName = "New " + workflowType.WorkTerm;
                    var workflow = Workflow.Activate( workflowType, workflowName );

                    // set attributes
                    if ( reservation != null )
                    {
                        workflow.SetAttributeValue( "Reservation", reservation.Guid );
                    }

                    if ( reservation.EventItemOccurrence != null )
                    {
                        workflow.SetAttributeValue( "EventItemOccurrenceGuid", reservation.EventItemOccurrence.Guid );
                    }

                    // launch workflow
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );
                }
            }

        }

        /// <summary>
        /// Save event calendar items state to ViewState.
        /// </summary>
        /// <param name="itemState">The list of EventCalendarItems to save.</param>
        private void SaveCalendarItemState( List<EventCalendarItem> itemState )
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ItemsState"] = JsonConvert.SerializeObject( itemState, Formatting.None, jsonSetting );
        }

        /// <summary>
        /// Retrieve event calendar items state from ViewState.
        /// </summary>
        /// <returns>Returns a List of EventCalendarItem objects.</returns>
        private List<EventCalendarItem> GetCalendarItemState()
        {
            List<EventCalendarItem> itemState;
            string json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                itemState = new List<EventCalendarItem>();
            }
            else
            {
                itemState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }
            return itemState;
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            if ( Session["CurrentCalendars"] == null )
            {
                // If the user's session has expired, create a new list in session and reset them
                // to the first step of the wizard.
                if ( cblCalendars.SelectedValuesAsInt.Count() > 0 )
                {
                    Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
                }
                else
                {
                    Session["CurrentCalendars"] = new List<int>();
                }
                SetActiveWizardStep( ActiveWizardStep.Event );
            }

            var eventCalendarList = ( List<int> ) Session["CurrentCalendars"];
            wpAttributes.Visible = false;
            phEventItemAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList.Distinct() )
                {
                    var itemsState = GetCalendarItemState();
                    var eventCalendarItem = itemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        itemsState.Add( eventCalendarItem );
                    }
                    SaveCalendarItemState( itemsState );

                    eventCalendarItem.LoadAttributes();
                    if ( eventCalendarItem.Attributes.Count > 0 )
                    {
                        wpAttributes.Visible = true;
                        phEventItemAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phEventItemAttributes, true, BlockValidationGroup );
                    }
                    else
                    {
                        wpAttributes.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Display the summary of items that will be created.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void DisplaySummary( RockContext rockContext )
        {
            string itemTemplate = "<p><ul><li><strong>{0}</strong> {1}</li></ul></p>";

            // Calendar Event Summary
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                string eventDescription;
                var schedule = new Schedule { iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent };
                if ( tglEventSelection.Checked )
                {
                    eventDescription = "An event occurrence will be created for the new \"" + tbCalendarEventName.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }
                else
                {
                    eventDescription = "An event occurrence will be created for the \"" + eipSelectedEvent.SelectedItem.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }

                var eventLiteral = new Literal() { Text = string.Format( itemTemplate, "Calendar Event", eventDescription ) };
                phChanges.Controls.Add( eventLiteral );
            }
        }

        /// <summary>
        /// Gets a specific page URL from a PageReference.
        /// </summary>
        /// <param name="pageGuid">The Guid of the page.</param>
        /// <param name="queryParams">Optional query parameters to be included in the URL.</param>
        /// <returns>Returns a string representing a specific page URL from a PageReference.</returns>
        private string GetPageUrl( string pageGuid, Dictionary<string, string> queryParams = null )
        {
            return new PageReference( pageGuid, queryParams ).BuildUrl();
        }

        /// <summary>
        /// Initializes the setup audience controls.
        /// </summary>
        private void Init_SetupAudienceControls()
        {
            gAudiences.DataKeyNames = new string[] { "Guid" };
            gAudiences.Actions.ShowAdd = true;
            gAudiences.Actions.AddClick += gAudiences_Add;
            gAudiences.GridRebind += gAudiences_GridRebind;

            if ( !Page.IsPostBack )
            {
                BindAudienceGrid();
            }
        }

        /// <summary>
        /// Initializes the set values from reservation.
        /// </summary>
        private void Init_SetValuesFromReservation()
        {
            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                var reservationService = new ReservationService( new RockContext() );
                var reservation = reservationService.Get( reservationId );
                if ( reservation != null )
                {
                    var schedule = new Schedule { iCalendarContent = reservation.Schedule.iCalendarContent };
                    tbCalendarEventName.Text = reservation.Name;
                    sbEventOccurrenceSchedule.iCalendarContent = reservation.Schedule.iCalendarContent;
                    lEventOccurrenceScheduleText.Text = reservation.Schedule.FriendlyScheduleText;
                }
            }
        }

        /// <summary>
        /// Initializes the set campus and event selection option.
        /// </summary>
        private void Init_SetCampusAndEventSelectionOption()
        {
            if ( !GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            {
                pnlNewEventSelection.Visible = false;
            }
        }

        /// <summary>
        /// Initializes the set default calendar.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void Init_SetDefaultCalendar( RockContext rockContext )
        {
            int defaultCalendarId = -1;
            Guid? calendarGuid = GetAttributeValue( AttributeKey.DefaultCalendar ).AsGuidOrNull();
            if ( calendarGuid != null )
            {
                var calendarService = new EventCalendarService( rockContext );
                var calendar = calendarService.Get( calendarGuid.Value );
                defaultCalendarId = calendar.Id;
            }

            cblCalendars.Items.Clear();
            foreach ( var calendar in
                new EventCalendarService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( c => c.Name ) )
            {
                if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    ListItem liCalendar = new ListItem( calendar.Name, calendar.Id.ToString() );
                    if ( calendar.Id == defaultCalendarId )
                    {
                        liCalendar.Selected = true;
                    }
                    cblCalendars.Items.Add( liCalendar );
                }
            }

            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
        }

        /// <summary>
        /// Initializes the set calendar event required.
        /// </summary>
        private void Init_SetCalendarEventRequired()
        {
            eipSelectedEvent.Help = "If you do not select an event item, no event occurrence will be created.";
        }

        /// <summary>
        /// Sets the active wizard step.
        /// </summary>
        /// <param name="step">The step.</param>
        private void SetActiveWizardStep( ActiveWizardStep step )
        {
            if ( step == ActiveWizardStep.Summary )
            {
                using ( var rockContext = new RockContext() )
                {
                    DisplaySummary( rockContext );
                }
            }
            else if ( step == ActiveWizardStep.Finished )
            {
                CommitResult result = null;
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        result = CommitChanges( rockContext );
                    } );
                }
                SetResultLinks( result );
            }

            SetLavaInstructions( step );
            SetupWizardCSSClasses( step );
            ShowInputPanel( step );
            SetupWizardButtons( step );
        }

        /// <summary>
        /// Sets the lava instructions.
        /// </summary>
        /// <param name="step">The step.</param>
        private void SetLavaInstructions( ActiveWizardStep step )
        {
            pnlLavaInstructions.Visible = false;
            string lavaTemplate = string.Empty;
            switch ( step )
            {
                case ActiveWizardStep.Event:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Event );
                    break;
                case ActiveWizardStep.EventOccurrence:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_EventOccurrence );
                    break;
                case ActiveWizardStep.Summary:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Summary );
                    break;
                case ActiveWizardStep.Finished:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Finished );
                    break;
            }

            if ( lavaTemplate != string.Empty )
            {
                pnlLavaInstructions.Visible = true;
                var mergeObjects = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "ActiveWizardStep", step.ToString() );
                mergeObjects.Add( "Page", ( int ) step + 1 );
                lLavaInstructions.Text = lavaTemplate.ResolveMergeFields( mergeObjects );
            }
        }

        /// <summary>
        /// Sets the appropriate CSS classes (active, complete or none) on wizard div elements.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardCSSClasses( ActiveWizardStep step )
        {
            string baseClass = "wizard-item";
            string eventClass = baseClass;
            string eventoccurrenceClass = baseClass;
            string summaryClass = baseClass;

            pnlWizard.Visible = ( step != ActiveWizardStep.ViewReservation && step != ActiveWizardStep.EditReservation );
            switch ( step )
            {
                case ActiveWizardStep.Event:
                    eventClass = baseClass + " active";
                    break;
                case ActiveWizardStep.EventOccurrence:
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Summary:
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " complete";
                    summaryClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Finished:
                    pnlWizard.Visible = false;
                    break;
                default:
                    break;
            }
            divEvent.Attributes.Remove( "class" );
            divEvent.Attributes.Add( "class", eventClass );
            divEventOccurrence.Attributes.Remove( "class" );
            divEventOccurrence.Attributes.Add( "class", eventoccurrenceClass );
            divSummary.Attributes.Remove( "class" );
            divSummary.Attributes.Add( "class", summaryClass );
        }

        /// <summary>
        /// This method commits all changes to the database at once.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>CommitResult.</returns>
        private CommitResult CommitChanges( RockContext rockContext )
        {
            var result = new CommitResult();

            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                var reservationService = new ReservationService( rockContext );
                var reservationLinkageService = new ReservationLinkageService( rockContext );
                var reservation = reservationService.Get( reservationId );
                if ( reservation != null )
                {
                    EventItem eventItem = null;
                    if ( tglEventSelection.Checked )  // "New Event" option selected.
                    {
                        var eventItemService = new EventItemService( rockContext );
                        var eventCalendarItemService = new EventCalendarItemService( rockContext );
                        var eventItemAudienceService = new EventItemAudienceService( rockContext );

                        // Create new EventItem
                        eventItem = new EventItem();
                        eventItem.Name = tbCalendarEventName.Text;
                        eventItem.Summary = tbEventSummary.Text;
                        eventItem.Description = htmlEventDescription.Text;
                        eventItem.IsActive = true;
                        if ( imgupPhoto.BinaryFileId != null )
                        {
                            eventItem.PhotoId = imgupPhoto.BinaryFileId;
                        }

                        // Add audiences
                        List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
                        foreach ( int audienceId in audiencesState )
                        {
                            var eventItemAudience = new EventItemAudience();
                            eventItemAudience.DefinedValueId = audienceId;
                            eventItem.EventItemAudiences.Add( eventItemAudience );
                        }

                        // Add calendar items from the UI
                        var calendarIds = new List<int>();
                        calendarIds.AddRange( cblCalendars.SelectedValuesAsInt );
                        var itemsState = GetCalendarItemState();
                        foreach ( var calendar in itemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ) )
                        {
                            var eventCalendarItem = new EventCalendarItem();
                            eventItem.EventCalendarItems.Add( eventCalendarItem );
                            eventCalendarItem.CopyPropertiesFrom( calendar );
                        }

                        eventItemService.Add( eventItem );
                        rockContext.SaveChanges();

                        foreach ( var eventCalendarItem in eventItem.EventCalendarItems )
                        {
                            eventCalendarItem.LoadAttributes();
                            Rock.Attribute.Helper.GetEditValues( phEventItemAttributes, eventCalendarItem );
                            eventCalendarItem.SaveAttributeValues();
                        }
                        result.EventId = eventItem.Id.ToString();

                    }
                    else // "Existing Event" option selected.
                    {
                        if ( eipSelectedEvent.SelectedValueAsId() != null )
                        {
                            var eventItemService = new EventItemService( rockContext );
                            eventItem = eventItemService.Get( eipSelectedEvent.SelectedValueAsId().Value );
                        }
                    }

                    // if eventItem is null, no EventItem was selected or created and we will not create an occurrence, either.
                    if ( eventItem != null )
                    {
                        // Create new EventItemOccurrence.
                        var eventItemOccurrence = new EventItemOccurrence { EventItemId = eventItem.Id };
                        eventItemOccurrence.CampusId = reservation.CampusId;
                        eventItemOccurrence.Location = tbLocationDescription.Text;
                        eventItemOccurrence.ContactPersonAliasId = reservation.EventContactPersonAliasId;
                        eventItemOccurrence.ContactPhone = reservation.EventContactPhone;
                        eventItemOccurrence.ContactEmail = reservation.EventContactEmail;
                        eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                        // Set Calendar.
                        string iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent ?? string.Empty;
                        var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
                        if ( calEvent != null && calEvent.DTStart != null )
                        {
                            if ( eventItemOccurrence.Schedule == null )
                            {
                                eventItemOccurrence.Schedule = new Schedule();
                            }

                            eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                        }

                        var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                        eventItemOccurrenceService.Add( eventItemOccurrence );
                        rockContext.SaveChanges();
                        result.EventOccurrenceId = eventItemOccurrence.Id.ToString();

                        var reservationLinkage = new ReservationLinkage();
                        reservationLinkage.Reservation = reservation;
                        reservationLinkage.ReservationId = reservation.Id;
                        reservationLinkage.EventItemOccurrence = eventItemOccurrence;
                        reservationLinkage.EventItemOccurrenceId = eventItemOccurrence.Id;
                        reservationLinkageService.Add( reservationLinkage );
                        rockContext.SaveChanges();

                        result.ReservationId = reservation.Id.ToString();
                        result.ReservationName = reservation.Name;
                    }


                    LaunchPostWizardWorkflow( rockContext, reservation );
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the link URLs for each object on the final page of the wizard.
        /// </summary>
        /// <param name="result">The result.</param>
        private void SetResultLinks( CommitResult result )
        {
            lblReservationTitle.Text = result.ReservationName;

            if ( string.IsNullOrWhiteSpace( result.EventId ) )
            {
                liEventLink.Visible = false;
            }
            else
            {
                var qryEventDetail = new Dictionary<string, string>();
                qryEventDetail.Add( "EventItemId", result.EventId );
                hlEventDetail.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_DETAIL, qryEventDetail );
            }

            if ( string.IsNullOrWhiteSpace( result.EventOccurrenceId ) )
            {
                liEventOccurrenceLink.Visible = false;
            }
            else
            {
                var qryEventOccurrence = new Dictionary<string, string>();
                qryEventOccurrence.Add( "EventItemOccurrenceId", result.EventOccurrenceId );
                hlEventOccurrence.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_OCCURRENCE, qryEventOccurrence );

                bool showEventDetailsLink = GetAttributeValue( AttributeKey.DisplayEventDetailsLink ).AsBoolean();
                if ( !showEventDetailsLink )
                {
                    liExternalEventLink.Visible = false;
                }
                else
                {
                    var qryExternalEventOccurrence = new Dictionary<string, string>();
                    qryExternalEventOccurrence.Add( "EventOccurrenceId", result.EventOccurrenceId );
                    hlExternalEventDetails.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.EventDetailsPage ), qryExternalEventOccurrence );
                }
            }
        }

        /// <summary>
        /// Displays the appropriate input panel.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void ShowInputPanel( ActiveWizardStep step )
        {
            pnlEvent.Visible = false;
            pnlEvent_Header.Visible = false;
            pnlEventOccurrence.Visible = false;
            pnlEventOccurrence_Header.Visible = false;
            pnlSummary.Visible = false;
            pnlSummary_Header.Visible = false;
            pnlFinished.Visible = false;
            pnlFinished_Header.Visible = false;

            switch ( step )
            {
                case ActiveWizardStep.Event:
                    pnlEvent.Visible = true;
                    pnlEvent_Header.Visible = true;
                    break;
                case ActiveWizardStep.EventOccurrence:
                    pnlEventOccurrence.Visible = true;
                    pnlEventOccurrence_Header.Visible = true;
                    break;
                case ActiveWizardStep.Summary:
                    pnlSummary.Visible = true;
                    pnlSummary_Header.Visible = true;
                    break;
                case ActiveWizardStep.Finished:
                    pnlFinished.Visible = true;
                    pnlFinished_Header.Visible = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Enables or disables wizard buttons, allowing the user to go backward but not skip forward.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardButtons( ActiveWizardStep step )
        {
            lbEvent.Enabled = false;
            lbEventOccurrence.Enabled = false;

            switch ( step )
            {
                case ActiveWizardStep.EventOccurrence:
                    lbEvent.Enabled = true;
                    break;
                case ActiveWizardStep.Summary:
                    lbEvent.Enabled = true;
                    if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
                    {
                        lbEventOccurrence.Enabled = true;
                    }
                    else
                    {
                        lbEventOccurrence.Enabled = false;
                    }
                    break;
                case ActiveWizardStep.Finished:
                    break;
                default:
                    break;
            }
        }

        #endregion


        #region Helper Classes

        /// <summary>
        /// Class CommitResult.
        /// </summary>
        public class CommitResult
        {
            /// <summary>
            /// The reservation identifier
            /// </summary>
            public string ReservationId, EventId, EventOccurrenceId, ReservationName;
            /// <summary>
            /// Initializes a new instance of the <see cref="CommitResult"/> class.
            /// </summary>
            public CommitResult()
            {
                ReservationId = "";
                EventId = "";
                EventOccurrenceId = "";
                ReservationName = "";
            }
        }

        /// <summary>
        /// Enum ActiveWizardStep
        /// </summary>
        private enum ActiveWizardStep { ViewReservation, EditReservation, Event, EventOccurrence, Summary, Finished }
        #endregion


        /// <summary>
        /// Handles the Click event of the lbCreateNewEventLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateNewEventLinkage_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                Init_SetValuesFromReservation();
                Init_SetCampusAndEventSelectionOption();
                Init_SetDefaultCalendar( rockContext );
                Init_SetCalendarEventRequired();
            }
            SetActiveWizardStep( ActiveWizardStep.Event );
        }


        /// <summary>
        /// Handles the Click event of the lbReturnToReservation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReturnToReservation_Click( object sender, EventArgs e )
        {
            ExitWizard();
        }
    }
}
