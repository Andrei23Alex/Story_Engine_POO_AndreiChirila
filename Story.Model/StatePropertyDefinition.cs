namespace Story.Model;

public class StatePropertyDefinition
{
    public string Key { get; set; } = "";
    public string HudLabel { get; set; } = "";
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 100;
    public double Initial { get; set; } = 0;
    public bool VisibleInHud { get; set; } = true;
    public int HudOrder { get; set; } = 0;
    public string? OnMinBlock { get; set; }
    public string? OnMaxBlock { get; set; }
}