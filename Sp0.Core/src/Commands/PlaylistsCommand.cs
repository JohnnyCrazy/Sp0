using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web;

namespace Sp0.Core
{
  internal class PlaylistsCommand : ICommand
  {
    private IConsole _console;
    private ISpotifyService _spotifyService;
    private ApplicationConfig _config;

    public PlaylistsCommand(IConsole console, ISpotifyService spotifyService, ApplicationConfig config)
    {
      _console = console;
      _spotifyService = spotifyService;
      _config = config;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("playlists", (playlists) =>
      {
        playlists.Description = "Gets information and manage spotify playlists.";

        CreateCommand(playlists);
        ChangeCommand(playlists);
        GetCommand(playlists);
        GetItemsCommand(playlists);
        AddItemsCommand(playlists);
        OfUserCommand(playlists);

        SubcommandHelper.ShowHelpOnExecute(playlists, _console);
      });
    }

    private void OfUserCommand(CommandLineApplication playlists)
    {
      playlists.Command("of-user", (ofUser) =>
      {
        ofUser.Description = "Get a List of a User's Playlists";

        var userId = ofUser.OptionalOption<string>(
          "-u|--user-id",
          "The user’s Spotify user ID.",
          "Currently logged in user",
          CommandOptionType.SingleValue
        );

        var paging = new PagingOptions(ofUser);

        var output = new OutputArgument(ofUser);
        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");
        var onUri = output.WithFormat("uri");

        ofUser.OnExecuteAsync(async (cancel) =>
        {
          if (userId.HasValue())
          {
            _spotifyService.EnsureCredentialsSet(out var spotify);
            var page = await spotify.Playlists.GetUsers(userId.Value()!, new PlaylistGetUsersRequest
            {
              Limit = paging.Limit.ParsedValue ?? 50,
              Offset = paging.Offset.ParsedValue ?? 0
            });
            if (paging.All.HasValue())
            {
              var allPages = await spotify.PaginateAll(page);
              onJson(() => allPages.ToList().ForEach((playlist) => _console.WriteObject(playlist)));
              onId(() => allPages.ToList().ForEach((playlist) => _console.WriteLine(playlist.Id)));
              onUri(() => allPages.ToList().ForEach((playlist) => _console.WriteLine(playlist.Uri)));
            }
            else
            {
              onJson(() => _console.WriteObject(page));
              onId(() => page.Items.ForEach((item) => _console.WriteLine(item.Id)));
              onUri(() => page.Items.ForEach((item) => _console.WriteLine(item.Uri)));
            }
          }
          else
          {
            _spotifyService.EnsureUserLoggedIn(out var spotify);
            var page = await spotify.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest
            {
              Limit = paging.Limit.ParsedValue ?? 50,
              Offset = paging.Offset.ParsedValue ?? 0
            });
            if (paging.All.HasValue())
            {
              var allPages = await spotify.PaginateAll(page);
              onJson(() => allPages.ToList().ForEach((playlist) => _console.WriteObject(playlist)));
              onId(() => allPages.ToList().ForEach((playlist) => _console.WriteLine(playlist.Id)));
              onUri(() => allPages.ToList().ForEach((playlist) => _console.WriteLine(playlist.Uri)));
            }
            else
            {
              onJson(() => _console.WriteObject(page));
              onId(() => page.Items.ForEach((item) => _console.WriteLine(item.Id)));
              onUri(() => page.Items.ForEach((item) => _console.WriteLine(item.Uri)));
            }
          }
        });
      });
    }

