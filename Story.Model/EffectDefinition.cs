namespace Story.Model;

public class EffectDefinition
{
    public string Type { get; set; } = "ADD"; // "ADD" sau "SET"
    public string Property { get; set; } = "";
    public double Value { get; set; } = 0;
    public override string ToString() =>
        Type == "SET" ? $"{Property} = {Value}" : $"{Property} += {Value}";
}