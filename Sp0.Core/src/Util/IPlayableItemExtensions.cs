using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  public static class IPlayableItemExtensions
  {
    public static string ToId(this IPlayableItem item)
    {
      if (item is FullTrack track)
        return track.Id;
      else if (item is FullEpisode episode)
        return episode.Id;
      throw new Exception($"Unkown type to convert: {item.GetType()}");
    }

    public static string ToUri(this IPlayableItem item)
    {
      if (item is FullTrack track)
        return track.Uri;
      else if (item is FullEpisode episode)
        return episode.Uri;
      throw new Exception($"Unkown type to convert: {item.GetType()}");
    }
  }
}
