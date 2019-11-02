
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class CustomGalleryRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage AddCustomGalleryTagsRequest(
      string url,
      IEnumerable<string> tags)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (tags == null)
        throw new ArgumentNullException(nameof (tags));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (tags),
          string.Join(",", tags)
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
    internal HttpRequestMessage AddFilteredOutGalleryTagRequest(
      string url,
      string tag)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (tag),
          tag
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
    internal HttpRequestMessage RemoveCustomGalleryTagsRequest(
      string url,
      IEnumerable<string> tags)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (tags == null)
        throw new ArgumentNullException(nameof (tags));
      url = string.Format("{0}?tags={1}", (object) url, (object) WebUtility.UrlEncode(string.Join(",", tags)));
      return new HttpRequestMessage(HttpMethod.Delete, url);
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage RemoveFilteredOutGalleryTagRequest(
      string url,
      string tag)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(tag))
        throw new ArgumentNullException(nameof (tag));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (tag),
          tag
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
