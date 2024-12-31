namespace DCManagement.Classes;
internal class PersonCollection : Dictionary<int, Person> {
    private Dictionary<int, string> _idsAndNames = [];
    public Dictionary<int, string> ListboxDatasource {
        get => _idsAndNames; 
    }
    public PersonCollection() { }
    internal void AddRangeUnsafe(Dictionary<int, Person> internalDict) {
        foreach (var kvp in internalDict){
            Add(kvp.Key, kvp.Value.Clone());
            _idsAndNames.Add(kvp.Key, kvp.Value.FullName);
        }
    }
    public void Add(Person person) {
        if (ContainsKey(person.PersonID))
            throw new ArgumentException("Cannot add locations with duplicate IDs");
        Add(person.PersonID, person);
        _idsAndNames.Add(person.PersonID, person.FullName);
    }
    public PersonCollection Clone() {
        PersonCollection cloned = [];
        cloned.AddRangeUnsafe(this);
        return cloned;
    }
    public new bool ContainsValue(Person person) {
        foreach (var kvp in this) {
            if (kvp.Value.Equals(person)) return true;
        }
        return false;
    }
    public bool ConflictsWithAny(Person OtherPerson) {
        foreach (Person person in Values)
            if (person.PersonID == OtherPerson.PersonID)
                return true;
        return false;
    }
    public bool Remove(Person person) {
        if (ContainsValue(person)){
            return Remove(person.PersonID) && _idsAndNames.Remove(person.PersonID);
        }
        else
            return false;
    }

}
