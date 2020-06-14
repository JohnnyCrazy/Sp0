using System.Diagnostics.CodeAnalysis;

namespace Sp0.Core
{
  public class SpotifyUri
  {
    public SpotifyUri(string? type, string id)
    {
      Type = type;
      Id = id;
    }

    public string? Type { get; }

    public string Id { get; }

    public static bool TryParse(string? input, [NotNullWhen(true)] out SpotifyUri? uri)
    {
      uri = null;
      if (input == null)
        return false;
      var splitted = input.Split(':');
      if (splitted.Length < 3)
        return false;
      if (splitted[0] != "spotify")
        return false;

      uri = new SpotifyUri(splitted[1], splitted[2]);
      return true;
    }

    public override string ToString()
    {
      return $"spotify:{Type}:{Id}";
    }
  }
}
