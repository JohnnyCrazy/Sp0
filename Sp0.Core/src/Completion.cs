using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  /// <summary>
  /// This will be pulled into another module
  /// </summary>
  public class Completion
  {

  }

  public interface ICompletionProvider
  {
    string Name { get; }
    Task<string> GenerateCompletion(CommandLineApplication app);
  }

  public static class CompletionExtensions
  {
    public static void CompletionCommand(this CommandLineApplication app, string name, List<ICompletionProvider> providers)
    {
      app.Command(name, (completion) =>
      {
        completion.Description = "Output shell completion code for the specified shell.";

        var shell = completion.Argument<string>(
          "shell",
          $"The shell you want to generate completion code for. Possible values: {string.Join(",", providers.Select(p => p.Name))}"
        ).IsRequired().Accepts((arg) => arg.Values(providers.Select(p => p.Name).ToArray()));

        completion.OnExecuteAsync(async (cancel) =>
        {
          var provider = providers.Where(p => p.Name == shell.Value).FirstOrDefault();
          if (provider != null)
          {
            var completion = await provider.GenerateCompletion(app);
            // TODO: Don't use global Console
            Console.WriteLine(completion);
          }
        });
      });
    }
  }
}
