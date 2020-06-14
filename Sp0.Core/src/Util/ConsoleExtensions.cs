using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpotifyAPI.Web.Auth;

namespace Sp0.Core
{
  public static class ConsoleExtensions
  {
    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
      ContractResolver = new DefaultContractResolver
      {
        NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = false }
      },
      Formatting = Formatting.None
    };
    public static void WithColor(this IConsole console, ConsoleColor color, Action callback)
    {
      console.ForegroundColor = color;
      callback();
      console.ResetColor();
    }

    public static void WriteObject(this IConsole console, object value)
    {
      console.WriteLine(JsonConvert.SerializeObject(value, settings));
    }
  }
}
