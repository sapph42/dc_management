namespace DCManagement.Classes;
public class Slot {
    public int SlotID { get; set; }
    public required SlotType SlotSkill { get; set; }
    public int MinQty { get; set; }
    public int GoalQty { get; set; }
    public List<Person> Assigned { get; set; } = [];
    public int AssignedToSlot {
        get {
            return Assigned
                .Count(p => p.Skills
                    .Select(s => s.SlotTypeID)
                    .Contains(SlotSkill.SlotTypeID)
                );
        }
    }
    public bool HasMinimum {
        get {
            return MinQty <= AssignedToSlot;
        }
    }
    public bool HasAvailable {
        get {
            int unlocked = Assigned
                .Count(p => p.Skills.Select(s => s.SlotTypeID).Contains(SlotSkill.SlotTypeID) &&
                    !p.AssignmentLocked
                );
            return HasMinimum && unlocked > 0;
        }
    }
    public bool HasGoal {
        get {
            return GoalQty <= AssignedToSlot;
        }
    }
    public int UnderMin {
        get {
            return MinQty - AssignedToSlot;
        }
    }
    public Slot() { }
}
