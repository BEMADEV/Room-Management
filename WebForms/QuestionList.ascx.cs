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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// User control for managing note types
    /// </summary>
    [DisplayName( "Question List" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "A list of questions tied to a resource or location" )]
    public partial class QuestionList : RockBlock, ISecondaryBlock
    {
        #region Properties
        /// <summary>
        /// Gets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        public int ResourceId
        {
            get
            {
                return PageParameter( "ResourceId" ).AsInteger();
            }
        }

        /// <summary>
        /// Gets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        public int LocationId
        {
            get
            {
                return PageParameter( "LocationId" ).AsInteger();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rGrid.DataKeyNames = new string[] { "QuestionId" };

            // Offer adding, editing, reordering, deleting and copying only to people who may change these questions
            var canEdit = CanEditQuestions( ResourceId, LocationId );
            rGrid.Actions.ShowAdd = canEdit;
            rGrid.RowClickEnabled = canEdit;
            var reorderField = rGrid.ColumnsOfType<ReorderField>().FirstOrDefault();
            if ( reorderField != null )
            {
                reorderField.Visible = canEdit;
            }

            var deleteField = rGrid.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteField != null )
            {
                deleteField.Visible = canEdit;
            }

            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridReorder += rGrid_GridReorder;
            rGrid.GridRebind += rGrid_GridRebind;

            var securityField = rGrid.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            var lbCopyQuestions = new LinkButton();
            lbCopyQuestions.ID = "lbCopyQuestions";
            lbCopyQuestions.CssClass = "btn btn-default btn-sm pull-left";
            lbCopyQuestions.Text = "<i class='fa fa-clone'></i> Copy Questions From...";
            lbCopyQuestions.ToolTip = "Copies questions from another resource/location to this one.";
            lbCopyQuestions.Click += lbCopyQuestions_Click;
            lbCopyQuestions.Visible = canEdit;
            rGrid.Actions.AddCustomActionControl( lbCopyQuestions );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Deletes the selected question and its attribute when present; used by the Question List grid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var questionService = new ReservationQuestionService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var question = questionService.Get( e.RowKeyId );
                if ( question != null )
                {
                    // Check the question's own resource or location, so a crafted row key can't delete another owner's question
                    if ( !CanEditQuestions( question.ResourceId, question.LocationId ) )
                    {
                        ShowNotAuthorizedToEdit();
                        return;
                    }

                    var attribute = question.Attribute;
                    questionService.Delete( question );
                    if ( attribute != null )
                    {
                        attributeService.Delete( attribute );
                    }

                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( null );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            if ( !CanEditQuestions( ResourceId, LocationId ) )
            {
                ShowNotAuthorizedToEdit();
                return;
            }

            var rockContext = new RockContext();
            var attributes = GetAttributeList();
            if ( attributes != null )
            {
                var attributeService = new AttributeService( rockContext );
                var databaseAttributes = attributeService.GetByIds( attributes.Select( a => a.Id ).ToList() ).OrderBy( a => a.Order ).ToList();
                attributeService.Reorder( databaseAttributes, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var questionService = new ReservationQuestionService( rockContext );
            var attributeService = new AttributeService( rockContext );

            // Save the question
            int questionId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out questionId ) )
            {
                questionId = 0;
            }

            ReservationQuestion question = null;

            if ( questionId != 0 )
            {
                question = questionService.Get( questionId );
            }

            // Check the owner being saved to: the existing question's own resource or location, or this page's for a new one
            var canSave = question != null
                ? CanEditQuestions( question.ResourceId, question.LocationId )
                : CanEditQuestions( ResourceId, LocationId );
            if ( !canSave )
            {
                hfIdValue.Value = string.Empty;
                modalDetails.Hide();
                ShowNotAuthorizedToEdit();
                return;
            }

            if ( question == null )
            {
                question = new ReservationQuestion();
                if ( ResourceId != 0 )
                {
                    question.ResourceId = ResourceId;
                }
                else
                {
                    if ( LocationId != 0 )
                    {
                        question.LocationId = LocationId;
                    }
                }
                questionService.Add( question );
            }

            var newAttribute = GetAttribute( rockContext );

            var entityTypeId = 0;
            if ( ResourceId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;
            }
            else if ( LocationId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION_LOCATION.AsGuid() ).Id;
            }
            newAttribute.EntityTypeId = entityTypeId;

            if ( newAttribute.AbbreviatedName == "" || ( newAttribute.AbbreviatedName == newAttribute.Name && newAttribute.Name.Length > 100 ) )
            {
                newAttribute.AbbreviatedName = newAttribute.Name.Substring( 0, 100 );
            }

            // Controls will show warnings
            if ( !newAttribute.IsValid )
            {
                return;
            }

            // Scope the attribute to this question's resource or location. SaveAttributeEdits always overwrites the qualifier,
            // so it's passed on every save, including edits, or an existing qualifier would be cleared.
            string qualifierColumn;
            string qualifierValue;
            GetQuestionQualifier( question.ResourceId, question.LocationId, out qualifierColumn, out qualifierValue );

            Rock.Model.Attribute savedAttribute = Helper.SaveAttributeEdits( newAttribute, entityTypeId, qualifierColumn, qualifierValue, rockContext );

            EntityTypeAttributesCache.Clear();

            question.AttributeId = savedAttribute.Id;

            if ( question.IsValid )
            {
                rockContext.SaveChanges();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCopyQuestions control by opening up a modal with
        /// a list of resources or locations to copy questions from.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbCopyQuestions_Click( object sender, EventArgs e )
        {
            if ( !CanEditQuestions( ResourceId, LocationId ) )
            {
                ShowNotAuthorizedToEdit();
                return;
            }

            ddlResourceCopySource.Items.Clear();

            var rockContext = new RockContext();
            var qryResources = new ResourceService( rockContext ).Queryable().AsNoTracking();
            var itemList = new List<ListItem>();
            var list = new ResourceService( new RockContext() ).Queryable().AsNoTracking()
                .Where( r => r.Id != ResourceId )
                .OrderBy( r => r.Name )
                .Select( r => new
                {
                    r.Id,
                    r.Name,
                    CampuName = r.Campus.Name
                } );

            foreach ( var item in list )
            {
                var listItem = new ListItem( item.Name, item.Id.ToString() );
                listItem.Attributes.Add( "OptionGroup", item.CampuName );
                ddlResourceCopySource.Items.Add( listItem );
            }

            // Add an empty item
            ddlResourceCopySource.Items.Insert( 0, new ListItem() );
            ddlResourceCopySource.SelectedIndex = -1;
            ddlResourceCopySource.DataBind();

            mdCopyQuestions.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCopyQuestions control copying the questions from the source Resource
        /// to the Location or Resource that is being edited.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdCopyQuestions_SaveClick( object sender, EventArgs e )
        {
            if ( !CanEditQuestions( ResourceId, LocationId ) )
            {
                mdCopyQuestions.Hide();
                ShowNotAuthorizedToEdit();
                return;
            }

            try
            {
                var sourceResourceId = ddlResourceCopySource.SelectedValue.AsIntegerOrNull();
                int? sourceLocationId = null;
                if ( locpLocations != null && locpLocations.Location != null )
                {
                    sourceLocationId = locpLocations.Location.Id;
                }

                var rockContext = new RockContext();
                var questionService = new ReservationQuestionService( rockContext );
                var attributeService = new AttributeService( rockContext );

                // Number the copies after this resource's or location's existing questions, the same way adding a question does
                var existingAttributes = GetAttributeList();
                var nextOrder = existingAttributes.Any() ? existingAttributes.Max( a => a.Order ) + 1 : 1;

                // Copy questions from a source Resource..
                if ( sourceResourceId != null )
                {
                    var sourceQuestionList = questionService.Queryable().Where( q => q.ResourceId == sourceResourceId ).OrderBy( q => q.Attribute.Order ).ToList();
                    nextOrder = CopyQuestionAttributes( rockContext, questionService, attributeService, nextOrder, sourceQuestionList );
                }

                // Copy questions from a source Location... (as long as the source location is not the same as the location being edited)
                if ( sourceLocationId != null && sourceLocationId != LocationId )
                {
                    var sourceQuestionList = questionService.Queryable().Where( q => q.LocationId == sourceLocationId ).OrderBy( q => q.Attribute.Order ).ToList();
                    nextOrder = CopyQuestionAttributes( rockContext, questionService, attributeService, nextOrder, sourceQuestionList );
                }

                EntityTypeAttributesCache.Clear();

                BindGrid();
                mdCopyQuestions.Hide();
            }
            catch ( Exception ex )
            {
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Copies the question attributes to either a resource or a location. Each copy is numbered from
        /// <paramref name="nextOrder" /> and keyed with <see cref="AssignUniqueQuestionKey" />, so copies get the same
        /// entity-type-wide unique keys as questions added by hand, and their order matches their key.
        /// NOTE: This method calls the SaveChanges() method on the rockContext multiple times, so each saved copy
        /// counts as taken when the next one is keyed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="questionService">The question service where the new questions are being added.</param>
        /// <param name="attributeService">The attribute service.</param>
        /// <param name="nextOrder">The order to try for the first copy.</param>
        /// <param name="sourceQuestionList">The source questions, in the order they should be copied.</param>
        /// <returns>The order to try for the next copy.</returns>
        private int CopyQuestionAttributes( RockContext rockContext, ReservationQuestionService questionService, AttributeService attributeService, int nextOrder, List<ReservationQuestion> sourceQuestionList )
        {
            // Set the entityType and key format based on the type being edited (resource or location)
            var entityTypeId = 0;
            var keyFormat = string.Empty;
            var ownerId = 0;
            if ( ResourceId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;
                keyFormat = "Q{0}_ResourceId{1}";
                ownerId = ResourceId;
            }
            else if ( LocationId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION_LOCATION.AsGuid() ).Id;
                keyFormat = "Q{0}_LocationId{1}";
                ownerId = LocationId;
            }

            foreach ( ReservationQuestion question in sourceQuestionList )
            {
                var cloneQuestion = question.Clone() as ReservationQuestion;
                cloneQuestion.CreatedByPersonAlias = null;
                cloneQuestion.CreatedByPersonAliasId = CurrentPersonAliasId;
                cloneQuestion.CreatedDateTime = RockDateTime.Now;
                cloneQuestion.ModifiedByPersonAlias = null;
                cloneQuestion.ModifiedByPersonAliasId = CurrentPersonAliasId;
                cloneQuestion.ModifiedDateTime = RockDateTime.Now;
                cloneQuestion.Id = 0;
                cloneQuestion.Guid = Guid.NewGuid();
                cloneQuestion.Location = null;
                cloneQuestion.LocationId = null;
                cloneQuestion.Resource = null;
                cloneQuestion.ResourceId = null;
                cloneQuestion.Attribute = null;

                // Make a copy of the question's corresponding attribute.
                var cloneAttribute = question.Attribute.Clone( false ) as Rock.Model.Attribute;
                cloneAttribute.Id = 0;
                cloneAttribute.Guid = Guid.NewGuid();
                cloneAttribute.IsSystem = false;
                cloneAttribute.EntityTypeId = entityTypeId;
                cloneAttribute.AttributeQualifiers.Clear();

                // Set the cloned question to this resource's or location's Id.
                if ( ResourceId != 0 )
                {
                    cloneQuestion.ResourceId = ResourceId;
                }
                else if ( LocationId != 0 )
                {
                    cloneQuestion.LocationId = LocationId;
                }

                // Scope the copy to this resource or location; the clone would otherwise keep the source's qualifier
                string qualifierColumn;
                string qualifierValue;
                GetQuestionQualifier( cloneQuestion.ResourceId, cloneQuestion.LocationId, out qualifierColumn, out qualifierValue );
                cloneAttribute.EntityTypeQualifierColumn = qualifierColumn;
                cloneAttribute.EntityTypeQualifierValue = qualifierValue;

                // Key the copy after this owner's existing questions, unique across the entity type
                cloneAttribute.Order = nextOrder;
                AssignUniqueQuestionKey( cloneAttribute, attributeService, entityTypeId, keyFormat, ownerId, null );
                nextOrder = cloneAttribute.Order + 1;

                foreach ( var qualifier in question.Attribute.AttributeQualifiers )
                {
                    var newQualifier = qualifier.Clone( false );
                    newQualifier.Id = 0;
                    newQualifier.Guid = Guid.NewGuid();
                    newQualifier.IsSystem = false;
                    newQualifier.Attribute = null;
                    newQualifier.AttributeId = 0;
                    cloneAttribute.AttributeQualifiers.Add( newQualifier );
                }

                attributeService.Add( cloneAttribute );
                rockContext.SaveChanges();

                cloneQuestion.AttributeId = cloneAttribute.Id;
                questionService.Add( cloneQuestion );
                rockContext.SaveChanges();
            }

            return nextOrder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            List<ReservationQuestion> questionList = GetQuestions();

            rGrid.DataSource = questionList.Select( q => new
            {
                Id = q.Attribute.Id,
                QuestionId = q.Id,
                Order = q.Attribute.Order,
                Question = q.Attribute.Name,
                FieldType = q.Attribute.FieldType.Name
            } )
            .OrderBy( q => q.Order )
            .ToList();
            

            rGrid.DataBind();
        }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns>List&lt;Question&gt;.</returns>
        private List<ReservationQuestion> GetQuestions()
        {
            var questionList = new List<ReservationQuestion>();
            var questionService = new ReservationQuestionService( new RockContext() );
            if ( ResourceId != 0 )
            {
                questionList = questionService.Queryable().Where( q => q.ResourceId == ResourceId ).ToList();
            }
            else
            {
                if ( LocationId != 0 )
                {
                    questionList = questionService.Queryable().Where( q => q.LocationId == LocationId ).ToList();
                }
            }

            return questionList;
        }

        /// <summary>
        /// Determines whether the current person may add, edit, reorder, delete or copy questions for a resource or location:
        /// they need Edit on this block or Edit on the resource (or location) that owns the questions, matching the Resource
        /// Detail and Location Layout List rules. The grid uses this to show its editing controls, and every handler that
        /// changes questions checks it again on the server, against the question's own owner where there is one.
        /// </summary>
        /// <param name="resourceId">The owning resource identifier, or <c>null</c>/0 if the questions belong to a location.</param>
        /// <param name="locationId">The owning location identifier, used when there is no resource.</param>
        /// <returns><c>true</c> if the current person may change the questions; otherwise, <c>false</c>.</returns>
        private bool CanEditQuestions( int? resourceId, int? locationId )
        {
            if ( UserCanEdit )
            {
                return true;
            }

            var rockContext = new RockContext();
            if ( resourceId.HasValue && resourceId.Value != 0 )
            {
                var resource = new ResourceService( rockContext ).Get( resourceId.Value );
                return resource != null && resource.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            if ( locationId.HasValue && locationId.Value != 0 )
            {
                var location = new LocationService( rockContext ).Get( locationId.Value );
                return location != null && location.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            return false;
        }

        /// <summary>
        /// Tells the person they aren't allowed to change these questions.
        /// </summary>
        private void ShowNotAuthorizedToEdit()
        {
            nbMessage.Text = "You are not authorized to change these questions.";
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        protected void ShowEdit( int? questionId )
        {
            ReservationQuestion question = null;
            if ( questionId.HasValue )
            {
                question = new ReservationQuestionService( new RockContext() ).Get( questionId.Value );
            }
            else
            {
                question = new ReservationQuestion();
                question.Attribute = new Rock.Model.Attribute();
                question.Attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
            }

            // Hidden add buttons and disabled row clicks don't stop a crafted postback, so check before opening the editor
            var canEdit = questionId.HasValue
                ? question != null && CanEditQuestions( question.ResourceId, question.LocationId )
                : CanEditQuestions( ResourceId, LocationId );
            if ( !canEdit )
            {
                ShowNotAuthorizedToEdit();
                return;
            }

            var reservedKeyNames = new List<string>();
            GetAttributeList().Where( a => !a.Guid.Equals( question.Attribute.Guid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            //var includeFields = new List<FieldTypeCache>();
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.MEMO.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.DATE.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ) );
            //edtQuestion.IncludedFieldTypes = includeFields.ToArray();

            edtQuestion.ReservedKeyNames = reservedKeyNames.ToList();
            edtQuestion.IsKeyEditable = false;

            Type objectType = null;
            if ( ResourceId != 0 )
            {
                objectType = typeof( ReservationResource );
            }
            else
            {
                if ( LocationId != 0 )
                {
                    objectType = typeof( ReservationLocation );
                }
            }

            edtQuestion.SetAttributeProperties( question.Attribute, objectType );

            hfIdValue.Value = questionId.ToString();
            modalDetails.Show();
        }

        /// <summary>
        /// Builds the question attribute and assigns a key that is unique for its resource or location entity type.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The attribute to associate with the reservation question.</returns>
        private Rock.Model.Attribute GetAttribute( RockContext rockContext )
        {
            List<Rock.Model.Attribute> attributeList = GetAttributeList();

            // Create and update a new attribute object with new values
            rockContext = rockContext ?? new RockContext();
            var internalAttributeService = new AttributeService( rockContext );

            Rock.Model.Attribute oldAttribute = null;
            var newAttribute = new Rock.Model.Attribute();

            if ( edtQuestion.AttributeId.HasValue )
            {
                oldAttribute = internalAttributeService.Get( edtQuestion.AttributeId.Value );
            }

            if ( oldAttribute != null )
            {
                newAttribute.CopyPropertiesFrom( oldAttribute );
            }

            edtQuestion.GetAttributeProperties( newAttribute );

            if ( oldAttribute == null )
            {
                newAttribute.Order = attributeList.Any() ? attributeList.Max( a => a.Order ) + 1 : 1;
            }

            if ( !newAttribute.Key.Contains( "_ResourceId" ) && !newAttribute.Key.Contains( "_LocationId" ) )
            {
                int entityTypeId;
                string keyFormat;
                int ownerId;
                if ( ResourceId != 0 )
                {
                    entityTypeId = EntityTypeCache.Get( typeof( ReservationResource ) ).Id;
                    keyFormat = "Q{0}_ResourceId{1}";
                    ownerId = ResourceId;
                }
                else if ( LocationId != 0 )
                {
                    entityTypeId = EntityTypeCache.Get( typeof( ReservationLocation ) ).Id;
                    keyFormat = "Q{0}_LocationId{1}";
                    ownerId = LocationId;
                }
                else
                {
                    return newAttribute;
                }

                AssignUniqueQuestionKey( newAttribute, internalAttributeService, entityTypeId, keyFormat, ownerId, edtQuestion.AttributeId );
            }

            return newAttribute;
        }

        /// <summary>
        /// Gets the attribute qualifier that scopes a question to its resource or location. Question attributes live on the
        /// ReservationResource or ReservationLocation entity type; qualifying them by that entity's ResourceId or LocationId
        /// property makes Rock load each question only for its own resource or location, in Lava and REST as well as here.
        /// Keys stay unique across the whole entity type (see <see cref="AssignUniqueQuestionKey" />), since older questions
        /// may still be unqualified.
        /// </summary>
        /// <param name="resourceId">The question's resource identifier, if it belongs to a resource.</param>
        /// <param name="locationId">The question's location identifier, if it belongs to a location.</param>
        /// <param name="qualifierColumn">The qualifier column, or <c>null</c> if the question has no owner.</param>
        /// <param name="qualifierValue">The qualifier value, or <c>null</c> if the question has no owner.</param>
        private static void GetQuestionQualifier( int? resourceId, int? locationId, out string qualifierColumn, out string qualifierValue )
        {
            qualifierColumn = null;
            qualifierValue = null;

            if ( resourceId.HasValue )
            {
                qualifierColumn = "ResourceId";
                qualifierValue = resourceId.Value.ToString();
            }
            else if ( locationId.HasValue )
            {
                qualifierColumn = "LocationId";
                qualifierValue = locationId.Value.ToString();
            }
        }

        /// <summary>
        /// Gives a question attribute the first <c>Q{Order}_ResourceId{Id}</c> or <c>Q{Order}_LocationId{Id}</c> key, starting
        /// at its current <see cref="Rock.Model.Attribute.Order" />, that no other attribute of the same entity type already uses,
        /// and sets the order to match the key. Keys must be unique across the whole entity type, not just one resource or
        /// location, because Rock loads attributes by key and silently drops duplicates. Used when adding, editing and copying
        /// questions so all three number keys the same way.
        /// </summary>
        /// <param name="attribute">The question attribute to key. Its order is the first number tried.</param>
        /// <param name="attributeService">The attribute service used to look for existing keys.</param>
        /// <param name="entityTypeId">The ReservationResource or ReservationLocation entity type identifier.</param>
        /// <param name="keyFormat">The key format, with the order as {0} and the owner identifier as {1}.</param>
        /// <param name="ownerId">The resource or location identifier.</param>
        /// <param name="excludeAttributeId">The attribute being edited, so its own key doesn't count as taken.</param>
        private static void AssignUniqueQuestionKey( Rock.Model.Attribute attribute, AttributeService attributeService, int entityTypeId, string keyFormat, int ownerId, int? excludeAttributeId )
        {
            // Try each order in turn until the key isn't used by another attribute of this entity type
            do
            {
                attribute.Key = String.Format( keyFormat, attribute.Order, ownerId );
                attribute.Order++;
            }
            while ( attributeService.Queryable().Any( a =>
                a.EntityTypeId == entityTypeId &&
                a.Key == attribute.Key &&
                ( !excludeAttributeId.HasValue || a.Id != excludeAttributeId.Value ) ) );

            attribute.Order--;
        }

        /// <summary>
        /// Gets the attribute list.
        /// </summary>
        /// <returns>List&lt;Rock.Model.Attribute&gt;.</returns>
        private List<Rock.Model.Attribute> GetAttributeList()
        {
            var rockContext = new RockContext();
            var questionService = new ReservationQuestionService( rockContext );
            var attributeList = new List<Rock.Model.Attribute>();
            if ( ResourceId != 0 )
            {
                attributeList = questionService.Queryable().Where( q => q.ResourceId == ResourceId ).Select( q => q.Attribute ).ToList();
            }
            else
            {
                if ( LocationId != 0 )
                {
                    attributeList = questionService.Queryable().Where( q => q.LocationId == LocationId ).Select( q => q.Attribute ).ToList();
                }
            }

            return attributeList;
        }

        #endregion
    }
}