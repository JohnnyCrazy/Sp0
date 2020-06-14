using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  internal class ConfigCommand : ICommand
  {
    private IConsole _console;
    private ApplicationConfig _config;

    public ConfigCommand(IConsole console, ApplicationConfig config)
    {
      _console = console;
      _config = config;
    }
    public void Register(CommandLineApplication app)
    {
      app.Command("config", (config) =>
      {
        config.Description = "Update or delete config values";

        var clear = config.Option("-d|--delete", "Delete the config file", CommandOptionType.NoValue);
        var clientId = config.Option(
          "--client-id", "Sets the client id of your spotify application", CommandOptionType.SingleValue
        );
        var clientSecret = config.Option(
          "--client-secret", "Sets the client secret of your spotify application", CommandOptionType.SingleValue
        );

        config.OnExecuteAsync(async (cancel) =>
        {
          if (clear.HasValue())
          {
            _console.WriteLine("The config file has been deleted");
            _config.Delete();
            return;
          }

          if (clientId.HasValue())
            _config.SpotifyApp.ClientId = clientId.Value();

          if (clientSecret.HasValue())
            _config.SpotifyApp.ClientSecret = clientSecret.Value();

          await _config.Save();
        });
      });
    }
  }
}
