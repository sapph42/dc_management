namespace DCManagement.Classes;
public class Slot : Skill {
    public int SlotID { get; set; }
    public int MinQty { get; set; }
    public int GoalQty { get; set; }
    public List<Person> Assigned { get; set; } = [];
    public int AssignedToSlot {
        get {
            return Assigned.Count;
        }
    }
    public bool HasMinimum {
        get {
            return AssignedToSlot >= MinQty;
        }
    }
    public bool HasAvailable {
        get {
            int locked = Assigned
                .Count(p => p.AssignmentLocked);
            return AssignedToSlot > MinQty && AssignedToSlot - locked > 0;
        }
    }
    public bool HasAvailableForGoal {
        get {
            int locked = Assigned
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
}
