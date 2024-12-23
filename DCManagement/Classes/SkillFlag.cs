namespace DCManagement.Classes;
internal class SkillFlag {
    public static DynamicEnum Flags = [];
    private uint _enumValue = 0;
    public uint Value {
        get { return _enumValue; }
    }
    public SkillFlag() { }
    public void SetValue(uint value) {
        if (value > Flags.Keys.Sum(key => (uint)key))
            throw new ArgumentOutOfRangeException(nameof(value));
        _enumValue = value;
    }
    public override string ToString() {
        if (Flags.TryGetValue(_enumValue, out string? singleVal))
            return singleVal;
        List<uint> keys = [.. Flags.Keys.Order()];
        List<string> output = [];
        foreach (uint key in keys) {
            if ((key & _enumValue) == key)
                output.Add(Flags[key]);
        }
        return string.Join(", ", output);
    }
    public string ToString(string? format) {
        return format switch {
            null or "G" or "g" or "F" or "f" => ToString(),
            "D" or "d" => _enumValue.ToString(),
            "X" or "x" => Convert.ToString(_enumValue, 16),
            _ => throw new FormatException("format contains an invalid specification"),
        };
    }
    public uint[] GetValues() {
        if (Flags.ContainsKey(_enumValue))
            return [_enumValue];
        List<uint> keys = [.. Flags.Keys.Order()];
        List<uint> returnValues = [];
        foreach (uint key in keys) {
            if ((key & _enumValue) == key)
                returnValues.Add(key);
        }
        return [.. returnValues];
    }
}
