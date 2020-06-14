using System.Net.NetworkInformation;
using System.Threading;
using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Collections.Generic;

namespace Sp0.Core
{
  public class LoginService : ILoginService
  {
    private ApplicationConfig _appConfig;
    private ISpotifyService _spotifyService;

    public LoginService(ApplicationConfig appConfig, ISpotifyService spotify)
    {
      _appConfig = appConfig;
      _spotifyService = spotify;
    }

    public Uri GenerateLoginURI(Uri address, string clientId, IList<string> scopes, string state)
    {
      var request = new LoginRequest(address, clientId, LoginRequest.ResponseType.Code)
      {
        Scope = scopes,
        State = state
      };
      return request.ToUri();
    }

    public Task<string?> WaitForLogin(Uri address, int port, TimeSpan timeout, string state)
    {
      var tcs = new TaskCompletionSource<string?>();

      var server = new EmbedIOAuthServer(address, port);
      server.AuthorizationCodeReceived += async (sender, response) =>
      {
        await server.Stop();
        if (response.State != state)
        {
          tcs.SetResult("Given state parameter was not correct.");
          return;
        }

        var tokenResponse = await _spotifyService.OAuth.RequestToken(
          new AuthorizationCodeTokenRequest(
            _appConfig.SpotifyApp.ClientId!,
            _appConfig.SpotifyApp.ClientSecret!,
            response.Code,
            server.BaseUri
          )
        );
        _appConfig.SpotifyToken.AccessToken = tokenResponse.AccessToken;
        _appConfig.SpotifyToken.RefreshToken = tokenResponse.RefreshToken;
        _appConfig.SpotifyToken.CreatedAt = tokenResponse.CreatedAt;
        _appConfig.SpotifyToken.ExpiresIn = tokenResponse.ExpiresIn;
        _appConfig.SpotifyToken.TokenType = tokenResponse.TokenType;

        // Create a temporary spotify with access token to fetch user
        var spotify = new SpotifyClient(_spotifyService.Config.WithToken(tokenResponse.AccessToken));
        var me = await spotify.UserProfile.Current();

        _appConfig.Account.Id = me.Id;
        _appConfig.Account.DisplayName = me.DisplayName;
        _appConfig.Account.Uri = me.Uri;

        await _appConfig.Save();
        server.Dispose();
        tcs.SetResult(null);
      };

      var ct = new CancellationTokenSource(timeout);
      ct.Token.Register(() =>
      {
        server.Stop();
        server.Dispose();
        tcs.TrySetCanceled();
      }, useSynchronizationContext: false);

      server.Start();

      return tcs.Task;
    }
  }
}
