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

using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

using com.bemaservices.RoomManagement.Model;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// The controller class for the ScheduledLocations
    /// </summary>
    public partial class ScheduledLocationsController : Rock.Rest.ApiController<Rock.Model.Category>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledLocationsController" /> class.
        /// </summary>
        public ScheduledLocationsController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { }
    }
    public partial class ScheduledLocationsController
    {
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/ScheduledLocations/GetChildren/{id}/{rootLocationId}" )]
        public IQueryable<TreeViewItem> GetChildren(
            int id,
            int rootLocationId = 0,
            string locationTypeIds = "",
            int? reservedLocationEntitySetId = null,
            int? conflictedLocationEntitySetId = null,
            int? attendeeCount = null )
        {
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );

            List<int> reservationLocationTypeList = locationTypeIds.Split( ',' ).AsIntegerList();
            List<int> reservedLocationIds = new List<int>();
            List<int> conflictedLocationIds = new List<int>();

            IQueryable<Location> qry;
            if ( id == 0 )
            {
                qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                if ( rootLocationId != 0 )
                {
                    qry = qry.Where( a => a.Id == rootLocationId );
                }
            }
            else
            {
                qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == id );
            }

            // limit to only active, Named Locations (don't show home addresses, etc)
            qry = qry.Where( a => a.Name != null && a.Name != string.Empty && a.IsActive == true );            

            List<Location> locationList = new List<Location>();
            List<TreeViewItem> locationNameList = new List<TreeViewItem>();

            var person = GetPerson();

            if( reservedLocationEntitySetId.HasValue || conflictedLocationEntitySetId.HasValue )
            {
                var entitySetItemService = new EntitySetItemService( rockContext );
                if ( reservedLocationEntitySetId.HasValue )
                {
                    reservedLocationIds = entitySetItemService
                       .GetByEntitySetId( reservedLocationEntitySetId.Value )
                       .Select( esi => esi.EntityId )
                       .ToList();
                }

                if ( conflictedLocationEntitySetId.HasValue )
                {
                    conflictedLocationIds = entitySetItemService
                       .GetByEntitySetId( reservedLocationEntitySetId.Value )
                       .Select( esi => esi.EntityId )
                       .ToList();
                }
            }

            foreach ( var location in qry.OrderBy( l => l.Name ) )
            {
                if ( location.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var descendantLocations = locationService.GetAllDescendents( location.Id );
                    bool isValidLocationType = ( !reservationLocationTypeList.Any() || !location.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( location.LocationTypeValueId.Value ) );
                    bool hasValidDescendant = ( !reservationLocationTypeList.Any() || descendantLocations.Any( dl => !dl.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( dl.LocationTypeValueId.Value ) ) );
                    if ( isValidLocationType || hasValidDescendant )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = location.Id.ToString();

                        var color = "Green";
                        if ( location.LocationTypeValueId.HasValue && !reservationLocationTypeList.Contains( location.LocationTypeValueId.Value ) )
                        {
                            color = "Black";
                        }
                        else
                        {
                            if ( reservedLocationIds.Contains( location.Id ) )
                            {
                                color = "Red";
                            }
                            else
                            {
                                if ( conflictedLocationIds.Contains( location.Id ) )
                                {
                                    color = "Orange";
                                }
                                else
                                {
                                    if ( attendeeCount != null && location.FirmRoomThreshold != null && attendeeCount.Value > location.FirmRoomThreshold.Value )
                                    {
                                        color = "Orange";
                                    }
                                }
                            }
                        }

                        treeViewItem.Name = string.Format( "<span style='color:{0};'>{1}</span><small style='color:grey;'>{2}</small>",
                            color,
                            System.Web.HttpUtility.HtmlEncode( location.Name ),
                            location.FirmRoomThreshold != null ? "\t(" + location.FirmRoomThreshold + ")" : "" );
                        treeViewItem.IsActive = true;
                        locationNameList.Add( treeViewItem );
                    }
                }
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = locationList.Select( a => a.Id ).ToList();

            var qryHasChildren = locationService.Queryable().AsNoTracking()
                .Where( l =>
                    l.ParentLocationId.HasValue &&
                    resultIds.Contains( l.ParentLocationId.Value ) )
                .Select( l => l.ParentLocationId.Value )
                .Distinct()
                .ToList();

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var item in locationNameList )
            {
                int locationId = int.Parse( item.Id );
                item.HasChildren = qryHasChildrenList.Any( a => a == locationId );
            }

            return locationNameList.AsQueryable();
        }

    }
}
