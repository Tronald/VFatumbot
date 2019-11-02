
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
  /// <summary>Topic related actions.</summary>
  public class TopicEndpoint : EndpointBase, ITopicEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the TopicEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public TopicEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the TopicEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal TopicEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal TopicRequestBuilder RequestBuilder { get; } = new TopicRequestBuilder();

    /// <summary>Get the list of default topics.</summary>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<ITopic>> GetDefaultTopicsAsync()
    {
      string url = "topics/defaults";
      IEnumerable<ITopic> topics1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Topic> topics = await this.SendRequestAsync<IEnumerable<Topic>>(request).ConfigureAwait(false);
        topics1 = (IEnumerable<ITopic>) topics;
      }
      return topics1;
    }

    /// <summary>View a single item in a gallery topic.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="topicId">
    ///     The ID or URL-formatted name of the topic. If using a topic's name, replace its spaces with
    ///     underscores (Mother's_Day).
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryItem> GetGalleryTopicItemAsync(
      string galleryItemId,
      string topicId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(topicId))
        throw new ArgumentNullException(nameof (topicId));
      string url = string.Format("topics/{0}/{1}", (object) topicId.Replace(" ", "_"), (object) galleryItemId);
      IGalleryItem galleryItem;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryItem items = await this.SendRequestAsync<GalleryItem>(request).ConfigureAwait(false);
        galleryItem = (IGalleryItem) items;
      }
      return galleryItem;
    }

    /// <summary>View gallery items for a topic.</summary>
    /// <param name="topicId">
    ///     The ID or URL-formatted name of the topic. If using a topic's name, replace its spaces with
    ///     underscores (Mother's_Day).
    /// </param>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Viral</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Week</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetGalleryTopicItemsAsync(
      string topicId,
      CustomGallerySortOrder? sort = CustomGallerySortOrder.Viral,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(topicId))
        throw new ArgumentNullException(nameof (topicId));
      CustomGallerySortOrder? nullable1 = sort;
      sort = new CustomGallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : CustomGallerySortOrder.Viral);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("topics/{0}/{1}/{2}/{3}", (object) topicId.Replace(" ", "_"), (object) sortValue, (object) windowValue, (object) page);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> items = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) items;
      }
      return galleryItems;
    }
  }
}
