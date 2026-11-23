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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Holds every database change for the Room Management modernization. Each change lives in its own
    /// method so it can be reviewed on its own, and every step must be safe to run more than once.
    /// </summary>
    [MigrationNumber( 50, "1.16.6" )]
    public class Modernization : Migration
    {
        /// <summary>
        /// Applies the modernization changes in order.
        /// </summary>
        public override void Up()
        {
            AddReservationLavaCampusContextSetting();
        }

        /// <summary>
        /// Reverts the modernization changes in the reverse order of <see cref="Up" />.
        /// </summary>
        public override void Down()
        {
            RemoveReservationLavaCampusContextSetting();
        }

        /// <summary>
        /// Registers the Reservation Lava block's "Enable Campus Context" setting. The block already reads this
        /// setting to default the campus filter to the page's campus context, but it was never declared, so
        /// administrators could not turn it on. The helper matches on the attribute key, so an attribute Rock
        /// auto-registered from the block's declaration is adopted and given this GUID rather than duplicated.
        /// </summary>
        private void AddReservationLavaCampusContextSetting()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                RESERVATION_LAVA_BLOCK_TYPE_GUID,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Enable Campus Context",
                "EnableCampusContext",
                "",
                "When enabled, select the current campus as the default campus filter when no campus preference is saved.",
                8,
                @"False",
                ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID );
        }

        /// <summary>
        /// Removes the Reservation Lava block's "Enable Campus Context" setting.
        /// </summary>
        private void RemoveReservationLavaCampusContextSetting()
        {
            RockMigrationHelper.DeleteAttribute( ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID );
        }

        /// <summary>
        /// The Reservation Lava block type, registered in migration 002.
        /// </summary>
        private const string RESERVATION_LAVA_BLOCK_TYPE_GUID = "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635";

        /// <summary>
        /// The Reservation Lava block's "Enable Campus Context" attribute.
        /// </summary>
        private const string ENABLE_CAMPUS_CONTEXT_ATTRIBUTE_GUID = "2CABFAB8-E48F-4CAF-A723-0FD0DC4FC2AA";
    }
}
