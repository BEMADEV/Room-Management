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
using Rock.SystemGuid;

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// Class DefaultReportTemplate.
    /// Implements the <see cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    /// </summary>
    /// <seealso cref="com.bemaservices.RoomManagement.ReportTemplates.ReportTemplate" />
    [System.ComponentModel.Description( "The default report template" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Default" )]
    [EntityTypeGuid( "9B74314A-37E0-40F2-906C-2862C93F8888" )]
    public class DefaultReportTemplate : LavaV2ReportTemplate
    {
    }
}
