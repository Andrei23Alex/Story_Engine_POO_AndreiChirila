using System.Text.Json;
using System.Text.Json.Serialization;

namespace Story.Model;

[JsonConverter(typeof(ConditionDefinitionConverter))]
public abstract class ConditionDefinition
{
    [JsonIgnore]
    public abstract string NodeType { get; }
    public abstract ConditionDefinition Clone();
    public abstract override string ToString();
}

public class ComparisonCondition : ConditionDefinition
{
    public override string NodeType => "COMPARISON";
    public string Property { get; set; } = "";
    public string Operator { get; set; } = "==";
    public double Value { get; set; } = 0;
    public override ConditionDefinition Clone() =>
        new ComparisonCondition { Property = Property, Operator = Operator, Value = Value };
    public override string ToString() => $"{Property} {Operator} {Value}";
}

public class AndCondition : ConditionDefinition
{
    public override string NodeType => "AND";
    public List<ConditionDefinition> Conditions { get; set; } = new();
    public override ConditionDefinition Clone()
    {
        var c = new AndCondition();
        c.Conditions.AddRange(Conditions.Select(x => x.Clone()));
        return c;
    }
    public override string ToString() => $"AND ({Conditions.Count} condiții)";
}

public class OrCondition : ConditionDefinition
{
    public override string NodeType => "OR";
    public List<ConditionDefinition> Conditions { get; set; } = new();
    public override ConditionDefinition Clone()
    {
        var c = new OrCondition();
        c.Conditions.AddRange(Conditions.Select(x => x.Clone()));
        return c;
    }
    public override string ToString() => $"OR ({Conditions.Count} condiții)";
}

public class ConditionDefinitionConverter : JsonConverter<ConditionDefinition>
{
    public override ConditionDefinition? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ReadFromElement(doc.RootElement);
    }

    private static ConditionDefinition ReadFromElement(JsonElement el)
    {
        var type = el.GetProperty("type").GetString() ?? "";
        if (type == "COMPARISON")
        {
            return new ComparisonCondition
            {
                Property = el.GetProperty("property").GetString() ?? "",
                Operator = el.GetProperty("operator").GetString() ?? "==",
                Value = el.GetProperty("value").GetDouble()
            };
        }
        var children = el.GetProperty("conditions")
                         .EnumerateArray()
                         .Select(ReadFromElement)
                         .ToList();
        if (type == "AND") return new AndCondition { Conditions = children };
        if (type == "OR") return new OrCondition { Conditions = children };
        throw new JsonException($"Tip necunoscut de conditie: {type}");
    }

    public override void Write(
        Utf8JsonWriter writer, ConditionDefinition value, JsonSerializerOptions options)
    {
        WriteNode(writer, value);
    }

    private static void WriteNode(Utf8JsonWriter w, ConditionDefinition cond)
    {
        w.WriteStartObject();
        w.WriteString("type", cond.NodeType);
        switch (cond)
        {
            case ComparisonCondition c:
                w.WriteString("property", c.Property);
                w.WriteString("operator", c.Operator);
                w.WriteNumber("value", c.Value);
                break;
            case AndCondition a:
                w.WritePropertyName("conditions");
                w.WriteStartArray();
                foreach (var ch in a.Conditions) WriteNode(w, ch);
                w.WriteEndArray();
                break;
            case OrCondition o:
                w.WritePropertyName("conditions");
                w.WriteStartArray();
                foreach (var ch in o.Conditions) WriteNode(w, ch);
                w.WriteEndArray();
                break;
        }
        w.WriteEndObject();
    }
}