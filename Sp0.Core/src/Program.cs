using System.Threading;
using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Swan.Logging;

namespace Sp0.Core
{
  public class Program
  {
    public static async Task<int> Main(string[] args)
    {
      Logger.NoLogging();
      // This is a bug in the SWAN Logging library, need this hack to bring back the cursor
      AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exiting();
      Thread.GetDomain().UnhandledException += (sender, e) => Exiting();

      var config = await ApplicationConfig.Load();
      var services = new ServiceCollection()
          .AddSingleton(config)
          .AddSingleton<IConsole>(PhysicalConsole.Singleton)
          .AddSingleton<App>()
          .AddSingleton<ISpotifyService, SpotifyService>()
          .AddSingleton<ILoginService, LoginService>()
          .AddSingleton<ICommand, LoginCommand>()
          .AddSingleton<ICommand, LogoutCommand>()
          .AddSingleton<ICommand, ConfigCommand>()
          .AddSingleton<ICommand, UserProfileCommand>()
          .AddSingleton<ICommand, TracksCommand>()
          .AddSingleton<ICommand, PlayerCommand>()
          .AddSingleton<ICommand, PlaylistsCommand>()
          .BuildServiceProvider();

      return await services.GetService<App>().RunApp(args, services);
    }

    private static void Exiting()
    {
      Console.CursorVisible = true;
    }
  }
}
