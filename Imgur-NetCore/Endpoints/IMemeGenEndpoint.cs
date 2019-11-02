
using Imgur.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.API.Endpoints
{
  /// <summary>Meme related actions.</summary>
  public interface IMemeGenEndpoint
  {
    /// <summary>Get the list of default memes.</summary>
    /// <returns></returns>
    Task<IEnumerable<IImage>> GetDefaultMemesAsync();
  }
}
