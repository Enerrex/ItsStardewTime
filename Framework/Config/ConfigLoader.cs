using StardewModdingAPI;

namespace ItsStardewTime.Framework.Config;

internal static class ConfigLoader
{
    /// <summary>
    /// Load config.json. If parsing fails, back it up (if present), recreate defaults, and log what happened.
    /// </summary>
    internal static ModConfig LoadOrResetConfig(IModHelper helper, IMonitor monitor)
    {
        const string configName = "config.json";

        try
        {
            // ReadConfig will create config.json if it doesn't exist.
            var config = helper.ReadConfig<ModConfig>();
            return config;
        }
        catch (Exception ex)
        {
            monitor.Log(
                $"Failed to read {configName}. The file is likely invalid JSON or incompatible with this version. " +
                "Backing up the file (if it exists) to preserve values and creating a default config.",
                LogLevel.Warn
            );
            monitor.Log(ex.ToString());

            // Try to back up the config file, if it exists on disk.
            try
            {
                // DirectoryPath is the folder containing manifest.json.
                string mod_dir = helper.DirectoryPath;
                string config_path = Path.Combine(mod_dir, configName);

                if (File.Exists(config_path))
                {
                    string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
                    string backup_path = Path.Combine(mod_dir, $"config.invalid.{timestamp}.json");

                    // Avoid overwriting
                    if (File.Exists(backup_path))
                        backup_path = Path.Combine(mod_dir, $"config.invalid.{timestamp}.{Guid.NewGuid():N}.json");

                    File.Copy(config_path, backup_path, overwrite: false);

                    monitor.Log($"Backed up invalid config to: {Path.GetFileName(backup_path)}", LogLevel.Info);
                }
                else
                {
                    monitor.Log("No existing config.json found to back up.", LogLevel.Info);
                }
            }
            catch (Exception backup_ex)
            {
                monitor.Log("Failed to back up config.json. Proceeding to recreate defaults anyway.", LogLevel.Warn);
                monitor.Log(backup_ex.ToString());
            }

            // Recreate defaults and write them out.
            try
            {
                var defaults = new ModConfig();
                helper.WriteConfig(defaults);

                monitor.Log("Recreated config.json with default values.", LogLevel.Info);
                return defaults;
            }
            catch (Exception write_ex)
            {
                monitor.Log("Failed to recreate config.json. The mod may not function correctly until this is fixed.", LogLevel.Error);
                monitor.Log(write_ex.ToString());

                // Last resort: return defaults in-memory so the mod can limp along.
                return new ModConfig();
            }
        }
    }
}