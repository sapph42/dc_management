using System;

namespace DCManagement.Classes; 
internal class LocationCollection : Dictionary<int, Location> {
    private Dictionary<int, string> _idsAndNames = [];
    public Dictionary<int, string> ListboxDatasource {
        get => _idsAndNames;
    }
    public LocationCollection() { }
    internal void AddRangeUnsafe(Dictionary<int, Location> internalDict) {
        foreach (var kvp in internalDict){
            Add(kvp.Key, kvp.Value.Clone());
            _idsAndNames.Add(kvp.Key, kvp.Value.Name ?? "");
        }
    }
    public void Add(Location location) {
        if (IntersectsWithAny(location))
            throw new ArgumentException("Two locations may not overlap");
        if (ContainsKey(location.LocID))
            throw new ArgumentException("Cannot add locations with duplicate IDs");
        Add(location.LocID, location);
        _idsAndNames.Add(location.LocID, location.Name ?? "");

    }
    public LocationCollection Clone() {
        LocationCollection cloned = [];
        cloned.AddRangeUnsafe(this);
        return cloned;
    }
    public new bool ContainsValue(Location location) {
        foreach (var kvp in this) {
            if (kvp.Value.Equals(location)) return true;
        }
        return false;
    }
    public Location? FindByPoint(Point point) {
        foreach (Location location in Values) {
            if (location.IsPointInside(point)) return location;
        }
        return null;
    }
    public bool IntersectsWithAny(Location OtherLocation) {
        foreach (Location location in Values)
            if (location.IntersectsWith(OtherLocation))
                return true;
        return false;
    }
    public bool IntersectsWithAny(Rectangle rect) {
        foreach (Location location in Values)
            if (location.IntersectsWith(rect))
                return true;
        return false;
    }
    public int? IntersectWithByID(Rectangle rect) {
        foreach (Location location in Values)
            if (location.IntersectsWith(rect))
                return location.LocID;
        return null;
    }
    public bool Remove(Location location) {
        if (ContainsValue(location))
            return Remove(location.LocID) && _idsAndNames.Remove(location.LocID);
        else
            return false;
    }

}
