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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using static com.bemaservices.RoomManagement.Model.ReservationService;

namespace Rock.Rest.Controllers
{

    /// <summary>
    /// Class ResourcesController.
    /// Implements the <see cref="Rock.Rest.ApiController{com.bemaservices.RoomManagement.Model.Resource}" />
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiController{com.bemaservices.RoomManagement.Model.Resource}" />
    public partial class ResourcesController : Rock.Rest.ApiController<com.bemaservices.RoomManagement.Model.Resource>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcesController" /> class.
        /// </summary>
        public ResourcesController() : base( new com.bemaservices.RoomManagement.Model.ResourceService( new Rock.Data.RockContext() ) ) { }
    }


    public partial class ResourcesController
    {
        /// <summary>
        /// Returns the resource tree items the current person is authorized to view. The picker reads the
        /// tree with a POST, which Rock's <see cref="SecuredAttribute" /> would otherwise treat as an edit,
        /// so the endpoint asks only for View; each category and resource is still filtered by View below.
        /// </summary>
        /// <param name="options">The tree picker options.</param>
        /// <returns>The authorized resource and category tree items.</returns>
        [HttpPost]
        [Authenticate, Secured( Authorization.VIEW )]
        [System.Web.Http.Route( "api/v2/plugins/com.bemaservices/roommanagement/models/resource/tree" )]
        [RestActionGuid( "eb007a09-7a4e-4773-bd5a-729c8fc61e93" )]
        public IHttpActionResult PostTreeItems(
            [FromBody] UniversalItemTreePickerOptionsBag options )
        {
            if ( options == null )
            {
                return BadRequest( "Tree picker options are required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var resourceService = new ResourceService( rockContext );
                var categoryService = new CategoryService( rockContext );
                var currentPerson = GetPerson();
                var expandGuids = GetExpandGuids( resourceService,
                    options.ExpandToValues?.AsGuidList(),
                    currentPerson );
                var resourcesAndCategories = LoadCategories( categoryService,
                        resourceService,
                    options.ParentValue.AsGuidOrNull(),
                    expandGuids,
                    currentPerson );

                return Ok( resourcesAndCategories );
            }
        }

        /// <summary>
        /// Gets the authorized category ancestors needed to expand the requested resources.
        /// </summary>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="expandToGuids">The resource GUIDs to expand to.</param>
        /// <param name="currentPerson">The person requesting the tree.</param>
        /// <returns>The authorized category ancestor GUIDs.</returns>
        private List<Guid> GetExpandGuids( ResourceService resourceService,
            List<Guid> expandToGuids,
            Rock.Model.Person currentPerson )
        {
            var expandGuids = new List<Guid>();

            if ( expandToGuids == null )
            {
                return expandGuids;
            }

            foreach ( var guid in expandToGuids )
            {
                var resource = resourceService.Get( guid );
                if ( resource == null || !resource.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                var category = resource.Category;

                while ( category != null )
                {
                    if ( category.IsAuthorized( Authorization.VIEW, currentPerson ) && !expandGuids.Contains( category.Guid ) )
                    {
                        expandGuids.Add( category.Guid );
                    }

                    category = category.ParentCategory;
                }
            }

            return expandGuids;
        }

        /// <summary>
        /// Builds one level of the resource category tree using only categories and resources the person may view.
        /// </summary>
        /// <param name="categoryService">The category service.</param>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="parentGuid">The parent category GUID, or null for root categories.</param>
        /// <param name="expandGuids">The category GUIDs to expand.</param>
        /// <param name="currentPerson">The person requesting the tree.</param>
        /// <returns>The authorized tree items for this level.</returns>
        private List<TreeItemBag> LoadCategories( CategoryService categoryService,
            ResourceService resourceService,
            Guid? parentGuid,
            List<Guid> expandGuids,
            Rock.Model.Person currentPerson )
        {
            var resourceEntityType = EntityTypeCache.Get( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESOURCE.AsGuid() );
            var categoryQry = categoryService.Queryable()
                .Where( c => c.EntityTypeId == resourceEntityType.Id )
                .Where( c => c.Name != null && c.Name != string.Empty )
                .Where( c =>
                    (
                        parentGuid.HasValue
                        && c.ParentCategory.Guid == parentGuid.Value
                    )
                    || ( !parentGuid.HasValue && !c.ParentCategoryId.HasValue ) );

            var items = new List<TreeItemBag>();
            var categoryList = categoryQry.ToList()
                .Where( c => c.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();

            var categoryIds = categoryList.Select( c => c.Id ).ToList();
            var categoriesWithViewableChildren = categoryIds.Any()
                ? categoryService.Queryable()
                    .Where( c => c.ParentCategoryId.HasValue && categoryIds.Contains( c.ParentCategoryId.Value ) )
                    .Where( c => c.EntityTypeId == resourceEntityType.Id && c.Name != null && c.Name != string.Empty )
                    .ToList()
                    .Where( c => c.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .Select( c => c.ParentCategoryId.Value )
                    .Distinct()
                    .ToList()
                : new List<int>();

            if ( parentGuid != null )
            {
                items.AddRange( LoadResources( resourceService, parentGuid.Value, currentPerson ) );
            }

            foreach ( var category in categoryList )
            {
                var resourceChildren = LoadResources( resourceService,
                    category.Guid,
                    currentPerson );

                var item = new TreeItemBag
                {
                    Value = category.Guid.ToString(),
                    IconCssClass = category.IconCssClass,
                    Text = category.Name,
                    Type = "Category",
                    IsFolder = true,
                    IsActive = true,
                    HasChildren = resourceChildren.Any() || categoriesWithViewableChildren.Contains( category.Id )
                };

                if ( expandGuids.Contains( category.Guid ) )
                {
                    var categoryChildren = LoadCategories( categoryService,
                            resourceService,
                        category.Guid,
                        expandGuids,
                        currentPerson );
                    item.Children = new List<TreeItemBag>();
                    item.Children.AddRange( categoryChildren );
                    item.Children.AddRange( resourceChildren );
                }

                items.Add( item );
            }

            return items;
        }

        /// <summary>
        /// Loads resources in a category that the requesting person is authorized to view.
        /// </summary>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="categoryGuid">The category GUID.</param>
        /// <param name="currentPerson">The person requesting the tree.</param>
        /// <returns>The authorized resource tree items.</returns>
        private List<TreeItemBag> LoadResources( ResourceService resourceService, Guid categoryGuid, Rock.Model.Person currentPerson )
        {
            var resourceQry = resourceService.Queryable()
                .Where( l => l.Name != null && l.Name != string.Empty )
                .Where( l => l.Category.Guid == categoryGuid );

            var items = new List<TreeItemBag>();

            foreach ( var resource in resourceQry )
            {
                if ( !resource.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                var item = new TreeItemBag
                {
                    Value = resource.Guid.ToString(),
                    Text = resource.Name,
                    IsFolder = false,
                    IsActive = resource.IsActive,
                    HasChildren = false
                };

                items.Add( item );
            }

            return items;
        }
    }
}
