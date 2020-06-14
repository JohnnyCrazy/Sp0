using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Sp0.Core
{
  public class StringListConverter : IValueParser
  {
    protected char _separator;

    public StringListConverter(char separator)
    {
      _separator = separator;
    }

    public Type TargetType { get => typeof(List<string>); }

    public object? Parse(string? argName, string? value, CultureInfo culture)
    {
      if (string.IsNullOrEmpty(value))
        return new List<string>();

      return value.Split(_separator).ToList();
    }
  }
}
