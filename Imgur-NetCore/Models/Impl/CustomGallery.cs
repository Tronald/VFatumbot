
using Imgur.API.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Imgur.API.Models.Impl
{
  /// <summary>A user's custom or filtered gallery.</summary>
  public class CustomGallery : ICustomGallery, IDataModel
  {
    /// <summary>
    ///     Username of the account that created the custom gallery.
    /// </summary>
    [JsonProperty("account_url")]
    public virtual string AccountUrl { get; set; }

    /// <summary>
    ///     The total number of gallery items in the custom gallery.
    /// </summary>
    [JsonProperty("item_count")]
    public virtual int ItemCount { get; set; }

    /// <summary>
    ///     A list of all the gallery items in the custom gallery.
    /// </summary>
    [JsonConverter(typeof (TypeConverter<IEnumerable<GalleryItem>>))]
    public virtual IEnumerable<IGalleryItem> Items { get; set; } = (IEnumerable<IGalleryItem>) new List<IGalleryItem>();

    /// <summary>The URL link to the custom gallery.</summary>
    public virtual string Link { get; set; }

    /// <summary>
    ///     An list of all the tag names in the custom gallery.
    /// </summary>
    public virtual IEnumerable<string> Tags { get; set; } = (IEnumerable<string>) new List<string>();
  }
}
