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
using System.IO;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using Rock.Lava;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that provides access to a filtered set of reservations.
    /// Lava objects are created in the block context to provide access to the set of reservations matching the filter parmeters.
    /// The <c>Reservations</c> collection contains information about the Reservation instances.
    /// The <c>ReservationSummaries</c> collection contains the actual occurrences of the Reservation that match the filter.
    /// </summary>
    public class ReservationSummariesBlock : LavaBlockBase, ILavaSecured
    {
        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "reservationsummaries";

        /// <summary>
        /// The attributes markup
        /// </summary>
        private string _attributesMarkup;
        /// <summary>
        /// The render errors
        /// </summary>
        private bool _renderErrors = true;

        /// <summary>
        /// The settings
        /// </summary>
        LavaElementAttributes _settings = new LavaElementAttributes();

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // first ensure that reservation summaries are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                return;
            }

            try
            {
                var dataSource = new ReservationSummariesLavaDataSource();

                _settings.ParseFromMarkup( _attributesMarkup, context );

                var reservationSummaries = dataSource.GetReservationSummaries( _settings, LavaHelper.GetRockContextFromLavaContext( context ) );

                AddLavaMergeFieldsToContext( context, reservationSummaries );

                base.OnRender( context, result );
            }
            catch ( Exception ex )
            {
                var message = "Reservation Summaries not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Adds the lava merge fields to context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reservationSummaries">The reservation summaries.</param>
        private void AddLavaMergeFieldsToContext( ILavaRenderContext context, List<ReservationSummary> reservationSummaries )
        {
            var reservations = reservationSummaries
                .OrderBy( e => e.ReservationStartDateTime )
                .GroupBy( e => e.ReservationId )
                .Select( e => e.ToList() )
                .ToList();

            reservationSummaries = reservationSummaries
                .OrderBy( e => e.ReservationStartDateTime )
                .ThenBy( e => e.ReservationName )
                .ToList();

            context["Reservations"] = reservations;
            context["ReservationSummaries"] = reservationSummaries;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "ReservationSummaries";
            }
        }

        #endregion
    }
}
