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

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// Class LavaReportTemplate.
    /// Implements the <see cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    /// </summary>
    /// <seealso cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    [System.ComponentModel.Description( "The lava report template" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Lava" )]
    [Rock.SystemGuid.EntityTypeGuid( "7EF82CCA-7874-4B8D-ADB7-896F05095354" )]
    public class LavaReportTemplate : LavaV2ReportTemplate
    {
    }
}
