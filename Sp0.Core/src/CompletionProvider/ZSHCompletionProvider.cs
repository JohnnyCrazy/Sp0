using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public class ZSHCompletionProvider : ICompletionProvider
  {
    public string Name => "zsh";

    public Task<string> GenerateCompletion(CommandLineApplication app)
    {
      var s = $@"function _{app.Name} {{
  _arguments -C \
    { string.Join("", app.Options.Select(o => CreateOptionCompletion(o))) }" +
  $@"{CreateCommandCompletion(app)} ""*::arg:->args""

  case $state in
    (args)
       if type ""_{app.Name}_$line[1]"" 2>/dev/null | grep -q 'function'
       then
         _{app.Name}_$line[1]
       fi
    ;;
  esac
}}
{string.Join('\n', app.Commands.Select(cmd => GenerateCommandFunction(app.Name, cmd)))}
compdef _{app.Name} {app.Name}
";
      return Task.FromResult(s);
    }

    private string GenerateCommandFunction(string? name, CommandLineApplication cmd)
    {
      var sb = new StringBuilder();
      sb.AppendLine($@"function _{name}_{cmd.Name} {{
  _arguments -C \
    { string.Join("", cmd.Options.Select(o => CreateOptionCompletion(o))) }" +
      $@"{CreateCommandCompletion(cmd)} ""*::arg:->args""

  case $state in
    (args)
      echo ""$line"" >> test.log
       echo ""_{name}_{cmd.Name}_$line[1]"" >> test.log
       if type ""_{name}_{cmd.Name}_$line[1]"" 2>/dev/null | grep -q 'function'
       then
         _{name}_{cmd.Name}_$line[1]
       fi
    ;;
  esac
}}");
      cmd.Commands.ForEach(nestedCmd => sb.AppendLine(GenerateCommandFunction($"{name}_{cmd.Name}", nestedCmd)));
      return sb.ToString();
    }

    private string CreateOptionCompletion(CommandOption option)
    {
      var sb = new StringBuilder();
      if (!string.IsNullOrEmpty(option.ShortName))
      {
        sb.AppendLine(option.OptionType switch
        {
          CommandOptionType.NoValue => (
            $@"'(--{option.LongName} -{option.ShortName})'{{--{option.LongName},-{option.ShortName}}}'[{option.Description?.ShellEscape()}]' \"
          ),
          CommandOptionType.SingleValue => (
            $@"'(--{option.LongName} -{option.ShortName})'{{--{option.LongName},-{option.ShortName}}}'[{option.Description?.ShellEscape()}]:value:' \"
          ),
          _ => (
            $@"'(--{option.LongName} -{option.ShortName})'{{--{option.LongName},-{option.ShortName}}}'[{option.Description?.ShellEscape()}]' \"
          ),
        });
      }
      else
      {
        sb.AppendLine(option.OptionType switch
        {
          CommandOptionType.NoValue => ($@"'--{option.LongName}[{option.Description?.ShellEscape()}]' \"),
          CommandOptionType.SingleValue => ($@"'--{option.LongName}[{option.Description?.ShellEscape()}]:value:' \"),
          _ => ($@"'--{option.LongName}[{option.Description?.ShellEscape()}]' \"),
        });
      }
      return sb.ToString();
    }

    private string CreateCommandCompletion(CommandLineApplication app)
    {
      var sb = new StringBuilder();
      var commands = string.Join(" ", app.
        Commands.Select(a => $"{a.Name}\\:'{a.Description?.ShellEscape() ?? string.Empty}'")
      );
      sb.AppendLine($@"""1: :(({commands}))"" \");
      return sb.ToString();
    }
  }
}
