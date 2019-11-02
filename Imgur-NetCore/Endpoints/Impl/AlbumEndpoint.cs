
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
  /// <summary>Album related actions.</summary>
  public class AlbumEndpoint : EndpointBase, IAlbumEndpoint, IEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the AlbumEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public AlbumEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the AlbumEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal AlbumEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal AlbumRequestBuilder RequestBuilder { get; } = new AlbumRequestBuilder();

    /// <summary>
    ///     Takes a list of imageIds to add to the album. For anonymous albums, {albumId} should be the
    ///     deletehash
    ///     that is returned at creation.
    /// </summary>
    /// <param name="albumId">The id or deletehash of the album.</param>
    /// <param name="imageIds">The imageIds that you want to be added to the album.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> AddAlbumImagesAsync(string albumId, IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      string url = string.Format("album/{0}/add", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.AddAlbumImagesRequest(url, imageIds))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Create a new album.</summary>
    /// <param name="title">The title of the album.</param>
    /// <param name="description">The description of the album.</param>
    /// <param name="privacy">Sets the privacy level of the album.</param>
    /// <param name="layout">Sets the layout to display the album.</param>
    /// <param name="coverId">The Id of an image that you want to be the cover of the album.</param>
    /// <param name="imageIds">The imageIds that you want to be included in the album.</param>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IAlbum> CreateAlbumAsync(
      string title = null,
      string description = null,
      AlbumPrivacy? privacy = null,
      AlbumLayout? layout = null,
      string coverId = null,
      IEnumerable<string> imageIds = null)
    {
      string url = "album";
      IAlbum album1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateAlbumRequest(url, title, description, privacy, layout, coverId, imageIds))
      {
        Album album = await this.SendRequestAsync<Album>(request).ConfigureAwait(false);
        album1 = (IAlbum) album;
      }
      return album1;
    }

    /// <summary>
    ///     Delete an album with a given Id. You are required to be logged in as the user to delete the album. For anonymous
    ///     albums, {albumId} should be the deletehash that is returned at creation.
    /// </summary>
    /// <param name="albumId">The id or deletehash of the album.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteAlbumAsync(string albumId)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("album/{0}", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Favorite an album with a given Id. OAuth authentication required.
    /// </summary>
    /// <param name="albumId">The album id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> FavoriteAlbumAsync(string albumId)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("album/{0}/favorite", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
      {
        string imgurResult = await this.SendRequestAsync<string>(request).ConfigureAwait(false);
        flag = imgurResult.Equals("favorited", StringComparison.OrdinalIgnoreCase);
      }
      return flag;
    }

    /// <summary>Get information about a specific album.</summary>
    /// <param name="albumId">The album id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IAlbum> GetAlbumAsync(string albumId)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("album/{0}", (object) albumId);
      IAlbum album1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Album album = await this.SendRequestAsync<Album>(request).ConfigureAwait(false);
        album1 = (IAlbum) album;
      }
      return album1;
    }

    /// <summary>Get information about an image in an album.</summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="albumId">The album id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> GetAlbumImageAsync(string imageId, string albumId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("album/{0}/image/{1}", (object) albumId, (object) imageId);
      IImage image;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Image returnImage = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image = (IImage) returnImage;
      }
      return image;
    }

    /// <summary>Return all of the images in the album.</summary>
    /// <param name="albumId">The album id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IImage>> GetAlbumImagesAsync(string albumId)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("album/{0}/images", (object) albumId);
      IEnumerable<IImage> images1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Image> images = await this.SendRequestAsync<IEnumerable<Image>>(request).ConfigureAwait(false);
        images1 = (IEnumerable<IImage>) images;
      }
      return images1;
    }

    /// <summary>
    ///     Takes a list of imageIds and removes from the album. For anonymous albums, {albumId} should be the
    ///     deletehash that is returned at creation.
    /// </summary>
    /// <param name="albumId">The id or deletehash of the album.</param>
    /// <param name="imageIds">The imageIds that you want to be removed from the album.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> RemoveAlbumImagesAsync(string albumId, IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      string url = string.Format("album/{0}/remove_images", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.RemoveAlbumImagesRequest(url, imageIds))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Sets the images for an album, removes all other images and only uses the images in this request. For anonymous
    ///     albums, {albumId} should be the deletehash that is returned at creation.
    /// </summary>
    /// <param name="albumId">The id or deletehash of the album.</param>
    /// <param name="imageIds">The imageIds that you want to be added to the album.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> SetAlbumImagesAsync(string albumId, IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      string url = string.Format("album/{0}", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.SetAlbumImagesRequest(url, imageIds))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Update the information of an album. For anonymous albums, {albumId} should be the deletehash that is returned at
    ///     creation.
    /// </summary>
    /// <param name="albumId">The id or deletehash of the album.</param>
    /// <param name="title">The title of the album.</param>
    /// <param name="description">The description of the album.</param>
    /// <param name="privacy">Sets the privacy level of the album.</param>
    /// <param name="layout">Sets the layout to display the album.</param>
    /// <param name="coverId">The Id of an image that you want to be the cover of the album.</param>
    /// <param name="imageIds">The imageIds that you want to be included in the album.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> UpdateAlbumAsync(
      string albumId,
      string title = null,
      string description = null,
      AlbumPrivacy? privacy = null,
      AlbumLayout? layout = null,
      string coverId = null,
      IEnumerable<string> imageIds = null)
    {
      if (string.IsNullOrWhiteSpace(albumId))
        throw new ArgumentNullException(nameof (albumId));
      string url = string.Format("album/{0}", (object) albumId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.UpdateAlbumRequest(url, title, description, privacy, layout, coverId, imageIds))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }
  }
}
