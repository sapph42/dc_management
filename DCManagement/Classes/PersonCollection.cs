namespace DCManagement.Classes;
public class PersonCollection : Dictionary<int, Person> {
    public Dictionary<int, string> ListboxDatasource =>
        this
            .Select(kvp => (kvp.Key, kvp.Value.FullName))
            .ToDictionary();
    public Dictionary<int, string> ActiveListboxDatasource => 
        this
            .Where(kvp => kvp.Value.Active)
            .Select(kvp => (kvp.Key, kvp.Value.FullName))
            .ToDictionary();
    public PersonCollection() { }
    internal void AddRangeUnsafe(Dictionary<int, Person> internalDict) {
        foreach (var kvp in internalDict){
            Add(kvp.Key, kvp.Value.Clone());
        }
    }
    public void Add(Person person) {
        if (person.PersonID is null)
            return;
        int personID = (int)person.PersonID;
        if (ContainsValue(person))
            return;
        if (ContainsKey(personID))
            throw new ArgumentException("Cannot add people with duplicate IDs");
        Add(personID, person);
    }
    public void AssignTeam(Person Person, Team team) {
        this[(int)Person.PersonID!].Team = team;
    }
    public void AssignTeam(int PersonID, Team team) {
        this[PersonID].Team = team;
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
        return Remove(personID);
    }

}
