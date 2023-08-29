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
using System.Linq;
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// Class ReservationTypePicker.
    /// Implements the <see cref="Rock.Web.UI.Controls.RockDropDownList" />
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockDropDownList" />
    public class ReservationTypePicker : RockDropDownList
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationTypePicker" /> class.
        /// </summary>
        public ReservationTypePicker()
            : base()
        {
            Label = "Reservation Type";
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                LoadItems( null );
            }
        }
        private List<int> ReservationTypeIds
        {
            get
            {
                return ViewState["ReservationTypeIds"] as List<int> ?? new ReservationTypeService(new Rock.Data.RockContext()).Queryable().Select( c => c.Id ).ToList();
            }

            set
            {
                ViewState["ReservationTypeIds"] = value;
            }
        }
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? true;
            }

            set
            {
                ViewState["IncludeInactive"] = value;
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>The reservations.</value>
        public List<ReservationType> ReservationTypes
        {
            set
            {
                ReservationTypeIds = value != null ? value.Select( c => c.Id ).ToList() : new List<int>();
                LoadItems( null );
            }
        }

        /// <summary>
        /// Gets or sets the selected reservation identifier.
        /// </summary>
        /// <value>The selected reservation identifier.</value>
        public int? SelectedReservationTypeId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                CheckItem( value );

                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                    this.SelectedValue = id.ToString();
                }
                else
                {
                    // if setting ReservationTypeId to NULL or 0, just default to the first item in the list (which should be nothing)
                    if ( this.Items.Count > 0 )
                    {
                        this.SelectedIndex = 0;
                    }
                }
            }
        }

        public void CheckItem( int? value )
        {
            if ( value.HasValue && value.Value > 0 &&
                this.Items.FindByValue( value.Value.ToString() ) == null &&
                new ReservationTypeService( new Rock.Data.RockContext() ).Get( value.Value ) != null )
            {
                LoadItems( value );
            }
        }

        private void LoadItems( int? selectedValue )
        {
            // Get all the campi
            var reservationTypes = new ReservationTypeService( new Rock.Data.RockContext() ).Queryable().AsNoTracking()
                .Where( c =>
                    ( ReservationTypeIds.Contains( c.Id )
                        && (  c.IsActive || IncludeInactive )
                    )
                    || ( selectedValue.HasValue && c.Id == selectedValue.Value ) )
                .OrderBy( c => c.Name )
                .ToList();

            // Get the current text for the first item if its value is empty
            string firstItemText = Items.Count > 0 && Items[0].Value == string.Empty ? Items[0].Text : string.Empty;

            List<int> selectedItems = new List<int>();

            if ( reservationTypes.Count == 1 )
            {
                // if this is required then auto-select the only reservation type
                if ( this.Required )
                {
                    selectedItems.Add( reservationTypes[0].Id );
                }
            }
            else
            {
                /*
                 * 2020-04-09 ETD
                 * Don't set the Visible property here. If a block setting or somthing else is hiding the control this will show it.
                 * Removed this to fix issue #4172.
                 * this.Visible = true;
                 */

                selectedItems = Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();
            }

            Items.Clear();

            // Add a blank first item.
            Items.Add( new ListItem( firstItemText, string.Empty ) );

            foreach ( ReservationType reservationType in reservationTypes )
            {
                var li = new ListItem( reservationType.Name, reservationType.Id.ToString() );
                li.Selected = selectedItems.Contains( reservationType.Id );
                Items.Add( li );
            }
        }
    }
}