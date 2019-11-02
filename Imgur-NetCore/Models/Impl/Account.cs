
using Imgur.API.Enums;
using Imgur.API.JsonConverters;
using Newtonsoft.Json;
using System;

namespace Imgur.API.Models.Impl
{
  /// <summary>
  ///     This model is used to represent the basic account information.
  /// </summary>
  public class Account : IAccount, IDataModel
  {
    /// <summary>A basic description the user has filled out.</summary>
    public virtual string Bio { get; set; }

    /// <summary>
    ///     Utc timestamp of account creation, converted from epoch time.
    /// </summary>
    [JsonConverter(typeof (EpochTimeConverter))]
    public virtual DateTimeOffset Created { get; set; }

    /// <summary>The account id for the username requested.</summary>
    public virtual int Id { get; set; }

    /// <summary>
    ///     The reputation for the account, in its numerical format.
    /// </summary>
    public virtual float Reputation { get; set; }

    /// <summary>
    ///     The account username, will be the same as requested in the URL.
    /// </summary>
    public virtual string Url { get; set; }

    /// <summary>Notoriety level based on a user's reputation.</summary>
    public NotorietyLevel Notoriety
    {
      get
      {
        if ((double) this.Reputation >= 20000.0)
          return NotorietyLevel.Glorious;
        if ((double) this.Reputation >= 8000.0 && (double) this.Reputation <= 19999.0)
          return NotorietyLevel.Renowned;
        if ((double) this.Reputation >= 4000.0 && (double) this.Reputation <= 7999.0)
          return NotorietyLevel.Idolized;
        if ((double) this.Reputation >= 2000.0 && (double) this.Reputation <= 3999.0)
          return NotorietyLevel.Trusted;
        if ((double) this.Reputation >= 1000.0 && (double) this.Reputation <= 1999.0)
          return NotorietyLevel.Liked;
        if ((double) this.Reputation >= 400.0 && (double) this.Reputation <= 999.0)
          return NotorietyLevel.Accepted;
        return (double) this.Reputation >= 0.0 && (double) this.Reputation <= 399.0 ? NotorietyLevel.Neutral : NotorietyLevel.ForeverAlone;
      }
    }
  }
}
