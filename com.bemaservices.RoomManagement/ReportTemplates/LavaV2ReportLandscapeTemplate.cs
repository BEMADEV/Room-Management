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
using System.ComponentModel.Composition;

using iTextSharp.text;
using PuppeteerSharp.Media;

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// Class LavaReportLandscapeTemplate.
    /// Implements the <see cref="com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate" />
    /// </summary>
    /// <seealso cref="com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate" />
    [System.ComponentModel.Description( "The lava v2 report template in landscape" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Lava V2 Landscape" )]
    public class LavaV2ReportLandscapeTemplate : LavaV2ReportTemplate
    {
        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        protected override PaperFormat PaperFormat => new PaperFormat( PaperFormat.A4.Height, PaperFormat.A4.Width );
    }
}
