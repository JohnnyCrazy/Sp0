using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  public interface ISpotifyService
  {
    SpotifyClientConfig Config { get; }

    SpotifyClient? Spotify { get; }

    OAuthClient OAuth { get; }

    bool EnsureUserLoggedIn([NotNull] out SpotifyClient? spotify);

    bool EnsureCredentialsSet([NotNull] out SpotifyClient? spotify);
  }
}
