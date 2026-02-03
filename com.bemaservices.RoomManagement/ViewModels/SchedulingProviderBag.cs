using Rock.ViewModels.Utility;

namespace com.bemaservices.RoomManagement.ViewModels
{
    /// <summary>
    /// The item details for the Scheduling Provider Detail block.
    /// </summary>
    public class SchedulingProviderBag : EntityBagBase
    {
        public string Description { get; set; }

        public ListItemBag EntityType { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }
    }
}
