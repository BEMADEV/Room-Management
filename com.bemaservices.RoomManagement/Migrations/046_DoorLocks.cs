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
    [MigrationNumber( 46, "1.16.0" )]
    public class DoorLocks : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddReservationDoorLockSchedulesModel();
            UpdateReservationTypeModel();
        }

        /// <summary>
        /// Adds the reservation door lock schedules model.
        /// </summary>
        private void AddReservationDoorLockSchedulesModel()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [StartTimeOffset] [int] NOT NULL,
	                [EndTimeOffset] [int] NOT NULL,
                    [Note] [nvarchar](max) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationDoorLockSchedule] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationDoorLockSchedule_ModifiedByPersonAliasId]
" );
        }

        /// <summary>
        /// Updates the reservation type model.
        /// </summary>
        private void UpdateReservationTypeModel()
        {
            Sql( @"ALTER TABLE [_com_bemaservices_RoomManagement_ReservationType] ADD [DisplayReservationDoorLockSchedules] [bit] NOT NULL DEFAULT 0;" );
            Sql( @"ALTER TABLE [_com_bemaservices_RoomManagement_ReservationType] ADD [DoorLockInstructions] [nvarchar](max) NULL;" );
            Sql( @"UPDATE [_com_bemaservices_RoomManagement_ReservationType] SET [DisplayReservationDoorLockSchedules] = 0;
                " );
            Sql( @" UPDATE [_com_bemaservices_RoomManagement_ReservationType] SET [DoorLockInstructions] = 'Typically, door lock providers that integrate with Room Management use the duration of the reservation including setup and cleanup times to determine door status. If you want to create custom door lock times to forward to your provider, enter the start and end times here.'" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}