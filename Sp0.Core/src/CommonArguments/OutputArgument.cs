using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System;
using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public class OutputArgument
  {
    private CommandOption _option;
    private HashSet<string> _formats;
    private string? _defaultFormat;

    public OutputArgument(CommandLineApplication app)
    {
      _formats = new HashSet<string>();
      _option = app.Option("-o|--output", $"Specify the output format", CommandOptionType.SingleValue);
      _option.OnValidate((context) =>
      {
        var value = _option.Value();
        if (value == null || _formats.Contains(value))
          return ValidationResult.Success;

        throw new ValidationException($"The format {value} is not supported by the command!");
      });
    }

    public Action<Action> WithFormat(string format, bool isDefaultFormat = false)
    {
      if (isDefaultFormat)
        _defaultFormat = format;

      _formats.Add(format);
      UpdateDescription();

      return (callback) =>
      {
        var value = _option.Value();
        if ((value == null && isDefaultFormat) || value == format)
          callback();
      };
    }

    private void UpdateDescription()
    {
      _option.Description = $"Optional: Specify the output format. Available formats: {string.Join(",", _formats)}. Default: {_formats.First()}";
    }
  }
}
