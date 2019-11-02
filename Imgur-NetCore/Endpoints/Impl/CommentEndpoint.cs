
using Imgur.API.Authentication;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Imgur.API.RequestBuilders;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints.Impl
{
  /// <summary>Comment related actions.</summary>
  public class CommentEndpoint : EndpointBase, ICommentEndpoint, IEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the CommentEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public CommentEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the CommentEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal CommentEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal CommentRequestBuilder RequestBuilder { get; } = new CommentRequestBuilder();

    /// <summary>
    ///     Creates a new comment, returns the ID of the comment.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="comment">The comment text, this is what will be displayed.</param>
    /// <param name="galleryItemId">The ID of the item in the gallery that you wish to comment on.</param>
    /// <param name="parentId">The ID of the parent comment, this is an alternative method to create a reply.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> CreateCommentAsync(
      string comment,
      string galleryItemId,
      string parentId = null)
    {
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = nameof (comment);
      int id;
      using (HttpRequestMessage request = this.RequestBuilder.CreateCommentRequest(url, comment, galleryItemId, parentId))
      {
        Comment returnComment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        id = returnComment.Id;
      }
      return id;
    }

    /// <summary>
    ///     Create a reply for the given comment, returns the ID of the comment.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="comment">The comment text, this is what will be displayed.</param>
    /// <param name="galleryItemId">The ID of the item in the gallery that you wish to comment on.</param>
    /// <param name="parentId">The comment id that you are replying to.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<int> CreateReplyAsync(
      string comment,
      string galleryItemId,
      string parentId)
    {
      if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentNullException(nameof (comment));
      if (string.IsNullOrWhiteSpace(galleryItemId))
        throw new ArgumentNullException(nameof (galleryItemId));
      if (string.IsNullOrWhiteSpace(parentId))
        throw new ArgumentNullException(nameof (parentId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("comment/{0}", (object) parentId);
      int id;
      using (HttpRequestMessage request = this.RequestBuilder.CreateReplyRequest(url, comment, galleryItemId))
      {
        Comment returnComment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        id = returnComment.Id;
      }
      return id;
    }

    /// <summary>
    ///     Delete a comment by the given id.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> DeleteCommentAsync(int commentId)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("comment/{0}", (object) commentId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Delete, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>Get information about a specific comment.</summary>
    /// <param name="commentId">The comment id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IComment> GetCommentAsync(int commentId)
    {
      string url = string.Format("comment/{0}", (object) commentId);
      IComment comment1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Comment comment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        comment1 = (IComment) comment;
      }
      return comment1;
    }

    /// <summary>
    ///     Get the comment with all of the replies for the comment.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<IComment> GetRepliesAsync(int commentId)
    {
      string url = string.Format("comment/{0}/replies", (object) commentId);
      IComment comment1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Comment comment = await this.SendRequestAsync<Comment>(request).ConfigureAwait(false);
        comment1 = (IComment) comment;
      }
      return comment1;
    }

    /// <summary>
    ///     Report a comment for being inappropriate.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="reason">The reason why the comment is inappropriate.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> ReportCommentAsync(int commentId, ReportReason reason)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("comment/{0}/report", (object) commentId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.ReportItemRequest(url, reason))
      {
        bool? nullable = await this.SendRequestAsync<bool?>(request).ConfigureAwait(false);
        bool? reported = nullable;
        nullable = new bool?();
        bool? nullable1 = reported;
        flag = !nullable1.HasValue || nullable1.GetValueOrDefault();
      }
      return flag;
    }

    /// <summary>
    ///     Vote on a comment.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="vote">The vote.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> VoteCommentAsync(int commentId, VoteOption vote)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string voteValue = string.Format("{0}", (object) vote).ToLower();
      string url = string.Format("comment/{0}/vote/{1}", (object) commentId, (object) voteValue);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }
  }
}
