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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock.Data;
using Rock.Model;
namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Resource
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_Resource" )]
    [DataContract]
    public class Resource : Rock.Data.Model<Resource>, Rock.Data.IRockEntity, Rock.Data.ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        [Required]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>The category id.</value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the approval group identifier.
        /// </summary>
        /// <value>The approval group identifier.</value>
        [DataMember]
        public int? ApprovalGroupId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>The quantity.</value>
        [DataMember]
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [DataMember]
        [MaxLength( 2000 )]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        [DataMember]
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the  photo identifier.
        /// </summary>
        /// <value>The  photo identifier.</value>
        [DataMember]
        public int? PhotoId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>The campus.</value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the approval group.
        /// </summary>
        /// <value>The approval group.</value>
        [LavaInclude]
        public virtual Group ApprovalGroup { get; set; }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <value>The photo URL.</value>
        [LavaInclude]
        [NotMapped]
        public virtual string PhotoUrl
        {
            get
            {
                return Resource.GetPhotoUrl( this );
            }

            private set
            {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets or sets the photo.
        /// </summary>
        /// <value>The photo.</value>
        [DataMember]
        public virtual BinaryFile Photo { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name (and Id) that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> containing the Name (and Id) that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format( "{0} ({1})", Name, Id );
        }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetPhotoUrl( Resource resource, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPhotoUrl( resource.Id, resource.PhotoId, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetPhotoUrl( int resourceId, int? maxWidth = null, int? maxHeight = null )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                Resource resource = new ResourceService( rockContext ).Get( resourceId );
                return GetPhotoUrl( resource, maxWidth, maxHeight );
            }
        }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="PhotoId">The photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>System.String.</returns>
        public static string GetPhotoUrl( int? resourceId, int? PhotoId, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = string.Empty;
            if ( PhotoId.HasValue )
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

                virtualPath = string.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, PhotoId );

                if ( System.Web.HttpContext.Current == null )
                {
                    return virtualPath;
                }
                else
                {
                    return VirtualPathUtility.ToAbsolute( virtualPath );
                }
            }

            return virtualPath;
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public static string GetPhotoImageTag( Resource resource, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            if ( resource != null )
            {
                return GetPhotoImageTag( resource.Id, resource.PhotoId, maxWidth, maxHeight, altText, className );
            }
            else
            {
                return GetPhotoImageTag( null, null, maxWidth, maxHeight, altText, className );
            }

        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="PhotoId">The photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public static string GetPhotoImageTag( int? resourceId, int? PhotoId, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? string.Empty :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? string.Empty :
                string.Format( " class='{0}'", className );

            if ( PhotoId.HasValue )
            {
                photoUrl.AppendFormat( "GetImage.ashx?id={0}", PhotoId );
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

        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration for the Resource model
    /// </summary>
    public partial class ResourceConfiguration : EntityTypeConfiguration<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceConfiguration" /> class.
        /// </summary>
        public ResourceConfiguration()
        {
            this.HasRequired( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ApprovalGroup ).WithMany().HasForeignKey( r => r.ApprovalGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Resource" );
        }
    }

    #endregion

}
