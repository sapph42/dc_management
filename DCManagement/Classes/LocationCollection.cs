using System.Collections;

namespace DCManagement.Classes {
    internal class LocationCollection : IEnumerable<Location> {
        private List<Location> _locations = [];
        public LocationCollection() { }
        public void Add(Location location) {
            if (IntersectsWithAny(location))
                throw new ArgumentException("Two locations may not overlap");
            _locations.Add(location);
        }

        public IEnumerator<Location> GetEnumerator() {
            return ((IEnumerable<Location>)_locations).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_locations).GetEnumerator();
        }
        public Location? FindByPoint(Point point) {
            foreach (Location location in _locations) { 
                if (location.IsPointInside(point)) return location;
            }
            return null;
        }
        public bool IntersectsWithAny(Location OtherLocation) {
            foreach (Location location in _locations)
                if (location.IntersectsWith(OtherLocation))
                    return true;
            return false;
        }
    }
}
