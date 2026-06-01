namespace Story.Model;

public class StoryDefinition
{
    public string Title { get; set; } = "";
    public string StartBlock { get; set; } = "";
    public List<StatePropertyDefinition> Properties { get; set; } = new();
    public List<StoryBlock> Blocks { get; set; } = new();
}