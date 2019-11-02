
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class ImageRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UpdateImageRequest(
      string url,
      string title = null,
      string description = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      Dictionary<string, string> source = new Dictionary<string, string>();
      if (title != null)
        source.Add(nameof (title), title);
      if (description != null)
        source.Add(nameof (description), description);
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UploadImageBinaryRequest(
      string url,
      byte[] image,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (image == null)
        throw new ArgumentNullException(nameof (image));
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
      MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent(string.Format("{0}", (object) DateTime.UtcNow.Ticks))
      {
        {
          (HttpContent) new StringContent("file"),
          "type"
        },
        {
          (HttpContent) new ByteArrayContent(image),
          nameof (image)
        }
      };
      if (!string.IsNullOrWhiteSpace(albumId))
        multipartFormDataContent.Add((HttpContent) new StringContent(albumId), "album");
      if (title != null)
        multipartFormDataContent.Add((HttpContent) new StringContent(title), nameof (title));
      if (description != null)
        multipartFormDataContent.Add((HttpContent) new StringContent(description), nameof (description));
      httpRequestMessage.Content = (HttpContent) multipartFormDataContent;
      return httpRequestMessage;
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UploadImageStreamRequest(
      string url,
      Stream image,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (image == null)
        throw new ArgumentNullException(nameof (image));
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
      MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent(string.Format("{0}", (object) DateTime.UtcNow.Ticks))
      {
        {
          (HttpContent) new StringContent("file"),
          "type"
        },
        {
          (HttpContent) new StreamContent(image),
          nameof (image)
        }
      };
      if (!string.IsNullOrWhiteSpace(albumId))
        multipartFormDataContent.Add((HttpContent) new StringContent(albumId), "album");
      if (title != null)
        multipartFormDataContent.Add((HttpContent) new StringContent(title), nameof (title));
      if (description != null)
        multipartFormDataContent.Add((HttpContent) new StringContent(description), nameof (description));
      httpRequestMessage.Content = (HttpContent) multipartFormDataContent;
      return httpRequestMessage;
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UploadImageUrlRequest(
      string url,
      string imageUrl,
      string albumId = null,
      string title = null,
      string description = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(imageUrl))
        throw new ArgumentNullException(nameof (imageUrl));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "type",
          "URL"
        },
        {
          "image",
          imageUrl
        }
      };
      if (!string.IsNullOrWhiteSpace(albumId))
        source.Add("album", albumId);
      if (title != null)
        source.Add(nameof (title), title);
      if (description != null)
        source.Add(nameof (description), description);
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
