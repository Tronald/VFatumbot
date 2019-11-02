
using Imgur.API.Authentication;
using Imgur.API.JsonConverters;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Builder class for endpoints.</summary>
  public abstract class EndpointBase : IEndpoint
  {
    internal const string OAuth2RequiredExceptionMessage = "OAuth authentication required.";

    /// <summary>
    ///     Initializes a new instance of the EndpointBase class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    protected EndpointBase(IApiClient apiClient)
      : this(apiClient, new HttpClient())
    {
      if (apiClient == null)
        throw new ArgumentNullException(nameof (apiClient));
      this.ApiClient = apiClient;
    }

    /// <summary>
    ///     Initializes a new instance of the EndpointBase class.
    /// </summary>
    internal EndpointBase()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the EndpointBase class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal EndpointBase(IApiClient apiClient, HttpClient httpClient)
    {
      if (apiClient == null)
        throw new ArgumentNullException(nameof (apiClient));
      if (httpClient == null)
        throw new ArgumentNullException(nameof (httpClient));
      httpClient.DefaultRequestHeaders.Remove("Authorization");
      httpClient.DefaultRequestHeaders.Remove("X-Mashape-Key");
      httpClient.DefaultRequestHeaders.Accept.Clear();
      httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiClient.OAuth2Token != null ? string.Format("Bearer {0}", (object) apiClient.OAuth2Token.AccessToken) : string.Format("Client-ID {0}", (object) apiClient.ClientId));
      IMashapeClient mashapeClient = apiClient as IMashapeClient;
      if (mashapeClient != null)
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Mashape-Key", mashapeClient.MashapeKey);
      httpClient.BaseAddress = new Uri(apiClient.EndpointUrl);
      httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      this.ApiClient = apiClient;
      this.HttpClient = httpClient;
    }

    /// <summary>Interface for all API authentication types.</summary>
    public virtual IApiClient ApiClient { get; private set; }

    /// <summary>
    ///     The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.
    /// </summary>
    public virtual HttpClient HttpClient { get; }

    /// <summary>Switch from one client type to another.</summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    public virtual void SwitchClient(IApiClient apiClient)
    {
      if (apiClient == null)
        throw new ArgumentNullException(nameof (apiClient));
      this.ApiClient = apiClient;
      this.HttpClient.DefaultRequestHeaders.Remove("Authorization");
      this.HttpClient.DefaultRequestHeaders.Remove("X-Mashape-Key");
      this.HttpClient.DefaultRequestHeaders.Accept.Clear();
      this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiClient.OAuth2Token != null ? string.Format("Bearer {0}", (object) apiClient.OAuth2Token.AccessToken) : string.Format("Client-ID {0}", (object) apiClient.ClientId));
      IMashapeClient mashapeClient = apiClient as IMashapeClient;
      if (mashapeClient != null)
        this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Mashape-Key", mashapeClient.MashapeKey);
      this.HttpClient.BaseAddress = new Uri(apiClient.EndpointUrl);
      this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    ///     Parses the string stringResponse from the endpoint into an expected type T.
    /// </summary>
    /// <typeparam name="T">The expected output type, Image, bool, etc.</typeparam>
    /// <param name="response">The response from the endpoint.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <returns></returns>
    internal virtual T ProcessEndpointResponse<T>(HttpResponseMessage response)
    {
      if (response == null)
        throw new ImgurException("The response from the endpoint is missing.");
      string str = (string) null;
      if (response.Content != null)
      {
        Task<string> task = response.Content.ReadAsStringAsync();
        Task.WaitAll((Task) task);
        str = task.Result.Trim();
      }
      if (string.IsNullOrWhiteSpace(str))
        throw new ImgurException(string.Format("The response from the endpoint is missing. {0} {1}", (object) (int) response.StatusCode, (object) response.ReasonPhrase));
      if (str.StartsWith("<"))
        throw new ImgurException(string.Format("The response from the endpoint is invalid. {0} {1}", (object) (int) response.StatusCode, (object) response.ReasonPhrase));
      if (this.ApiClient is IMashapeClient && str.StartsWith("{\"message\":"))
        throw new MashapeException(JsonConvert.DeserializeObject<MashapeError>(str).Message);
      if (str.StartsWith("{\"data\":{\"error\":"))
        throw new ImgurException(JsonConvert.DeserializeObject<Basic<ImgurError>>(str).Data.Error);
      if (typeof (T) == typeof (IOAuth2Token) || typeof (T) == typeof (OAuth2Token))
        return JsonConvert.DeserializeObject<T>(str);
      if (!JsonConvert.DeserializeObject<Basic<object>>(str).Success)
        throw new ImgurException(JsonConvert.DeserializeObject<Basic<ImgurError>>(str).Data.Error);
      JsonConverter[] jsonConverterArray = new JsonConverter[1]
      {
        (JsonConverter) new GalleryItemConverter()
      };
      return JsonConvert.DeserializeObject<Basic<T>>(str, jsonConverterArray).Data;
    }

    /// <summary>Send requests to the service.</summary>
    /// <param name="message">The HttpRequestMessage that should be sent.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <returns></returns>
    internal virtual async Task<T> SendRequestAsync<T>(HttpRequestMessage message)
    {
      if (message == null)
        throw new ArgumentNullException(nameof (message));
      HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(message).ConfigureAwait(false);
      this.UpdateRateLimit(httpResponse.Headers);
      return this.ProcessEndpointResponse<T>(httpResponse);
    }

    /// <summary>
    ///     Updates the ApiClient's RateLimit
    ///     with the values from the last response from the Api.
    /// </summary>
    /// <param name="headers">The headers from the last request to the endpoint.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal virtual void UpdateRateLimit(HttpResponseHeaders headers)
    {
      if (headers == null)
        throw new ArgumentNullException(nameof (headers));
      string s1 = string.Empty;
      string s2 = string.Empty;
      if (this.ApiClient is IImgurClient && headers.Any<KeyValuePair<string, IEnumerable<string>>>((Func<KeyValuePair<string, IEnumerable<string>>, bool>) (x => x.Key.Equals("X-RateLimit-ClientLimit"))))
      {
        s1 = headers.GetValues("X-RateLimit-ClientLimit").FirstOrDefault<string>();
        s2 = headers.GetValues("X-RateLimit-ClientRemaining").FirstOrDefault<string>();
      }
      if (this.ApiClient is IMashapeClient && headers.Any<KeyValuePair<string, IEnumerable<string>>>((Func<KeyValuePair<string, IEnumerable<string>>, bool>) (x => x.Key.Equals("X-RateLimit-Requests-Limit"))))
      {
        s1 = headers.GetValues("X-RateLimit-Requests-Limit").FirstOrDefault<string>();
        s2 = headers.GetValues("X-RateLimit-Requests-Remaining").FirstOrDefault<string>();
      }
      int result1;
      if (!int.TryParse(s1, out result1))
        result1 = this.ApiClient.RateLimit.ClientLimit;
      int result2;
      if (!int.TryParse(s2, out result2))
        result2 = this.ApiClient.RateLimit.ClientRemaining;
      this.ApiClient.RateLimit.ClientLimit = result1;
      this.ApiClient.RateLimit.ClientRemaining = result2;
    }
  }
}
