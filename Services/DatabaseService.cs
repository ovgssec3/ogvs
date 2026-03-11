using System.Text.Json;
using CecGradingSystem.Models;

namespace CecGradingSystem.Services;

public class DatabaseService
{
    private const string DATA_FILE = "grading_data.json";

    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        WriteIndented               = true
    };

    private GradingDatabase _db = new();

    public DatabaseService()
    {
        Load();
    }

    public GradingDatabase Db => _db;

    public void Save()
    {
        File.WriteAllText(DATA_FILE, JsonSerializer.Serialize(_db, _opts));
    }

    private void Load()
    {
        if (!File.Exists(DATA_FILE)) { _db = new GradingDatabase(); return; }
        try
        {
            var json = File.ReadAllText(DATA_FILE);
            _db = JsonSerializer.Deserialize<GradingDatabase>(json, _opts) ?? new GradingDatabase();
            Console.WriteLine($"[DB] Loaded: {_db.Students.Count} students, {_db.StudentGrades.Count} grade records.");
        }
        catch { _db = new GradingDatabase(); Console.WriteLine("[DB] Could not parse database. Starting fresh."); }
    }
}
