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
using System.Data.Entity.Infrastructure;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Builds the custom reservation history that Rock does not generate for these plugin entities.
    /// Used by the reservation save hook, detail block, and queued workflow-trigger transaction.
    /// </summary>
    public class ReservationChangeTrackingHelper
    {
        private readonly RockContext _rockContext;

        /// <summary>
        /// Uses the caller's context so tracked originals and reservation ownership are shared with the save.
        /// </summary>
        /// <param name="rockContext">The context tracking the reservation and its children.</param>
        public ReservationChangeTrackingHelper( RockContext rockContext )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
        }

        /// <summary>
        /// Builds the detail block's pre-edit snapshot for schedule, approval, and existing-child attribute comparisons.
        /// Retains the reservation type so BuildAttributeChanges can load the correct qualified attribute originals before persistence.
        /// </summary>
        /// <param name="reservation">The reservation before the editor applies its changes.</param>
        /// <returns>A detached snapshot with an independent schedule and child records.</returns>
        public Reservation BuildOldReservation( Reservation reservation )
        {
            if ( reservation == null )
            {
                throw new ArgumentNullException( nameof( reservation ) );
            }

            var oldReservation = new Reservation
            {
                Id = reservation.Id,
                Guid = reservation.Guid,
                ReservationTypeId = reservation.ReservationTypeId,
                Schedule = new Schedule { iCalendarContent = reservation.Schedule?.iCalendarContent },
                ApprovalState = reservation.ApprovalState
            };

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                var oldLocation = new ReservationLocation();
                oldLocation.CopyPropertiesFrom( reservationLocation );
                oldLocation.Location = reservationLocation.Location ?? new LocationService( _rockContext ).Get( reservationLocation.LocationId );
                oldLocation.ReservationId = reservation.Id;
                oldReservation.ReservationLocations.Add( oldLocation );
            }

            foreach ( var reservationResource in reservation.ReservationResources )
            {
                var oldResource = new ReservationResource();
                oldResource.CopyPropertiesFrom( reservationResource );
                oldResource.Resource = reservationResource.Resource ?? new ResourceService( _rockContext ).Get( reservationResource.ResourceId );
                oldResource.ReservationId = reservation.Id;
                oldReservation.ReservationResources.Add( oldResource );
            }

            return oldReservation;
        }

        /// <summary>
        /// Returns the detail block's attribute differences before persistence without storing tracking state on the reservation.
        /// The block keeps this list locally and writes its history in the same transaction as the attribute values.
        /// </summary>
        /// <param name="originalReservation">The reservation snapshot from before editing.</param>
        /// <param name="reservation">The reservation with the edited attribute values.</param>
        /// <returns>The meaningful attribute changes for the reservation and its existing children.</returns>
        public History.HistoryChangeList BuildAttributeChanges( Reservation originalReservation, Reservation reservation )
        {
            if ( originalReservation == null )
            {
                throw new ArgumentNullException( nameof( originalReservation ) );
            }

            if ( reservation == null )
            {
                throw new ArgumentNullException( nameof( reservation ) );
            }

            var changes = new History.HistoryChangeList();
            AddAttributeChanges( changes, originalReservation, reservation, string.Empty );

            foreach ( var location in reservation.ReservationLocations )
            {
                var originalLocation = originalReservation.ReservationLocations.FirstOrDefault( item => item.Guid == location.Guid );
                if ( originalLocation != null )
                {
                    AddAttributeChanges( changes, originalLocation, location,
                        string.Format( "([Location] {0}) ", GetLocationName( location.LocationId, location.Location ) ) );
                }
            }

            foreach ( var resource in reservation.ReservationResources )
            {
                var originalResource = originalReservation.ReservationResources.FirstOrDefault( item => item.Guid == resource.Guid );
                if ( originalResource != null )
                {
                    AddAttributeChanges( changes, originalResource, resource,
                        string.Format( "([Resource] {0}) ", GetResourceName( resource.ResourceId, resource.Resource ) ) );
                }
            }

            return changes;
        }

        /// <summary>
        /// Builds reservation property history plus resource, location, and door-lock schedule history from the context's tracked
        /// entries before the save hook loses their originals. Used by the reservation save hook for every save path (detail block,
        /// API, and workflow actions). Attribute history is handled separately by the detail block; unrelated core entities are left to Rock.
        /// </summary>
        /// <param name="reservation">The reservation being saved.</param>
        /// <param name="reservationEntry">The tracked reservation entry.</param>
        /// <param name="state">The reservation entry state.</param>
        /// <returns>The accumulated history changes for the reservation aggregate.</returns>
        internal History.HistoryChangeList BuildHistoryChanges( Reservation reservation, DbEntityEntry reservationEntry, EntityState state )
        {
            var changes = new History.HistoryChangeList();
            if ( state == EntityState.Added )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Reservation" );
            }

            AddReservationPropertyChanges( changes, reservation, reservationEntry, state );
            foreach ( var entry in _rockContext.ChangeTracker.Entries().ToList() )
            {
                if ( entry.State == EntityState.Unchanged || entry.State == EntityState.Detached || ReferenceEquals( entry.Entity, reservation ) )
                {
                    continue;
                }

                var resource = entry.Entity as ReservationResource;
                if ( resource != null && BelongsToReservation( reservation, resource, resource.Reservation, resource.ReservationId ) )
                {
                    AddResourceChanges( changes, resource, entry );
                    continue;
                }

                var location = entry.Entity as ReservationLocation;
                if ( location != null && BelongsToReservation( reservation, location, location.Reservation, location.ReservationId ) )
                {
                    AddLocationChanges( changes, location, entry );
                    continue;
                }

                var doorLockSchedule = entry.Entity as ReservationDoorLockSchedule;
                if ( doorLockSchedule != null && BelongsToReservation( reservation, doorLockSchedule, doorLockSchedule.Reservation, doorLockSchedule.ReservationId ) )
                {
                    AddDoorLockScheduleChanges( changes, entry );
                }
            }

            return changes;
        }

        /// <summary>
        /// Writes custom history synchronously in the caller's context. The detail block uses its existing attribute-save transaction;
        /// the queued save-hook transaction uses a fresh context and writes property history before launching workflows.
        /// </summary>
        /// <param name="reservationId">The saved reservation identifier.</param>
        /// <param name="changes">The captured changes, or null when the transaction only launches workflows.</param>
        /// <param name="modifiedByPersonAliasId">The person responsible for the original save.</param>
        /// <param name="sourceOfChange">The source recorded by the original save context.</param>
        public void SaveHistory( int reservationId, History.HistoryChangeList changes, int? modifiedByPersonAliasId, string sourceOfChange )
        {
            if ( changes == null || !changes.Any() )
            {
                return;
            }

            if ( reservationId <= 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( reservationId ), "Reservation history requires a saved reservation." );
            }

            HistoryService.SaveChanges( _rockContext, typeof( Reservation ),
                com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                reservationId, changes, modifiedByPersonAliasId: modifiedByPersonAliasId, sourceOfChange: sourceOfChange );
        }

        /// <summary>
        /// Adds the reservation's business-property changes using Rock's standard equality and formatting rules.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="reservation">The current reservation.</param>
        /// <param name="entry">The tracked reservation entry.</param>
        /// <param name="state">The reservation entry state.</param>
        private void AddReservationPropertyChanges( History.HistoryChangeList changes, Reservation reservation, DbEntityEntry entry, EntityState state )
        {
            AddPropertyChange( changes, entry, state, "Name", "Name" );
            AddPropertyChange( changes, entry, state, "Note", "Note" );
            AddPropertyChange( changes, entry, state, "NumberAttending", "Number Attending" );
            AddPropertyChange( changes, entry, state, "SetupTime", "Setup Time" );
            AddPropertyChange( changes, entry, state, "CleanupTime", "Cleanup Time" );
            AddPropertyChange( changes, entry, state, "EventContactPhone", "Event Contact Phone Number" );
            AddPropertyChange( changes, entry, state, "EventContactEmail", "Event Contact Email" );
            AddPropertyChange( changes, entry, state, "AdministrativeContactPhone", "Administrative Contact Phone Number" );
            AddPropertyChange( changes, entry, state, "AdministrativeContactEmail", "Administrative Contact Email" );
            AddPropertyChange( changes, entry, state, "ApprovalState", "Approval State" );
            AddPropertyChange( changes, entry, state, "CampusId", "Campus", GetCampusName );
            AddPropertyChange( changes, entry, state, "ReservationMinistryId", "Ministry", GetMinistryName );
            AddPropertyChange( changes, entry, state, "EventContactPersonAliasId", "Event Contact", GetPersonAliasName );
            AddPropertyChange( changes, entry, state, "AdministrativeContactPersonAliasId", "Administrative Contact", GetPersonAliasName );

            var oldPhotoId = GetOldValue( entry, "SetupPhotoId", state );
            var newPhotoId = GetCurrentValue( entry, "SetupPhotoId" );
            if ( !Equals( oldPhotoId, newPhotoId ) )
            {
                var verb = oldPhotoId == null
                    ? History.HistoryVerb.Add
                    : newPhotoId == null ? History.HistoryVerb.Delete : History.HistoryVerb.Modify;
                changes.AddChange( verb, History.HistoryChangeType.Property, "setup photo." );
            }

            AddScheduleChange( changes, reservation, entry, state );
        }

        /// <summary>
        /// Adds a scalar-property change when Rock reports different original and current values.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="entry">The tracked entity entry.</param>
        /// <param name="state">The entity entry state.</param>
        /// <param name="propertyName">The model property name.</param>
        /// <param name="historyName">The name shown in reservation history.</param>
        /// <param name="formatValue">Optional formatter for foreign-key values.</param>
        private void AddPropertyChange( History.HistoryChangeList changes, DbEntityEntry entry, EntityState state, string propertyName, string historyName, Func<int?, string> formatValue = null )
        {
            var oldValue = GetOldValue( entry, propertyName, state );
            var newValue = GetCurrentValue( entry, propertyName );
            var oldText = formatValue == null ? ToHistoryString( oldValue ) : formatValue( ToNullableInt( oldValue ) );
            var newText = formatValue == null ? ToHistoryString( newValue ) : formatValue( ToNullableInt( newValue ) );
            History.EvaluateChange( changes, historyName, oldText, newText );
        }

        /// <summary>
        /// Adds a friendly schedule change when the reservation's schedule or its tracked calendar content changed.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="reservation">The current reservation.</param>
        /// <param name="entry">The tracked reservation entry.</param>
        /// <param name="state">The reservation entry state.</param>
        private void AddScheduleChange( History.HistoryChangeList changes, Reservation reservation, DbEntityEntry entry, EntityState state )
        {
            var originalScheduleId = ToNullableInt( GetOldValue( entry, "ScheduleId", state ) );
            var currentScheduleId = ToNullableInt( GetCurrentValue( entry, "ScheduleId" ) );
            var scheduleEntry = _rockContext.ChangeTracker.Entries<Schedule>()
                .FirstOrDefault( item => item.Entity.Id == currentScheduleId && item.State != EntityState.Unchanged );
            var scheduleValuesChanged = state == EntityState.Added ||
                entry.Property( "ScheduleId" ).IsModified ||
                entry.Property( "SetupTime" ).IsModified ||
                entry.Property( "CleanupTime" ).IsModified ||
                entry.Property( "FirstOccurrenceStartDateTime" ).IsModified ||
                entry.Property( "LastOccurrenceEndDateTime" ).IsModified;
            if ( !scheduleValuesChanged && Equals( originalScheduleId, currentScheduleId ) && scheduleEntry == null )
            {
                return;
            }

            var currentSchedule = reservation.Schedule;
            if ( currentSchedule == null && currentScheduleId.HasValue )
            {
                currentSchedule = new ScheduleService( _rockContext ).Get( currentScheduleId.Value );
            }

            Schedule originalSchedule = null;
            if ( state != EntityState.Added && originalScheduleId.HasValue )
            {
                if ( scheduleEntry != null && scheduleEntry.State == EntityState.Modified )
                {
                    originalSchedule = new Schedule
                    {
                        iCalendarContent = scheduleEntry.Property( "iCalendarContent" ).OriginalValue as string
                    };
                }
                else
                {
                    originalSchedule = new ScheduleService( _rockContext ).Get( originalScheduleId.Value );
                }
            }

            var oldText = Reservation.GetFriendlyReservationScheduleText( originalSchedule, reservation.ReservationType,
                ToNullableInt( GetOldValue( entry, "SetupTime", state ) ), ToNullableInt( GetOldValue( entry, "CleanupTime", state ) ),
                GetNullableDateTime( GetOldValue( entry, "FirstOccurrenceStartDateTime", state ) ),
                GetNullableDateTime( GetOldValue( entry, "LastOccurrenceEndDateTime", state ) ) );
            var newText = Reservation.GetFriendlyReservationScheduleText( currentSchedule, reservation.ReservationType,
                reservation.SetupTime, reservation.CleanupTime, reservation.FirstOccurrenceStartDateTime, reservation.LastOccurrenceEndDateTime );
            History.EvaluateChange( changes, "Schedule", oldText, newText );
        }

        /// <summary>
        /// Adds resource additions, removals, and meaningful approval or quantity changes.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="resource">The changed reservation resource.</param>
        /// <param name="entry">The tracked reservation-resource entry.</param>
        private void AddResourceChanges( History.HistoryChangeList changes, ReservationResource resource, DbEntityEntry entry )
        {
            var currentResourceName = GetResourceName( resource.ResourceId, resource.Resource );
            if ( entry.State == EntityState.Added )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, string.Format( "[Resource] {0} {1}", resource.Quantity, currentResourceName ) );
                return;
            }

            var oldResourceId = ToNullableInt( GetOldValue( entry, "ResourceId", entry.State ) );
            var oldResourceName = GetResourceName( oldResourceId, resource.Resource );
            if ( entry.State == EntityState.Deleted )
            {
                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, string.Format( "[Resource] {0}", oldResourceName ) );
                return;
            }

            if ( oldResourceId != resource.ResourceId )
            {
                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, string.Format( "[Resource] {0}", oldResourceName ) );
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, string.Format( "[Resource] {0} {1}", resource.Quantity, currentResourceName ) );
                return;
            }

            var historyPrefix = string.Format( "[Resource] {0}", currentResourceName );
            AddPropertyChange( changes, entry, entry.State, "ApprovalState", string.Format( "{0} Approval State", historyPrefix ) );
            AddPropertyChange( changes, entry, entry.State, "Quantity", string.Format( "{0} Quantity", historyPrefix ) );
        }

        /// <summary>
        /// Adds location additions, removals, and meaningful approval-state changes.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="location">The changed reservation location.</param>
        /// <param name="entry">The tracked reservation-location entry.</param>
        private void AddLocationChanges( History.HistoryChangeList changes, ReservationLocation location, DbEntityEntry entry )
        {
            var currentLocationName = GetLocationName( location.LocationId, location.Location );
            if ( entry.State == EntityState.Added )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, string.Format( "[Location] {0}", currentLocationName ) );
                return;
            }

            var oldLocationId = ToNullableInt( GetOldValue( entry, "LocationId", entry.State ) );
            var oldLocationName = GetLocationName( oldLocationId, location.Location );
            if ( entry.State == EntityState.Deleted )
            {
                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, string.Format( "[Location] {0}", oldLocationName ) );
            }
            else if ( oldLocationId != location.LocationId )
            {
                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, string.Format( "[Location] {0}", oldLocationName ) );
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, string.Format( "[Location] {0}", currentLocationName ) );
            }
            else
            {
                var historyPrefix = string.Format( "[Location] {0}", currentLocationName );
                AddPropertyChange( changes, entry, entry.State, "ApprovalState", string.Format( "{0} Approval State", historyPrefix ) );
            }
        }

        /// <summary>
        /// Adds door-lock schedule additions, removals, and modifications to the parent reservation history, whether the schedule
        /// was saved by the detail block or on its own (for example through the API), since its pre-save touches the parent reservation.
        /// Modified rows are labeled with the schedule's original offsets so multiple schedules on one reservation stay distinguishable.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="entry">The tracked door-lock schedule entry.</param>
        private void AddDoorLockScheduleChanges( History.HistoryChangeList changes, DbEntityEntry entry )
        {
            var useOriginalValues = entry.State != EntityState.Added;
            var historyPrefix = string.Format( "[Door Lock Schedule] {0} - {1}",
                ToHistoryString( GetValue( entry, "StartTimeOffset", useOriginalValues ) ),
                ToHistoryString( GetValue( entry, "EndTimeOffset", useOriginalValues ) ) );

            if ( entry.State == EntityState.Added || entry.State == EntityState.Deleted )
            {
                var verb = entry.State == EntityState.Added ? History.HistoryVerb.Add : History.HistoryVerb.Delete;
                changes.AddChange( verb, History.HistoryChangeType.Property, historyPrefix );
                return;
            }

            AddPropertyChange( changes, entry, entry.State, "StartTimeOffset", string.Format( "{0} Start Time Offset", historyPrefix ) );
            AddPropertyChange( changes, entry, entry.State, "EndTimeOffset", string.Format( "{0} End Time Offset", historyPrefix ) );
            AddPropertyChange( changes, entry, entry.State, "Note", string.Format( "{0} Note", historyPrefix ) );
        }

        /// <summary>
        /// Compares the detail block's attribute dictionaries with the stored originals before attribute persistence.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="originalEntity">The entity before editing.</param>
        /// <param name="currentEntity">The entity after editing.</param>
        /// <param name="namePrefix">The prefix used for nested attribute names.</param>
        private void AddAttributeChanges( History.HistoryChangeList changes, Rock.Attribute.IHasAttributes originalEntity, Rock.Attribute.IHasAttributes currentEntity, string namePrefix )
        {
            if ( currentEntity.Attributes == null || !currentEntity.Attributes.Any() )
            {
                return;
            }

            if ( originalEntity.Attributes == null )
            {
                Rock.Attribute.Helper.LoadAttributes( originalEntity, _rockContext );
            }

            if ( originalEntity.Attributes == null )
            {
                return;
            }

            foreach ( var attribute in currentEntity.Attributes.Values )
            {
                var oldValue = originalEntity.AttributeValues.ContainsKey( attribute.Key )
                    ? originalEntity.AttributeValues[attribute.Key].Value : string.Empty;
                var newValue = currentEntity.AttributeValues.ContainsKey( attribute.Key )
                    ? currentEntity.AttributeValues[attribute.Key].Value : string.Empty;
                AddAttributeChange( changes, attribute, namePrefix + attribute.Name, oldValue, newValue );
            }
        }

        /// <summary>
        /// Formats the detail block's attribute differences, honoring suppression and sensitive fields.
        /// </summary>
        /// <param name="changes">The accumulated changes.</param>
        /// <param name="attribute">The attribute definition and field formatter.</param>
        /// <param name="valueName">The reservation history label.</param>
        /// <param name="oldValue">The raw previous value.</param>
        /// <param name="newValue">The raw current value.</param>
        private static void AddAttributeChange( History.HistoryChangeList changes, AttributeCache attribute, string valueName, string oldValue, string newValue )
        {
            if ( attribute.IsSuppressHistoryLogging || ( oldValue ?? string.Empty ) == ( newValue ?? string.Empty ) )
            {
                return;
            }

            var field = attribute.FieldType.Field;
            var oldFormattedValue = field.FormatValue( null, oldValue ?? string.Empty, attribute.QualifierValues, false );
            var newFormattedValue = field.FormatValue( null, newValue ?? string.Empty, attribute.QualifierValues, false );
            History.EvaluateChange( changes, valueName, oldFormattedValue, newFormattedValue, field.IsSensitive() );
        }

        /// <summary>
        /// Determines whether a tracked child belongs to the reservation, including newly added aggregates without identifiers.
        /// </summary>
        /// <param name="reservation">The reservation aggregate root.</param>
        /// <param name="child">The tracked child entity.</param>
        /// <param name="childReservation">The child's navigation reference.</param>
        /// <param name="reservationId">The child's foreign key.</param>
        /// <returns>Whether the child belongs to the reservation.</returns>
        private static bool BelongsToReservation( Reservation reservation, object child, Reservation childReservation, int reservationId )
        {
            if ( ReferenceEquals( childReservation, reservation ) )
            {
                return true;
            }

            if ( reservation.Id != 0 )
            {
                return reservationId == reservation.Id;
            }

            return reservation.ReservationResources.Any( item => ReferenceEquals( item, child ) ) ||
                reservation.ReservationLocations.Any( item => ReferenceEquals( item, child ) ) ||
                reservation.ReservationDoorLockSchedules.Any( item => ReferenceEquals( item, child ) );
        }

        /// <summary>
        /// Gets the original property value or the CLR default for a newly added entity.
        /// </summary>
        /// <param name="entry">The tracked entity entry.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="state">The entry state.</param>
        /// <returns>The original or default value.</returns>
        private static object GetOldValue( DbEntityEntry entry, string propertyName, EntityState state )
        {
            if ( state != EntityState.Added )
            {
                return entry.Property( propertyName ).OriginalValue;
            }

            var propertyType = entry.Entity.GetType().GetProperty( propertyName ).PropertyType;
            return propertyType.IsValueType ? Activator.CreateInstance( propertyType ) : null;
        }

        /// <summary>
        /// Reads an original or current value from an EF entry for history comparison.
        /// </summary>
        /// <param name="entry">The tracked entity entry.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="original">Whether to read the original value.</param>
        /// <returns>The requested property value.</returns>
        private static object GetValue( DbEntityEntry entry, string propertyName, bool original )
        {
            return original ? entry.Property( propertyName ).OriginalValue : entry.Property( propertyName ).CurrentValue;
        }

        /// <summary>
        /// Gets the current tracked property value for history comparison.
        /// </summary>
        /// <param name="entry">The tracked entity entry.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The current property value.</returns>
        private static object GetCurrentValue( DbEntityEntry entry, string propertyName )
        {
            return GetValue( entry, propertyName, false );
        }

        /// <summary>
        /// Converts an EF value to the nullable integer used by foreign-key history formatters.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The nullable integer.</returns>
        private static int? ToNullableInt( object value )
        {
            return value == null ? ( int? ) null : Convert.ToInt32( value );
        }

        /// <summary>
        /// Converts an EF value to the nullable date-time used by schedule history formatting.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The nullable date-time.</returns>
        private static DateTime? GetNullableDateTime( object value )
        {
            return value == null ? ( DateTime? ) null : Convert.ToDateTime( value );
        }

        /// <summary>
        /// Formats a property using the existing reservation history's null and string conventions.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The history string, or an empty string for null.</returns>
        private static string ToHistoryString( object value )
        {
            return value == null ? string.Empty : value.ToString();
        }

        /// <summary>
        /// Resolves the campus name displayed in reservation history.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>The campus name or "None".</returns>
        private static string GetCampusName( int? campusId )
        {
            var campus = campusId.HasValue ? CampusCache.Get( campusId.Value ) : null;
            return campus == null ? "None" : campus.Name;
        }

        /// <summary>
        /// Resolves the ministry name displayed in reservation history.
        /// </summary>
        /// <param name="ministryId">The ministry identifier.</param>
        /// <returns>The ministry name or "None".</returns>
        private string GetMinistryName( int? ministryId )
        {
            var ministry = ministryId.HasValue ? new ReservationMinistryService( _rockContext ).Get( ministryId.Value ) : null;
            return ministry == null ? "None" : ministry.Name;
        }

        /// <summary>
        /// Resolves the contact name displayed in reservation history.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>The full name, or an empty string when unavailable.</returns>
        private string GetPersonAliasName( int? personAliasId )
        {
            if ( !personAliasId.HasValue )
            {
                return string.Empty;
            }

            var personAlias = new PersonAliasService( _rockContext ).Queryable( "Person" ).FirstOrDefault( item => item.Id == personAliasId.Value );
            return personAlias?.Person?.FullName ?? string.Empty;
        }

        /// <summary>
        /// Resolves a resource name for child and attribute history, preferring the loaded navigation value.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="resource">The optional loaded resource.</param>
        /// <returns>The resource name, or an empty string.</returns>
        private string GetResourceName( int? resourceId, Resource resource = null )
        {
            if ( resource != null && ( !resourceId.HasValue || resource.Id == resourceId.Value ) )
            {
                return resource.Name;
            }

            var item = resourceId.HasValue ? new ResourceService( _rockContext ).Get( resourceId.Value ) : null;
            return item == null ? string.Empty : item.Name;
        }

        /// <summary>
        /// Resolves a location name for child and attribute history, preferring the loaded navigation value.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="location">The optional loaded location.</param>
        /// <returns>The location name, or an empty string.</returns>
        private string GetLocationName( int? locationId, Location location = null )
        {
            if ( location != null && ( !locationId.HasValue || location.Id == locationId.Value ) )
            {
                return location.Name;
            }

            var item = locationId.HasValue ? new LocationService( _rockContext ).Get( locationId.Value ) : null;
            return item == null ? string.Empty : item.Name;
        }
    }
}
