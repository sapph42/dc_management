using System.Collections;

namespace DCManagement.Classes {
    internal class LocationCollection : IEnumerable<Location> {
        private Dictionary<int, Location> _locations = [];
        public LocationCollection() { }
        internal void AddRangeUnsafe(Dictionary<int, Location> internalDict) {
            _locations = internalDict;
        }
        public void Add(Location location) {
            if (IntersectsWithAny(location))
                throw new ArgumentException("Two locations may not overlap");
            if (_locations.ContainsKey(location.LocID))
                throw new ArgumentException("Cannot add locations with duplicate IDs");
            _locations.Add(location.LocID, location);
        }
        public void Update(Location location) {
            _locations[location.LocID] = location;
        }
        public bool Remove(Location location) {
            if (_locations.ContainsValue(location))
                return _locations.Remove(location.LocID);
            else
                return false;
        }
        public LocationCollection Clone() {
            LocationCollection cloned = new LocationCollection();
            cloned.AddRangeUnsafe(_locations);
            return cloned;
        }
        public bool Remove(int id) {
            return _locations.Remove(id);
        }
        public IEnumerator<Location> GetEnumerator() {
            return ((IEnumerable<Location>)_locations).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_locations).GetEnumerator();
        }
        public Location? FindByPoint(Point point) {
            foreach (Location location in _locations.Values) { 
                if (location.IsPointInside(point)) return location;
            }
            return null;
        }
        public bool IntersectsWithAny(Location OtherLocation) {
            foreach (Location location in _locations.Values)
                if (location.IntersectsWith(OtherLocation))
                    return true;
            return false;
        }
        public bool IntersectsWithAny(Rectangle rect) {
            foreach (Location location in _locations.Values)
                if (location.IntersectsWith(rect))
                    return true;
            return false;
        }
    }
}
