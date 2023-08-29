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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using static com.bemaservices.RoomManagement.Model.ReservationService;

namespace Rock.Rest.Controllers
{

    /// <summary>
    /// Class ReservationLinkagesController.
    /// Implements the <see cref="Rock.Rest.ApiController{com.bemaservices.RoomManagement.Model.ReservationLinkage}" />
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiController{com.bemaservices.RoomManagement.Model.ReservationLinkage}" />
    public class ReservationLinkagesController : Rock.Rest.ApiController<com.bemaservices.RoomManagement.Model.ReservationLinkage>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationLinkagesController" /> class.
        /// </summary>
        public ReservationLinkagesController() : base( new com.bemaservices.RoomManagement.Model.ReservationLinkageService( new Rock.Data.RockContext() ) ) { }
    }
}

