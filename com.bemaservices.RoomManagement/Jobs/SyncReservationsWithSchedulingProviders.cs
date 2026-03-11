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
using System.Text;

using com.bemaservices.RoomManagement.Attribute;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.SchedulingProviders.Data;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Jobs
{
    /// <summary>
    /// Job to sync reservations with external scheduling providers (import and export).
    /// This job can import reservations from scheduling providers (like Google Calendar, Microsoft Outlook) 
    /// into Rock, and export reservations from Rock to those providers.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [SchedulingProviderField(
        "Import Scheduling Providers",
        Description = "Select the scheduling providers to import from. Leave blank to import from all active providers.",
        Key = AttributeKey.ImportSchedulingProviders,
        IsRequired = false,
        Category = CategoryKey.ImportSettings,
        Order = 0 )]
    [SlidingDateRangeField(
        "Import Date Range",
        Description = "The date range for importing events from scheduling providers. Defaults to the last month through the next 6 months.",
        Key = AttributeKey.ImportDateRange,
        IsRequired = false,
        DefaultValue = "-1m||6m",
        Category = CategoryKey.ImportSettings,
        Order = 1 )]
    [ReservationTypeField(
        "Default Reservation Type",
        Description = "The default reservation type for reservations imported from scheduling providers.",
        Key = AttributeKey.DefaultReservationType,
        IsRequired = true,
        Category = CategoryKey.ImportSettings,
        Order = 2 )]
    [ReservationApprovalStateField(
        "Default Approval State",
        Description = "The default approval state for reservations imported from scheduling providers.",
        Key = AttributeKey.DefaultApprovalState,
        IsRequired = true,
        DefaultValue = "1",
        Category = CategoryKey.ImportSettings,
        Order = 3 )]
    [SystemCommunicationField(
        "Import Notification Template",
        Description = "Optional system communication template to send when reservations are imported from scheduling providers. The reservation will be available as the merge object.",
        Key = AttributeKey.ImportNotificationTemplate,
        IsRequired = false,
        Category = CategoryKey.ImportSettings,
        Order = 4 )]

    [SchedulingProviderField(
        "Export Scheduling Providers",
        Description = "Select the scheduling providers to export to. Leave blank to export to all active providers.",
        Key = AttributeKey.ExportSchedulingProviders,
        IsRequired = false,
        Category = CategoryKey.ExportSettings,
        Order = 0 )]
    [SlidingDateRangeField(
        "Export Date Range",
        Description = "The date range for exporting reservations to scheduling providers. Only reservations with occurrences in this range will be exported. Defaults to the last month through the next 6 months.",
        Key = AttributeKey.ExportDateRange,
        IsRequired = false,
        DefaultValue = "-1m||6m",
        Category = CategoryKey.ExportSettings,
        Order = 1 )]
    [DataViewField(
        "Reservations DataView",
        Description = "Optional DataView to filter which reservations to push to scheduling providers. If not specified, all active reservations will be synced.",
        Key = AttributeKey.ReservationsDataView,
        IsRequired = false,
        EntityTypeName = "com.bemaservices.RoomManagement.Model.Reservation",
        Category = CategoryKey.ExportSettings,
        Order = 2 )]
    [DisallowConcurrentExecution]
    public class SyncReservationsWithSchedulingProviders : RockJob
    {
        /// <summary>
        /// Keys to use for Job Attribute Categories
        /// </summary>
        private static class CategoryKey
        {
            /// <summary>
            /// The import settings category
            /// </summary>
            public const string ImportSettings = "Import Settings";

            /// <summary>
            /// The export settings category
            /// </summary>
            public const string ExportSettings = "Export Settings";
        }

        /// <summary>
        /// Keys to use for Job Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The import scheduling providers
            /// </summary>
            public const string ImportSchedulingProviders = "ImportSchedulingProviders";

            /// <summary>
            /// The import date range
            /// </summary>
            public const string ImportDateRange = "ImportDateRange";

            /// <summary>
            /// The default reservation type
            /// </summary>
            public const string DefaultReservationType = "DefaultReservationType";

            /// <summary>
            /// The default approval state
            /// </summary>
            public const string DefaultApprovalState = "DefaultApprovalState";

            /// <summary>
            /// The import notification template
            /// </summary>
            public const string ImportNotificationTemplate = "ImportNotificationTemplate";

            /// <summary>
            /// The export scheduling providers
            /// </summary>
            public const string ExportSchedulingProviders = "ExportSchedulingProviders";

            /// <summary>
            /// The export date range
            /// </summary>
            public const string ExportDateRange = "ExportDateRange";

            /// <summary>
            /// The reservations data view
            /// </summary>
            public const string ReservationsDataView = "ReservationsDataView";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncReservationsWithSchedulingProviders" /> class.
        /// </summary>
        public SyncReservationsWithSchedulingProviders()
        {
        }

        /// <summary>
        /// Executes this job instance.
        /// </summary>
        /// <remarks>
        /// This method imports reservations from selected scheduling providers and/or exports 
        /// Rock reservations to selected scheduling providers. The job will only process 
        /// providers that have been explicitly selected in the job configuration.
        /// </remarks>
        public override void Execute()
        {
            var rockContext = new RockContext();
            var schedulingProviderService = new SchedulingProviderService( rockContext );

            int reservationsImported = 0;
            int reservationsExported = 0;
            var resultMessages = new StringBuilder();

            // Import reservations from providers
            var selectedImportProviderGuids = GetAttributeValue( AttributeKey.ImportSchedulingProviders )
                .SplitDelimitedValues()
                .AsGuidList();
            var importProviders = schedulingProviderService.Queryable()
                .Where( sp => sp.IsActive )
                .Where( sp => selectedImportProviderGuids.Contains( sp.Guid ) )
                .ToList();

            foreach ( var provider in importProviders )
            {
                int providerImportCount = ImportReservationsFromProvider(
                    rockContext,
                    provider,
                    resultMessages );

                reservationsImported += providerImportCount;
            }

            // Export reservations to providers
            var selectedExportProviderGuids = GetAttributeValue( AttributeKey.ExportSchedulingProviders )
                .SplitDelimitedValues()
                .AsGuidList();
            var exportProviders = schedulingProviderService.Queryable()
                .Where( sp => sp.IsActive )
                .Where( sp => selectedExportProviderGuids.Contains( sp.Guid ) )
                .ToList();

            foreach ( var provider in exportProviders )
            {
                int providerExportCount = ExportReservationsToProvider(
                    rockContext,
                    provider,
                    resultMessages );

                reservationsExported += providerExportCount;
            }

            // Set the job result message
            var resultSummary = new StringBuilder();
            resultSummary.AppendLine( $"Imported: {reservationsImported} reservation(s)" );
            resultSummary.AppendLine( $"Exported: {reservationsExported} reservation(s)" );

            if ( resultMessages.Length > 0 )
            {
                resultSummary.AppendLine();
                resultSummary.AppendLine( "Details:" );
                resultSummary.Append( resultMessages );
            }

            this.Result = resultSummary.ToString();
        }

        #region Private Methods

        /// <summary>
        /// Imports reservations from a single scheduling provider.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="provider">The scheduling provider.</param>
        /// <param name="schedulingProviderLocationService">The scheduling provider location service.</param>
        /// <param name="schedulingProviderReservationService">The scheduling provider reservation service.</param>
        /// <param name="reservationService">The reservation service.</param>
        /// <param name="resultMessages">The result messages.</param>
        /// <returns>The number of reservations imported from this provider.</returns>
        private int ImportReservationsFromProvider(
            RockContext rockContext,
            SchedulingProvider provider,
            StringBuilder resultMessages )
        {
            int providerImportCount = 0;

            var reservationService = new ReservationService( rockContext );
            var schedulingProviderService = new SchedulingProviderService( rockContext );
            var schedulingProviderReservationService = new SchedulingProviderReservationService( rockContext );
            var schedulingProviderLocationService = new SchedulingProviderLocationService( rockContext );

            try
            {
                var component = provider.GetSchedulingComponent();
                if ( component == null )
                {
                    resultMessages.AppendLine( $"[{provider.Name}] Unable to load component" );
                    return 0;
                }

                // Get all locations linked to this provider
                var schedulingProviderLocations = schedulingProviderLocationService.Queryable()
                    .Where( spl => spl.SchedulingProviderId == provider.Id )
                    .ToList();

                if ( !schedulingProviderLocations.Any() )
                {
                    resultMessages.AppendLine( $"[{provider.Name}] No locations configured for import" );
                    return 0;
                }

                // Collect all events from all locations
                var allProviderEvents = new Dictionary<string, EventDTO>();

                // Get date range from attribute
                var importDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues(
                    GetAttributeValue( AttributeKey.ImportDateRange ) );
                var startDate = importDateRange.Start;
                var endDate = importDateRange.End;

                foreach ( var schedulingProviderLocation in schedulingProviderLocations )
                {
                    List<string> errorMessages;
                    var eventsForLocation = component.GetProviderEventsForLocation(
                        provider,
                        schedulingProviderLocation.ExternalId,
                        startDate,
                        endDate,
                        out errorMessages );

                    if ( errorMessages.Any() )
                    {
                        resultMessages.AppendLine( $"[{provider.Name}] Errors getting events for location {schedulingProviderLocation.Location?.Name}: {string.Join( ", ", errorMessages )}" );
                    }

                    // Group events by ExternalId (same event can appear for multiple rooms)
                    foreach ( var providerEvent in eventsForLocation )
                    {
                        if ( string.IsNullOrWhiteSpace( providerEvent.ExternalId ) )
                        {
                            continue;
                        }

                        if ( !allProviderEvents.ContainsKey( providerEvent.ExternalId ) )
                        {
                            allProviderEvents[providerEvent.ExternalId] = providerEvent;
                        }
                        else
                        {
                            // Merge locations from the same event
                            var existingEvent = allProviderEvents[providerEvent.ExternalId];
                            foreach ( var location in providerEvent.Locations )
                            {
                                if ( !existingEvent.Locations.Any( l => l.ExternalId == location.ExternalId ) )
                                {
                                    existingEvent.Locations.Add( location );
                                }
                            }
                        }
                    }
                }

                // Get configuration values
                var defaultApprovalState = GetAttributeValue( AttributeKey.DefaultApprovalState ).ConvertToEnumOrNull<ReservationApprovalState>()
                        ?? ReservationApprovalState.PendingInitialApproval;
                var defaultReservationTypeId = GetAttributeValue( AttributeKey.DefaultReservationType ).AsInteger();
                var importNotificationGuid = GetAttributeValue( AttributeKey.ImportNotificationTemplate ).AsGuidOrNull();

                // Convert provider events to reservations
                foreach ( var providerEvent in allProviderEvents.Values )
                {
                    try
                    {
                        Reservation reservation = null;
                        // Check if this event already exists
                        var existingLink = schedulingProviderReservationService.Queryable()
                            .FirstOrDefault( spr =>
                                spr.SchedulingProviderId == provider.Id &&
                                spr.ExternalId == providerEvent.ExternalId );

                        if ( existingLink == null )
                        {

                            // New event - convert to reservation
                            reservation = DataConverter.GenerateReservationFromProviderEvent(
                                providerEvent,
                                rockContext,
                                defaultReservationTypeId,
                                defaultApprovalState,
                                provider.Id );

                            if ( reservation == null )
                            {
                                resultMessages.AppendLine( $"[{provider.Name}] Failed to convert event: {providerEvent.Title}" );
                                continue;
                            }

                            reservationService.Add( reservation );
                        }
                        else
                        {
                            // Event already imported - check if we need to update
                            var existingReservation = existingLink.Reservation;

                            // Compare timestamps to determine which is more recent
                            var providerModified = providerEvent.ModifiedDateTime ?? providerEvent.CreatedDateTime ?? DateTime.MinValue;
                            var rockModified = existingReservation.ModifiedDateTime ?? existingReservation.CreatedDateTime ?? DateTime.MinValue;

                            if ( providerModified > rockModified )
                            {
                                reservation = DataConverter.UpdateReservationFromProviderEvent(
                                    existingReservation,
                                    providerEvent,
                                    rockContext,
                                    provider.Id );
                            }
                            else
                            {
                                continue; // Rock reservation is more recent, skip update
                            }
                        }

                        rockContext.SaveChanges();

                        // Link to provider
                        if ( existingLink == null )
                        {
                            var providerReservation = new SchedulingProviderReservation
                            {
                                SchedulingProviderId = provider.Id,
                                ReservationId = reservation.Id,
                                ExternalId = providerEvent.ExternalId
                            };
                            schedulingProviderReservationService.Add( providerReservation );
                            rockContext.SaveChanges();
                        }

                        providerImportCount++;

                        // Send notification if configured
                        if ( importNotificationGuid.HasValue )
                        {
                            SendImportNotification( rockContext, reservation, importNotificationGuid.Value );
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                        resultMessages.AppendLine( $"[{provider.Name}] Exception importing event {providerEvent.Title}: {ex.Message}" );
                    }
                }

                resultMessages.AppendLine( $"[{provider.Name}] Successfully imported {providerImportCount} reservation(s)" );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                resultMessages.AppendLine( $"[{provider.Name}] Exception during import: {ex.Message}" );
            }

            return providerImportCount;
        }

        /// <summary>
        /// Exports reservations to a single scheduling provider.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="provider">The scheduling provider.</param>
        /// <param name="reservationService">The reservation service.</param>
        /// <param name="schedulingProviderReservationService">The scheduling provider reservation service.</param>
        /// <param name="reservationIds">The optional list of reservation IDs to filter by.</param>
        /// <param name="resultMessages">The result messages.</param>
        /// <returns>The number of reservations exported to this provider.</returns>
        private int ExportReservationsToProvider(
            RockContext rockContext,
            SchedulingProvider provider,
            StringBuilder resultMessages )
        {
            var providerExportCount = 0;
            var reservationService = new ReservationService( rockContext );
            var schedulingProviderService = new SchedulingProviderService( rockContext );
            var schedulingProviderReservationService = new SchedulingProviderReservationService( rockContext );
            var schedulingProviderLocationService = new SchedulingProviderLocationService( rockContext );

            try
            {
                var component = provider.GetSchedulingComponent();
                if ( component == null )
                {
                    resultMessages.AppendLine( $"[{provider.Name}] Unable to load component" );
                    return 0;
                }

                // Get Existing Links to provider
                var providerLocations = schedulingProviderLocationService.Queryable()
                        .Where( spl => spl.SchedulingProviderId == provider.Id )
                        .ToList();

                var providerReservations = schedulingProviderReservationService.Queryable()
                    .Where( spr => spr.SchedulingProviderId == provider.Id )
                    .ToList();

                // Get reservations to export
                var reservationsQuery = reservationService.Queryable();

                // Grab only reservations with locations linked to this provider
                var providerLocationIds = providerLocations.Select( pl => pl.LocationId ).ToList();
                reservationsQuery = reservationsQuery.Where( r =>
                    r.ReservationLocations.Any( rl => providerLocationIds.Contains( rl.LocationId ) ) );

                // Filter reservations by date range
                var exportDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues(
                    GetAttributeValue( AttributeKey.ExportDateRange ) );
                var startDate = exportDateRange.Start;
                var endDate = exportDateRange.End;

                if ( startDate != null )
                {
                    reservationsQuery = reservationsQuery.Where( r =>
                        r.LastOccurrenceEndDateTime > startDate.Value );
                }

                if ( endDate != null )
                {
                    reservationsQuery = reservationsQuery.Where( r =>
                        r.FirstOccurrenceStartDateTime < endDate.Value );
                }

                // Filter reservations by DataView if specified
                var dataViewGuid = GetAttributeValue( AttributeKey.ReservationsDataView ).AsGuidOrNull();
                if ( dataViewGuid.HasValue )
                {
                    var dataView = new DataViewService( rockContext ).Get( dataViewGuid.Value );
                    if ( dataView != null )
                    {
                        var parameterExpression = reservationService.ParameterExpression;
                        var whereExpression = dataView.GetExpression( reservationService, parameterExpression );
                        reservationsQuery = reservationsQuery
                                .Where( parameterExpression, whereExpression, null );
                    }
                }

                var reservationsToExport = reservationsQuery
                    .ToList();

                if ( !reservationsToExport.Any() )
                {
                    resultMessages.AppendLine( $"[{provider.Name}] No reservations to export" );
                    return 0;
                }

                // Process each reservation
                foreach ( var reservation in reservationsToExport )
                {
                    try
                    {
                        // Convert reservation to provider event format
                        var providerEvent = DataConverter.GenerateProviderEventFromReservation( reservation, rockContext, provider.Id );
                        if ( providerEvent == null )
                        {
                            resultMessages.AppendLine( $"[{provider.Name}] Failed to convert reservation {reservation.Id} to provider event" );
                            continue;
                        }

                        // Check if reservation is already linked to this provider
                        var existingLink = providerReservations
                            .FirstOrDefault( spr => spr.ReservationId == reservation.Id );

                        List<string> errorMessages;
                        if ( existingLink != null )
                        {
                            // Get existing provider event to check modification time
                            var existingProviderEvent = component.GetProviderEvent(
                                provider,
                                existingLink.ExternalId,
                                out errorMessages );

                            if ( errorMessages.Any() )
                            {
                                resultMessages.AppendLine( $"[{provider.Name}] Error getting provider event for reservation {reservation.Id}: {string.Join( ", ", errorMessages )}" );
                            }

                            // Check if the provider event is more recent than the reservation, if so skip exporting this reservation
                            var providerModified = existingProviderEvent?.ModifiedDateTime ?? existingProviderEvent?.CreatedDateTime ?? DateTime.MinValue;
                            var rockModified = reservation.ModifiedDateTime ?? reservation.CreatedDateTime ?? DateTime.MinValue;

                            if ( providerModified > rockModified )
                            {
                                continue;
                            }

                            // Update the existing provider event
                            providerEvent.ExternalId = existingLink.ExternalId;
                            var updatedEvent = component.UpdateProviderEvent( provider, providerEvent, out errorMessages );
                            if ( updatedEvent!= null )
                            {
                                providerExportCount++;
                            }
                            else
                            {
                                var errorMessage = errorMessages.Any() ? string.Join( ", ", errorMessages ) : "Unknown error";
                                resultMessages.AppendLine( $"[{provider.Name}] Failed to update reservation {reservation.Id}: {errorMessage}" );
                            }
                        }
                        else
                        {
                            // Create new provider event
                            var createdEvent = component.CreateProviderEvent( provider, providerEvent, out errorMessages );

                            if ( createdEvent != null && !errorMessages.Any() )
                            {
                                // Link the newly created event to the reservation
                                var providerReservation = new SchedulingProviderReservation
                                {
                                    SchedulingProviderId = provider.Id,
                                    ReservationId = reservation.Id,
                                    ExternalId = createdEvent.ExternalId
                                };
                                schedulingProviderReservationService.Add( providerReservation );
                                rockContext.SaveChanges();

                                providerExportCount++;
                            }
                            else
                            {
                                var errorMessage = errorMessages.Any() ? string.Join( ", ", errorMessages ) : "Unknown error";
                                resultMessages.AppendLine( $"[{provider.Name}] Failed to create reservation {reservation.Id}: {errorMessage}" );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                        resultMessages.AppendLine( $"[{provider.Name}] Exception exporting reservation {reservation.Id}: {ex.Message}" );
                    }
                }

                resultMessages.AppendLine( $"[{provider.Name}] Successfully exported {providerExportCount} reservation(s)" );
                return providerExportCount;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                resultMessages.AppendLine( $"[{provider.Name}] Exception during export: {ex.Message}" );
                return 0;
            }
        }

        /// <summary>
        /// Sends an import notification for a newly imported reservation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="systemCommunicationGuid">The system communication unique identifier.</param>
        private void SendImportNotification( RockContext rockContext, Reservation reservation, Guid systemCommunicationGuid )
        {
            try
            {
                var systemCommunication = new SystemCommunicationService( rockContext ).Get( systemCommunicationGuid );
                if ( systemCommunication == null )
                {
                    return;
                }

                var recipients = new List<RockMessageRecipient>();

                // Get event coordinator or admin contact
                if ( reservation.EventContactPersonAliasId.HasValue )
                {
                    var person = reservation.EventContactPersonAlias?.Person;
                    if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Reservation", reservation );
                        recipients.Add( new RockEmailMessageRecipient( person, mergeFields ) );
                    }
                }

                if ( recipients.Any() )
                {
                    var emailMessage = new RockEmailMessage( systemCommunication );
                    emailMessage.SetRecipients( recipients );
                    emailMessage.Send();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
            }
        }

        #endregion
    }
}
