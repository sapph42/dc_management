namespace DCManagement.Classes;
public class EntitySlots : List<Slot> {
    public int EntityID { get; set; }
    public bool Remove(int slotID) { 
        return Remove(this.First(s => s.SlotID == slotID));
    }
}