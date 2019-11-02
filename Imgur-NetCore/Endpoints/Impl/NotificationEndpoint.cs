
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
  /// <summary>Notification related actions.</summary>
  public class NotificationEndpoint : EndpointBase, INotificationEndpoint
  {
    /// <summary>
    ///     Initializes a new instance of the NotificationsEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    public NotificationEndpoint(IApiClient apiClient)
      : base(apiClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the NotificationsEndpoint class.
    /// </summary>
    /// <param name="apiClient">The type of client that will be used for authentication.</param>
    /// <param name="httpClient"> The class for sending HTTP requests and receiving HTTP responses from the endpoint methods.</param>
    internal NotificationEndpoint(IApiClient apiClient, HttpClient httpClient)
      : base(apiClient, httpClient)
    {
    }

    internal NotificationRequestBuilder RequestBuilder { get; } = new NotificationRequestBuilder();

    /// <summary>
    ///     Returns the data about a specific notification.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="notificationId">The notification id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<INotification> GetNotificationAsync(string notificationId)
    {
      if (string.IsNullOrWhiteSpace(notificationId))
        throw new ArgumentNullException(nameof (notificationId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("notification/{0}", (object) notificationId);
      INotification notification1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Notification notification = await this.SendRequestAsync<Notification>(request).ConfigureAwait(false);
        notification1 = (INotification) notification;
      }
      return notification1;
    }

    /// <summary>
    ///     Returns all of the notifications for the user.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="newNotifications">false for all notifications, true for only non-viewed notification. Default is true.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<INotifications> GetNotificationsAsync(bool newNotifications = true)
    {
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string newNotificationsValue = string.Format("{0}", (object) newNotifications).ToLower();
      string url = string.Format("notification?new={0}", (object) newNotificationsValue);
      INotifications notifications1;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Get, url))
      {
        Notifications notifications = await this.SendRequestAsync<Notifications>(request).ConfigureAwait(false);
        notifications1 = (INotifications) notifications;
      }
      return notifications1;
    }

    /// <summary>
    ///     Marks notifications as viewed.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="ids">The notification id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> MarkNotificationsViewedAsync(IEnumerable<string> ids)
    {
      if (ids == null)
        throw new ArgumentNullException(nameof (ids));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = "notification";
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.MarkNotificationsViewedRequest(url, ids))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }

    /// <summary>
    ///     Marks a notification as viewed.
    ///     OAuth authentication required.
    /// </summary>
    /// <param name="notificationId">The notification id.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    /// <exception cref="T:Imgur.API.ImgurException">Thrown when an error is found in a response from an Imgur endpoint.</exception>
    /// <exception cref="T:Imgur.API.MashapeException">Thrown when an error is found in a response from a Mashape endpoint.</exception>
    /// <returns></returns>
    public async Task<bool> MarkNotificationViewedAsync(string notificationId)
    {
      if (string.IsNullOrWhiteSpace(notificationId))
        throw new ArgumentNullException(nameof (notificationId));
      if (this.ApiClient.OAuth2Token == null)
        throw new ArgumentNullException("OAuth2Token", "OAuth authentication required.");
      string url = string.Format("notification/{0}", (object) notificationId);
      bool flag;
      using (HttpRequestMessage request = this.RequestBuilder.CreateRequest(HttpMethod.Post, url))
        flag = await this.SendRequestAsync<bool>(request).ConfigureAwait(false);
      return flag;
    }
  }
}
