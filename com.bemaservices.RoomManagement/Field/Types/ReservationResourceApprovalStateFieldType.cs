﻿// <copyright>
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
using com.bemaservices.RoomManagement.Model;
using Rock.Field.Types;

namespace com.bemaservices.RoomManagement.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of connection states
    /// </summary>
    [Serializable]
    public class ReservationResourceApprovalStateFieldType : EnumFieldType<ReservationResourceApprovalState>
    {
    }
}