    private void AddItemsCommand(CommandLineApplication playlists)
    {
      playlists.Command("add-items", (addItems) =>
      {
        addItems.Description = "Add tracks/episodes to a playlist";

        var playlistId = addItems.Argument("playlist-id", "The Spotify ID for the playlist.").IsRequired();

        var uris = addItems.Argument<List<SpotifyUri>>(
          "uris",
          "A comma-separated list of Spotify URIs to add, can be track or episode URIs."
        ).IsRequired();

        addItems.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var addItemsResponse = await spotify.Playlists.AddItems(playlistId.Value!, new PlaylistAddItemsRequest(
            uris.ParsedValue.Select((uri) => uri.ToString()).ToList()
          ));
          _console.WriteLine(addItemsResponse.SnapshotId);
        });
      });
    }

    private void GetItemsCommand(CommandLineApplication playlists)
    {
      playlists.Command("get-items", (getTracks) =>
      {
        getTracks.Description = "Get the tracks/episodes of a playlist.";

        var playlistId = getTracks.Argument("playlist-id", "The Spotify ID for the playlist.").IsRequired();

        var market = getTracks.OptionalOption<string>(
           "-m|--market",
           "The specifc market, used for track re-linking",
           "none",
           CommandOptionType.SingleValue
        );
        var paging = new PagingOptions(getTracks);

        var output = new OutputArgument(getTracks);
        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");
        var onUri = output.WithFormat("uri");

        getTracks.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureCredentialsSet(out var spotify);
          var page = await spotify.Playlists.GetItems(playlistId.Value!, new PlaylistGetItemsRequest
          {
            Limit = paging.Limit.ParsedValue ?? 100,
            Offset = paging.Offset.ParsedValue ?? 0,
            Market = market.Value()
          });
          if (paging.All.HasValue())
          {
            var allPages = await spotify.PaginateAll(page);
            onJson(() => allPages.ToList().ForEach((item) => _console.WriteObject(item)));
            onId(() => allPages.ToList().ForEach((item) => item.Track.ToId()));
            onUri(() => allPages.ToList().ForEach((item) => item.Track.ToUri()));
          }
          else
          {
            onJson(() => _console.WriteObject(page));
            onId(() => page.Items.ForEach((item) => _console.WriteLine(item.Track.ToId())));
            onUri(() => page.Items.ForEach((item) => _console.WriteLine(item.Track.ToUri())));
          }
        });
      });
    }

    private void GetCommand(CommandLineApplication playlists)
    {
      playlists.Command("get", (get) =>
      {
        get.Description = "Get a playlist owned by a Spotify user.";

        var playlistId = get.Argument("playlist-id", "The Spotify ID for the playlist.").IsRequired();

        var market = get.OptionalOption<string>(
          "-m|--market",
          "The specifc market, used for track re-linking",
          "none",
          CommandOptionType.SingleValue
        );

        var output = new OutputArgument(get);
        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");
        var onUri = output.WithFormat("uri");

        get.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureCredentialsSet(out var spotify);

          var playlistResult = await spotify.Playlists.Get(playlistId.Value!);
          onJson(() => _console.WriteObject(playlistResult));
          onId(() => _console.WriteLine(playlistResult.Id));
          onUri(() => _console.WriteLine(playlistResult.Uri));
        });
      });
    }

    private void ChangeCommand(CommandLineApplication playlists)
    {
      playlists.Command("change", (change) =>
      {
        change.Description = "Change a playlist’s name and public/private state. (The user must, of course, own the playlist.)";

        var playlistId = change
          .Argument("playlist-id", "The Spotify ID for the playlist.")
          .IsRequired();

        var name = change.OptionalOption<string?>(
          "-n|--name",
          "The new name for the playlist",
          "none",
          CommandOptionType.SingleValue);

        var privateFlag = change.OptionalOption<bool?>(
          "-p|--private",
          "If set, the playlist will be private.",
          "unset",
          CommandOptionType.NoValue);

        var collaborative = change.OptionalOption<bool?>(
          "-c|--collaborative",
          "If set, the playlist will be collaborative",
          "unset",
          CommandOptionType.NoValue);

        var description = change.OptionalOption<string?>(
          "-d|--description",
          "Value for playlist description as displayed in Spotify Clients and in the Web API.",
          "none",
          CommandOptionType.SingleValue);

        change.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var changeResult = await spotify.Playlists.ChangeDetails(playlistId.Value!,
            new PlaylistChangeDetailsRequest
            {
              Name = name.Value(),
              Collaborative = collaborative.HasValue() ? true : false,
              Description = description.ParsedValue,
              Public = privateFlag.HasValue() ? false : true,
            });
          _console.WriteObject(changeResult);
        });
      });
    }

    private void CreateCommand(CommandLineApplication playlists)
    {
      playlists.Command("create", (create) =>
      {
        create.Description = "Create a playlist for a Spotify user. (The playlist will be empty until you add tracks.)";

        var name = create
          .Argument("name", "The name for the new playlist. This name does not need to be unique")
          .IsRequired();

        var userId = create.OptionalOption<string?>(
          "-u|--user-id",
          "The user’s Spotify user ID",
          "logged-in user id",
          CommandOptionType.SingleValue);

        var privateFlag = create.OptionalOption<bool?>(
          "-p |--private",
          "If set, the playlist will be private",
          "unset",
          CommandOptionType.NoValue);

        var collaborative = create.OptionalOption<bool?>(
          "-c|--collaborative",
          "If set the playlist will be collaborative",
          "unset",
          CommandOptionType.NoValue);

        var description = create.OptionalOption<string?>(
          "-d|--description",
          "Value for playlist description as displayed in Spotify Clients and in the Web API",
          "none",
          CommandOptionType.SingleValue);

        var output = new OutputArgument(create);
        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");
        var onUri = output.WithFormat("uri");


        create.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var createResult = await spotify.Playlists.Create(
            userId.Value() ?? _config.Account.Id!,
            new PlaylistCreateRequest(name.Value!)
            {
              Collaborative = collaborative.HasValue() ? true : false,
              Description = description.ParsedValue,
              Public = privateFlag.HasValue() ? false : true
            });
          onJson(() => _console.WriteObject(createResult));
          onId(() => _console.WriteLine(createResult.Id));
          onUri(() => _console.WriteLine(createResult.Uri));
        });
      });
    }
  }
}
