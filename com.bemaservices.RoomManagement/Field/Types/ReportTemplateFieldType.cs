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
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.ReportTemplates;
using com.bemaservices.RoomManagement.Web.UI.Controls;

using Rock;
using Rock.Data;
using Rock.Field;
using Rock.MergeTemplates;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) ReportTemplate
    /// Stored as ReportTemplate's Entity Type Guid
    /// </summary>
    public class ReportTemplateFieldType : Rock.Field.FieldType, IEntityFieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>System.String.</returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = value;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var reportTemplate = EntityTypeCache.Get( value.AsGuid() );
                if ( reportTemplate != null )
                {
                    formattedValue = reportTemplate.FriendlyName;
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>The control</returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var reportTemplatePicker = new ReportTemplatePicker { ID = id };

            var a = ReportTemplateContainer.Instance;
            var b = a.Components;
            var c = b.Values;
            var d = c.Where( v => v.Value.IsActive == true );
            var e = d.Select( v => v.Value.EntityType );
            var f = e.ToList();

            var allReportTemplates = ReportTemplateContainer.Instance.Components.Values
                .Where( v => v.Value.IsActive == true )
                .Select( v => v.Value.EntityType );

            var reportTemplateList = allReportTemplates
                .ToList();

            if ( reportTemplateList.Any() )
            {
                reportTemplatePicker.ReportTemplates = reportTemplateList;
                return reportTemplatePicker;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns ReportTemplate Entity Type Guid as string
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>System.String.</returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            ReportTemplatePicker reportTemplatePicker = control as ReportTemplatePicker;

            if ( reportTemplatePicker != null )
            {
                int? reportTemplateId = reportTemplatePicker.SelectedReportTemplateId;
                if ( reportTemplateId.HasValue )
                {
                    var reportTemplate = EntityTypeCache.Get( reportTemplateId.Value );
                    if ( reportTemplate != null )
                    {
                        return reportTemplate.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// Expects value as a ReportTemplate Entity Type Guid as string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            ReportTemplatePicker reportTemplatePicker = control as ReportTemplatePicker;

            if ( reportTemplatePicker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var reportTemplate = EntityTypeCache.Get( guid );
                reportTemplatePicker.SetValue( reportTemplate == null ? "0" : reportTemplate.Id.ToString() );
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns>Control.</returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var lbl = new Label();
            lbl.ID = string.Format( "{0}_lIs", id );
            lbl.AddCssClass( "data-view-filter-label" );
            lbl.Text = "Is";

            // hide the compare control when in SimpleFilter mode
            lbl.Visible = filterMode != FilterMode.SimpleFilter;

            return lbl;
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns>Control.</returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var cbList = new RockCheckBoxList();
            cbList.ID = string.Format( "{0}_cbList", id );
            cbList.AddCssClass( "js-filter-control" );
            cbList.RepeatDirection = RepeatDirection.Horizontal;

            var allReportTemplates = ReportTemplateContainer.Instance.Components.Values
                .Where( v => v.Value.IsActive == true )
                .Select( v => v.Value.EntityType );

            var reportTemplateList = allReportTemplates
                .ToList();

            if ( reportTemplateList.Any() )
            {
                foreach ( EntityTypeCache reportTemplate in reportTemplateList )
                {
                    cbList.Items.Add( new ListItem( reportTemplate.FriendlyName, reportTemplate.Guid.ToString() ) );
                }
                return cbList;
            }

            return null;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var reportTemplateGuids = value.SplitDelimitedValues().AsGuidList();

            var allReportTemplates = ReportTemplateContainer.Instance.Components.Values
                .Where( v => v.Value.IsActive == true )
                .Select( v => v.Value.EntityType );

            var reportTemplateList = reportTemplateGuids.Select( a => EntityTypeCache.Get( a ) ).Where( c => c != null );

            return reportTemplateList.Select( a => a.FriendlyName ).ToList().AsDelimited( ", ", " or " );
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns>System.String.</returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            return null;
        }

        /// <summary>
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetEqualToCompareValue()
        {
            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>System.String.</returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            if ( control != null && control is CheckBoxList )
            {
                CheckBoxList cbl = ( CheckBoxList ) control;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }
            }

            return values.AsDelimited( "," );
        }

        /// <summary>
        /// Sets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterCompareValue( Control control, string value )
        {
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is CheckBoxList && value != null )
            {
                var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                CheckBoxList cbl = ( CheckBoxList ) control;
                foreach ( ListItem li in cbl.Items )
                {
                    li.Selected = values.Contains( li.Value );
                }
            }
        }

        /// <summary>
        /// Gets the filters expression.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count == 1 )
            {
                List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                if ( selectedValues.Any() )
                {
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                }
            }

            return null;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = EntityTypeCache.Get( guid );
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
            EntityTypeCache item = null;
            if ( id.HasValue )
            {
                item = EntityTypeCache.Get( id.Value );
            }
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>IEntity.</returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IEntity.</returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                return EntityTypeCache.Get( guid.Value ) as IEntity;
            }

            return null;
        }

        #endregion

    }
}