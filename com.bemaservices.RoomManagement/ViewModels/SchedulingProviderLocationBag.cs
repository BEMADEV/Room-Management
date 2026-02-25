using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace com.bemaservices.RoomManagement.ViewModels
{
    /// <summary>
    /// Bag for Scheduling Provider Location entity.
    /// </summary>
    public class SchedulingProviderLocationBag : EntityBagBase
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the external identifier.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the scheduling provider.
        /// </summary>
        public ListItemBag SchedulingProvider { get; set; }

        /// <summary>
        /// Gets or sets the scheduling provider identifier.
        /// </summary>
        public int? SchedulingProviderId { get; set; }

        /// <summary>
        /// Gets or sets the scheduling provider identifier.
        /// </summary>
        public int? LocationId { get; set; }
    }
}