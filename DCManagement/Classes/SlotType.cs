namespace DCManagement.Classes; 
public class SlotType {
    public int SlotTypeID { get; set; }
    public required string Description { get; set; }
    public Color SlotColor { get; set; }
    public SlotType() { }
    public SlotType(int id, string desc) {
        SlotTypeID = id;
        Description = desc;
    }
    public SlotType(int id, string desc, Color color) {
        SlotTypeID = id;
        Description = desc;
        SlotColor = color;
    }
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml (ColorHex);
    }
}
