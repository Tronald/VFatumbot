
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
  /// <summary>Gallery related actions.</summary>
  public class GalleryEndpoint : EndpointBase, IGalleryEndpoint
  {
    /// <summary>
    ///     Get additional information about an album in the gallery.
    /// </summary>
    /// <param name="albumId">The album id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryAlbum> GetGalleryAlbumAsync(string albumId)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("gallery/album/{0}", (object) albumId);
      IGalleryAlbum galleryAlbum;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryAlbum album = await this.SendRequestAsync<GalleryAlbum>(request).ConfigureAwait(false);
        galleryAlbum = (IGalleryAlbum) album;
      }
      return galleryAlbum;
    }

    internal CommentRequestBuilder CommentRequestBuilder { get; } = new CommentRequestBuilder();

    /// <summary>
    ///     Create a comment for an item. OAuth authentication required.
    /// </summary>
    /// <param name="comment">The text of the comment.</param>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> CreateGalleryItemCommentAsync(string comment, string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("gallery/{0}/comment", (object) galleryItemId);
      int id;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateGalleryItemCommentRequest(url, comment))
      {
        Comment returnComment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        id = returnComment.Id;
      }
      return id;
    }

    /// <summary>
    ///     Reply to a comment that has been created for an item. OAuth authentication required.
    /// </summary>
    /// <param name="comment">The text of the comment.</param>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="parentId">The comment id that you are replying to.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> CreateGalleryItemCommentReplyAsync(
      string comment,
      string galleryItemId,
      string parentId)
    {
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(parentId))
        throw new ArgumentNullException(nameof (parentId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("gallery/{0}/comment/{1}", (object) galleryItemId, (object) parentId);
      int id;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateGalleryItemCommentRequest(url, comment))
      {
        Comment returnComment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        id = returnComment.Id;
      }
      return id;
    }

    /// <summary>Get information about a specific comment.</summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IComment> GetGalleryItemCommentAsync(
      int commentId,
      string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      string url = string.Format("gallery/{0}/comment/{1}", (object) galleryItemId, (object) commentId);
      IComment comment1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Comment comment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        comment1 = (IComment) comment;
      }
      return comment1;
    }

    /// <summary>The number of comments on an item.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> GetGalleryItemCommentCountAsync(string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      string url = string.Format("gallery/{0}/comments/count", (object) galleryItemId);
      int num;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
        num = await this.SendRequestAsync<int>(request).ConfigureAwait(false);
      return num;
    }

    /// <summary>List all of the IDs for the comments on an item.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<int>> GetGalleryItemCommentIdsAsync(
      string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      string url = string.Format("gallery/{0}/comments/ids", (object) galleryItemId);
      IEnumerable<int> ints;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<int> commentIds = await this.SendRequestAsync<IEnumerable<int>>(request).ConfigureAwait(false);
        ints = commentIds;
      }
      return ints;
    }

    /// <summary>Get all comments for a gallery item.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="sort">The order that comments should be sorted by.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IComment>> GetGalleryItemCommentsAsync(
      string galleryItemId,
      CommentSortOrder? sort = CommentSortOrder.Best)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      CommentSortOrder? nullable = sort;
      sort = new CommentSortOrder?(nullable.HasValue ? nullable.GetValueOrDefault() : CommentSortOrder.Best);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string url = string.Format("gallery/{0}/comments/{1}", (object) galleryItemId, (object) sortValue);
      IEnumerable<IComment> comments1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Comment> comments = await this.SendRequestAsync<IEnumerable<Comment>>(request).ConfigureAwait(false);
        comments1 = (IEnumerable<IComment>) comments;
      }
      return comments1;
    }

    /// <summary>
    ///     Initializes a new instance of the GalleryEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public GalleryEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the GalleryEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal GalleryEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal GalleryRequestBuilder RequestBuilder { get; } = new GalleryRequestBuilder();

    /// <summary>Returns the images in the gallery.</summary>
    /// <param name="section">The gallery section. Default: Hot</param>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Viral</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Day</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <param name="showViral">Show or hide viral images from the 'user' section. Default: true</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetGalleryAsync(
      GallerySection? section = GallerySection.Hot,
      GallerySortOrder? sort = GallerySortOrder.Viral,
      TimeWindow? window = TimeWindow.Day,
      int? page = null,
      bool? showViral = true)
    {
      GallerySection? nullable1 = section;
      section = new GallerySection?(nullable1.HasValue ? nullable1.GetValueOrDefault() : GallerySection.Hot);
      GallerySortOrder? nullable2 = sort;
      sort = new GallerySortOrder?(nullable2.HasValue ? nullable2.GetValueOrDefault() : GallerySortOrder.Viral);
      TimeWindow? nullable3 = window;
      window = new TimeWindow?(nullable3.HasValue ? nullable3.GetValueOrDefault() : TimeWindow.Week);
      bool? nullable4 = showViral;
      showViral = new bool?(!nullable4.HasValue || nullable4.GetValueOrDefault());
      string sectionValue = string.Format("{0}", (object) section).ToLower();
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string showViralValue = string.Format("{0}", (object) showViral).ToLower();
      string url = string.Format("gallery/{0}/{1}/{2}/{3}?showViral={4}", (object) sectionValue, (object) sortValue, (object) windowValue, (object) page, (object) showViralValue);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> gallery = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) gallery;
      }
      return galleryItems;
    }

    /// <summary>Returns a random set of gallery images.</summary>
    /// <param name="page">A page of random gallery images, from 0-50. Pages are regenerated every hour.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetRandomGalleryAsync(
      int? page = null)
    {
      string url = string.Format("gallery/random/random/{0}", (object) page);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> gallery = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) gallery;
      }
      return galleryItems;
    }

    /// <summary>
    ///     Share an Album or Image to the Gallery. OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="title">The title of the image. This is required.</param>
    /// <param name="topicId">The topic id - not the topic name.</param>
    /// <param name="bypassTerms">
    ///     If the user has not accepted the terms yet, this endpoint will return an error. To by-pass
    ///     the terms in general simply set this value to true.
    /// </param>
    /// <param name="mature">If the post is mature, set this value to true.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> PublishToGalleryAsync(
      string galleryItemId,
      string title,
      string topicId = null,
      bool? bypassTerms = null,
      bool? mature = null)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(title))
        throw new ArgumentNullException(nameof (title));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("gallery/{0}", (object) galleryItemId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.PublishToGalleryRequest(url, title, topicId, bypassTerms, mature))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Remove an item from the gallery. OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> RemoveFromGalleryAsync(string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("gallery/{0}", (object) galleryItemId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Report an item in the gallery. OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="reason">A reason why content is inappropriate.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> ReportGalleryItemAsync(string galleryItemId, ReportReason reason)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("gallery/{0}/report", (object) galleryItemId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.ReportItemRequest(url, reason))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Search the gallery with a given query string.</summary>
    /// <param name="qAll">Search for all of these words (and).</param>
    /// <param name="qAny">Search for any of these words (or).</param>
    /// <param name="qExactly">Search for exactly this word or phrase.</param>
    /// <param name="qNot">Exclude results matching this word or phrase.</param>
    /// <param name="fileType">Show results for a specific file type.</param>
    /// <param name="imageSize">Show results for a specific image size.</param>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Time</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Day</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> SearchGalleryAdvancedAsync(
      string qAll = null,
      string qAny = null,
      string qExactly = null,
      string qNot = null,
      ImageFileType? fileType = null,
      ImageSize? imageSize = null,
      GallerySortOrder? sort = GallerySortOrder.Time,
      TimeWindow? window = TimeWindow.All,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(qAll) && string.IsNullOrWhiteSpace(qAny) && string.IsNullOrWhiteSpace(qExactly) && string.IsNullOrWhiteSpace(qNot))
        throw new ArgumentNullException((string) null, "At least one search parameter must be provided (All | Any | Exactly | Not).");
      GallerySortOrder? nullable1 = sort;
      sort = new GallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : GallerySortOrder.Time);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.All);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("gallery/search/{0}/{1}/{2}", (object) sortValue, (object) windowValue, (object) page);
      url = this.RequestBuilder.SearchGalleryAdvancedRequest(url, qAll, qAny, qExactly, qNot, fileType, imageSize);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> searchResults = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) searchResults;
      }
      return galleryItems;
    }

    /// <summary>Search the gallery with a given query string.</summary>
    /// <param name="query">
    ///     Query string to search by. This parameter also supports boolean operators (AND, OR, NOT) and
    ///     indices (tag: user: title: ext: subreddit: album: meme:). An example compound query would be 'title: cats AND dogs
    ///     ext: gif'
    /// </param>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Time</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Day</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> SearchGalleryAsync(
      string query,
      GallerySortOrder? sort = GallerySortOrder.Time,
      TimeWindow? window = TimeWindow.All,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(query))
        throw new ArgumentNullException(nameof (query));
      GallerySortOrder? nullable1 = sort;
      sort = new GallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : GallerySortOrder.Time);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.All);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("gallery/search/{0}/{1}/{2}", (object) sortValue, (object) windowValue, (object) page);
      url = this.RequestBuilder.SearchGalleryRequest(url, query);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> searchResults = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) searchResults;
      }
      return galleryItems;
    }

    /// <summary>
    ///     Get additional information about an image in the gallery.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryImage> GetGalleryImageAsync(string imageId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      string url = string.Format("gallery/image/{0}", (object) imageId);
      IGalleryImage galleryImage;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryImage image = await this.SendRequestAsync<GalleryImage>(request).ConfigureAwait(false);
        galleryImage = (IGalleryImage) image;
      }
      return galleryImage;
    }

    /// <summary>View images for memes subgallery.</summary>
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
    public async Task<IEnumerable<IGalleryItem>> GetMemesSubGalleryAsync(
      MemesGallerySortOrder? sort = MemesGallerySortOrder.Viral,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      MemesGallerySortOrder? nullable1 = sort;
      sort = new MemesGallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : MemesGallerySortOrder.Viral);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("g/memes/{0}/{1}/{2}", (object) sortValue, (object) windowValue, (object) page);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> gallery = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) gallery;
      }
      return galleryItems;
    }

    /// <summary>View a single image in the memes gallery.</summary>
    /// <param name="imageId">The image id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryImage> GetMemesSubGalleryImageAsync(string imageId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      string url = string.Format("gallery/image/{0}", (object) imageId);
      IGalleryImage galleryImage;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryImage image = await this.SendRequestAsync<GalleryImage>(request).ConfigureAwait(false);
        galleryImage = (IGalleryImage) image;
      }
      return galleryImage;
    }

    /// <summary>View gallery images for a subreddit.</summary>
    /// <param name="subreddit">A valid subreddit name. Example: pics, gaming</param>
    /// <param name="sort">The order that the gallery should be sorted by. Default: Time</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Week</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryImage>> GetSubredditGalleryAsync(
      string subreddit,
      SubredditGallerySortOrder? sort = SubredditGallerySortOrder.Time,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(subreddit))
        throw new ArgumentNullException(nameof (subreddit));
      SubredditGallerySortOrder? nullable1 = sort;
      sort = new SubredditGallerySortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : SubredditGallerySortOrder.Time);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("gallery/r/{0}/{1}/{2}/{3}", (object) subreddit, (object) sortValue, (object) windowValue, (object) page);
      IEnumerable<IGalleryImage> galleryImages;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryImage> gallery = await this.SendRequestAsync<IEnumerable<GalleryImage>>(request).ConfigureAwait(false);
        galleryImages = (IEnumerable<IGalleryImage>) gallery;
      }
      return galleryImages;
    }

    /// <summary>View a single image in the subreddit.</summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="subreddit">A valid subreddit name. Example: pics, gaming</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryImage> GetSubredditImageAsync(
      string imageId,
      string subreddit)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      if (string.IsNullOrWhiteSpace(subreddit))
        throw new ArgumentNullException(nameof (subreddit));
      string url = string.Format("gallery/r/{0}/{1}", (object) subreddit, (object) imageId);
      IGalleryImage galleryImage;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryImage image = await this.SendRequestAsync<GalleryImage>(request).ConfigureAwait(false);
        galleryImage = (IGalleryImage) image;
      }
      return galleryImage;
    }

    /// <summary>View tags for a gallery item.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<ITagVotes> GetGalleryItemTagsAsync(string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      string url = string.Format("gallery/{0}/tags", (object) galleryItemId);
      ITagVotes tagVotes1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        TagVotes tagVotes = await this.SendRequestAsync<TagVotes>(request).ConfigureAwait(false);
        tagVotes1 = (ITagVotes) tagVotes;
      }
      return tagVotes1;
    }

    /// <summary>View images for a gallery tag.</summary>
    /// <param name="tag">The name of the tag.</param>
    /// <param name="sort">The order that the images in the gallery tag should be sorted by. Default: Viral</param>
    /// <param name="window">The time period that should be used in filtering requests. Default: Week</param>
    /// <param name="page">The data paging number. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<ITag> GetGalleryTagAsync(
      string tag,
      GalleryTagSortOrder? sort = GalleryTagSortOrder.Viral,
      TimeWindow? window = TimeWindow.Week,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      GalleryTagSortOrder? nullable1 = sort;
      sort = new GalleryTagSortOrder?(nullable1.HasValue ? nullable1.GetValueOrDefault() : GalleryTagSortOrder.Viral);
      TimeWindow? nullable2 = window;
      window = new TimeWindow?(nullable2.HasValue ? nullable2.GetValueOrDefault() : TimeWindow.Week);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string windowValue = string.Format("{0}", (object) window).ToLower();
      string url = string.Format("gallery/t/{0}/{1}/{2}/{3}", (object) tag, (object) sortValue, (object) windowValue, (object) page);
      ITag tag1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Tag returnTag = await this.SendRequestAsync<Tag>(request).ConfigureAwait(false);
        tag1 = (ITag) returnTag;
      }
      return tag1;
    }

    /// <summary>View a single item in a gallery tag.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="tag">The name of the tag.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryItem> GetGalleryTagImageAsync(
      string galleryItemId,
      string tag)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      string url = string.Format("gallery/t/{0}/{1}", (object) tag, (object) galleryItemId);
      IGalleryItem galleryItem;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryImage image = await this.SendRequestAsync<GalleryImage>(request).ConfigureAwait(false);
        galleryItem = (IGalleryItem) image;
      }
      return galleryItem;
    }

    /// <summary>
    ///     Vote for a tag. Send the same value again to undo a vote. OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="tag">Name of the tag (implicitly created, if doesn't exist).</param>
    /// <param name="vote">The vote.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> VoteGalleryTagAsync(
      string galleryItemId,
      string tag,
      VoteOption vote)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string voteValue = string.Format("{0}", (object) vote).ToLower();
      string url = string.Format("gallery/{0}/vote/tag/{1}/{2}", (object) galleryItemId, (object) tag, (object) voteValue);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Get the vote information about an image.</summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IVote> GetGalleryItemVotesAsync(string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      string url = string.Format("gallery/{0}/votes", (object) galleryItemId);
      IVote vote1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Vote vote = await this.SendRequestAsync<Vote>(request).ConfigureAwait(false);
        vote1 = (IVote) vote;
      }
      return vote1;
    }

    /// <summary>
    ///     Vote for an item. Send the same value again to undo a vote. OAuth authentication required.
    /// </summary>
    /// <param name="galleryItemId">The gallery item id.</param>
    /// <param name="vote">The vote.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> VoteGalleryItemAsync(string galleryItemId, VoteOption vote)
    {
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string voteValue = string.Format("{0}", (object) vote).ToLower();
      string url = string.Format("gallery/{0}/vote/{1}", (object) galleryItemId, (object) voteValue);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }
  }
}
