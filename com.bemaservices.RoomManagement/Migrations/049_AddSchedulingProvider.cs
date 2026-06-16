using System;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for SchedulingProvider and related tables.
    /// </summary>
    [MigrationNumber( 049, "1.17.6" )]
    public partial class AddSchedulingProvider : Migration
    {
        public override void Up()
        {
            var schedulingProviderTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProvider";
            // Create SchedulingProvider table
            AddTable(
                schedulingProviderTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String( maxLength: null ),
                    EntityTypeId = c.Int( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    MappingsJson = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 50 ),
                } );
            AddPrimaryKey( schedulingProviderTableName, "Id" );
            AddForeignKey( schedulingProviderTableName, "EntityTypeId", "dbo.EntityType", "Id", false );
            AddIndex( schedulingProviderTableName, "EntityTypeId" );

            // Create SchedulingProviderReservation table
            var schedulingProviderReservationTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProviderReservation";
            AddTable(
                schedulingProviderReservationTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SchedulingProviderId = c.Int( nullable: false ),
                    ReservationId = c.Int( nullable: false ),
                    ExternalId = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 50 ),
                } );
            AddPrimaryKey( schedulingProviderReservationTableName, "Id" );
            AddForeignKey( schedulingProviderReservationTableName, "SchedulingProviderId", schedulingProviderTableName, "Id", true );
            AddForeignKey( schedulingProviderReservationTableName, "ReservationId", "dbo._com_bemaservices_RoomManagement_Reservation", "Id", true );
            AddIndex( schedulingProviderReservationTableName, "SchedulingProviderId" );
            AddIndex( schedulingProviderReservationTableName, "ReservationId" );

            // Create SchedulingProviderLocation table
            var schedulingProviderLocationTableName = "dbo._com_bemaservices_RoomManagement_SchedulingProviderLocation";
            AddTable(
                schedulingProviderLocationTableName,
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SchedulingProviderId = c.Int( nullable: false ),
                    LocationId = c.Int( nullable: false ),
                    ExternalId = c.String( maxLength: null ),
                    Guid = c.Guid( nullable: false, defaultValueSql: "NEWID()" ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } );
            AddPrimaryKey( schedulingProviderLocationTableName, "Id" );
            AddForeignKey( schedulingProviderLocationTableName, "SchedulingProviderId", schedulingProviderTableName, "Id", true );
            AddForeignKey( schedulingProviderLocationTableName, "LocationId", "dbo.Location", "Id", true );
            AddIndex( schedulingProviderLocationTableName, "SchedulingProviderId" );
            AddIndex( schedulingProviderLocationTableName, "LocationId" );

            // Register the GoogleResources EntityType
            RockMigrationHelper.UpdateEntityType(
                "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources",
                "Google Resources",
                "com.bemaservices.RoomManagement.SchedulingProviders.GoogleResources, com.bemaservices.RoomManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                false,
                true,
                "A8F7D8B3-2C1E-4F9A-8D3B-1E5C6A7F8B9C" );
        }

        public override void Down()
        {

        }
    }
}
