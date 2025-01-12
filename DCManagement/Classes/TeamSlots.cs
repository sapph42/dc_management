namespace DCManagement.Classes;
public class TeamSlots : List<Slot> {
    public bool AtMinimum {
        get {
            return this.All(ts => ts.HasMinimum);
        }
    }
    public bool AtGoal {
        get {
            return this.All(ts => ts.HasGoal);
        }
    }
    public int[] AvailableSlots {
        get {
            return this.Where(ts => ts.HasAvailable).Select(ts => ts.SkillID).ToArray();
        }
    }
    public int[] AvailableSlotsForGoal {
        get {
            return this.Where(ts => ts.HasAvailableForGoal).Select(ts => ts.SkillID).ToArray();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>A List of int, int KeyValuePairs. The list consists of slots that are not at or above goal staffing. The values are the SlotTypeID, and the amount by which the slot is below minimum</returns>
    public List<KeyValuePair<int, int>> GoalNeeded() {
        if (AtGoal)
            return [];
        List<KeyValuePair<int, int>> returnData = [];
        foreach (Slot slot in this) {
            if (slot.HasGoal)
                continue;
            returnData.Add(new KeyValuePair<int, int>(slot.SkillID, slot.UnderGoal));
        }
        return returnData;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>A List of int, int KeyValuePairs. The list consists of slots that are not at or above minimum staffing. The values are the SlotTypeID, and the amount by which the slot is below minimum</returns>
    public List<KeyValuePair<int, int>> MinimumNeeded() {
        if (AtMinimum)
            return [];
        List<KeyValuePair<int, int>> returnData = [];
        foreach (Slot slot in this) {
            if (slot.HasMinimum)
                continue;
            returnData.Add( new KeyValuePair<int, int>(slot.SkillID, slot.UnderMin) );
        }
        return returnData;
    }
    public void RemovePerson(Person person) {
        foreach (Slot slot in this) {
            slot.UnassignToSlot(person);
        }
    }
    public Slot SlotBySkill(int SkillType) {
        return this.First(s => s.SkillID == SkillType);
    }
    public List<Slot> ToSlot() {
        return this;
    }
    public bool Remove(int slotID) { 
        return Remove(this.First(s => s.SlotID == slotID));
    }
}