namespace DCManagement.Classes;
public class Slot : Skill {
    private List<Person> _assinged = [];
    public int SlotID { get; set; }
    public int MinQty { get; set; }
    public int GoalQty { get; set; }
    public List<Person> Assigned { get => _assinged; }
    public int AssignedToSlot {
        get {
            return _assinged.Count;
        }
    }
    public bool HasMinimum {
        get {
            return AssignedToSlot >= MinQty;
        }
    }
    public bool HasAvailable {
        get {
            int locked = _assinged
                .Count(p => p.AssignmentLocked);
            return AssignedToSlot > MinQty && AssignedToSlot - locked > 0;
        }
    }
    public bool HasAvailableForGoal {
        get {
            int locked = _assinged
                .Count(p => p.AssignmentLocked);
            return AssignedToSlot > GoalQty && AssignedToSlot - locked > 0;
        }
    }
    public bool HasGoal {
        get {
            return AssignedToSlot >= GoalQty;
        }
    }
    public int UnderGoal {
        get {
            return GoalQty - AssignedToSlot;
        }
    }
    public int UnderMin {
        get {
            return MinQty - AssignedToSlot;
        }
    }
    public Slot() { }
    public void AssignSkillProperties(Skill skill) {
        SkillID = skill.SkillID;
        Description = skill.Description;
        SlotColor = skill.SlotColor;
    }
    public void AssignToSlot(Person person) {
        if (!_assinged.Contains(person))
            _assinged.Add(person);
        person.AssignedSlot = this;
    }
    public bool Equals(Slot? otherSlot) {
        if (otherSlot is null) return false;
        return SlotID == otherSlot.SlotID;
    }
    public override bool Equals(object? obj) => Equals(obj as Slot);
    public static bool operator ==(Slot? a, Slot? b) {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
    public static bool operator !=(Slot? a, Slot? b) {
        if (a is null && b is null) return false;
        if (a is null || b is null) return true;
        return !a.Equals(b);
    }
    public override int GetHashCode() => SlotID.GetHashCode();
    public Person GetAssignee(int iterator) {
        return _assinged[iterator];
    }
    public Person GetRandomAssignee() {
        return _assinged.Random();
    }
    public void SetAssignments(List<Person> people) {
        _assinged = people;
    } 
    public void UnassignAll() {
        for (int i = 0; i < _assinged.Count; i++) {
            _assinged[i].AssignedSlot = null;
        }
        _assinged.Clear();
    }
    public void UnassignToSlot(Person person) {
        while (_assinged.Contains(person))
            _assinged.Remove(person);
        person.AssignedSlot = null;
    }
}
