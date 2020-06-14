using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  internal class UserProfileCommand : ICommand
  {
    private IConsole _console;
    private ISpotifyService _spotifyService;

    public UserProfileCommand(IConsole console, ISpotifyService spotify)
    {
      _console = console;
      _spotifyService = spotify;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("user-profile", (userProfile) =>
      {
        userProfile.Description = "Gets information about a spotify user.";

        var ids = userProfile.Option<List<string>>(
          "-i|--ids",
          "Optional: A list of Spotify user ids, seperated by comma. If omitted, the currently logged in user is queried",
          CommandOptionType.SingleValue
        );
        var output = new OutputArgument(userProfile);

        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");
        var onUri = output.WithFormat("uri");

        userProfile.OnExecuteAsync(async (cancel) =>
        {
          if (ids.ParsedValue != null)
          {
            _spotifyService.EnsureCredentialsSet(out var spotify);

            var users = await Task.WhenAll(ids.ParsedValue.Select((id) => spotify.UserProfile.Get(id)));
            foreach (var user in users)
            {
              onJson(() => _console.WriteObject(user));
              onId(() => _console.WriteLine(user.Id));
              onUri(() => _console.WriteLine(user.Uri));
            }
          }
          else
          {
            _spotifyService.EnsureUserLoggedIn(out var spotify);

            var user = await spotify.UserProfile.Current();
            onJson(() => _console.WriteObject(user));
            onId(() => _console.WriteLine(user.Id));
            onUri(() => _console.WriteLine(user.Uri));
          }
        });
      });
    }
  }
}
