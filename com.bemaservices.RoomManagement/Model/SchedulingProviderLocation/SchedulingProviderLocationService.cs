using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Service class for <see cref="SchedulingProviderLocation"/>.
    /// </summary>
    public class SchedulingProviderLocationService : Service<SchedulingProviderLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderLocationService"/> class.
        /// </summary>
        /// <param name="context">The data context.</param>
        public SchedulingProviderLocationService( RockContext context ) : base( context )
        {
        }
    }
}
