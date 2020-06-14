using System;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  internal class LogoutCommand : ICommand
  {
    private IConsole _console;
    private ApplicationConfig _appConfig;

    public LogoutCommand(IConsole console, ApplicationConfig appConfig)
    {
      _console = console;
      _appConfig = appConfig;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("logout", (logout) =>
      {
        logout.Description = "Logout the current user, removing access and refresh tokens from the config.";

        logout.OnExecuteAsync(async (cancel) =>
        {
          _appConfig.SpotifyToken.AccessToken = null;
          _appConfig.SpotifyToken.RefreshToken = null;
          _appConfig.SpotifyToken.CreatedAt = null;
          _appConfig.SpotifyToken.ExpiresIn = null;
          _appConfig.SpotifyToken.TokenType = null;
          await _appConfig.Save();

          _console.WithColor(ConsoleColor.Green, () => _console.WriteLine("Account has been logged out!"));
        });
      });
    }
  }
}
