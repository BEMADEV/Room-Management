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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Attribute
{
    /// <summary>
    /// Static Helper class for the Room Management plugin
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Loads the questions for a reservation location: only the attributes that are questions for that location,
        /// with their answers. See <see cref="LoadQuestionAttributes" /> for how questions are recognized.
        /// </summary>
        /// <param name="reservationLocation">The reservation location.</param>
        public static void LoadReservationLocationAttributes( this ReservationLocation reservationLocation )
        {
            List<int> linkedAttributeIds;
            using ( var rockContext = new RockContext() )
            {
                linkedAttributeIds = new ReservationQuestionService( rockContext ).Queryable()
                    .Where( q => q.LocationId == reservationLocation.LocationId )
                    .Select( q => q.AttributeId )
                    .ToList();
            }

            var starterReservationLocation = new ReservationLocation();
            starterReservationLocation.CopyPropertiesFrom( reservationLocation );

            LoadQuestionAttributes( reservationLocation, starterReservationLocation, "LocationId", linkedAttributeIds );
        }

        /// <summary>
        /// Loads the questions for a reservation resource: only the attributes that are questions for that resource,
        /// with their answers. See <see cref="LoadQuestionAttributes" /> for how questions are recognized.
        /// </summary>
        /// <param name="reservationResource">The reservation resource.</param>
        public static void LoadReservationResourceAttributes( this ReservationResource reservationResource )
        {
            List<int> linkedAttributeIds;
            using ( var rockContext = new RockContext() )
            {
                linkedAttributeIds = new ReservationQuestionService( rockContext ).Queryable()
                    .Where( q => q.ResourceId == reservationResource.ResourceId )
                    .Select( q => q.AttributeId )
                    .ToList();
            }

            var starterReservationResource = new ReservationResource();
            starterReservationResource.CopyPropertiesFrom( reservationResource );

            LoadQuestionAttributes( reservationResource, starterReservationResource, "ResourceId", linkedAttributeIds );
        }

        /// <summary>
        /// Loads a reservation location's or resource's attributes onto a copy, then keeps only its questions. Rock already
        /// drops questions scoped to a different resource or location (their qualifier doesn't match), so an attribute is
        /// kept when it's scoped by <paramref name="qualifierColumn" /> or, for questions not scoped yet (before the
        /// modernization migration runs, or rows it skipped), when the ReservationQuestion table links it here. Other
        /// unqualified attributes added to these entity types stay hidden, as before.
        /// </summary>
        /// <param name="entity">The reservation location or resource to load the questions onto.</param>
        /// <param name="starterEntity">A copy of the entity used for loading every attribute.</param>
        /// <param name="qualifierColumn">The qualifier column that scopes questions to an owner: ResourceId or LocationId.</param>
        /// <param name="linkedAttributeIds">The attribute ids the ReservationQuestion table links to this owner.</param>
        private static void LoadQuestionAttributes( Rock.Attribute.IHasAttributes entity, Rock.Attribute.IHasAttributes starterEntity, string qualifierColumn, List<int> linkedAttributeIds )
        {
            Rock.Attribute.Helper.LoadAttributes( starterEntity );

            var linkedIds = new HashSet<int>( linkedAttributeIds );
            entity.Attributes = starterEntity.Attributes
                .Where( kvp => kvp.Value.EntityTypeQualifierColumn == qualifierColumn || linkedIds.Contains( kvp.Value.Id ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            entity.AttributeValues = starterEntity.AttributeValues
                .Where( kvp => entity.Attributes.ContainsKey( kvp.Key ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
        }
    }

}
