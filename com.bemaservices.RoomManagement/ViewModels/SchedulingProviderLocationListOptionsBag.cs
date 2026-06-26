using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace com.bemaservices.RoomManagement.ViewModels
{
    /// <summary>
    /// Options bag for the Scheduling Provider Location List block.
    /// </summary>
    public class SchedulingProviderLocationListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should be visible to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a valid DefinedType exists; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }        

        /// <summary>
        /// Gets or sets the name of the defined type.
        /// </summary>
        /// <value>
        /// The name of the defined type.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string LocationId { get; set; }

        /// <summary>
        /// Gets or sets the list of available scheduling providers.
        /// </summary>
        /// <value>
        /// The list of available scheduling providers.
        /// </value>
        public List<ListItemBag> SchedulingProviders { get; set; }
    }
}
