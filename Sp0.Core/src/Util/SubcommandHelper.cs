using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public static class SubcommandHelper
  {
    public static void ShowHelpOnExecute(CommandLineApplication app, IConsole console)
    {
      app.OnExecute(() =>
      {
        console.WriteLine("Please specify a subcommand.");
        app.ShowHelp();
        return 1;
      });
    }
  }
}
