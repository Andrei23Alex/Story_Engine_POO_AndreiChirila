using Story.Model;

namespace Story.Engine;

public class GameEngine
{
    private readonly StoryDefinition _story;
    private readonly Dictionary<string, StoryBlock> _blockMap;
    private GameState _state;

    public GameState State => _state;
    public StoryDefinition Story => _story;

    public StoryBlock? CurrentBlock
    {
        get
        {
            if (string.IsNullOrEmpty(_state.CurrentBlockId)) return null;

            // Căutare inteligentă - încercăm cheia directă, altfel căutăm ignorând literele mari/mici
            if (_blockMap.TryGetValue(_state.CurrentBlockId, out var b)) return b;

            var key = _blockMap.Keys.FirstOrDefault(k => string.Equals(k, _state.CurrentBlockId, StringComparison.OrdinalIgnoreCase));
            if (key != null) return _blockMap[key];

            // Ultimul resort: dacă motorul cere o cheie defectă, oferim blocul de pornire să nu crape ecranul
            return _blockMap.TryGetValue("start", out var start) ? start : _blockMap.Values.FirstOrDefault();
        }
    }

    public bool IsGameOver => CurrentBlock?.IsFinal == true;

    public GameEngine(StoryDefinition story)
    {
        _story = story ?? throw new ArgumentNullException(nameof(story));

        // Populăm dicționarul asigurându-ne că evităm chei duplicate cauzate de spații
        _blockMap = new Dictionary<string, StoryBlock>(StringComparer.OrdinalIgnoreCase);
        if (story.Blocks != null)
        {
            foreach (var b in story.Blocks)
            {
                if (b == null || string.IsNullOrWhiteSpace(b.Id)) continue;
                _blockMap[b.Id.Trim()] = b;
            }
        }

        _state = new GameState();
        _state.Initialize(story);

        // Corectăm starea inițială dacă a fost încărcată greșit ca fiind goală
        if (string.IsNullOrEmpty(_state.CurrentBlockId) || !_blockMap.ContainsKey(_state.CurrentBlockId))
        {
            _state.CurrentBlockId = _blockMap.ContainsKey("start") ? "start" : (_blockMap.Keys.FirstOrDefault() ?? "");
        }
    }

    public void Reset()
    {
        _state = new GameState();
        _state.Initialize(_story);
        if (string.IsNullOrEmpty(_state.CurrentBlockId) || !_blockMap.ContainsKey(_state.CurrentBlockId))
        {
            _state.CurrentBlockId = _blockMap.ContainsKey("start") ? "start" : (_blockMap.Keys.FirstOrDefault() ?? "");
        }
    }

    public List<DecisionDefinition> GetAvailableDecisions()
    {
        var block = CurrentBlock;
        if (block == null || block.Decisions == null) return new();
        return block.Decisions.Where(d => EvaluateCondition(d.Condition)).ToList();
    }

    public string? MakeDecision(DecisionDefinition decision)
    {
        if (decision == null) return _state.CurrentBlockId;

        var changed = new HashSet<string>();
        if (decision.Effects != null)
        {
            foreach (var effect in decision.Effects)
            {
                ApplyEffect(effect);
                changed.Add(effect.Property);
            }
        }

        var redirect = CheckRedirects(changed);
        _state.CurrentBlockId = redirect ?? decision.TargetBlock;
        return _state.CurrentBlockId;
    }

    public bool EvaluateCondition(ConditionDefinition? cond)
    {
        if (cond == null) return true;
        return cond switch
        {
            ComparisonCondition c => EvaluateComparison(c),
            AndCondition a => a.Conditions != null && a.Conditions.All(EvaluateCondition),
            OrCondition o => o.Conditions != null && o.Conditions.Any(EvaluateCondition),
            _ => true
        };
    }

    public void LoadState(GameState state) => _state = state?.Clone() ?? _state;
    public GameState SaveState() => _state.Clone();

    private void ApplyEffect(EffectDefinition effect)
    {
        if (effect == null || _story.Properties == null) return;
        var def = _story.Properties.FirstOrDefault(p => p.Key == effect.Property);
        if (def == null) return;

        double cur = _state.Properties.GetValueOrDefault(effect.Property, def.Initial);
        double newVal = effect.Type == "SET" ? effect.Value : cur + effect.Value;
        _state.Properties[effect.Property] = Math.Max(def.Min, Math.Min(def.Max, newVal));
    }

    private string? CheckRedirects(HashSet<string> changedKeys)
    {
        if (_story.Properties == null) return null;
        foreach (var def in _story.Properties)
        {
            if (!changedKeys.Contains(def.Key)) continue;
            if (!_state.Properties.TryGetValue(def.Key, out double val)) continue;

            if (val <= def.Min && !string.IsNullOrEmpty(def.OnMinBlock))
                return def.OnMinBlock;
            if (val >= def.Max && !string.IsNullOrEmpty(def.OnMaxBlock))
                return def.OnMaxBlock;
        }
        return null;
    }

    private bool EvaluateComparison(ComparisonCondition c)
    {
        double val = _state.Properties.GetValueOrDefault(c.Property, 0);
        return c.Operator switch
        {
            "<" => val < c.Value,
            "<=" => val <= c.Value,
            ">" => val > c.Value,
            ">=" => val >= c.Value,
            "==" => Math.Abs(val - c.Value) < 1e-9,
            "!=" => Math.Abs(val - c.Value) >= 1e-9,
            _ => false
        };
    }
}