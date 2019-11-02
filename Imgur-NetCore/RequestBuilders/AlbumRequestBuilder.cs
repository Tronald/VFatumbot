
using Imgur.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class AlbumRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage AddAlbumImagesRequest(
      string url,
      IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "ids",
          string.Join(",", imageIds)
        }
      };
      return new HttpRequestMessage(HttpMethod.Put, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage CreateAlbumRequest(
      string url,
      string title = null,
      string description = null,
      AlbumPrivacy? privacy = null,
      AlbumLayout? layout = null,
      string coverId = null,
      IEnumerable<string> imageIds = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      Dictionary<string, string> source = new Dictionary<string, string>();
      if (privacy.HasValue)
        source.Add(nameof (privacy), string.Format("{0}", (object) privacy).ToLower());
      if (layout.HasValue)
        source.Add(nameof (layout), string.Format("{0}", (object) layout).ToLower());
      if (coverId != null)
        source.Add("cover", coverId);
      if (title != null)
        source.Add(nameof (title), title);
      if (description != null)
        source.Add(nameof (description), description);
      if (imageIds != null)
        source.Add("ids", string.Join(",", imageIds));
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage RemoveAlbumImagesRequest(
      string url,
      IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      url = string.Format("{0}?ids={1}", (object) url, (object) WebUtility.UrlEncode(string.Join(",", imageIds)));
      return new HttpRequestMessage(HttpMethod.Delete, url);
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage SetAlbumImagesRequest(
      string url,
      IEnumerable<string> imageIds)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (imageIds == null)
        throw new ArgumentNullException(nameof (imageIds));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "ids",
          string.Join(",", imageIds)
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UpdateAlbumRequest(
      string url,
      string title = null,
      string description = null,
      AlbumPrivacy? privacy = null,
      AlbumLayout? layout = null,
      string coverId = null,
      IEnumerable<string> imageIds = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      Dictionary<string, string> source = new Dictionary<string, string>();
      if (privacy.HasValue)
        source.Add(nameof (privacy), string.Format("{0}", (object) privacy).ToLower());
      if (layout.HasValue)
        source.Add(nameof (layout), string.Format("{0}", (object) layout).ToLower());
      if (coverId != null)
        source.Add("cover", coverId);
      if (title != null)
        source.Add(nameof (title), title);
      if (description != null)
        source.Add(nameof (description), description);
      if (imageIds != null)
        source.Add("ids", string.Join(",", imageIds));
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
