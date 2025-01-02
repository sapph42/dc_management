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
            return this.Where(ts => ts.HasAvailable).Select(ts => ts.SlotSkill.SlotTypeID).ToArray();
        }
    }
    public List<KeyValuePair<int, int>> MinimumNeeded() {
        if (AtMinimum)
            return [];
        List<KeyValuePair<int, int>> returnData = [];
        foreach (Slot slot in this) {
            if (slot.HasMinimum)
                continue;
            returnData.Add( new KeyValuePair<int, int>(slot.SlotSkill.SlotTypeID, slot.UnderMin) );
        }
        return returnData;
    }
    public Slot SlotBySkill(int SkillType) {
        return this.First(s => s.SlotSkill.SlotTypeID == SkillType);
    }
    public bool Remove(int slotID) { 
        return Remove(this.First(s => s.SlotID == slotID));
    }
}