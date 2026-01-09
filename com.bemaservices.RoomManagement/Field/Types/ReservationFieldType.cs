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
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Web.UI.Controls;

using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Field.Types;
using Rock.Reporting;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Reservation
    /// Stored as Reservation's Guid
    /// </summary>
    [FieldTypeGuid( "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" )]
    public class ReservationFieldType : UniversalItemPickerFieldType, IEntityFieldType
    {

        /// <summary>
        /// Gets the list of items to be displayed in the picker.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>A list of item bags that will be rendered in the picker.</returns>
        protected override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            return new ReservationService( new RockContext() ).Queryable()
            .Select( item => new ListItemBag
            {
                Value = item.Guid.ToString(),
                Text = item.Name
            } )
            .ToList();
        }

        /// <summary>
        /// Gets the item bags for the values. If an item is not found
        /// (for example, no longer exists), then it should not be included
        /// in the returned list.
        /// </summary>
        /// <param name="values">The individual values that should be retrieved.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A list of <see cref="T:Rock.ViewModels.Utility.ListItemBag" /> objects that have the <see cref="P:Rock.ViewModels.Utility.ListItemBag.Value" /> and <see cref="P:Rock.ViewModels.Utility.ListItemBag.Text" /> properties filled in.</returns>
        protected override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            return new ReservationService( new RockContext() ).Queryable()
            .Where( item => values.Contains( item.Guid.ToString() ) )
            .Select( item => new ListItemBag
            {
                Value = item.Guid.ToString(),
                Text = item.Name
            } )
            .ToList();
        }

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new ReservationService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            Reservation item = null;
            if ( id.HasValue )
            {
                item = new ReservationService( new RockContext() ).Get( id.Value );
            }
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new ReservationService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}