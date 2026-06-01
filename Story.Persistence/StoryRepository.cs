using Story.Model;
using Story.Engine;
using System.IO.Compression;
using System.Text.Json;

namespace Story.Persistence;

public class StoryRepository
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public (StoryDefinition story, string extractPath) LoadFromZip(string zipPath)
    {
        string tmp = Path.Combine(Path.GetTempPath(), "StoryEngine_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        ZipFile.ExtractToDirectory(zipPath, tmp);

        string jsonPath = Path.Combine(tmp, "story.json");
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("Fișierul story.json lipsește din arhivă!");
        }

        string json = File.ReadAllText(jsonPath);
        var story = JsonSerializer.Deserialize<StoryDefinition>(json, Opts)
                    ?? throw new InvalidDataException("Fișier de poveste invalid.");

        // --- VALIDARE ȘI REPARARE STRUCTURALĂ DE URGENȚĂ ---
        if (story.Blocks == null)
        {
            story.Blocks = new List<StoryBlock>();
        }

        // Eliminăm blocurile complet null care pot apărea la conversie defectuoasă
        story.Blocks = story.Blocks.Where(b => b != null).ToList();

        // Dacă nu avem deloc blocuri, creăm unul de siguranță ca să nu crape aplicația
        if (story.Blocks.Count == 0)
        {
            story.Blocks.Add(new StoryBlock { Id = "start", Text = "Povestea nu conține blocuri vizibile în acest moment." });
        }

        // Curățăm și normalizăm fiecare ID în mod individual
        for (int i = 0; i < story.Blocks.Count; i++)
        {
            var b = story.Blocks[i];

            // Dacă ID-ul este gol sau lipsă, îi dăm o denumire unică
            if (string.IsNullOrWhiteSpace(b.Id))
            {
                b.Id = (i == 0) ? "start" : "bloc_" + i;
            }
            else
            {
                b.Id = b.Id.Trim();
                // Forțăm termenul 'start' să fie mereu scris cu litere mici
                if (string.Equals(b.Id, "start", StringComparison.OrdinalIgnoreCase))
                {
                    b.Id = "start";
                }
            }

            if (b.Decisions == null) b.Decisions = new List<DecisionDefinition>();
        }

        // Siguranță supremă: Dacă niciun bloc nu se numește "start", îl forțăm pe primul să devină start
        if (!story.Blocks.Any(b => b.Id == "start"))
        {
            story.Blocks[0].Id = "start";
        }

        return (story, tmp);
    }

    public void SaveToZip(StoryDefinition story, string extractPath, string zipPath)
    {
        string jsonPath = Path.Combine(extractPath, "story.json");
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(story, Opts));
        string imgDir = Path.Combine(extractPath, "images");
        if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);
        if (File.Exists(zipPath)) File.Delete(zipPath);
        ZipFile.CreateFromDirectory(extractPath, zipPath);
    }

    public void SaveGameState(GameState state, string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(state, Opts));

    public GameState LoadGameState(string path)
    {
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GameState>(json, Opts) ?? new GameState();
    }
}