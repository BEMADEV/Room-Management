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
using Rock.Data;


namespace com.bemaservices.RoomManagement.SchedulingProviders
{
    [Export( typeof( SchedulingProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Google Resources" )]
    [Rock.SystemGuid.EntityTypeGuid( "46C006ED-1CCF-4FD1-837F-5647B26D2A7D" )]
    public class GoogleResources : SchedulingProviderComponent
    {
        public override bool ExportData( RockContext rockContext, out List<string> errorMessages )
        {
            throw new System.NotImplementedException();
        }

        public override bool ImportData( RockContext rockContext, out List<string> errorMessages )
        {
            throw new System.NotImplementedException();
        }
    }
}