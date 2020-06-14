using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  public class SpotifyService : ISpotifyService
  {
    private ApplicationConfig _appConfig;
    private IConsole _console;
    private SpotifyClientConfig _config;
    private SpotifyClient? _spotify;
    private OAuthClient _oauth;

    public SpotifyService(ApplicationConfig appConfig, IConsole console)
    {
      _appConfig = appConfig;
      _console = console;

      if (!string.IsNullOrEmpty(appConfig.SpotifyToken.RefreshToken))
      {
        // We're logged in as a user
        _config = CreateForUser();
        _spotify = new SpotifyClient(_config);
      }
      else if (
        !string.IsNullOrEmpty(appConfig.SpotifyApp.ClientId)
        && !string.IsNullOrEmpty(appConfig.SpotifyApp.ClientSecret)
      )
      {
        _config = CreateForCredentials();
        _spotify = new SpotifyClient(_config);
      }
      else
      {
        _config = SpotifyClientConfig.CreateDefault();
      }

      _oauth = new OAuthClient(_config);
    }

    private SpotifyClientConfig CreateForUser()
    {
      return SpotifyClientConfig
        .CreateDefault()
        .WithAuthenticator(new AuthorizationCodeAuthenticator(
          _appConfig.SpotifyApp.ClientId!,
          _appConfig.SpotifyApp.ClientSecret!,
          new AuthorizationCodeTokenResponse
          {
            AccessToken = _appConfig.SpotifyToken.AccessToken!,
            CreatedAt = (DateTime)_appConfig.SpotifyToken.CreatedAt!,
            RefreshToken = _appConfig.SpotifyToken.RefreshToken!,
            ExpiresIn = (int)_appConfig.SpotifyToken.ExpiresIn!,
            TokenType = _appConfig.SpotifyToken.TokenType!,
          }
        ))
        .WithRetryHandler(new SimpleRetryHandler());
    }

    private SpotifyClientConfig CreateForCredentials()
    {
      return SpotifyClientConfig
        .CreateDefault()
        .WithAuthenticator(new ClientCredentialsAuthenticator(
          _appConfig.SpotifyApp.ClientId!,
          _appConfig.SpotifyApp.ClientSecret!,
          string.IsNullOrEmpty(_appConfig.SpotifyToken.AccessToken) ? null : new ClientCredentialsTokenResponse
          {
            AccessToken = _appConfig.SpotifyToken.AccessToken!,
            CreatedAt = (DateTime)_appConfig.SpotifyToken.CreatedAt!,
            ExpiresIn = (int)_appConfig.SpotifyToken.ExpiresIn!,
            TokenType = _appConfig.SpotifyToken.TokenType!,
          }
        ))
        .WithRetryHandler(new SimpleRetryHandler());
    }

    public bool EnsureUserLoggedIn([NotNull] out SpotifyClient? spotify)
    {
      if (Spotify == null || !(Config.Authenticator is AuthorizationCodeAuthenticator))
      {
        spotify = null;
        _console.WithColor(ConsoleColor.Red, () =>
        {
          _console.WriteLine("This action requires a user to be logged in - try `sp0 login`");
        });
        Environment.Exit(1);
      }

      spotify = Spotify;
      return true;
    }

    public bool EnsureCredentialsSet([NotNull] out SpotifyClient? spotify)
    {
      if (Spotify == null || Config.Authenticator == null)
      {
        spotify = null;
        _console.WithColor(ConsoleColor.Red, () =>
        {
          _console.WriteLine("This action requires spotify application credentials to be set - try `sp0 config --help`");
        });
        Environment.Exit(1);
      }

      spotify = Spotify;
      return true;
    }

    public SpotifyClientConfig Config { get => _config; }

    public SpotifyClient? Spotify { get => _spotify; }

    public OAuthClient OAuth { get => _oauth; }
  }
}
