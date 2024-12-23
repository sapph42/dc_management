namespace DCManagement.Classes; 
public class DynamicEnum : Dictionary<uint, string> {
    public DynamicEnum() { }
    public void AddRange(IEnumerable<KeyValuePair<uint, string>> enumerable) {
        enumerable = enumerable.OrderBy(x => x.Key);
        foreach (KeyValuePair<uint, string> kvp in enumerable) {
            if (kvp.Value is null)
                continue;
            if (ContainsKey(kvp.Key) || ContainsValue(kvp.Value))
                continue;
            Add(kvp.Key, kvp.Value);
        }
    }
    public void AddRange(IEnumerable<string> values) {
        foreach (string value in values) {
            if (ContainsValue(value))
                continue;
            _ = Add(value);
        }
    }
    public new void Add(uint key, string value) {
        Add(key, value, false);
    }
    public void Add(uint key, string value, bool compositeOverride) {
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate values are not permitted");
        if (ContainsValue(value))
            throw new ArgumentException("Duplicate names are not permitted");
        bool isPowOf2 = (key > 0) && ((key & (key - 1)) == 0);
        if (!isPowOf2 && !compositeOverride)
            throw new ArgumentException("New values must be powers of 2 unless compositeOverride is true");
        base.Add(key, value);
    }
    public uint Add(string value) {
        if (ContainsValue(value))
            throw new ArgumentException("Duplicate names are not permitted");
        uint highestVal = Keys.Max();
        uint maxFlag = 1u << (31 - (int)System.Numerics.BitOperations.LeadingZeroCount(highestVal));
        uint newVal;
        if (maxFlag <= uint.MaxValue >> 1)
            newVal = maxFlag << 1;
        else
            throw new InvalidOperationException("Maximum flag value reached");
        Add(newVal, value);
        return newVal;
    }
    public uint? GetValue(string name) {
        if (ContainsValue(name))
            return this.FirstOrDefault(kvp => kvp.Value == name).Key;
        return null;
    }
    public string? GetName(uint value) {
        if (TryGetValue(value, out string? name))
            return name;
        return null;
    }
}
