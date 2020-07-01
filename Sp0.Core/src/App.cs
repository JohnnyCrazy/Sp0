using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;

namespace Sp0.Core
{
  public class App
  {
    private IConsole _console;

    public App(IConsole console)
    {
      _console = console;
    }

    public async Task<int> RunApp(string[] args, ServiceProvider services)
    {
      var app = new CommandLineApplication
      {
        Name = ApplicationConfig.AppName,
        Description = "A cross-platform CLI Tool for requesting the Spotify Web API, with a focus on automation.",
      };
      app.Conventions.UseDefaultConventions();
      app.ValueParsers.Add(new StringListConverter(','));
      app.ValueParsers.Add(new SpotifyUriListConverter(','));
      app.ValueParsers.Add(new SpotifyUriConverter());

      services
        .GetServices<ICommand>()
        .ToList()
        .ForEach(c => c.Register(app));

      SubcommandHelper.ShowHelpOnExecute(app, _console);

      app.ExtendedHelpText = $"\n\nThe config file is located at {ApplicationConfig.AppConfigFilePath}";
      app.VersionOption(
        "--version",
        () => $"sp0 version {Version.AppVersion}"
      );

      app.CompletionCommand("completion", new List<ICompletionProvider> { new ZSHCompletionProvider() });

      try
      {
        return await app.ExecuteAsync(args);
      }
      catch (APIException ex)
      {
        if (ex.Response != null)
        {
          _console.WriteObject(new { Status = ex.Response.StatusCode, Error = ex.Message });
        }
        return 1;
      }
    }
  }
}
