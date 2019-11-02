
using Imgur.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal abstract class RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage CreateRequest(HttpMethod httpMethod, string url)
    {
      if (httpMethod == (HttpMethod) null)
        throw new ArgumentNullException(nameof (httpMethod));
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      return new HttpRequestMessage(httpMethod, url);
    }

    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage ReportItemRequest(string url, ReportReason reason)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (reason),
          ((int) reason).ToString()
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
