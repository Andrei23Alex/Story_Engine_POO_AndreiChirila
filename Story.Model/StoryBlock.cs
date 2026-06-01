namespace Story.Model;

public class StoryBlock
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsFinal { get; set; } = false;
    public string? BackgroundImage { get; set; }
    public List<DecisionDefinition> Decisions { get; set; } = new();
}