using System.Linq;
using Rock.Data;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Service class for <see cref="SchedulingProvider"/>.
    /// </summary>
    public class SchedulingProviderService : Service<SchedulingProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingProviderService"/> class.
        /// </summary>
        /// <param name="context">The data context.</param>
        public SchedulingProviderService( RockContext context ) : base( context )
        {
        }
    }
}
