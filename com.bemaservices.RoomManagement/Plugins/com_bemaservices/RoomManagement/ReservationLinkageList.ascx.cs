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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// A Block that displays the linkages related to an event registration instance.
    /// </summary>
    [DisplayName( "Reservation Linkage List" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Displays the linkages associated with a reservation." )]

    #region Block Attributes

    [LinkedPage(
        "Linkage Page",
        "The page for creating a reservation linkage",
        Key = AttributeKey.LinkageDetailPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_LINKAGE,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Calendar Item Page",
        "The page to view calendar item details",
        Key = AttributeKey.CalendarItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAIL,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Content Item Page",
        "The page for viewing details about a content channel item",
        Key = AttributeKey.ContentItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.CONTENT_DETAIL,
        IsRequired = false,
        Order = 3 )]

    #endregion

    public partial class ReservationLinkageList : RockBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The page for editing the linkage details.
            /// </summary>
            public const string LinkageDetailPage = "LinkagePage";

            /// <summary>
            /// The page for editing the Calendar Item associated with a linkage.
            /// </summary>
            public const string CalendarItemDetailPage = "CalendarItemDetailPage";

            /// <summary>
            /// The page for editing the Content Channel Item associated with a linkage.
            /// </summary>
            public const string ContentItemDetailPage = "ContentItemDetailPage";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLinkages.EmptyDataText = "No Linkages Found";
            gLinkages.DataKeyNames = new string[] { "Id" };
            gLinkages.Actions.AddClick += gLinkages_AddClick;
            gLinkages.RowDataBound += gLinkages_RowDataBound;
            gLinkages.GridRebind += gLinkages_GridRebind;
            this.BlockUpdated += ReservationLinkageList_BlockUpdated;

            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the ReservationLinkageList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ReservationLinkageList_BlockUpdated( object sender, EventArgs e )
        {
            BindLinkagesGrid();
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
                ShowDetail();
            }
        }


        #endregion

        #region Events

        #region Linkage Tab Events

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fLinkages_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLinkages_GridRebind( object sender, EventArgs e )
        {
            var reservationId = PageParameter( "ReservationId" ).AsIntegerOrNull();
            if ( reservationId.HasValue )
            {
                var reservation = new ReservationService( new RockContext() ).Get( reservationId.Value );
                if ( reservation == null )
                {
                    return;
                }

                gLinkages.ExportTitleName = reservation.Name + " Linkages";
                gLinkages.ExportFilename = gLinkages.ExportFilename ?? reservation.Name + "ReservationLinkages";

                BindLinkagesGrid();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gLinkages_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var reservationLinkage = e.Row.DataItem as ReservationLinkage;

                if ( reservationLinkage != null && reservationLinkage.EventItemOccurrence != null )
                {
                    if ( reservationLinkage.EventItemOccurrence.EventItem != null )
                    {
                        var lCalendarItem = e.Row.FindControl( "lCalendarItem" ) as Literal;

                        if ( lCalendarItem != null )
                        {
                            var calendarItems = new List<string>();

                            foreach ( var calendarItem in reservationLinkage.EventItemOccurrence.EventItem.EventCalendarItems )
                            {
                                if ( calendarItem.EventItem != null && calendarItem.EventCalendar != null )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "EventCalendarId", calendarItem.EventCalendarId.ToString() );
                                    qryParams.Add( "EventItemId", calendarItem.EventItem.Id.ToString() );

                                    var calendarEventUrl = LinkedPageUrl( AttributeKey.CalendarItemDetailPage, qryParams );

                                    if ( string.IsNullOrWhiteSpace( calendarEventUrl ) )
                                    {
                                        calendarItems.Add( string.Format( "{0} ({1})", calendarItem.EventItem.Name, calendarItem.EventCalendar.Name ) );
                                    }
                                    else
                                    {
                                        calendarItems.Add( string.Format( "<a href='{0}'>{1}</a> ({2})", calendarEventUrl, calendarItem.EventItem.Name, calendarItem.EventCalendar.Name ) );
                                    }
                                }
                            }

                            lCalendarItem.Text = calendarItems.AsDelimited( "<br/>" );
                        }

                        if ( reservationLinkage.EventItemOccurrence.ContentChannelItems.Any() )
                        {
                            var lContentItem = e.Row.FindControl( "lContentItem" ) as Literal;

                            if ( lContentItem != null )
                            {
                                var contentItems = new List<string>();

                                foreach ( var contentItem in reservationLinkage.EventItemOccurrence.ContentChannelItems
                                    .Where( c => c.ContentChannelItem != null )
                                    .Select( c => c.ContentChannelItem ) )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "ContentItemId", contentItem.Id.ToString() );

                                    var contentItemUrl = LinkedPageUrl( AttributeKey.ContentItemDetailPage, qryParams );

                                    if ( string.IsNullOrWhiteSpace( contentItemUrl ) )
                                    {
                                        contentItems.Add( contentItem.Title );
                                    }
                                    else
                                    {
                                        contentItems.Add( string.Format( "<a href='{0}'>{1}</a>", contentItemUrl, contentItem.Title ) );
                                    }
                                }

                                lContentItem.Text = contentItems.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLinkages_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LinkageDetailPage, "LinkageId", 0, "ReservationId", PageParameter( "ReservationId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Delete event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLinkages_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var reservationLinkageService = new ReservationLinkageService( rockContext );
                var reservationLinkage = reservationLinkageService.Get( e.RowKeyId );
                if ( reservationLinkage != null )
                {
                    reservationLinkageService.Delete( reservationLinkage );
                    rockContext.SaveChanges();
                }
            }

            BindLinkagesGrid();
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            bool readOnly = true;

            int? reservationId = PageParameter( "ReservationId" ).AsInteger();

            if ( reservationId.HasValue )
            {
                var reservation = new ReservationService( new RockContext() ).Get( reservationId.Value );
                if ( reservation != null )
                {
                    if ( reservation.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                    {
                        readOnly = false;
                    }

                    var canCreateReservations = reservation.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    if ( canCreateReservations &&
                        ( reservation.CreatedByPersonAliasId == CurrentPersonAliasId ||
                        reservation.AdministrativeContactPersonAliasId == CurrentPersonAliasId ||
                        reservationId == 0 )
                        )
                    {
                        readOnly = false;
                    }
                }
                else
                {
                    pnlLinkages.Visible = false;
                }
            }
            else
            {
                pnlLinkages.Visible = false;
            }

            gLinkages.Actions.ShowAdd = !readOnly;

            BindLinkagesGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindLinkagesGrid()
        {
            int? reservationId = PageParameter( "ReservationId" ).AsInteger();

            if ( !reservationId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var qry = new ReservationLinkageService( rockContext )
                    .Queryable( "EventItemOccurrence.EventItem.EventCalendarItems.EventCalendar,EventItemOccurrence.ContentChannelItems.ContentChannelItem" )
                    .AsNoTracking()
                    .Where( r => r.ReservationId == reservationId.Value );

                IOrderedQueryable<ReservationLinkage> orderedQry = null;
                SortProperty sortProperty = gLinkages.SortProperty;
                if ( sortProperty != null )
                {
                    orderedQry = qry.Sort( sortProperty );
                }
                else
                {
                    orderedQry = qry.OrderByDescending( r => r.CreatedDateTime );
                }

                gLinkages.SetLinqDataSource( orderedQry );
                gLinkages.DataBind();
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion
    }
}