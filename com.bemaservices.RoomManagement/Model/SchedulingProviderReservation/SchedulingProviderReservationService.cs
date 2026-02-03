using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Service class for <see cref="SchedulingProviderReservation"/>.
    /// </summary>
    public class SchedulingProviderReservationService : Service<SchedulingProviderReservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderReservationService"/> class.
        /// </summary>
        /// <param name="context">The data context.</param>
        public SchedulingProviderReservationService( RockContext context ) : base( context )
        {
        }
    }
}
