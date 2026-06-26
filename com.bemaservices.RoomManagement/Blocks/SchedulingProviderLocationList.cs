using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.ViewModels;
using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.DefinedValueList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Blocks
{
    /// <summary>
    /// Displays a list of scheduling provider locations.
    /// </summary>

    [DisplayName( "Scheduling Provider Location List" )]
    [Category( "BEMA Software Services > Room Management" )]
    [Description( "Displays a list of scheduling provider locations." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "4812453e-a541-4007-9d9b-deaad8c5d15d" )]
    [Rock.SystemGuid.BlockTypeGuid( "9cc1b275-670e-43cc-b8c1-aa91ce0fa41d" )]
    [CustomizedGrid]
    public class SchedulingProviderLocationList : RockEntityListBlockType<SchedulingProviderLocation>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string LocationId = "LocationId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Cached value of the current Defined Type, should be access via the <see cref="GetDefinedType(RockContext)"/> method.
        /// </summary>
        private Location _location;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Plugins/com_bemaservices/RoomManagement/schedulingProviderLocationList.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SchedulingProviderLocationListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SchedulingProviderLocationListOptionsBag GetBoxOptions()
        {
            var location = GetLocation();

            var schedulingProviders = new SchedulingProviderService( RockContext )
                .Queryable()
                .Where( sp => sp.IsActive )
                .OrderBy( sp => sp.Name )
                .Select( sp => new ListItemBag
                {
                    Value = sp.Guid.ToString(),
                    Text = sp.Name
                } )
                .ToList();

            var options = new SchedulingProviderLocationListOptionsBag()
            {
                IsBlockVisible = location != null,
                LocationName = location?.Name,
                LocationId = location?.Id.ToString(),
                SchedulingProviders = schedulingProviders
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<SchedulingProviderLocation> GetListQueryable( RockContext rockContext )
        {
            var location = GetLocation();
            IEnumerable<SchedulingProviderLocation> locations = new List<SchedulingProviderLocation>();

            if ( location != null )
            {
                locations = new SchedulingProviderLocationService( rockContext ).Queryable()
                    .Include( a => a.SchedulingProvider )
                    .Where( a => a.LocationId == location.Id );
            }

            return locations.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<SchedulingProviderLocation> GetGridBuilder()
        {
            return new GridBuilder<SchedulingProviderLocation>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "externalId", a => a.ExternalId )
                .AddTextField( "schedulingProvider", a => a.SchedulingProvider?.Name )
                .AddAttributeFields( GetGridAttributes() );
        }

        private Location GetLocation()
        {
            if ( _location == null )
            {
                var locationService = new LocationService( RockContext );
                _location = locationService.Get( PageParameter( PageParameterKey.LocationId ) );
            }

            return _location;
        }

        private SchedulingProviderLocationBag GetEntityBagForEdit( SchedulingProviderLocation entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new SchedulingProviderLocationBag()
            {
                SchedulingProvider = entity.SchedulingProvider == null
                    ? null
                    : new ListItemBag
                    {
                        Value = entity.SchedulingProvider.Guid.ToString(),
                        Text = entity.SchedulingProvider.Name
                    },
                SchedulingProviderId = entity.SchedulingProviderId,
                ExternalId = entity.ExternalId,
                LocationId = entity.LocationId,
                IdKey = entity.IdKey,
                Id = entity.Id
            };

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new SchedulingProviderLocationService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{SchedulingProviderLocation.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {SchedulingProviderLocation.FriendlyTypeName}." );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the specified entity for editing.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${SchedulingProviderLocation.FriendlyTypeName}." );
            }

            var entityService = new SchedulingProviderLocationService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                var location = GetLocation();

                entity = new SchedulingProviderLocation
                {
                    Id = 0,
                    LocationId = location.Id
                };
            }

            entity.LoadAttributes();

            return ActionOk( GetEntityBagForEdit( entity ) );
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="bag">The bag that contains all the information required to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( SchedulingProviderLocationBag bag )
        {
            var location = GetLocation();
            var entityService = new SchedulingProviderLocationService( RockContext );
            SchedulingProviderLocation entity;

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${SchedulingProviderLocation.FriendlyTypeName}." );
            }

            if ( bag.IdKey.IsNullOrWhiteSpace() )
            {
                entity = new SchedulingProviderLocation
                {
                    Id = 0,
                    LocationId = location.Id
                };
            }
            else
            {
                entity = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( entity == null )
            {
                return ActionBadRequest( $"{SchedulingProviderLocation.FriendlyTypeName} not found." );
            }

            entity.LoadAttributes( RockContext );
            entity.ExternalId = bag.ExternalId;
            entity.SchedulingProviderId = bag.SchedulingProvider.GetEntityId<SchedulingProvider>( RockContext ).Value;

            if ( bag.AttributeValues != null )
            {
                entity.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
            }

            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() );
            }

            RockContext.WrapTransaction( () =>
            {
                if ( entity.Id.Equals( 0 ) )
                {
                    entityService.Add( entity );
                }

                RockContext.SaveChanges();

                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk();
        }

        #endregion
    }
}
