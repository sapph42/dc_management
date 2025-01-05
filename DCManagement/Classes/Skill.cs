namespace DCManagement.Classes; 
public class Skill {
    public int SkillID { get; set; }
    public required string Description { get; set; }
    public Color SlotColor { get; set; }
    public int Priority { get; set; }
    public Skill() { }
    public Skill(int id, string desc) {
        SkillID = id;
        Description = desc;
    }
    public Skill(int id, string desc, Color color) {
        SkillID = id;
        Description = desc;
        SlotColor = color;
    }
    public Skill(int id, string desc, string color) {
        SkillID = id;
        Description = desc;
        SlotColor = ColorTranslator.FromHtml(color); 
    }
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml (ColorHex);
    }
}
