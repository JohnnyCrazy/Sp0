using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public static class CommandLineAppExtensions
  {
    public static CommandOption<T> OptionalOption<T>(
      this CommandLineApplication application,
      string template,
      string description,
      object defaultValue,
      CommandOptionType optionType
    )
    {
      return application.Option<T>(
        template,
        $"Optional: {description}, Default: {defaultValue}",
        optionType
      );
    }
  }
}
