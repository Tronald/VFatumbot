
using Imgur.API.Authentication;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API.RequestBuilders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Custom Gallery related actions.</summary>
  public class CustomGalleryEndpoint : EndpointBase, ICustomGalleryEndpoint, IEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the CustomGalleryEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public CustomGalleryEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the CustomGalleryEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal CustomGalleryEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal CustomGalleryRequestBuilder RequestBuilder { get; } = new CustomGalleryRequestBuilder();

    /// <summary>
    ///     Add tags to a user's custom gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="tags">The tags that should be added.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> AddCustomGalleryTagsAsync(IEnumerable<string> tags)
    {
      if (tags == null)
        throw new ArgumentNullException(nameof (tags));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "g/custom/add_tags";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.AddCustomGalleryTagsRequest(url, tags))
      {
        bool? nullable = await this.SendRequestAsync<bool?>(request).ConfigureAwait(false);
        bool? added = nullable;
        nullable = new bool?();
        bool? nullable1 = added;
        flag = !nullable1.HasValue || nullable1.GetValueOrDefault();
      }
      return flag;
    }

    /// <summary>
    ///     Add tags to filter out.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="tag">The tag that should be filtered out.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> AddFilteredOutGalleryTagAsync(string tag)
    {
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "g/block_tag";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.AddFilteredOutGalleryTagRequest(url, tag))
      {
        bool? nullable = await this.SendRequestAsync<bool?>(request).ConfigureAwait(false);
        bool? added = nullable;
        nullable = new bool?();
        bool? nullable1 = added;
        flag = !nullable1.HasValue || nullable1.GetValueOrDefault();
      }
      return flag;
    }

    /// <summary>
    ///     View images for current user's custom gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Viral</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Week</param>
    /// <param name="page">Set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<ICustomGallery> GetCustomGalleryAsync(
      CustomGallerySortOrder? sort = CustomGallerySortOrder.Viral,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      CustomGallerySortOrder? nullable1 = sort;
      sort = new CustomGallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : CustomGallerySortOrder.Viral);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("g/custom/{0}/{1}/{2}", (object) sortValue, (object) windowValue, (object) page);
      ICustomGallery customGallery;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        CustomGallery gallery = await this.SendRequestAsync<CustomGallery>(request).ConfigureAwait(false);
        customGallery = (ICustomGallery) gallery;
      }
      return customGallery;
    }

    /// <summary>
    ///     View a single item in a user's custom gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryItem> GetCustomGalleryItemAsync(string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("g/custom/{0}", (object) galleryItemId);
      IGalleryItem galleryItem1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryItem galleryItem = await this.SendRequestAsync<GalleryItem>(request).ConfigureAwait(false);
        galleryItem1 = (IGalleryItem) galleryItem;
      }
      return galleryItem1;
    }

    /// <summary>
    ///     Retrieve user's filtered out gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Viral</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Week</param>
    /// <param name="page">Set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<ICustomGallery> GetFilteredOutGalleryAsync(
      CustomGallerySortOrder? sort = CustomGallerySortOrder.Viral,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      CustomGallerySortOrder? nullable1 = sort;
      sort = new CustomGallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : CustomGallerySortOrder.Viral);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("g/filtered/{0}/{1}/{2}", (object) sortValue, (object) windowValue, (object) page);
      ICustomGallery customGallery;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        CustomGallery gallery = await this.SendRequestAsync<CustomGallery>(request).ConfigureAwait(false);
        customGallery = (ICustomGallery) gallery;
      }
      return customGallery;
    }

    /// <summary>
    ///     Remove tags from a custom gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="tags">The tags that should be removed.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> RemoveCustomGalleryTagsAsync(IEnumerable<string> tags)
    {
      if (tags == null)
        throw new ArgumentNullException(nameof (tags));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "g/custom/remove_tags";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.RemoveCustomGalleryTagsRequest(url, tags))
      {
        bool? nullable = await this.SendRequestAsync<bool?>(request).ConfigureAwait(false);
        bool? removed = nullable;
        nullable = new bool?();
        bool? nullable1 = removed;
        flag = !nullable1.HasValue || nullable1.GetValueOrDefault();
      }
      return flag;
    }

    /// <summary>
    ///     Remove a filtered out tag.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="tag">The tag that should be removed.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> RemoveFilteredOutGalleryTagAsync(string tag)
    {
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "g/unblock_tag";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.RemoveFilteredOutGalleryTagRequest(url, tag))
      {
        bool? nullable = await this.SendRequestAsync<bool?>(request).ConfigureAwait(false);
        bool? removed = nullable;
        nullable = new bool?();
        bool? nullable1 = removed;
        flag = !nullable1.HasValue || nullable1.GetValueOrDefault();
      }
      return flag;
    }
  }
}
