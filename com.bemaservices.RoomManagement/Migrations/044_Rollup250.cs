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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 44, "1.14.0" )]
    public class Rollup250 : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddReservationResourceColumn();
            AddReservedItemsView();
            AddReservationResourceWorkflowActionLocation();
        }

        /// <summary>
        /// Adds the reservation resource workflow action location.
        /// </summary>
        private void AddReservationResourceWorkflowActionLocation()
        {
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "2441F4FC-3812-4511-9E55-6BA46141D767", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Location Attribute", "LocationAttribute", "Rock will try to find a ReservationLocation with this location, and will ignore it if one is not found.", 1, @"", "71D4AA48-B087-4107-832D-1FEB905A5A72" ); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.AddReservationResource:Location Attribute         
        }

        /// <summary>
        /// Adds the reserved items view.
        /// </summary>
        private void AddReservedItemsView()
        {
            RockMigrationHelper.UpdateDefinedValue( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Location-Based", "A location-based view for the Room Management home page.", "BD2162EB-5CC4-438A-8C0E-F2760897E2A0", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD2162EB-5CC4-438A-8C0E-F2760897E2A0", "466DC361-B813-445A-8883-FED7E5D4229B", @"{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/LocationBasedView.lava' %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD2162EB-5CC4-438A-8C0E-F2760897E2A0", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F", @"RockEntity" );
        }

        /// <summary>
        /// Adds the reservation resource column.
        /// </summary>
        private void AddReservationResourceColumn()
        {

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ADD [ReservationLocationId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId] FOREIGN KEY([ReservationLocationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId]

" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RemoveReservationResourceColumn();
            RemoveReservedItemView();
        }

        /// <summary>
        /// Removes the reserved item view.
        /// </summary>
        private void RemoveReservedItemView()
        {
            RockMigrationHelper.DeleteDefinedValue( "BD2162EB-5CC4-438A-8C0E-F2760897E2A0" ); // Location-Based
        }

        /// <summary>
        /// Removes the reservation resource column.
        /// </summary>
        private void RemoveReservationResourceColumn()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ReservationLocationId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP COLUMN[ReservationLocationId]
" );
        }
    }
}