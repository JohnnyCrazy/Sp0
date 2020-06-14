using System.Xml;
using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  internal class LoginCommand : ICommand
  {
    private IConsole _console;
    private ApplicationConfig _appConfig;
    private ILoginService _login;

    private static Uri addressDefault = new Uri("http://localhost:5000/callback");
    private static int portDefault = 5000;
    // seconds --> 5 minutes
    private static int timeoutDefault = 60 * 5;

    public LoginCommand(IConsole console, ApplicationConfig appConfig, ILoginService login)
    {
      _console = console;
      _appConfig = appConfig;
      _login = login;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("login", (login) =>
      {
        login.Description = "Log into a user account via OAuth2. Only required when accessing user related data.";

        var addressCmd = login.Option<Uri?>(
          "-a|--address",
          $"Optional: URI of the webserver used to authenticate. Also needs to be added as redirect uri to your spotify app. Default: {addressDefault}",
          CommandOptionType.SingleValue
        );
        addressCmd.Validators.Add(new AbsoluteURIValidator());

        var portOpt = login.Option<int?>(
          "-p|--port",
          $"Optional: Listen port of the authentication webserver. Default: {portDefault}",
          CommandOptionType.SingleValue
        );

        var scopesOpt = login.Option<List<string>?>(
          "-s|--scopes",
          $"Optional: A comma seperated list of scopes to request. Default: All",
          CommandOptionType.SingleValue
        );

        var timeoutOpt = login.Option<int?>(
          "-t|--timeout",
          $"Optional: Timeout of command in seconds. Default: ${timeoutDefault}",
          CommandOptionType.SingleValue
        );

        var stateOpt = login.Option<int?>(
          "--state",
          $"Optional: State value used for the authentcation. Default: random generated string",
          CommandOptionType.SingleValue
        );

        login.OnExecuteAsync(async (cancel) =>
        {
          if (
            string.IsNullOrEmpty(_appConfig.SpotifyApp.ClientId)
            || string.IsNullOrEmpty(_appConfig.SpotifyApp.ClientSecret))
          {
            _console.WriteLine("client-id or client-secret not set. Please run `spotify-cli config` before logging in");
            Environment.Exit(1);
          }

          var state = stateOpt.Value() ?? Guid.NewGuid().ToString();
          var address = addressCmd.ParsedValue ?? addressDefault;
          var port = portOpt.ParsedValue ?? portDefault;
          var timeout = TimeSpan.FromSeconds(timeoutOpt.ParsedValue ?? timeoutDefault);
          var scopes = scopesOpt.ParsedValue ?? new List<string> {
            Scopes.AppRemoteControl,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic,
            Scopes.PlaylistReadCollaborative,
            Scopes.PlaylistReadPrivate,
            Scopes.Streaming,
            Scopes.UgcImageUpload,
            Scopes.UserFollowModify,
            Scopes.UserFollowRead,
            Scopes.UserLibraryModify,
            Scopes.UserLibraryRead,
            Scopes.UserModifyPlaybackState,
            Scopes.UserReadCurrentlyPlaying,
            Scopes.UserReadEmail,
            Scopes.UserReadPlaybackPosition,
            Scopes.UserReadPlaybackState,
            Scopes.UserReadPrivate,
            Scopes.UserReadRecentlyPlayed,
            Scopes.UserTopRead,
          };

          var uri = _login.GenerateLoginURI(address, _appConfig.SpotifyApp.ClientId, scopes, state);
          BrowserUtil.Open(uri);

          _console.WriteLine("If no browser opened, visit the following URL manually:\n");
          _console.WithColor(ConsoleColor.Cyan, () => _console.WriteLine($"{uri}\n"));

          var logginError = await _login.WaitForLogin(address, port, timeout, state);

          if (logginError == null)
          {
            _console.WithColor(ConsoleColor.Green, () =>
            {
              _console.WriteLine($"You're now logged in as {_appConfig.Account.DisplayName} ({_appConfig.Account.Id})");
            });
          }
          else
          {
            _console.WithColor(ConsoleColor.Red, () => _console.WriteLine($"Login failed: {logginError}"));
          }
        });
      });
    }
  }
}
