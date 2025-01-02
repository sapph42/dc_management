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
        if (person.PersonID is null)
            return;
        int personID = (int)person.PersonID;
        if (ContainsKey(personID))
            throw new ArgumentException("Cannot add locations with duplicate IDs");
        Add(personID, person);
        _idsAndNames.Add(personID, person.FullName);
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
        if (person.PersonID is null)
            return false;
        int personID = (int)person.PersonID;
        if (ContainsValue(person)){
            return Remove(personID) && _idsAndNames.Remove(personID);
        }
        else
            return false;
    }

}
