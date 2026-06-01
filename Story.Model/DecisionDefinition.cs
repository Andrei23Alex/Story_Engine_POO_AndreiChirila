namespace Story.Model;

public class DecisionDefinition
{
    public string Text { get; set; } = "";
    public string TargetBlock { get; set; } = "";
    public string? Icon { get; set; }
    public ConditionDefinition? Condition { get; set; }
    public List<EffectDefinition> Effects { get; set; } = new();
}