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
        [HttpPost]
        [System.Web.Http.Route( "api/v2/plugins/com.bemaservices/roommanagement/models/resource/tree" )]
        [RestActionGuid( "eb007a09-7a4e-4773-bd5a-729c8fc61e93" )]
        public IHttpActionResult PostTreeItems(
            [FromBody] UniversalItemTreePickerOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var resourceService = new ResourceService( rockContext );
                var categoryService = new CategoryService( rockContext );
                var expandGuids = GetExpandGuids( resourceService,
                    options.ExpandToValues?.AsGuidList() );
                var resourcesAndCategories = LoadCategories( categoryService,
                        resourceService,
                    options.ParentValue.AsGuidOrNull(),
                    expandGuids );

                return Ok( resourcesAndCategories );
            }
        }

        private List<Guid> GetExpandGuids( ResourceService resourceService,
            List<Guid> expandToGuids )
        {
            var expandGuids = new List<Guid>();

            if ( expandToGuids == null )
            {
                return expandGuids;
            }

            foreach ( var guid in expandToGuids )
            {
                var category = resourceService.Get( guid )?.Category;

                while ( category != null )
                {
                    if ( !expandGuids.Contains( category.Guid ) )
                    {
                        expandGuids.Add( category.Guid );
                    }

                    category = category.ParentCategory;
                }
            }

            return expandGuids;
        }

        private List<TreeItemBag> LoadCategories( CategoryService categoryService,
            ResourceService resourceService,
            Guid? parentGuid,
            List<Guid> expandGuids )
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

            if ( parentGuid != null )
            {
                items.AddRange( LoadResources( resourceService, parentGuid.Value ) );
            }

            foreach ( var category in categoryQry )
            {
                var resourceChildren = LoadResources( resourceService,
                    category.Guid );

                var item = new TreeItemBag
                {
                    Value = category.Guid.ToString(),
                    IconCssClass = category.IconCssClass,
                    Text = category.Name,
                    Type = "Category",
                    IsFolder = true,
                    IsActive = true,
                    HasChildren = resourceChildren.Any() || category.ChildCategories.Any()
                };

                if ( expandGuids.Contains( category.Guid ) )
                {
                    var categoryChildren = LoadCategories( categoryService,
                            resourceService,
                        category.Guid,
                        expandGuids );
                    item.Children = new List<TreeItemBag>();
                    item.Children.AddRange( categoryChildren );
                    item.Children.AddRange( resourceChildren );
                }

                items.Add( item );
            }

            return items;
        }

        private List<TreeItemBag> LoadResources( ResourceService resourceService,
            Guid categoryGuid )
        {
            var resourceQry = resourceService.Queryable()
                .Where( l => l.Name != null && l.Name != string.Empty )
                .Where( l => l.Category.Guid == categoryGuid );

            var items = new List<TreeItemBag>();

            foreach ( var resource in resourceQry )
            {
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

