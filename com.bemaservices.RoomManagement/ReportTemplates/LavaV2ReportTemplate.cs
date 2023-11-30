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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Pdf;
using Rock.Web.Cache;

using Document = iTextSharp.text.Document;

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// Class LavaReportTemplate.
    /// Implements the <see cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    /// </summary>
    /// <seealso cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    [System.ComponentModel.Description( "The lava v2 report template" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Lava V2" )]
    public class LavaV2ReportTemplate : ReportTemplate
    {
        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>The exceptions.</value>
        public override List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="reservationSummaryList">The reservation summary list.</param>
        /// <param name="logoFileUrl">The logo file URL.</param>
        /// <param name="font">The font.</param>
        /// <param name="filterStartDate">The filter start date.</param>
        /// <param name="filterEndDate">The filter end date.</param>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <returns>System.Byte[].</returns>
        public override byte[] GenerateReport( List<ReservationService.ReservationSummary> reservationSummaryList, string logoFileUrl, string font, DateTime? filterStartDate, DateTime? filterEndDate, string lavaTemplate = "" )
        {
            Font zapfdingbats = new Font( Font.ZAPFDINGBATS );

            // Date Ranges
            var today = RockDateTime.Today;
            var filterStartDateTime = filterStartDate.HasValue ? filterStartDate.Value : today;
            var filterEndDateTime = filterEndDate.HasValue ? filterEndDate.Value : today.AddMonths( 1 );

            // Build the Lava html
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                ReservationType = r.ReservationType,
                ApprovalState = r.ApprovalState.ConvertToString(),
                ApprovalStateInt = r.ApprovalState.ConvertToInt(),
                Locations = r.ReservationLocations.ToList(),
                Resources = r.ReservationResources.ToList(),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventTimeDescription = r.EventTimeDescription,
                EventDateTimeDescription = r.EventDateTimeDescription,
                ReservationTimeDescription = r.ReservationTimeDescription,
                ReservationDateTimeDescription = r.ReservationDateTimeDescription,
                SetupPhotoId = r.SetupPhotoId,
                SetupPhotoLink = GlobalAttributesCache.Value( "InternalApplicationRoot" ) + string.Format( "/GetImage.ashx?id={0}", r.SetupPhotoId ?? 0 ),
                Note = r.Note,
                RequesterAlias = r.RequesterAlias,
                EventContactPersonAlias = r.EventContactPersonAlias,
                EventContactEmail = r.EventContactEmail,
                EventContactPhoneNumber = r.EventContactPhoneNumber,
                ReservationMinistry = r.ReservationMinistry,
                MinistryName = r.ReservationMinistry != null ? r.ReservationMinistry.Name : string.Empty,
            } )
                .ToList();

            var lavaReservationSummaries = reservationSummaries
                .OrderBy( r => r.EventStartDateTime )
                .GroupBy( r => r.EventStartDateTime.Date )
                .Select( r => r.ToList() )
                .ToList();

            // Build a list of dates and then all the reservations on those dates.
            // Each date contains a few useful details depending on how you want
            // to present the data.
            // Date = The date containing these reservations.
            // Reservations = The ordered list of reservations for this day.
            // Locations = The ordered list of reservation locations being used (for example with a room setup sheet).
            // Resources = The ordered list of resources being used (for example to easily see where resources are supposed to go).
            var lavaReservationDates = reservationSummaries
                .OrderBy( r => r.EventStartDateTime )
                .GroupBy( r => r.EventStartDateTime.Date )
                .Select( r => new
                {
                    Date = r.Key,
                    Reservations = r.ToList(),
                    Locations = r
                        .SelectMany( a => a.Locations, ( a, b ) => new
                        {
                            Name = b.Location.Name,
                            Reservation = a,
                            Location = b
                        } )
                        .OrderBy( a => a.Reservation.EventStartDateTime )
                        .ThenBy( a => a.Name )
                        .ToList(),
                    Resources = r
                        .SelectMany( a => a.Resources, ( a, b ) => new
                        {
                            Name = b.Resource.Name,
                            Reservation = a,
                            Resource = b
                        } )
                        .OrderBy( a => a.Reservation.EventStartDateTime )
                        .ThenBy( a => a.Name )
                        .ToList()
                } )
                .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "ReservationSummaries", lavaReservationSummaries );
            mergeFields.Add( "ReservationDates", lavaReservationDates );
            mergeFields.Add( "FilterStartDate", filterStartDateTime );
            mergeFields.Add( "FilterEndDate", filterEndDateTime );
            mergeFields.Add( "ImageUrl", logoFileUrl.EncodeHtml() );
            mergeFields.Add( "ReportFont", font );
            mergeFields.Add( "CheckMark", new Phrase( "\u0034", zapfdingbats ).ToString() ); // This doesn't actually work.

            string mergeHtml = lavaTemplate.ResolveMergeFields( mergeFields );
            using ( var pdfGenerator = new PdfGenerator() )
            {
                var pdfStream = pdfGenerator.GetPDFDocumentFromHtml( mergeHtml );
                var memoryStream = new MemoryStream( pdfStream.ReadBytesToEnd() );
                return memoryStream.ToArray();
            }
        }
    }
}
