using System.Text.Json;

namespace MaterialDepot.Core.Settings;

/// <summary>
/// Settings as a JSON file, by default <c>%APPDATA%\MaterialDepot\settings.json</c>.
/// </summary>
public sealed class JsonSettingsStore(string path, Func<DepotSettings> createDefault) : ISettingsStore
{
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public bool HasSavedSettings => File.Exists(path);

    public DepotSettings Load()
    {
        if (!File.Exists(path))
        {
            return createDefault();
        }

        try
        {
            return JsonSerializer.Deserialize<DepotSettings>(File.ReadAllText(path), options) ?? createDefault();
        }
        catch (JsonException)
        {
            return createDefault();
        }
    }

    public void Save(DepotSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, options));
    }
}
