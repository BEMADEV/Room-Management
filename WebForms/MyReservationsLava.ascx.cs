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

    [CustomRadioListField( "Role",
        Key = AttributeKey.Role,
        Description = "Display the active reservations that the current user Initiated / is a Contact for, or is currently Assigned To for approval.",
        ListSource = "0^Assigned To,1^Initiated",
        IsRequired = true,
        DefaultValue = "0",
        Order = 0 )]
    [CustomRadioListField( "Status",
        Key = AttributeKey.Status,
        Description = "Display upcoming reservations, or past reservations.",
        ListSource = "0^Upcoming,1^Past",
        IsRequired = true,
        DefaultValue = "0",
        Order = 1 )]
    [CodeEditorField( "Contents",
        Key = AttributeKey.Contents,
        Description = "The Lava template to use for displaying reservations assigned to current user.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/MyReservationsSortable.lava' %}",
        Category = "",
        Order = 3 )]
    [TextField( "Set Panel Title",
        Key = AttributeKey.SetPanelTitle,
        Description = "The title to display in the panel header. Leave empty to have the block name.",
        IsRequired = false,
        Order = 4 )]
    [TextField( "Set Panel Icon",
        Key = AttributeKey.SetPanelIcon,
        Description = "The icon to display in the panel header.",
        IsRequired = false,
        Order = 5 )]
    [Rock.SystemGuid.BlockTypeGuid( "0D2E85E7-5881-42C4-9CB1-6F8830BB620B" )]
    public partial class MyReservationsLava : Rock.Web.UI.RockBlock
    {
        #region Keys

        /// <summary>
        /// The block setting keys.
        /// </summary>
        private static class AttributeKey
        {
            public const string Role = "Role";
            public const string Status = "Status";
            public const string Contents = "Contents";
            public const string SetPanelTitle = "SetPanelTitle";
            public const string SetPanelIcon = "SetPanelIcon";
        }

        #endregion Keys

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
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders the configured reservation Lava template, using an empty list for anonymous visitors.
        /// </summary>
        private void BindData()
        {
            try
            {
                string role = GetAttributeValue( AttributeKey.Role );
                if ( string.IsNullOrWhiteSpace( role ) )
                {
                    role = "0";
                }

                string status = GetAttributeValue( AttributeKey.Status );
                if ( string.IsNullOrWhiteSpace( status ) )
                {
                    status = "0";
                }

                string contents = GetAttributeValue( AttributeKey.Contents );
                string panelTitle = GetAttributeValue( AttributeKey.SetPanelTitle );
                string panelIcon = GetAttributeValue( AttributeKey.SetPanelIcon );

                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                IEnumerable<Reservation> reservations = Enumerable.Empty<Reservation>();
                if ( CurrentPersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var reservationQueryOptions = new ReservationQueryOptions();
                        if ( role == "1" )
                        {
                            reservationQueryOptions.ReservationsByPersonId = CurrentPersonId.Value;
                        }
                        else
                        {
                            reservationQueryOptions.ApprovalsByPersonId = CurrentPersonId.Value;
                        }

                        var reservationService = new ReservationService( rockContext );
                        var reservationQry = reservationService.Queryable( reservationQueryOptions ).AsNoTracking();

                        var rockDateTime = RockDateTime.Now;
                        if ( status == "1" )
                        {
                            reservationQry = reservationQry.Where( r => r.LastOccurrenceEndDateTime < rockDateTime );
                        }
                        else
                        {
                            reservationQry = reservationQry.Where( r => r.LastOccurrenceEndDateTime >= rockDateTime );
                        }

                        reservations = reservationQry.ToList().OrderBy( r => r.NextStartDateTime ).ToList();
                    }
                }

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Role", role );
                mergeFields.Add( "Status", status );
                mergeFields.Add( "Reservations", reservations );
                mergeFields.Add( "PanelTitle", panelTitle );
                mergeFields.Add( "PanelIcon", panelIcon );

                lContents.Text = contents.ResolveMergeFields( mergeFields );

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