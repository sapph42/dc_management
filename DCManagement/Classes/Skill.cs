using Microsoft.Data.SqlClient;
using System.Data;

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
        SlotColor = ColorTranslator.FromHtml("#" + color); 
    }
    public bool Equals(Skill otherSkill) {
        return SkillID == otherSkill.SkillID;
    }
    public static List<Skill> FromDatabase() {
        using SqlConnection conn = new(Program.SqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetSkills";
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        List<Skill> skills = [];
        while (reader.Read()) {
            skills.Add(new() {
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml("#" + reader.GetString(2)),
                Priority = reader.GetInt32(3)
            });
        }
        reader.Close();
        return skills;
    }
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml ("#" + ColorHex);
    }
    public override string ToString() {
        return Description;
    }
}
