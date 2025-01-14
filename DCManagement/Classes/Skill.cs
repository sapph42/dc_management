namespace DCManagement.Classes; 
public class Skill : IEquatable<Skill> {
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
        SlotColor = ColorTranslator.FromHtml("#" + color); 
    }
    public bool Equals(Skill? otherSkill) {
        if (otherSkill is null) return false;
        return SkillID == otherSkill.SkillID;
    }
    public override bool Equals(object? obj) => Equals(obj as Skill);
    public static bool operator ==(Skill? a, Skill? b) {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
    public static bool operator !=(Skill? a, Skill? b) {
        if (a is null && b is null) return false;
        if (a is null || b is null) return true;
        return !a.Equals(b);
    }
    public override int GetHashCode() => SkillID.GetHashCode();
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml ("#" + ColorHex);
    }
    public override string ToString() {
        return Description;
    }
}
