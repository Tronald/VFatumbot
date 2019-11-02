
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class CommentRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage CreateCommentRequest(
      string url,
      string comment,
      string galleryItemId,
      string parentId)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "image_id",
          galleryItemId
        },
        {
          nameof (comment),
          comment
        }
      };
      if (!string.IsNullOrWhiteSpace(parentId))
        source.Add("parent_id", parentId);
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage CreateGalleryItemCommentRequest(
      string url,
      string comment)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (comment),
          comment
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
    internal HttpRequestMessage CreateReplyRequest(
      string url,
      string comment,
      string galleryItemId)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "image_id",
          galleryItemId
        },
        {
          nameof (comment),
          comment
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
