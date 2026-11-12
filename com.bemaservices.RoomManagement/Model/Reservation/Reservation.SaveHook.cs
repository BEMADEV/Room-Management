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
using com.bemaservices.RoomManagement.Transactions;
using Rock;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Adds Rock's reservation save hook to the model.
    /// </summary>
    public partial class Reservation
    {
        /// <summary>
        /// Delegates property-history capture to the tracking helper and queues history and workflows only after a successful commit.
        /// </summary>
        internal class SaveHook : EntitySaveHook<Reservation>
        {
            private ReservationChangeTransaction _changeTransaction;

            /// <summary>
            /// Captures reservation property history and trigger values while EF's original values are still available.
            /// </summary>
            protected override void PreSave()
            {
                if ( PreSaveState == EntityContextState.Added || PreSaveState == EntityContextState.Modified )
                {
                    var rockContext = RockContext;
                    if ( rockContext == null )
                    {
                        throw new InvalidOperationException( "Reservation changes require a RockContext." );
                    }

                    var entry = rockContext.Entry( Entity );
                    var changes = new ReservationChangeTrackingHelper( rockContext ).BuildHistoryChanges( Entity, entry, entry.State );
                    _changeTransaction = new ReservationChangeTransaction( entry, changes, rockContext.SourceOfChange );
                }

                base.PreSave();
            }

            /// <summary>
            /// Records committed identifiers and queues the transaction that writes history before launching workflows.
            /// Rock invokes this callback only after the save or enclosing transaction commits.
            /// </summary>
            protected override void PostSave()
            {
                if ( _changeTransaction != null )
                {
                    _changeTransaction.SetReservationId( Entity.Id, Entity.ModifiedByPersonAliasId );
                    _changeTransaction.SetTriggerModifiedDateTime( Entity.ModifiedDateTime );
                    _changeTransaction.Enqueue();
                }

                base.PostSave();
            }
        }
    }
}
