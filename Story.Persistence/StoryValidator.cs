using Story.Model;

namespace Story.Persistence;

public class StoryValidator
{
    public List<string> Validate(StoryDefinition story, string? extractPath = null)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(story.Title))
            errors.Add("Titlul poveștii lipsește.");
        if (string.IsNullOrWhiteSpace(story.StartBlock))
            errors.Add("Blocul de start lipsește.");

        var blockIds = story.Blocks.Select(b => b.Id).ToHashSet();
        if (!string.IsNullOrWhiteSpace(story.StartBlock) && !blockIds.Contains(story.StartBlock))
            errors.Add($"Blocul de start '{story.StartBlock}' nu există.");

        story.Blocks.GroupBy(b => b.Id).Where(g => g.Count() > 1)
            .ToList().ForEach(g => errors.Add($"ID duplicat de bloc: '{g.Key}'."));

        var propKeys = story.Properties.Select(p => p.Key).ToHashSet();

        story.Properties.GroupBy(p => p.Key).Where(g => g.Count() > 1)
            .ToList().ForEach(g => errors.Add($"Cheie duplicată: '{g.Key}'."));

        foreach (var prop in story.Properties)
        {
            if (prop.Min > prop.Max)
                errors.Add($"Proprietatea '{prop.Key}': min > max.");
            if (prop.Initial < prop.Min || prop.Initial > prop.Max)
                errors.Add($"Proprietatea '{prop.Key}': inițialul {prop.Initial} nu e în [{prop.Min}, {prop.Max}].");
            if (!string.IsNullOrEmpty(prop.OnMinBlock) && !blockIds.Contains(prop.OnMinBlock))
                errors.Add($"Proprietatea '{prop.Key}': onMinBlock '{prop.OnMinBlock}' inexistent.");
            if (!string.IsNullOrEmpty(prop.OnMaxBlock) && !blockIds.Contains(prop.OnMaxBlock))
                errors.Add($"Proprietatea '{prop.Key}': onMaxBlock '{prop.OnMaxBlock}' inexistent.");
        }

        foreach (var block in story.Blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Id))
                errors.Add("Un bloc are ID-ul gol.");
            if (extractPath != null && !string.IsNullOrEmpty(block.BackgroundImage))
            {
                if (!File.Exists(Path.Combine(extractPath, block.BackgroundImage)))
                    errors.Add($"Bloc '{block.Id}': imaginea '{block.BackgroundImage}' lipsește.");
            }
            foreach (var dec in block.Decisions)
            {
                if (string.IsNullOrWhiteSpace(dec.TargetBlock))
                    errors.Add($"Bloc '{block.Id}', decizia '{dec.Text}': blocul țintă lipsește.");
                else if (!blockIds.Contains(dec.TargetBlock))
                    errors.Add($"Bloc '{block.Id}', decizia '{dec.Text}': target '{dec.TargetBlock}' inexistent.");

                foreach (var eff in dec.Effects)
                    if (!propKeys.Contains(eff.Property))
                        errors.Add($"Bloc '{block.Id}': efect pe proprietatea inexistentă '{eff.Property}'.");

                if (dec.Condition != null)
                    ValidateCondition(dec.Condition, propKeys, block.Id, errors);
            }
        }
        return errors;
    }

    private static void ValidateCondition(ConditionDefinition cond, HashSet<string> propKeys, string blockId, List<string> errors)
    {
        switch (cond)
        {
            case ComparisonCondition c:
                if (!propKeys.Contains(c.Property))
                    errors.Add($"Bloc '{blockId}': condiție pe proprietatea inexistentă '{c.Property}'.");
                break;
            case AndCondition a:
                a.Conditions.ForEach(ch => ValidateCondition(ch, propKeys, blockId, errors));
                break;
            case OrCondition o:
                o.Conditions.ForEach(ch => ValidateCondition(ch, propKeys, blockId, errors));
                break;
        }
    }
}