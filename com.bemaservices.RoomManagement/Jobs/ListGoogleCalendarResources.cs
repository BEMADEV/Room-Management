// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license/
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
using System.Linq;
using System.Text;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using com.bemaservices.RoomManagement.Model;

namespace com.bemaservices.RoomManagement.Jobs
{
    /// <summary>
    /// Job to list all Google Calendar resources accessible by configured service accounts.
    /// This helps administrators identify the correct calendar IDs (email addresses) to use
    /// when configuring SchedulingProviderLocation records.
    /// </summary>
    [DefinedValueField(
        "Scheduling Providers",
        Description = "Select the Google Resources scheduling providers to list calendars for. Leave blank to list calendars for all active Google Resources providers.",
        IsRequired = false,
        AllowMultiple = true,
        DefinedTypeGuid = "YOUR_SCHEDULING_PROVIDER_DEFINED_TYPE_GUID", // Update with actual GUID
        Key = AttributeKey.SchedulingProviders,
        Order = 0 )]
    [BooleanField(
        "Show All Calendars",
        Description = "Show all calendars accessible to the service account. If false, only resource calendars (rooms/equipment) will be highlighted.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowAllCalendars,
        Order = 1 )]
    [DisallowConcurrentExecution]
    public class ListGoogleCalendarResources : IJob
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string SchedulingProviders = "SchedulingProviders";
            public const string ShowAllCalendars = "ShowAllCalendars";
        }
        #endregion

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();
            var schedulingProviderService = new SchedulingProviderService( rockContext );
            var resultMessages = new StringBuilder();

            // Get job configuration
            var selectedProviderGuids = dataMap.GetString( AttributeKey.SchedulingProviders )
                .SplitDelimitedValues()
                .AsGuidList();
            var showAllCalendars = dataMap.GetString( AttributeKey.ShowAllCalendars ).AsBoolean();

            // Get providers to process
            var providersQuery = schedulingProviderService.Queryable()
                .Where( sp => sp.IsActive );

            if ( selectedProviderGuids.Any() )
            {
                providersQuery = providersQuery.Where( sp => selectedProviderGuids.Contains( sp.Guid ) );
            }

            var providers = providersQuery.ToList()
                .ToList();

            if ( !providers.Any() )
            {
                context.Result = "No active Google Resources scheduling providers found.";
                return;
            }

            resultMessages.AppendLine( $"Listing Google Calendar Resources" );
            resultMessages.AppendLine( $"=================================" );
            resultMessages.AppendLine();

            int totalProviders = 0;
            int totalCalendars = 0;
            int totalResourceCalendars = 0;

            foreach ( var provider in providers )
            {
                totalProviders++;
                resultMessages.AppendLine( $"Provider: {provider.Name}" );
                resultMessages.AppendLine( new string( '-', 80 ) );

                var component = provider.GetSchedulingComponent() as SchedulingProviders.GoogleResources;
                if ( component == null )
                {
                    resultMessages.AppendLine( "  ERROR: Unable to load component" );
                    resultMessages.AppendLine();
                    continue;
                }

                List<string> errorMessages;
                var calendars = component.GetAvailableCalendars( provider, out errorMessages );

                if ( errorMessages.Any() )
                {
                    resultMessages.AppendLine( "  Errors:" );
                    foreach ( var error in errorMessages )
                    {
                        resultMessages.AppendLine( $"    - {error}" );
                    }
                    resultMessages.AppendLine();
                    continue;
                }

                if ( !calendars.Any() )
                {
                    resultMessages.AppendLine( "  No calendars found. Verify:" );
                    resultMessages.AppendLine( "    - Service account has been granted access to calendars" );
                    resultMessages.AppendLine( "    - Domain-wide delegation is enabled (if accessing resource calendars)" );
                    resultMessages.AppendLine( "    - Appropriate OAuth scopes are configured" );
                    resultMessages.AppendLine();
                    continue;
                }

                totalCalendars += calendars.Count;

                // Identify resource calendars
                var resourceCalendars = calendars.Where( c =>
                    c.Id.Contains( "@resource.calendar.google.com" ) ||
                    ( c.Summary != null && (
                        c.Summary.ToLower().Contains( "room" ) ||
                        c.Summary.ToLower().Contains( "conference" ) ||
                        c.Summary.ToLower().Contains( "equipment" ) ||
                        c.Summary.ToLower().Contains( "resource" )
                    ) )
                ).ToList();

                totalResourceCalendars += resourceCalendars.Count;

                // Display resource calendars first
                if ( resourceCalendars.Any() )
                {
                    resultMessages.AppendLine( "  RESOURCE CALENDARS (Rooms/Equipment):" );
                    resultMessages.AppendLine();

                    foreach ( var calendar in resourceCalendars.OrderBy( c => c.Summary ) )
                    {
                        resultMessages.AppendLine( $"    Calendar ID:  {calendar.Id}" );
                        resultMessages.AppendLine( $"    Name:         {calendar.Summary}" );
                        
                        if ( !string.IsNullOrWhiteSpace( calendar.Description ) )
                        {
                            resultMessages.AppendLine( $"    Description:  {calendar.Description}" );
                        }
                        
                        resultMessages.AppendLine( $"    Access:       {calendar.AccessRole}" );
                        resultMessages.AppendLine( $"    Primary:      {calendar.Primary.GetValueOrDefault( false )}" );
                        resultMessages.AppendLine();
                    }
                }
                else
                {
                    resultMessages.AppendLine( "  No resource calendars found." );
                    resultMessages.AppendLine();
                }

                // Display other calendars if requested
                if ( showAllCalendars )
                {
                    var otherCalendars = calendars.Except( resourceCalendars ).ToList();
                    
                    if ( otherCalendars.Any() )
                    {
                        resultMessages.AppendLine( "  OTHER CALENDARS:" );
                        resultMessages.AppendLine();

                        foreach ( var calendar in otherCalendars.OrderBy( c => c.Summary ) )
                        {
                            resultMessages.AppendLine( $"    Calendar ID:  {calendar.Id}" );
                            resultMessages.AppendLine( $"    Name:         {calendar.Summary}" );
                            resultMessages.AppendLine( $"    Access:       {calendar.AccessRole}" );
                            resultMessages.AppendLine();
                        }
                    }
                }

                // Check current configuration
                var schedulingProviderLocations = new SchedulingProviderLocationService( rockContext )
                    .Queryable()
                    .Where( spl => spl.SchedulingProviderId == provider.Id )
                    .ToList();

                if ( schedulingProviderLocations.Any() )
                {
                    resultMessages.AppendLine( "  CURRENT CONFIGURATION:" );
                    resultMessages.AppendLine();

                    foreach ( var spl in schedulingProviderLocations )
                    {
                        var matchingCalendar = calendars.FirstOrDefault( c => c.Id == spl.ExternalId );
                        var locationName = spl.Location?.Name ?? $"Location ID: {spl.LocationId}";
                        
                        if ( matchingCalendar != null )
                        {
                            resultMessages.AppendLine( $"    ✓ {locationName}" );
                            resultMessages.AppendLine( $"      External ID: {spl.ExternalId}" );
                            resultMessages.AppendLine( $"      Calendar:    {matchingCalendar.Summary}" );
                        }
                        else
                        {
                            resultMessages.AppendLine( $"    ✗ {locationName}" );
                            resultMessages.AppendLine( $"      External ID: {spl.ExternalId}" );
                            resultMessages.AppendLine( $"      ERROR:       Calendar not found or not accessible" );
                            
                            // Check if it looks like a numeric ID
                            if ( int.TryParse( spl.ExternalId, out _ ) )
                            {
                                resultMessages.AppendLine( $"      NOTE:        External ID appears to be numeric. Google Calendar IDs must be email addresses." );
                            }
                        }
                        resultMessages.AppendLine();
                    }
                }

                resultMessages.AppendLine();
            }

            // Summary
            resultMessages.AppendLine( "SUMMARY" );
            resultMessages.AppendLine( "=======" );
            resultMessages.AppendLine( $"Providers processed:    {totalProviders}" );
            resultMessages.AppendLine( $"Total calendars found:  {totalCalendars}" );
            resultMessages.AppendLine( $"Resource calendars:     {totalResourceCalendars}" );
            resultMessages.AppendLine();
            resultMessages.AppendLine( "CONFIGURATION INSTRUCTIONS" );
            resultMessages.AppendLine( "==========================" );
            resultMessages.AppendLine( "To configure a SchedulingProviderLocation:" );
            resultMessages.AppendLine( "1. Copy the 'Calendar ID' from the resource calendar list above" );
            resultMessages.AppendLine( "2. Navigate to: Room Management > Scheduling Providers > [Provider] > Locations" );
            resultMessages.AppendLine( "3. Add or edit a location" );
            resultMessages.AppendLine( "4. Paste the Calendar ID into the 'External ID' field" );
            resultMessages.AppendLine( "5. Save the location" );
            resultMessages.AppendLine();
            resultMessages.AppendLine( "Or update directly in SQL:" );
            resultMessages.AppendLine( "UPDATE [_com_bemaservices_RoomManagement_SchedulingProviderLocation]" );
            resultMessages.AppendLine( "SET [ExternalId] = 'calendar-id@domain.com'" );
            resultMessages.AppendLine( "WHERE [Id] = [location-id]" );

            context.Result = resultMessages.ToString();
        }
    }
}