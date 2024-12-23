namespace DCManagement.Classes; 
internal class SlotType {
    public int? SlotTypeID { get; set; }
    public string? Description { get; set; }
    public SkillFlag SkillFlag { get; set;} = new SkillFlag();
    public Color SlotColor { get; set; }
    public SlotType() { }
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml (ColorHex);
    }
}
