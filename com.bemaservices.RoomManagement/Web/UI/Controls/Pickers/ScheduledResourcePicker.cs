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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// Class ScheduledResourcePicker.
    /// Implements the <see cref="Rock.Web.UI.Controls.ItemPicker" />
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.ItemPicker" />
    [ToolboxData( "<{0}:ScheduledResourcePicker runat=server></{0}:ScheduledResourcePicker>" )]
    public class ScheduledResourcePicker : Rock.Web.UI.Controls.ItemPicker
    {
        #region Controls

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        public int? CampusId
        {
            get { return ViewState["CampusId"] as int? ?? 0; }
            set
            {
                ViewState["CampusId"] = value;
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


        /// <summary>
        /// Gets or sets the location ids.
        /// </summary>
        /// <value>The location ids.</value>
        public string LocationIds
        {
            get { return ViewState["LocationIds"] as string ?? string.Empty; }
            set
            {
                ViewState["LocationIds"] = value;
            }
        }



        /// <summary>
        /// The checkbox to show inactive groups
        /// </summary>
        private RockCheckBox _cbShowAllResources;

        /// <summary>
        /// Gets a value indicating whether [show all resources].
        /// </summary>
        /// <value><c>true</c> if [show all resources]; otherwise, <c>false</c>.</value>
        public bool ShowAllResources
        {
            get
            {
                return _cbShowAllResources.Checked;
            }
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _cbShowAllResources = new RockCheckBox();
            _cbShowAllResources.ContainerCssClass = "pull-right";
            _cbShowAllResources.SelectedIconCssClass = "fa fa-check-square-o";
            _cbShowAllResources.UnSelectedIconCssClass = "fa fa-square-o";
            _cbShowAllResources.ID = this.ID + "_cbShowAllResources";
            _cbShowAllResources.Text = "Show All Resources";
            _cbShowAllResources.AutoPostBack = true;
            _cbShowAllResources.CheckedChanged += _cbShowAllResources_CheckedChanged;
            this.Controls.Add( _cbShowAllResources );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();
            this.IconCssClass = "fa fa-cogs";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="resource">The Workflow Type.</param>
        public void SetValue( Resource resource )
        {
            if ( resource != null )
            {
                ItemId = resource.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = resource.Category;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = resource.Name;
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
        /// <param name="resources">The schedules.</param>
        public void SetValues( IEnumerable<Resource> resources )
        {
            var resourceList = resources.ToList();

            if ( resourceList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var resource in resourceList )
                {
                    if ( resource != null )
                    {
                        ids.Add( resource.Id.ToString() );
                        names.Add( resource.Name );
                        var parentCategory = resource.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
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
            var resource = new ResourceService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( resource );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var resources = new ResourceService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( resources );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>The item rest URL.</value>
        public override string ItemRestUrl
        {
            get { return "~/api/com_bemaservices/ScheduledResources/GetChildren/"; }
        }

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            base.RenderCustomPickerActions( writer );

            _cbShowAllResources.RenderControl( writer );
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        /// <param name="includeAllCampuses">if set to <c>true</c> [include all campuses].</param>
        public void SetExtraRestParams( bool includeAllCampuses )
        {
            SetExtraRestParams();
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        public void SetExtraRestParams()
        {
            StringBuilder additionalParams = new StringBuilder();
            additionalParams.Append( "?getCategorizedItems=true" );
            additionalParams.Append( "&showUnnamedEntityItems=true" );
            additionalParams.Append( "&showCategoriesThatHaveNoChildren=true" );
            additionalParams.AppendFormat( "&includeAllCampuses={0}", _cbShowAllResources.Checked.ToTrueFalse() );

            if ( CampusId.HasValue )
            {
                additionalParams.AppendFormat( "&CampusId={0}", CampusId );
            }

            if ( ReservationId.HasValue )
            {
                additionalParams.AppendFormat( "&ReservationId={0}", ReservationId );
            }

            if ( ICalendarContent.IsNotNullOrWhiteSpace() )
            {
                additionalParams.AppendFormat( "&iCalendarContent={0}", Uri.EscapeUriString( ICalendarContent ) );
            }

            if ( SetupTime.HasValue )
            {
                additionalParams.AppendFormat( "&SetupTime={0}", SetupTime.Value );
            }

            if ( CleanupTime.HasValue )
            {
                additionalParams.AppendFormat( "&CleanupTime={0}", CleanupTime.Value );
            }

            if ( LocationIds.IsNotNullOrWhiteSpace() )
            {
                additionalParams.AppendFormat( "&LocationIds={0}", LocationIds );
            }

            ItemRestUrlExtraParams = additionalParams.ToString();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _cbShowAllResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void _cbShowAllResources_CheckedChanged( object sender, EventArgs e )
        {
            ShowDropDown = true;
            SetExtraRestParams();
        }
    }
}