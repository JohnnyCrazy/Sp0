using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using SpotifyAPI.Web;

namespace Sp0.Core
{
  internal class TracksCommand : ICommand
  {
    private IConsole _console;
    private ISpotifyService _spotifyService;

    public TracksCommand(IConsole console, ISpotifyService spotify)
    {
      _console = console;
      _spotifyService = spotify;
    }

    public void Register(CommandLineApplication app)
    {
      app.Command("tracks", (tracks) =>
      {
        tracks.Description = "Gets information about spotify tracks.";

        GetCommand(tracks);
        AudioFeaturesCommand(tracks);
        AudioAnalysisCommand(tracks);

        SubcommandHelper.ShowHelpOnExecute(tracks, _console);
      });
    }

    private void AudioAnalysisCommand(CommandLineApplication tracks)
    {
      tracks.Command("audio-analysis", (audioAnalysis) =>
      {
        audioAnalysis.Description = "Gets audio-analysis of one track.";

        var id = audioAnalysis.Argument<SpotifyUri>(
          "id|uri",
          "A Spotify track id or uri."
        ).IsRequired();

        var output = new OutputArgument(audioAnalysis);
        var onJson = output.WithFormat("json", true);

        audioAnalysis.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureCredentialsSet(out var spotify);

          var analysis = await spotify.Tracks.GetAudioAnalysis(id.ParsedValue.Id);

          onJson(() => _console.WriteObject(analysis));
        });
      });
    }

    private void AudioFeaturesCommand(CommandLineApplication tracks)
    {
      tracks.Command("audio-features", (audioFeatures) =>
      {
        audioFeatures.Description = "Gets audio-features of one or multiple tracks.";

        var ids = audioFeatures.Argument<List<SpotifyUri>>(
          "ids|uris",
          "A list of Spotify track ids or uris, seperated by comma."
        ).IsRequired();

        var output = new OutputArgument(audioFeatures);
        var onJson = output.WithFormat("json", true);

        audioFeatures.OnExecuteAsync(async (cancel) =>
        {
          _spotifyService.EnsureCredentialsSet(out var spotify);

          var features = await spotify.Tracks.GetSeveralAudioFeatures(new TracksAudioFeaturesRequest(
            ids.ParsedValue.Select((uri) => uri.Id).ToList()
          ));

          onJson(() => features.AudioFeatures.ForEach((track) => _console.WriteObject(track)));
        });
      });
    }

    private void GetCommand(CommandLineApplication tracks)
    {
      tracks.Command("get", (get) =>
      {
        get.Description = "Gets information about one or multiple tracks.";

        var ids = get.Argument<List<SpotifyUri>>(
          "ids|uris",
          "A list of Spotify track ids or uris, seperated by comma."
        ).IsRequired();

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

          var tracks = await spotify.Tracks.GetSeveral(new TracksRequest(
            ids.ParsedValue.Select((uri) => uri.Id).ToList()
          )
          { Market = market.Value() });

          onJson(() => tracks.Tracks.ForEach((track) => _console.WriteObject(track)));
          onId(() => tracks.Tracks.ForEach((track) => _console.WriteLine(track.Id)));
          onUri(() => tracks.Tracks.ForEach((track) => _console.WriteLine(track.Uri)));
        });
      });
    }
  }
}
