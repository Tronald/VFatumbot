
using Imgur.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Imgur.API.RequestBuilders
{
  internal class GalleryRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage PublishToGalleryRequest(
      string url,
      string title,
      string topicId = null,
      bool? bypassTerms = null,
      bool? mature = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(title))
        throw new ArgumentNullException(nameof (title));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (title),
          title
        }
      };
      if (topicId != null)
        source.Add("topic", topicId);
      if (bypassTerms.HasValue)
        source.Add("terms", string.Format("{0}", (object) bypassTerms).ToLower());
      if (mature.HasValue)
        source.Add(nameof (mature), string.Format("{0}", (object) mature).ToLower());
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal string SearchGalleryAdvancedRequest(
      string url,
      string qAll = null,
      string qAny = null,
      string qExactly = null,
      string qNot = null,
      ImageFileType? fileType = null,
      ImageSize? imageSize = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(qAll) && string.IsNullOrWhiteSpace(qAny) && string.IsNullOrWhiteSpace(qExactly) && string.IsNullOrWhiteSpace(qNot))
        throw new ArgumentNullException((string) null, "At least one search parameter must be provided (All | Any | Exactly | Not).");
      StringBuilder stringBuilder = new StringBuilder();
      if (!string.IsNullOrWhiteSpace(qAll))
        stringBuilder.Append(string.Format("&q_all={0}", (object) WebUtility.UrlEncode(qAll)));
      if (!string.IsNullOrWhiteSpace(qAny))
        stringBuilder.Append(string.Format("&q_any={0}", (object) WebUtility.UrlEncode(qAny)));
      if (!string.IsNullOrWhiteSpace(qExactly))
        stringBuilder.Append(string.Format("&q_exactly={0}", (object) WebUtility.UrlEncode(qExactly)));
      if (!string.IsNullOrWhiteSpace(qNot))
        stringBuilder.Append(string.Format("&q_not={0}", (object) WebUtility.UrlEncode(qNot)));
      if (fileType.HasValue)
        stringBuilder.Append(string.Format("&q_type={0}", (object) WebUtility.UrlEncode(fileType.ToString().ToLower())));
      if (imageSize.HasValue)
        stringBuilder.Append(string.Format("&q_size_px={0}", (object) WebUtility.UrlEncode(imageSize.ToString().ToLower())));
      return string.Format("{0}?{1}", (object) url, (object) stringBuilder).Replace("?&", "?");
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal string SearchGalleryRequest(string url, string query)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(query))
        throw new ArgumentNullException(nameof (query));
      return string.Format("{0}?q={1}", (object) url, (object) WebUtility.UrlEncode(query));
    }
  }
}
