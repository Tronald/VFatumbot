
using Imgur.API.Authentication;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API.RequestBuilders;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Gets credit limit.</summary>
  public class RateLimitEndpoint : EndpointBase, IRateLimitEndpoint, IEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the RateLimitEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public RateLimitEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the RateLimitEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal RateLimitEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal RateLimitRequestBuilder RequestBuilder { get; } = new RateLimitRequestBuilder();

    /// <summary>Gets remaining credits for the application.</summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <returns></returns>
    public async Task<IRateLimit> GetRateLimitAsync()
    {
      string url = "credits";
      IRateLimit limit;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        RateLimit rateLimit = await this.SendRequestAsync<RateLimit>(request).ConfigureAwait(false);
        limit = (IRateLimit) rateLimit;
        rateLimit = (RateLimit) null;
      }
      return limit;
    }
  }
}
