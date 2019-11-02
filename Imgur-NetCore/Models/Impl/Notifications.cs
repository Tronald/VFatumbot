
using Imgur.API.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Imgur.API.Models.Impl
{
  /// <summary>Account notifications.</summary>
  public class Notifications : INotifications, IDataModel
  {
    /// <summary>A list of message notifications.</summary>
    [JsonConverter(typeof (TypeConverter<IEnumerable<Notification>>))]
    public virtual IEnumerable<INotification> Messages { get; set; } = (IEnumerable<INotification>) new List<INotification>();

    /// <summary>A list of comment notifications.</summary>
    [JsonConverter(typeof (TypeConverter<IEnumerable<Notification>>))]
    public virtual IEnumerable<INotification> Replies { get; set; } = (IEnumerable<INotification>) new List<INotification>();
  }
}
