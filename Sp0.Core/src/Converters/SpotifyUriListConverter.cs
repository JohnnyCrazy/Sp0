using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Sp0.Core
{
  public class SpotifyUriListConverter : IValueParser
  {
    protected char _separator;

    public SpotifyUriListConverter(char separator)
    {
      _separator = separator;
    }

    public Type TargetType { get => typeof(List<SpotifyUri>); }

    public object? Parse(string? argName, string? value, CultureInfo culture)
    {
      if (string.IsNullOrEmpty(value))
        return new List<SpotifyUri>();

      return value.Split(_separator).Select(val =>
      {
        if (SpotifyUri.TryParse(val, out var uri))
          return uri;
        return new SpotifyUri(null, val);
      }).ToList();
    }
  }
}
