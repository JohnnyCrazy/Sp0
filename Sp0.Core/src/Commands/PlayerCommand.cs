using System.Runtime.Intrinsics.X86;
using System.Linq;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web;

namespace Sp0.Core
{
  public class PlayerCommand : ICommand
  {
    private IConsole _console;
    private ISpotifyService _spotifyService;

    public PlayerCommand(IConsole console, ISpotifyService spotify)
    {
      _console = console;
      _spotifyService = spotify;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("player", (player) =>
      {
        player.Description = "Interact with Spotify Players over the Spotify Connect API Endpoints";

        PauseCommand(player);
        PlayCommand(player);
        QueueCommand(player);
        DevicesCommand(player);
        NextCommand(player);
        PreviousCommand(player);
        TransferCommand(player);
        CurrentPlaybackCommand(player);
        SeekPlaybackCommand(player);
        VolumeCommand(player);

        SubcommandHelper.ShowHelpOnExecute(player, _console);
      });
    }

    private CommandOption DeviceIdOption(CommandLineApplication app)
    {
      return app.Option(
        "-d|--device-id",
        "Optional: The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.",
        CommandOptionType.SingleValue
      );
    }

    private void VolumeCommand(CommandLineApplication player)
    {
      player.Command("volume", (volume) =>
      {
        volume.Description = "Set the volume for the user’s current playback device.";

        var volumePercent = volume.Argument<int>(
          "volume_percent",
          "The volume to set. Must be a value from 0 to 100 inclusive."
        ).IsRequired();

        var deviceId = DeviceIdOption(volume);
        volume.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var volumeResult = await spotify.Player.SetVolume(new PlayerVolumeRequest(volumePercent.ParsedValue)
          {
            DeviceId = deviceId.Value()
          });
          _console.WriteObject(volumeResult);
        });
      });
    }

    private void SeekPlaybackCommand(CommandLineApplication player)
    {
      player.Command("seek", (seek) =>
      {
        seek.Description = "Seeks to the given position in the user’s currently playing track.";

        var deviceId = DeviceIdOption(seek);

        var positionMs = seek.Argument<long>(
          "positionMs",
          "The position in milliseconds to seek to. Must be a positive number."
        ).IsRequired();

        seek.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var seekResult = await spotify.Player.SeekTo(new PlayerSeekToRequest(positionMs.ParsedValue)
          {
            DeviceId = deviceId.Value()
          });
          _console.WriteObject(seekResult);
        });
      });
    }

    private void CurrentPlaybackCommand(CommandLineApplication player)
    {
      player.Command("current-playback", (currentPlayback) =>
      {
        currentPlayback.Description = "Get information about the user’s current playback state, including track or episode, progress, and active device.";

        var market = currentPlayback.Option<string>(
          "-m|--market",
          "Optional: The specifc market, used for track re-linking",
          CommandOptionType.SingleValue
        );

        currentPlayback.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var currentPlaybackResult = await spotify.Player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest
          {
            Market = market.Value()
          });
          _console.WriteObject(currentPlaybackResult);
        });
      });
    }

    private void TransferCommand(CommandLineApplication player)
    {
      player.Command("transfer", (transfer) =>
      {
        transfer.Description = "Transfer playback to a new device and determine if it should start playing.";

        var deviceId = transfer.Argument(
          "device-id",
          "ID of the device on which playback should be started/transferred."
        ).IsRequired();

        var play = transfer.Option(
          "-p|--play",
          "true: ensure playback happens on new device. false or not provided: keep the current playback state",
          CommandOptionType.NoValue
        );

        transfer.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var transferResult = await spotify.Player.TransferPlayback(
            new PlayerTransferPlaybackRequest(new List<string> {
              deviceId.Value!
            })
            {
              Play = play.HasValue()
            }
          );
          _console.WriteObject(transferResult);
        });
      });
    }

    private void NextCommand(CommandLineApplication player)
    {
      player.Command("next", (next) =>
      {
        next.Description = "Skips to next track in the user’s queue.";

        var deviceId = DeviceIdOption(next);

        next.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var nextResult = await spotify.Player.SkipNext(new PlayerSkipNextRequest
          {
            DeviceId = deviceId.Value()
          });
          _console.WriteObject(nextResult);
        });
      });
    }

    private void PreviousCommand(CommandLineApplication player)
    {
      player.Command("previous", (previous) =>
      {
        previous.Description = "Skips to previous track in the user’s queue.";

        var deviceId = DeviceIdOption(previous);

        previous.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var previousResult = await spotify.Player.SkipPrevious(new PlayerSkipPreviousRequest
          {
            DeviceId = deviceId.Value()
          });
          _console.WriteObject(previousResult);
        });
      });
    }

    private void DevicesCommand(CommandLineApplication player)
    {
      player.Command("devices", (devices) =>
      {
        devices.Description = "Get information about a user’s available devices.";

        var output = new OutputArgument(devices);
        var onJson = output.WithFormat("json", true);
        var onId = output.WithFormat("id");

        devices.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var devices = await spotify.Player.GetAvailableDevices();
          onJson(() => _console.WriteObject(devices.Devices));
          onId(() => devices.Devices.Select((device) => device.Id).ToList().ForEach(id => _console.WriteLine(id)));
        });
      });
    }

    private void QueueCommand(CommandLineApplication player)
    {
      player.Command("queue", (queue) =>
      {
        queue.Description = "Add an Item to the User's Playback Queue ";

        var uri = queue.Argument(
          "uri",
          "The uri of the item to add to the queue. Must be a track or an episode uri."
        ).IsRequired();

        var deviceId = DeviceIdOption(queue);

        queue.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var queued = await spotify.Player.AddToQueue(new PlayerAddToQueueRequest(uri.Value!));
          _console.WriteObject(queued);
        });
      });
    }

    private void PlayCommand(CommandLineApplication player)
    {
      player.Command("play", (play) =>
      {
        play.Description = "Start/Resume Playback";

        var deviceId = DeviceIdOption(play);

        var contextUri = play.Option(
          "-c|--context-uri",
          "Optional: Spotify URI of the context to play. Valid contexts are albums, artists, playlists.",
          CommandOptionType.SingleValue
        );

        var uris = play.Option<List<string>>(
          "-u|--uris",
          "Optional: A list of Spotify track URIs to play, seperated by comma",
          CommandOptionType.SingleValue
        );

        var positionMs = play.Option<int?>(
          "-p|--positon-ms",
          "Optional: Indicates from what position to start playback. Must be a positive number. Passing in a position that is greater than the length of the track will cause the player to start playing the next song.",
          CommandOptionType.SingleValue
        );

        var offset = play.Option<string>(
          "-o|--offset",
          "Optional: Indicates from where in the context playback should start. Only available when context-uri corresponds to an album or playlist object, or when the uris parameter is used. Either a zero based position or URI",
          CommandOptionType.SingleValue
        );

        play.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);
          var position = 0;
          var isOffsetPosition = offset.HasValue() ? int.TryParse(offset.Value(), out position) : false;

          var resume = await spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest
          {
            DeviceId = deviceId.Value(),
            ContextUri = contextUri.Value(),
            Uris = uris.HasValue() ? uris.ParsedValue : null,
            PositionMs = positionMs.ParsedValue,
            OffsetParam = offset.HasValue() ? new PlayerResumePlaybackRequest.Offset
            {
              Position = isOffsetPosition ? position : (int?)null,
              Uri = offset.HasValue() && !isOffsetPosition ? offset.Value() : null,
            } : null
          });
          _console.WriteObject(resume);
        });
      });
    }

    private void PauseCommand(CommandLineApplication player)
    {
      player.Command("pause", (pause) =>
      {
        pause.Description = "Pause Playback";

        var deviceId = DeviceIdOption(pause);

        pause.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureUserLoggedIn(out var spotify);

          var pause = await spotify.Player.PausePlayback(new PlayerPausePlaybackRequest
          {
            DeviceId = deviceId.Value()
          });
          _console.WriteObject(pause);
        });
      });
    }
  }
}
