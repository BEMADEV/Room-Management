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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_Reservation" )]
    [DataContract]
    public class Reservation : Rock.Data.Model<Reservation>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>The reservation type identifier.</value>
        [Required]
        [DataMember]
        public int ReservationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>The schedule identifier.</value>
        [Required]
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence identifier.
        /// </summary>
        /// <value>The event item occurrence identifier.</value>
        [DataMember]
        public int? EventItemOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the reservation ministry identifier.
        /// </summary>
        /// <value>The reservation ministry identifier.</value>
        [DataMember]
        public int? ReservationMinistryId { get; set; }

        /// <summary>
        /// Gets or sets the state of the approval.
        /// </summary>
        /// <value>The state of the approval.</value>
        [DataMember]
        public ReservationApprovalState ApprovalState { get; set; }

        /// <summary>
        /// Gets or sets the requester alias identifier.
        /// </summary>
        /// <value>The requester alias identifier.</value>
        [DataMember]
        public int? RequesterAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approver alias identifier.
        /// </summary>
        /// <value>The approver alias identifier.</value>
        [DataMember]
        public int? ApproverAliasId { get; set; }

        /// <summary>
        /// Gets or sets the setup time.
        /// </summary>
        /// <value>The setup time.</value>
        [DataMember]
        public int? SetupTime { get; set; }

        /// <summary>
        /// Gets or sets the cleanup time.
        /// </summary>
        /// <value>The cleanup time.</value>
        [DataMember]
        public int? CleanupTime { get; set; }

        /// <summary>
        /// Gets or sets the number attending.
        /// </summary>
        /// <value>The number attending.</value>
        [DataMember]
        public int? NumberAttending { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [DataMember]
        [MaxLength( 2500 )]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the setup photo identifier.
        /// </summary>
        /// <value>The setup photo identifier.</value>
        [DataMember]
        public int? SetupPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the name of the event contact.
        /// </summary>
        /// <value>The name of the event contact.</value>
        [DataMember]
        public int? EventContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the event contact phone.
        /// </summary>
        /// <value>A <see cref="System.String" /> representing the phone number of the event contact person.</value>
        [DataMember]
        [MaxLength( 50 )]
        public string EventContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the event contact.
        /// </summary>
        /// <value>A <see cref="System.String" /> representing the email of the event contact person.</value>
        [DataMember]
        [MaxLength( 400 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        public string EventContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the administrative contact.
        /// </summary>
        /// <value>The name of the administrative contact.</value>
        [DataMember]
        public int? AdministrativeContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact phone.
        /// </summary>
        /// <value>A <see cref="System.String" /> representing the phone number of the administrative contact person.</value>
        [DataMember]
        [MaxLength( 50 )]
        public string AdministrativeContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the administrative contact.
        /// </summary>
        /// <value>A <see cref="System.String" /> representing the email of the administrative contact person.</value>
        [DataMember]
        [MaxLength( 400 )]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        public string AdministrativeContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the first occurrence date time.
        /// </summary>
        /// <value>The first occurrence date time.</value>
        [DataMember]
        public DateTime? FirstOccurrenceStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last occurrence date time.
        /// </summary>
        /// <value>The last occurrence date time.</value>
        [DataMember]
        public DateTime? LastOccurrenceEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the initial approver alias identifier.
        /// </summary>
        /// <value>The initial approver alias identifier.</value>
        public int? InitialApproverAliasId { get; set; }

        /// <summary>
        /// Gets or sets the initial approval date time.
        /// </summary>
        /// <value>The initial approval date time.</value>
        public DateTime? InitialApprovalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the special approver alias identifier.
        /// </summary>
        /// <value>The special approver alias identifier.</value>
        public int? SpecialApproverAliasId { get; set; }

        /// <summary>
        /// Gets or sets the special approval date time.
        /// </summary>
        /// <value>The special approval date time.</value>
        public DateTime? SpecialApprovalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the final approver alias identifier.
        /// </summary>
        /// <value>The final approver alias identifier.</value>
        public int? FinalApproverAliasId { get; set; }

        /// <summary>
        /// Gets or sets the final approval date time.
        /// </summary>
        /// <value>The final approval date time.</value>
        public DateTime? FinalApprovalDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>The type of the reservation.</value>
        [DataMember]
        public virtual ReservationType ReservationType { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        [LavaInclude]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>The campus.</value>
        [LavaInclude]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the event item occurrence.
        /// </summary>
        /// <value>The event item occurrence.</value>
        [LavaInclude]
        public virtual EventItemOccurrence EventItemOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the reservation ministry.
        /// </summary>
        /// <value>The reservation ministry.</value>
        [LavaInclude]
        public virtual ReservationMinistry ReservationMinistry { get; set; }

        /// <summary>
        /// Gets or sets the requester alias.
        /// </summary>
        /// <value>The requester alias.</value>
        [LavaInclude]
        public virtual PersonAlias RequesterAlias { get; set; }

        /// <summary>
        /// Gets or sets the approver alias.
        /// </summary>
        /// <value>The approver alias.</value>
        [LavaInclude]
        public virtual PersonAlias ApproverAlias { get; set; }

        /// <summary>
        /// Gets or sets the reservation workflows.
        /// </summary>
        /// <value>The reservation workflows.</value>
        [LavaInclude]
        public virtual ICollection<ReservationWorkflow> ReservationWorkflows
        {
            get { return _reservationWorkflows; }
            set { _reservationWorkflows = value; }
        }
        /// <summary>
        /// The reservation workflows
        /// </summary>
        private ICollection<ReservationWorkflow> _reservationWorkflows;

        /// <summary>
        /// Gets or sets the reservation resources.
        /// </summary>
        /// <value>The reservation resources.</value>
        [LavaInclude]
        public virtual ICollection<ReservationResource> ReservationResources
        {
            get { return _reservationResources ?? ( _reservationResources = new Collection<ReservationResource>() ); }
            set { _reservationResources = value; }
        }
        /// <summary>
        /// The reservation resources
        /// </summary>
        private ICollection<ReservationResource> _reservationResources;

        /// <summary>
        /// Gets or sets the reservation linkages.
        /// </summary>
        /// <value>The reservation resources.</value>
        [LavaInclude]
        public virtual ICollection<ReservationLinkage> ReservationLinkages
        {
            get { return _reservationLinkages ?? ( _reservationLinkages = new Collection<ReservationLinkage>() ); }
            set { _reservationLinkages = value; }
        }
        /// <summary>
        /// The reservation linkages
        /// </summary>
        private ICollection<ReservationLinkage> _reservationLinkages;

        /// <summary>
        /// Gets or sets the reservation locations.
        /// </summary>
        /// <value>The reservation locations.</value>
        [LavaInclude]
        public virtual ICollection<ReservationLocation> ReservationLocations
        {
            get { return _reservationLocations ?? ( _reservationLocations = new Collection<ReservationLocation>() ); }
            set { _reservationLocations = value; }
        }
        /// <summary>
        /// The reservation locations
        /// </summary>
        private ICollection<ReservationLocation> _reservationLocations;

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <value>The setup photo URL.</value>
        [LavaInclude]
        [NotMapped]
        public virtual string SetupPhotoUrl
        {
            get
            {
                return Reservation.GetSetupPhotoUrl( this );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets or sets the setup photo.
        /// </summary>
        /// <value>The setup photo.</value>
        [DataMember]
        public virtual BinaryFile SetupPhoto { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias" /> representing the personalias who is the event contact person.
        /// </summary>
        /// <value>A <see cref="Rock.Model.PersonAlias" /> representing the personalias who is the event contact person.</value>
        [DataMember]
        public virtual PersonAlias EventContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias" /> representing the personalias who is the administrative contact person.
        /// </summary>
        /// <value>A <see cref="Rock.Model.PersonAlias" /> representing the personalias who is the administrative contact person.</value>
        [DataMember]
        public virtual PersonAlias AdministrativeContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the initial approver person alias.
        /// </summary>
        /// <value>The initial approver person alias.</value>
        public virtual PersonAlias InitialApproverAlias { get; set; }

        /// <summary>
        /// Gets or sets the special approver person alias.
        /// </summary>
        /// <value>The special approver person alias.</value>
        public virtual PersonAlias SpecialApproverAlias { get; set; }

        /// <summary>
        /// Gets or sets the final approver person alias.
        /// </summary>
        /// <value>The final approver person alias.</value>
        public virtual PersonAlias FinalApproverAlias { get; set; }

        /// <summary>
        /// Gets the friendly reservation time.
        /// </summary>
        /// <value>The friendly reservation time.</value>
        [LavaInclude]
        [NotMapped]
        public virtual string FriendlyReservationTime
        {
            get
            {
                return GetFriendlyReservationScheduleText();
            }
            private set
            {

            }
        }

        /// <summary>
        /// Gets the reservation modified date time.
        /// </summary>
        /// <value>The reservation modified date time.</value>
        [LavaInclude]
        [NotMapped]
        public virtual DateTime? ReservationModifiedDateTime
        {
            get
            {
                DateTime? modifiedDateTime = ModifiedDateTime;
                foreach ( var reservationLocation in ReservationLocations )
                {
                    if ( reservationLocation.ModifiedDateTime.HasValue )
                    {
                        if ( !modifiedDateTime.HasValue || reservationLocation.ModifiedDateTime > modifiedDateTime )
                        {
                            modifiedDateTime = reservationLocation.ModifiedDateTime;
                        }

                    }
                }

                foreach ( var reservationResource in ReservationResources )
                {
                    if ( reservationResource.ModifiedDateTime.HasValue )
                    {
                        if ( !modifiedDateTime.HasValue || reservationResource.ModifiedDateTime > modifiedDateTime )
                        {
                            modifiedDateTime = reservationResource.ModifiedDateTime;
                        }

                    }
                }

                return modifiedDateTime;
            }
            private set
            { }
        }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <value>The next start date time.</value>
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                DateTime? startDate = null;
                startDate = Schedule.GetNextStartDateTime( RockDateTime.Now );

                try
                {
                    if ( startDate.HasValue )
                    {
                        startDate = startDate.Value.AddMinutes( SetupTime ?? 0 );
                    }
                }
                catch
                {

                }

                return startDate;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>The parent authority.</value>
        public override Rock.Security.ISecured ParentAuthority
        {
            get
            {
                return this.ReservationType != null ? this.ReservationType : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets a list of scheduled start datetimes between the two specified dates, sorted by datetime.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns>List&lt;ReservationDateTime&gt;.</returns>
        public virtual List<ReservationDateTime> GetReservationTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            return GetReservationTimes( Schedule, beginDateTime, endDateTime );
        }

        /// <summary>
        /// Gets the reservation times.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns>List&lt;ReservationDateTime&gt;.</returns>
        public static List<ReservationDateTime> GetReservationTimes( Schedule schedule, DateTime beginDateTime, DateTime endDateTime )
        {
            if ( schedule != null )
            {
                var result = new List<ReservationDateTime>();

                var calEvent = schedule.GetICalEvent();
                if ( calEvent != null && calEvent.DtStart != null )
                {
                    var occurrences = InetCalendarHelper.GetOccurrences( schedule.iCalendarContent, beginDateTime, endDateTime );
                    result = occurrences
                        .Where( a =>
                            a.Period != null &&
                            a.Period.StartTime != null &&
                            a.Period.EndTime != null )
                        .Select( a => new ReservationDateTime
                        {
                            StartDateTime = DateTime.SpecifyKind( a.Period.StartTime.Value, DateTimeKind.Local ),
                            EndDateTime = DateTime.SpecifyKind( a.Period.EndTime.Value, DateTimeKind.Local )
                        } )
                        .OrderBy( a => a.StartDateTime )
                        .ToList();
                    {
                        // ensure the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                    }
                }

                return result;
            }
            else
            {
                return new List<ReservationDateTime>();
            }

        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetSetupPhotoUrl( Reservation reservation, int? maxWidth = null, int? maxHeight = null )
        {
            return GetSetupPhotoUrl( reservation.Id, reservation.SetupPhotoId, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetSetupPhotoUrl( int reservationId, int? maxWidth = null, int? maxHeight = null )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                Reservation reservation = new ReservationService( rockContext ).Get( reservationId );
                return GetSetupPhotoUrl( reservation, maxWidth, maxHeight );
            }
        }

        /// <summary>
        /// Gets the setup photo URL.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="setupPhotoId">The setup photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetSetupPhotoUrl( int? reservationId, int? setupPhotoId, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = string.Empty;
            if ( setupPhotoId.HasValue )
            {
                string widthHeightParams = string.Empty;
                if ( maxWidth.HasValue )
                {
                    widthHeightParams += string.Format( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    widthHeightParams += string.Format( "&maxheight={0}", maxHeight.Value );
                }

                virtualPath = string.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, setupPhotoId );
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }
        }

        /// <summary>
        /// Gets the setup photo image tag.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public static string GetSetupPhotoImageTag( Reservation reservation, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            if ( reservation != null )
            {
                return GetSetupPhotoImageTag( reservation.Id, reservation.SetupPhotoId, maxWidth, maxHeight, altText, className );
            }
            else
            {
                return GetSetupPhotoImageTag( null, null, maxWidth, maxHeight, altText, className );
            }

        }

        /// <summary>
        /// Gets the setup photo image tag.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="setupPhotoId">The setup photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public static string GetSetupPhotoImageTag( int? reservationId, int? setupPhotoId, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? string.Empty :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? string.Empty :
                string.Format( " class='{0}'", className );

            if ( setupPhotoId.HasValue )
            {
                photoUrl.AppendFormat( "GetImage.ashx?id={0}", setupPhotoId );
                if ( maxWidth.HasValue )
                {
                    photoUrl.AppendFormat( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    photoUrl.AppendFormat( "&maxheight={0}", maxHeight.Value );
                }

                return string.Format( "<img src='{0}'{1}{2}{3}/>", photoUrl.ToString(), styleString, altString, classString );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the friendly reservation schedule text.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetFriendlyReservationScheduleText()
        {
            return GetFriendlyReservationScheduleText( Schedule, ReservationType, SetupTime, CleanupTime, FirstOccurrenceStartDateTime, LastOccurrenceEndDateTime );
        }

        /// <summary>
        /// Gets the friendly reservation schedule text.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="reservationType">Type of the reservation.</param>
        /// <param name="setupTime">The setup time.</param>
        /// <param name="cleanupTime">The cleanup time.</param>
        /// <param name="firstOccurrenceStartDateTime">The first occurrence start date time.</param>
        /// <param name="lastOccurrenceEndDateTime">The last occurrence end date time.</param>
        /// <returns>System.String.</returns>
        public static string GetFriendlyReservationScheduleText( Schedule schedule, ReservationType reservationType, int? setupTime = null, int? cleanupTime = null, DateTime? firstOccurrenceStartDateTime = null, DateTime? lastOccurrenceEndDateTime = null )
        {
            string result = "";
            try
            {
                if ( schedule != null )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( schedule.ToFriendlyScheduleText() );

                    var calendarEvent = schedule.GetICalEvent();
                    if ( calendarEvent != null )
                    {
                        try
                        {
                            if ( calendarEvent.Duration != null )
                            {
                                var duration = calendarEvent.Duration;
                                if ( duration.Hours > 0 )
                                {
                                    if ( duration.Hours == 1 )
                                    {
                                        sb.AppendFormat( " for {0} hr", duration.Hours );
                                    }
                                    else
                                    {
                                        sb.AppendFormat( " for {0} hrs", duration.Hours );
                                    }

                                    if ( duration.Minutes > 0 )
                                    {
                                        sb.AppendFormat( " and {0} min", duration.Minutes );
                                    }
                                }
                                else
                                {
                                    if ( duration.Minutes > 0 )
                                    {
                                        sb.AppendFormat( " for {0} min", duration.Minutes );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }

                        try
                        {
                            if ( calendarEvent.RecurrenceRules != null && calendarEvent.RecurrenceRules.Any() )
                            {
                                if ( !firstOccurrenceStartDateTime.HasValue || !lastOccurrenceEndDateTime.HasValue )
                                {
                                    var beginDateTime = DateTime.MinValue.AddYears( 1 );
                                    var endDateTime = DateTime.MaxValue.AddYears( -1 );
                                    if ( !calendarEvent.RecurrenceRules.Any( r => ( r.Until != null && r.Until != DateTime.MinValue ) || ( r.Count != null && r.Count != -2147483648 ) ) )
                                    {
                                        if ( reservationType != null && reservationType.DefaultReservationDuration != null )
                                        {
                                            endDateTime = RockDateTime.Now.AddDays( reservationType.DefaultReservationDuration );
                                        }
                                    }

                                    var occurrences = GetReservationTimes( schedule, beginDateTime, endDateTime ).ToList();
                                    if ( occurrences.Count > 0 )
                                    {
                                        var firstReservationOccurrence = occurrences.First();
                                        var lastReservationOccurrence = occurrences.Last();

                                        try
                                        {
                                            firstOccurrenceStartDateTime = firstReservationOccurrence.StartDateTime.AddMinutes( -setupTime ?? 0 );
                                        }
                                        catch
                                        {
                                            firstOccurrenceStartDateTime = firstReservationOccurrence.StartDateTime;
                                        }

                                        try
                                        {
                                            lastOccurrenceEndDateTime = lastReservationOccurrence.EndDateTime.AddMinutes( cleanupTime ?? 0 );
                                        }
                                        catch
                                        {
                                            lastOccurrenceEndDateTime = lastReservationOccurrence.EndDateTime;
                                        }

                                    }
                                }

                                if ( firstOccurrenceStartDateTime.HasValue )
                                {
                                    sb.AppendFormat( " from {0}", firstOccurrenceStartDateTime.Value.ToShortDateString() );
                                }

                                if ( lastOccurrenceEndDateTime.HasValue )
                                {
                                    sb.AppendFormat( " to {0}", lastOccurrenceEndDateTime.Value.ToShortDateString() );
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }
                    }

                    result = sb.ToString();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                result = "An error occurred creating friendly text for your schedule.";
            }

            return result;
        }

        /// <summary>
        /// Creates a transaction to act a hook for workflow triggers before changes occur
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            if ( entry.State == System.Data.Entity.EntityState.Added || entry.State == System.Data.Entity.EntityState.Modified )
            {
                var transaction = new com.bemaservices.RoomManagement.Transactions.ReservationChangeTransaction( entry );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Determines whether [has approval rights to state] [the specified person].
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if [has approval rights to state] [the specified person]; otherwise, <c>false</c>.</returns>
        public bool HasApprovalRightsToState( Person person )
        {
            bool hasApprovalRightsToState = false;
            if ( ReservationType.HasApprovalRights( person, ApprovalGroupType.OverrideApprovalGroup, CampusId ) )
            {
                hasApprovalRightsToState = true;
            }
            else
            {
                switch ( ApprovalState )
                {
                    case ReservationApprovalState.PendingInitialApproval:
                        hasApprovalRightsToState = ReservationType.HasApprovalRights( person, ApprovalGroupType.InitialApprovalGroup, CampusId );
                        break;
                    case ReservationApprovalState.PendingFinalApproval:
                        hasApprovalRightsToState = ReservationType.HasApprovalRights( person, ApprovalGroupType.FinalApprovalGroup, CampusId );
                        break;
                }
            }

            return hasApprovalRightsToState;
        }

        /// <summary>
        /// Checks the edit after approval rights.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CheckEditAfterApprovalRights( Person person )
        {
            return (
                  ApprovalState != ReservationApprovalState.Approved ||
                  IsAuthorized( "EditAfterApproval", person )
                  );
        }

        /// <summary>
        /// Determines whether [has standard edit rights] [the specified person].
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if [has standard edit rights] [the specified person]; otherwise, <c>false</c>.</returns>
        public bool HasStandardEditRights( Person person )
        {
            var hasStandardEditRights = false;
            if ( IsAuthorized( Authorization.ADMINISTRATE, person ) )
            {
                hasStandardEditRights = true;
            }
            else
            {
                if (
                    IsAuthorized( Authorization.EDIT, person ) &&
                    (
                       ( CreatedByPersonAlias != null && CreatedByPersonAlias.PersonId == person.Id ) ||
                        ( EventContactPersonAlias != null && EventContactPersonAlias.PersonId == person.Id ) ||
                        ( AdministrativeContactPersonAlias != null && AdministrativeContactPersonAlias.PersonId == person.Id ) ||
                        Id == 0
                    )
                )
                {
                    hasStandardEditRights = true;
                }
            }
            return hasStandardEditRights;
        }

        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration for the Reservation model
    /// </summary>
    public partial class ReservationConfiguration : EntityTypeConfiguration<Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationConfiguration" /> class.
        /// </summary>
        public ReservationConfiguration()
        {
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.Reservations ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.EventItemOccurrence ).WithMany().HasForeignKey( r => r.EventItemOccurrenceId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ReservationMinistry ).WithMany().HasForeignKey( r => r.ReservationMinistryId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Schedule ).WithMany().HasForeignKey( r => r.ScheduleId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.RequesterAlias ).WithMany().HasForeignKey( r => r.RequesterAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.ApproverAlias ).WithMany().HasForeignKey( r => r.ApproverAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.InitialApproverAlias ).WithMany().HasForeignKey( r => r.InitialApproverAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.SpecialApproverAlias ).WithMany().HasForeignKey( r => r.SpecialApproverAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.FinalApproverAlias ).WithMany().HasForeignKey( r => r.FinalApproverAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SetupPhoto ).WithMany().HasForeignKey( p => p.SetupPhotoId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EventContactPersonAlias ).WithMany().HasForeignKey( p => p.EventContactPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.AdministrativeContactPersonAlias ).WithMany().HasForeignKey( p => p.AdministrativeContactPersonAliasId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Reservation" );
        }
    }

    #endregion

    #region Enumerations
    /// <summary>
    /// The enumeration for the reservation approval state
    /// </summary>
    public enum ReservationApprovalState
    {
        /// <summary>
        /// The created
        /// </summary>
        Draft = 0,

        /// <summary>
        /// The pending initial approval
        /// </summary>
        PendingInitialApproval = 1,

        /// <summary>
        /// The approved
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The denied
        /// </summary>
        Denied = 3,

        /// <summary>
        /// The changes needed
        /// </summary>
        ChangesNeeded = 4,

        /// <summary>
        /// The pending final approval
        /// </summary>
        PendingFinalApproval = 5,

        /// <summary>
        /// The pending special approval
        /// </summary>
        PendingSpecialApproval = 6,

        /// <summary>
        /// The cancelled
        /// </summary>
        Cancelled = 7
    }

    #endregion

    #region Helper Classes
    /// <summary>
    /// The view model for a Reservation DateTime
    /// </summary>
    public class ReservationDateTime
    {
        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        public DateTime StartDateTime { get; set; }
        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        public DateTime EndDateTime { get; set; }
    }

    #endregion
}
