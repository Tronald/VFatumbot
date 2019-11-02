
using Imgur.API.Authentication;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API.RequestBuilders;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Image related actions.</summary>
  public class ImageEndpoint : EndpointBase, IImageEndpoint, IEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the ImageEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public ImageEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the ImageEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal ImageEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal ImageRequestBuilder RequestBuilder { get; } = new ImageRequestBuilder();

    /// <summary>
    ///     Deletes an image. For an anonymous image, {id} must be the image's deletehash.
    ///     If the image belongs to your account then passing the ID of the image is sufficient.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteImageAsync(string imageId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      string url = string.Format("image/{0}", (object) imageId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Favorite an image with the given ID. OAuth authentication required.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> FavoriteImageAsync(string imageId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("image/{0}/favorite", (object) imageId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
      {
        string imgurResult = await this.SendRequestAsync<string>(request).ConfigureAwait(false);
        flag = imgurResult.Equals("favorited", StringComparison.OrdinalIgnoreCase);
      }
      return flag;
    }

    /// <summary>Get information about an image.</summary>
    /// <param name="imageId">The image id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> GetImageAsync(string imageId)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      string url = string.Format("image/{0}", (object) imageId);
      IImage image1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Image image = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image1 = (IImage) image;
      }
      return image1;
    }

    /// <summary>
    ///     Updates the title or description of an image.
    ///     You can only update an image you own and is associated with your account.
    ///     For an anonymous image, {id} must be the image's deletehash.
    /// </summary>
    /// <param name="imageId">The image id.</param>
    /// <param name="title">The title of the image.</param>
    /// <param name="description">The description of the image.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> UpdateImageAsync(string imageId, string title = null, string description = null)
    {
      if (string.IsNullOrWhiteSpace(imageId))
        throw new ArgumentNullException(nameof (imageId));
      string url = string.Format("image/{0}", (object) imageId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.UpdateImageRequest(url, title, description))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Upload a new image using a binary file.</summary>
    /// <param name="image">A binary file.</param>
    /// <param name="albumId">
    ///     The id of the album you want to add the image to. For anonymous albums, {albumId} should be the
    ///     deletehash that is returned at creation.
    /// </param>
    /// <param name="title">The title of the image.</param>
    /// <param name="description">The description of the image.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> UploadImageBinaryAsync(
      byte[] image,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (image == null)
        throw new ArgumentNullException(nameof (image));
      string url = nameof (image);
      IImage image1;
      using (HttpRequestMessage request = this.RequestBuilder.UploadImageBinaryRequest(url, image, albumId, title, description))
      {
        Image returnImage = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image1 = (IImage) returnImage;
      }
      return image1;
    }

    /// <summary>Upload a new image using a stream.</summary>
    /// <param name="image">A stream.</param>
    /// <param name="albumId">
    ///     The id of the album you want to add the image to. For anonymous albums, {albumId} should be the
    ///     deletehash that is returned at creation.
    /// </param>
    /// <param name="title">The title of the image.</param>
    /// <param name="description">The description of the image.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> UploadImageStreamAsync(
      Stream image,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (image == null)
        throw new ArgumentNullException(nameof (image));
      string url = nameof (image);
      IImage image1;
      using (HttpRequestMessage request = this.RequestBuilder.UploadImageStreamRequest(url, image, albumId, title, description))
      {
        Image returnImage = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image1 = (IImage) returnImage;
      }
      return image1;
    }

    /// <summary>Upload a new image using a URL.</summary>
    /// <param name="image">The URL for the image.</param>
    /// <param name="albumId">
    ///     The id of the album you want to add the image to. For anonymous albums, {albumId} should be the
    ///     deletehash that is returned at creation.
    /// </param>
    /// <param name="title">The title of the image.</param>
    /// <param name="description">The description of the image.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IImage> UploadImageUrlAsync(
      string image,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (string.IsNullOrWhiteSpace(image))
        throw new ArgumentNullException(nameof (image));
      string url = nameof (image);
      IImage image1;
      using (HttpRequestMessage request = this.RequestBuilder.UploadImageUrlRequest(url, image, albumId, title, description))
      {
        Image returnImage = await this.SendRequestAsync<Image>(request).ConfigureAwait(false);
        image1 = (IImage) returnImage;
      }
      return image1;
    }
  }
}
