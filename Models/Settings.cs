using System.IO;
using Newtonsoft.Json;

namespace EnvironMint.Models
{
    public class Settings
    {
        public List<string> ScanDrives { get; set; } = new List<string>();

        public bool AutoScanOnStartup { get; set; } = false;

        public string DefaultScriptLanguage { get; set; } = "PowerShell";

        public Settings()
        {
            ScanDrives = new List<string> { "C:" };
        }

        // saving:loading (static)
        public static string GetSettingsFilePath()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EnvironMint");

            Directory.CreateDirectory(appDataPath);
            return Path.Combine(appDataPath, "settings.json");
        }

        public static Settings Load()
        {
            string settingsPath = GetSettingsFilePath();

            if (File.Exists(settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    return settings ?? new Settings();
                }
                catch
                {
                    // if error revert to defaults
                    return new Settings();
                }
            }

            return new Settings();
        }

        public void Save()
        {
            string settingsPath = GetSettingsFilePath();
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(settingsPath, json);
        }
    }
}
