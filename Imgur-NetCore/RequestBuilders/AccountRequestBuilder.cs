
using Imgur.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Imgur.API.RequestBuilders
{
  internal class AccountRequestBuilder : RequestBuilderBase
  {
    /// <exception cref="T:System.ArgumentNullException">
    ///     Thrown when a null reference is passed to a method that does not accept it as a
    ///     valid argument.
    /// </exception>
    internal HttpRequestMessage UpdateAccountSettingsRequest(
      string url,
      string bio = null,
      bool? publicImages = null,
      bool? messagingEnabled = null,
      AlbumPrivacy? albumPrivacy = null,
      bool? acceptedGalleryTerms = null,
      string username = null,
      bool? showMature = null,
      bool? newsletterSubscribed = null)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentNullException(nameof (url));
      Dictionary<string, string> source = new Dictionary<string, string>();
      if (publicImages.HasValue)
        source.Add("public_images", string.Format("{0}", (object) publicImages).ToLower());
      if (messagingEnabled.HasValue)
        source.Add("messaging_enabled", string.Format("{0}", (object) messagingEnabled).ToLower());
      if (albumPrivacy.HasValue)
        source.Add("album_privacy", string.Format("{0}", (object) albumPrivacy).ToLower());
      if (acceptedGalleryTerms.HasValue)
        source.Add("accepted_gallery_terms", string.Format("{0}", (object) acceptedGalleryTerms).ToLower());
      if (showMature.HasValue)
        source.Add("show_mature", string.Format("{0}", (object) showMature).ToLower());
      if (newsletterSubscribed.HasValue)
        source.Add("newsletter_subscribed", string.Format("{0}", (object) newsletterSubscribed).ToLower());
      if (bio != null)
        source.Add(nameof (bio), bio);
      if (!string.IsNullOrWhiteSpace(username))
        source.Add(nameof (username), username);
      return new HttpRequestMessage(HttpMethod.Post, url)
      {
        Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) source.ToArray<KeyValuePair<string, string>>())
      };
    }
  }
}
