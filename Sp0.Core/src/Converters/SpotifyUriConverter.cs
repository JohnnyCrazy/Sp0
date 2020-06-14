using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Sp0.Core
{
  public class SpotifyUriConverter : IValueParser
  {
    public Type TargetType { get => typeof(SpotifyUri); }

    public object? Parse(string? argName, string? value, CultureInfo culture)
    {
      if (value == null)
        return null;
      if (SpotifyUri.TryParse(value, out var uri))
        return uri;
      return new SpotifyUri(null, value);
    }
  }
}
