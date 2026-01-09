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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.ReportTemplates;

using Rock;
using Rock.Attribute;
using Rock.Enums.Controls;
using Rock.Field.Types;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;

namespace com.bemaservices.RoomManagement.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) ReportTemplate
    /// Stored as ReportTemplate's Entity Type Guid
    /// </summary>
    [FieldTypeGuid( "6B88A513-4B4C-403B-ADFA-82C3A2B1C3B8" )]

    public class ReportTemplateFieldType : UniversalItemPickerFieldType
    {

        protected override bool IsMultipleSelection => false;

        protected override UniversalItemValuePickerDisplayStyle GetDisplayStyle( Dictionary<string, string> privateConfigurationValues )
        {
            return UniversalItemValuePickerDisplayStyle.List;
        }

        /// <summary>
        /// Gets the list of items to be displayed in the picker.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>A list of item bags that will be rendered in the picker.</returns>
        protected override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            var configuredValues = GetListSource( privateConfigurationValues );

            return configuredValues.Select( item => new ListItemBag
            {
                Value = item.Key,
                Text = item.Value
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
            var configuredValues = GetListSource( privateConfigurationValues );
            return configuredValues
                .Where( item => values.Contains( item.Key ) )
                .Select( item => new ListItemBag
                {
                    Value = item.Key,
                    Text = item.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        public Dictionary<string, string> GetListSource( Dictionary<string, string> configurationValues )
        {
            var allReportTemplates = ReportTemplateContainer.Instance.Components.Values
                .Where( v => v.Value.IsActive == true )
                .Select( v => v.Value.EntityType );

            var reportTemplateList = allReportTemplates
                .ToList();

            return reportTemplateList.ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }
    }
}