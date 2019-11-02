
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class OAuth2RequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage GetTokenByCodeRequest(
      string url,
      string code,
      string clientId,
      string clientSecret)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(code))
        throw new ArgumentNullException(nameof (code));
      if (string.IsNullOrWhiteSpace(clientId))
        throw new ArgumentNullException(nameof (clientId));
      if (string.IsNullOrWhiteSpace(clientSecret))
        throw new ArgumentNullException(nameof (clientSecret));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "client_id",
          clientId
        },
        {
          "client_secret",
          clientSecret
        },
        {
          "grant_type",
          "authorization_code"
        },
        {
          nameof (code),
          code
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
    internal HttpRequestMessage GetTokenByPinRequest(
      string url,
      string pin,
      string clientId,
      string clientSecret)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(pin))
        throw new ArgumentNullException(nameof (pin));
      if (string.IsNullOrWhiteSpace(clientId))
        throw new ArgumentNullException(nameof (clientId));
      if (string.IsNullOrWhiteSpace(clientSecret))
        throw new ArgumentNullException(nameof (clientSecret));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "client_id",
          clientId
        },
        {
          "client_secret",
          clientSecret
        },
        {
          "grant_type",
          nameof (pin)
        },
        {
          nameof (pin),
          pin
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
    internal HttpRequestMessage GetTokenByRefreshTokenRequest(
      string url,
      string refreshToken,
      string clientId,
      string clientSecret)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      if (string.IsNullOrWhiteSpace(refreshToken))
        throw new ArgumentNullException(nameof (refreshToken));
      if (string.IsNullOrWhiteSpace(clientId))
        throw new ArgumentNullException(nameof (clientId));
      if (string.IsNullOrWhiteSpace(clientSecret))
        throw new ArgumentNullException(nameof (clientSecret));
      Dictionary<string, string> source = new Dictionary<string, string>()
      {
        {
          "client_id",
          clientId
        },
        {
          "client_secret",
          clientSecret
        },
        {
          "grant_type",
          "refresh_token"
        },
        {
          "refresh_token",
          refreshToken
        }
      };
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
