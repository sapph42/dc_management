﻿namespace DCManagement.Classes; 
public class Person : IEquatable<Person> {
    private int? _teamID;
    private Team? _team;
    private bool _active = true;
    private bool _available = true;
    public int? PersonID { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public Team? Team {
        get => _team;
        set {
            _team = value;
            _teamID = value?.TeamID ?? _teamID;
        }
    }
    public int TeamID {
        get {
            return Team?.TeamID ?? _teamID ?? -1;
        }
    }
    public List<Skill> Skills { get; set; } = [];
    public Slot? AssignedSlot { get; set; }
    public bool Active { 
        get => _active; 
        set => _active = value; 
    }
    public bool Available { 
        get {
            return _available && _active;
        } 
        set => _available = value; 
    }
    public string? NameOverride { get; set; }
    public Label Label { get; set; } = new();
    public string FullName {
        get {
            if (!string.IsNullOrWhiteSpace(NameOverride))
                return NameOverride;
            return $"{LastName}, {FirstName}";
        }
    }
    public bool AssignmentLocked { get; set; } = false;
    public Person() { }
    public Person (object[] values) {
        PersonID = (int)values[0];
        LastName = (string)values[1];
        FirstName = (string)values[2];
        _teamID = values[3] == DBNull.Value ? null : (int)values[3];
        Active = (bool)values[4];
        Available = (bool)values[5] ;
    }
    public void AddSkill(Skill value) {
        if (Skills.Contains(value))
            return;
        Skills.Add(value);
    }
    public Person Clone() {
        Person clone = new() {
            PersonID = PersonID, 
            LastName = LastName,
            FirstName = FirstName,
            Team = Team,
            Skills = Skills,
            Active = Active,
            Available = Available
        };
        return clone;
    }
    public bool Equals(Person? otherPerson) {
        if (otherPerson == null) return false;
        return PersonID == otherPerson.PersonID;
    }
    public override bool Equals(object? obj) => Equals(obj as Person);
    public override int GetHashCode() => PersonID.GetHashCode();
    public void GenerateCenteredLabelTemplate(int centerXOn, int Y, Color backColor, Color? foreColor = null) {
        foreColor ??= Color.Black;
        Label = new() {
            Text = $"{LastName}, {FirstName[..1]}.",
            AutoSize = true,
            BackColor = backColor,
            ForeColor = (Color)foreColor,
            Tag = this,
            Width = 1,
            Location = new Point(0, 0)
        };
        Size textSize = TextRenderer.MeasureText(Label.Text, Label.Font);
        textSize.Width += Label.Padding.Left + Label.Padding.Right;
        Label.Size = textSize;
        Point loc = new() {
            X = centerXOn - (Label.Width / 2),
            Y = Y
        };
        Label.Location = loc;
    }
    public void GenerateLabelTemplate(Color backColor, Color? foreColor = null) {
        foreColor ??= Color.Black;
        Label = new() {
            Text = $"{LastName}, {FirstName[..1]}.",
            AutoSize = true,
            BackColor = backColor,
            ForeColor = (Color)foreColor,
            Tag = this,
            Width = 1,
            Location = new Point(0, 0)
        };
        Size textSize = TextRenderer.MeasureText(Label.Text, Label.Font);
        textSize.Width += Label.Padding.Left + Label.Padding.Right;
        Label.Size = textSize;
    }
    public List<int> GetSkillIDs() {
        if (Skills.Count > 0)
            return Skills.Select(s => s.SkillID).ToList();
        return [];
    }
    public bool HasSkill(int SkillTypeID) {
        return GetSkillIDs().Contains(SkillTypeID);
    }
    public void RemoveFromTeam() {
        Team = null;
        _teamID = -1;
    }
    public void SetSkills(IEnumerable<Skill> values) {
        Skills.Clear();
        Skills.AddRange(values);
    }
    public override string ToString() {
        return FullName; 
    }
}
