using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Postgirl.Domain.Persistence;

namespace Postgirl.Services;

public class StorageService
{
    private const string FileName = "postgirl_state.json";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private string GetPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Postgirl");

        Directory.CreateDirectory(dir);
        return Path.Combine(dir, FileName);
    }

    public async Task SaveAsync(AppState state)
    {
        var path = GetPath();
        var json = JsonSerializer.Serialize(state, _options);

        // atomic write
        var tmp = path + ".tmp";
        await File.WriteAllTextAsync(tmp, json);
        File.Move(tmp, path, true);
    }

    public async Task<AppState> LoadAsync()
    {
        var path = GetPath();

        if (!File.Exists(path))
            return new AppState();

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<AppState>(json, _options)
               ?? new AppState();
    }
}