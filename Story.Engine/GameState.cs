using Story.Model;

namespace Story.Engine;

public class GameState
{
    public string CurrentBlockId { get; set; } = "";
    public Dictionary<string, double> Properties { get; set; } = new();

    public void Initialize(StoryDefinition story)
    {
        CurrentBlockId = story.StartBlock;
        Properties.Clear();
        foreach (var p in story.Properties)
            Properties[p.Key] = p.Initial;
    }

    public GameState Clone() => new()
    {
        CurrentBlockId = CurrentBlockId,
        Properties = new Dictionary<string, double>(Properties)
    };
}