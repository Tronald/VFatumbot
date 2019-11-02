
using Imgur.API.Authentication;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API.RequestBuilders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Conversation related actions.</summary>
  public class ConversationEndpoint : EndpointBase, IConversationEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the ConversationEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public ConversationEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the ConversationEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal ConversationEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal ConversationRequestBuilder RequestBuilder { get; } = new ConversationRequestBuilder();

    /// <summary>
    ///     Block the user from sending messages to the user that is logged in.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="username">The sender that should be blocked.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> BlockSenderAsync(string username)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("conversations/block/{0}", (object) username);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Create a new message. OAuth authentication required.
    /// </summary>
    /// <param name="recipient">The recipient username, this person will receive the message.</param>
    /// <param name="body">The message itself, similar to the body of an email.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> CreateConversationAsync(string recipient, string body)
    {
      if (string.IsNullOrWhiteSpace(recipient))
        throw new ArgumentNullException(nameof (recipient));
      if (string.IsNullOrWhiteSpace(body))
        throw new ArgumentNullException(nameof (body));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("conversations/{0}", (object) recipient);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateMessageRequest(url, body))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Delete a conversation by the given id. OAuth authentication required.
    /// </summary>
    /// <param name="conversationId">The conversation id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteConversationAsync(string conversationId)
    {
      if (string.IsNullOrWhiteSpace(conversationId))
        throw new ArgumentNullException(nameof (conversationId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("conversations/{0}", (object) conversationId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Get information about a specific conversation. Includes messages.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="conversationId">The conversation id.</param>
    /// <param name="page">
    ///     Page of message thread. Starting at 1 for the most recent 25 messages and counting upwards. Default:
    ///     null
    /// </param>
    /// <param name="offset">Additional offset in current page.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IConversation> GetConversationAsync(
      string conversationId,
      int? page = null,
      int? offset = null)
    {
      if (string.IsNullOrWhiteSpace(conversationId))
        throw new ArgumentNullException(nameof (conversationId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string str = conversationId;
      int? nullable = page;
      // ISSUE: variable of a boxed type
      var local1 = (ValueType) (nullable ?? 1);
      nullable = offset;
      // ISSUE: variable of a boxed type
      var local2 = (ValueType) (nullable ?? 0);
      string url = string.Format("conversations/{0}/{1}/{2}", (object) str, (object) local1, (object) local2);
      IConversation conversation1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Conversation conversation = await this.SendRequestAsync<Conversation>(request).ConfigureAwait(false);
        conversation1 = (IConversation) conversation;
      }
      return conversation1;
    }

    /// <summary>
    ///     Get list of all conversations for the logged in user.
    ///     OAuth authentication required.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IEnumerable<IConversation>> GetConversationsAsync()
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "conversations";
      IEnumerable<IConversation> conversations1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        IEnumerable<Conversation> conversations = await this.SendRequestAsync<IEnumerable<Conversation>>(request).ConfigureAwait(false);
        conversations1 = (IEnumerable<IConversation>) conversations;
      }
      return conversations1;
    }

    /// <summary>
    ///     Report a user for sending messages that are against the Terms of Service.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="username">The sender that should be reported.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> ReportSenderAsync(string username)
    {
      if (string.IsNullOrWhiteSpace(username))
        throw new ArgumentNullException(nameof (username));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("conversations/report/{0}", (object) username);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }
  }
}
