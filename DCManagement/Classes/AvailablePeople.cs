namespace DCManagement.Classes; 
public class AvailablePeople {
    private List<Person> _people;
    public List<Person> People {
        get {
            CleanUnavailable();
            return _people;
        }
        set => _people = CleanUnavailable(value);
    }
    public AvailablePeople() {
        _people = [];
    }
    public void Add(Person? person) {
        if (person is null)
            return;
        if (_people.Contains(person))
            return;
        if (!person.Active || !person.Available)
            return;
        _people.Add(person);
    }
    /// <summary>
    /// Get the number of available people with a given skill.
    /// </summary>
    /// <param name="SkillTypeID">The SkillTypeID to check against</param>
    /// <returns>The count of people that have the specified SkillTypeID</returns>
    public int AvailbleSkills(int SkillTypeID) {
        return _people.Count(p => p.HasSkill(SkillTypeID));
    }
    public void CleanUnavailable() {
        _people = CleanUnavailable(_people);
    }
    public static List<Person> CleanUnavailable(List<Person> people) {
        if (people.All(p => p.Available && p.Active))
            return people;
        return people.Except(people.Where(p => !p.Active || !p.Available)).ToList();
    }
    /// <summary>
    /// Retreive a random person with the given SkillTypeID, and remove them from People.
    /// </summary>
    /// <param name="SkillTypeID"></param>
    /// <returns>The selected Person</returns>
    public Person GetPerson(int SkillTypeID) {
        CleanUnavailable();
        Person person = _people.Where(p => p.HasSkill(SkillTypeID)).Random();
        _people.Remove(person);
        return person;
    }
    public void Remove(Person? person) {
        if (person is null) 
            return;
        if (!_people.Contains(person)) 
            return;
        _people.Remove(person);
    }
}
