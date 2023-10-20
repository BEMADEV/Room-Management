
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Rock.Security;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Attribute;
using System.Data.Entity;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a lava template.
    /// </summary>
    [DisplayName( "My Reservations Lava" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Block to display reservations assigned to the current user.  The display format is controlled by a lava template." )]

    [CustomRadioListField( "Role", "Display the active reservations that the current user Initiated / is a Contact for, or is currently Assigned To for approval.", "0^Assigned To,1^Initiated", true, "0", "", 0 )]
    [CustomRadioListField( "Status", "Display upcoming reservations, or past reservations.", "0^Upcoming,1^Past", true, "0", "", 1 )]
    [CodeEditorField( "Contents", @"The Lava template to use for displaying reservations assigned to current user.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{% include 'Plugins/com_bemaservices/RoomManagement/Assets/Lava/MyReservationsSortable.lava' %}", "", 3 )]
    [TextField( "Set Panel Title", "The title to display in the panel header. Leave empty to have the block name.", required: false, order: 4 )]
    [TextField( "Set Panel Icon", "The icon to display in the panel header.", required: false, order: 5 )]
    [Rock.SystemGuid.BlockTypeGuid( "0D2E85E7-5881-42C4-9CB1-6F8830BB620B" )]
    public partial class MyReservationsLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                BindData();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods

        private void BindData()
        {
            try
            {
                string role = GetAttributeValue( "Role" );
                if ( string.IsNullOrWhiteSpace( role ) )
                {
                    role = "0";
                }

                string status = GetAttributeValue( "Status" );
                if ( string.IsNullOrWhiteSpace( status ) )
                {
                    status = "0";
                }

                string contents = GetAttributeValue( "Contents" );
                string panelTitle = GetAttributeValue( "SetPanelTitle" );
                string panelIcon = GetAttributeValue( "SetPanelIcon" );

                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                using ( var rockContext = new RockContext() )
                {
                    var reservationService = new ReservationService( rockContext );
                    var reservationQry = reservationService.Queryable().AsNoTracking();
                    var rockDateTime = RockDateTime.Now;

                    if ( status == "1" )
                    {
                        reservationQry = reservationQry.Where( r => r.LastOccurrenceEndDateTime < rockDateTime );
                    }
                    else
                    {
                        reservationQry = reservationQry.Where( r => r.LastOccurrenceEndDateTime >= rockDateTime );
                    }

                    if ( role == "1" )
                    {
                        reservationQry = reservationService.FilterByMyReservations( reservationQry,  CurrentPerson.Id );
                    }
                    else
                    {
                        reservationQry = reservationService.FilterByMyApprovals( reservationQry, CurrentPerson.Id );
                    }

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Role", role );
                    mergeFields.Add( "Status", status );
                    mergeFields.Add( "Reservations", reservationQry.ToList().OrderBy( r => r.NextStartDateTime ) );
                    mergeFields.Add( "PanelTitle", panelTitle );
                    mergeFields.Add( "PanelIcon", panelIcon );

                    lContents.Text = contents.ResolveMergeFields( mergeFields );
                }

            }
            catch ( Exception ex )
            {
                LogException( ex );
                lContents.Text = "Error Getting Reservations";
            }
        }

        #endregion
    }
}