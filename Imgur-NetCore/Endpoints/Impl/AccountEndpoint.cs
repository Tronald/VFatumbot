
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
  /// <summary>Account related actions.</summary>
  public class AccountEndpoint : EndpointBase, IAccountEndpoint, IEndpoint
  {
    internal AlbumRequestBuilder AlbumRequestBuilder { get; } = new AlbumRequestBuilder();

    /// <summary>
    ///     Delete an Album with a given id. OAuth authentication required.
    /// </summary>
    /// <param name="albumId">The album id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteAlbumAsync(string albumId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/album/{1}", (object) username, (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.AlbumRequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Get additional information about an album, this works the same as the Album Endpoint.
    /// </summary>
    /// <param name="albumId">The album id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IAlbum> GetAlbumAsync(string albumId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/album/{1}", (object) username, (object) albumId);
      IAlbum album1;
      using (HttpRequestMessage request = this.AlbumRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Album album = await this.SendRequestAsync<Album>(request).ConfigureAwait(false);
        album1 = (IAlbum) album;
      }
      return album1;
    }

    /// <summary>Return a list of all of the album IDs.</summary>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> GetAlbumCountAsync(string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/albums/count", (object) username);
      int num;
      using (HttpRequestMessage request = this.AlbumRequestBuilder.CreateRequest(HttpMethod.Get, url))
        num = await this.SendRequestAsync<int>(request).ConfigureAwait(false);
      return num;
    }

    /// <summary>Return a list of all of the album IDs.</summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetAlbumIdsAsync(
      string username = "me",
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/albums/ids/{1}", (object) username, (object) page);
      IEnumerable<string> strings;
      using (HttpRequestMessage request = this.AlbumRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<string> albums = await this.SendRequestAsync<IEnumerable<string>>(request).ConfigureAwait(false);
        strings = albums;
      }
      return strings;
    }

    /// <summary>
    ///     Get all the albums associated with the account.
    ///     Must be logged in as the user to see secret and hidden albums.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IAlbum>> GetAlbumsAsync(
      string username = "me",
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/albums/{1}", (object) username, (object) page);
      IEnumerable<IAlbum> albums1;
      using (HttpRequestMessage request = this.AlbumRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Album> albums = await this.SendRequestAsync<IEnumerable<Album>>(request).ConfigureAwait(false);
        albums1 = (IEnumerable<IAlbum>) albums;
      }
      return albums1;
    }

    internal CommentRequestBuilder CommentRequestBuilder { get; } = new CommentRequestBuilder();

    /// <summary>Delete a comment. OAuth authentication required.</summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteCommentAsync(int commentId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/comment/{1}", (object) username, (object) commentId);
      bool flag;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Return information about a specific comment.</summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IComment> GetCommentAsync(int commentId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/comment/{1}", (object) username, (object) commentId);
      IComment comment1;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Comment comment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        comment1 = (IComment) comment;
      }
      return comment1;
    }

    /// <summary>
    ///     Return a count of all of the comments associated with the account.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> GetCommentCountAsync(string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/comments/count", (object) username);
      int num;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateRequest(HttpMethod.Get, url))
        num = await this.SendRequestAsync<int>(request).ConfigureAwait(false);
      return num;
    }

    /// <summary>Return a list of all of the comment IDs.</summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="sort">The order that the comments should be sorted by. Default: Newest</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<int>> GetCommentIdsAsync(
      string username = "me",
      CommentSortOrder? sort = CommentSortOrder.Newest,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      CommentSortOrder? nullable = sort;
      sort = new CommentSortOrder?(nullable.HasValue ? nullable.GetValueOrDefault() : CommentSortOrder.Newest);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string url = string.Format("account/{0}/comments/ids/{1}/{2}", (object) username, (object) sortValue, (object) page);
      IEnumerable<int> ints;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<int> comments = await this.SendRequestAsync<IEnumerable<int>>(request).ConfigureAwait(false);
        ints = comments;
      }
      return ints;
    }

    /// <summary>Return the comments the user has created.</summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="sort">The order that the comments should be sorted by. Default: Newest</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IComment>> GetCommentsAsync(
      string username = "me",
      CommentSortOrder? sort = CommentSortOrder.Newest,
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      CommentSortOrder? nullable = sort;
      sort = new CommentSortOrder?(nullable.HasValue ? nullable.GetValueOrDefault() : CommentSortOrder.Newest);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string url = string.Format("account/{0}/comments/{1}/{2}", (object) username, (object) sortValue, (object) page);
      IEnumerable<IComment> comments1;
      using (HttpRequestMessage request = this.CommentRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Comment> comments = await this.SendRequestAsync<IEnumerable<Comment>>(request).ConfigureAwait(false);
        comments1 = (IEnumerable<IComment>) comments;
      }
      return comments1;
    }

    /// <summary>
    ///     Initializes a new instance of the AccountEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public AccountEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the AccountEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal AccountEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal AccountRequestBuilder RequestBuilder { get; } = new AccountRequestBuilder();

    /// <summary>
    ///     Request standard user information.
    ///     If you need the username for the account that is logged in, it is returned in the request for an access token.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IAccount> GetAccountAsync(string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}", (object) username);
      IAccount account1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Account account = await this.SendRequestAsync<Account>(request).ConfigureAwait(false);
        account1 = (IAccount) account;
      }
      return account1;
    }

    /// <summary>
    ///     Returns the account settings. OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IAccountSettings> GetAccountSettingsAsync()
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "account/me/settings";
      IAccountSettings accountSettings;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        AccountSettings settings = await this.SendRequestAsync<AccountSettings>(request).ConfigureAwait(false);
        accountSettings = (IAccountSettings) settings;
      }
      return accountSettings;
    }

    /// <summary>
    ///     Sends an email to the user to verify that their email is valid to upload to gallery.
    ///     OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> SendVerificationEmailAsync()
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "account/me/verifyemail";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Updates the account settings for a given user. OAuth authentication required.
    /// </summary>
    /// <param name="bio">The biography of the user, is displayed in the gallery profile page.</param>
    /// <param name="publicImages">Set the users images to private or public by default.</param>
    /// <param name="messagingEnabled">Allows the user to enable / disable private messages.</param>
    /// <param name="albumPrivacy">Sets the default privacy level of albums the users creates.</param>
    /// <param name="acceptedGalleryTerms">The user agreement to the Imgur Gallery terms.</param>
    /// <param name="username">A valid Imgur username (between 4 and 63 alphanumeric characters).</param>
    /// <param name="showMature">Toggle display of mature images in gallery list endpoints.</param>
    /// <param name="newsletterSubscribed">Toggle subscription to email newsletter.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> UpdateAccountSettingsAsync(
      string bio = null,
      bool? publicImages = null,
      bool? messagingEnabled = null,
      AlbumPrivacy? albumPrivacy = null,
      bool? acceptedGalleryTerms = null,
      string username = null,
      bool? showMature = null,
      bool? newsletterSubscribed = null)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "account/me/settings";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.UpdateAccountSettingsRequest(url, bio, publicImages, messagingEnabled, albumPrivacy, acceptedGalleryTerms, username, showMature, newsletterSubscribed))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Checks to see if user has verified their email address. OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> VerifyEmailAsync()
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "account/me/verifyemail";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    internal GalleryRequestBuilder GalleryRequestBuilder { get; } = new GalleryRequestBuilder();

    /// <summary>
    ///     Returns the users favorited images. OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetAccountFavoritesAsync()
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "account/me/favorites";
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> favorites = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) favorites;
      }
      return galleryItems;
    }

    /// <summary>
    ///     Return the images the user has favorited in the gallery.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <param name="sort">The order that the account gallery should be sorted by. Default: Newest</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetAccountGalleryFavoritesAsync(
      string username = "me",
      int? page = null,
      AccountGallerySortOrder? sort = AccountGallerySortOrder.Newest)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      AccountGallerySortOrder? nullable = sort;
      sort = new AccountGallerySortOrder?(nullable.HasValue ? nullable.GetValueOrDefault() : AccountGallerySortOrder.Newest);
      string sortValue = string.Format("{0}", (object) sort).ToLower();
      string url = string.Format("account/{0}/gallery_favorites/{1}/{2}", (object) username, (object) page, (object) sortValue);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> favorites = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) favorites;
      }
      return galleryItems;
    }

    /// <summary>
    ///     Return the images a user has submitted to the gallery.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IGalleryItem>> GetAccountSubmissionsAsync(
      string username = "me",
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/submissions/{1}", (object) username, (object) page);
      IEnumerable<IGalleryItem> galleryItems;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<GalleryItem> submissions = await this.SendRequestAsync<IEnumerable<GalleryItem>>(request).ConfigureAwait(false);
        galleryItems = (IEnumerable<IGalleryItem>) submissions;
      }
      return galleryItems;
    }

    /// <summary>The totals for a users gallery information.</summary>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IGalleryProfile> GetGalleryProfileAsync(string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/gallery_profile", (object) username);
      IGalleryProfile galleryProfile;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        GalleryProfile profile = await this.SendRequestAsync<GalleryProfile>(request).ConfigureAwait(false);
        galleryProfile = (IGalleryProfile) profile;
      }
      return galleryProfile;
    }

    internal ImageRequestBuilder ImageRequestBuilder { get; } = new ImageRequestBuilder();

    /// <summary>
    ///     Deletes an Image. You are required to be logged in as the user whom created the image.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteImageAsync(string imageId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/image/{1}", (object) username, (object) imageId);
      bool flag;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Return information about a specific image.</summary>
    /// <param name="imageId">The image's id.</param>
    /// <param name="username">The user account. Default: me</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> GetImageAsync(string imageId, string username = "me")
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (username.Equals("me", StringComparison.OrdinalIgnoreCase) && this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/image/{1}", (object) username, (object) imageId);
      IImage image1;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Image image = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image1 = (IImage) image;
      }
      return image1;
    }

    /// <summary>
    ///     Returns the total number of images associated with the account.
    ///     OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> GetImageCountAsync(string username = "me")
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/images/count", (object) username);
      int num;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
        num = await this.SendRequestAsync<int>(request).ConfigureAwait(false);
      return num;
    }

    /// <summary>
    ///     Returns a list of Image IDs that are associated with the account.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetImageIdsAsync(
      string username = "me",
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/images/ids/{1}", (object) username, (object) page);
      IEnumerable<string> strings;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<string> images = await this.SendRequestAsync<IEnumerable<string>>(request).ConfigureAwait(false);
        strings = images;
      }
      return strings;
    }

    /// <summary>
    ///     Return all of the images associated with the account.
    ///     You can page through the images by setting the page, this defaults to 0.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="username">The user account. Default: me</param>
    /// <param name="page">Allows you to set the page number so you don't have to retrieve all the data at once. Default: null</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IImage>> GetImagesAsync(
      string username = "me",
      int? page = null)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("account/{0}/images/{1}", (object) username, (object) page);
      IEnumerable<IImage> images1;
      using (HttpRequestMessage request = this.ImageRequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Image> images = await this.SendRequestAsync<IEnumerable<Image>>(request).ConfigureAwait(false);
        images1 = (IEnumerable<IImage>) images;
      }
      return images1;
    }

    /// <summary>
    ///     Returns all of the reply notifications for the user.
    ///     OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <param name="newNotifications">false for all notifications, true for only non-viewed notification. Default is true.</param>
    /// <returns></returns>
    public async Task<INotifications> GetNotificationsAsync(bool newNotifications = true)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string newNotificationsValue = string.Format("{0}", (object) newNotifications).ToLower();
      string url = string.Format("account/me/notifications?new={0}", (object) newNotificationsValue);
      INotifications notifications1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Notifications notifications = await this.SendRequestAsync<Notifications>(request).ConfigureAwait(false);
        notifications1 = (INotifications) notifications;
      }
      return notifications1;
    }
  }
}
