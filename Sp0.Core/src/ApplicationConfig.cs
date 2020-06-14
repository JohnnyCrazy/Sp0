using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sp0.Core
{
  public class ApplicationConfig
  {
    public const string AppName = "sp0";
    public static string AppConfigPath = Path.Combine(
      Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData,
        Environment.SpecialFolderOption.Create
      ),
      AppName
    );
    public static string AppConfigFilePath = Path.Combine(
      AppConfigPath,
      "config.json"
    );

    public AccountConfig Account { get; } = new AccountConfig();

    public SpotifyAppConfig SpotifyApp { get; } = new SpotifyAppConfig();

    public SpotifyTokenConfig SpotifyToken { get; } = new SpotifyTokenConfig();


    public async Task Save()
    {
      await File.WriteAllTextAsync(AppConfigFilePath, JsonConvert.SerializeObject(this), Encoding.UTF8);
    }

    public void Delete()
    {
      File.Delete(AppConfigFilePath);
    }

    public static async Task<ApplicationConfig> Load()
    {
      Directory.CreateDirectory(AppConfigPath);
      if (!File.Exists(AppConfigFilePath))
      {
        var config = new ApplicationConfig();
        await config.Save();
        return config;
      }
      var configContent = await File.ReadAllTextAsync(AppConfigFilePath);
      return JsonConvert.DeserializeObject<ApplicationConfig>(configContent);
    }
  }

  public class AccountConfig
  {
    public string? Id { get; set; } = default!;

    public string? DisplayName { get; set; } = default!;

    public string? Uri { get; set; } = default!;
  }

  public class SpotifyAppConfig
  {
    public string? ClientId { get; set; } = default!;

    public string? ClientSecret { get; set; } = default!;
  }

  public class SpotifyTokenConfig
  {
    public string? AccessToken { get; set; } = default!;

    public string? RefreshToken { get; set; } = default!;

    public int? ExpiresIn { get; set; } = default!;

    public string? TokenType { get; set; } = default!;

    public DateTime? CreatedAt { get; set; } = default!;
  }
}
