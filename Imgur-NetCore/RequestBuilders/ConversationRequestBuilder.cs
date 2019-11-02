
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class ConversationRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage CreateMessageRequest(string url, string body)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(body))
        throw new ArgumentNullException(nameof (body));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          nameof (body),
          body
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
