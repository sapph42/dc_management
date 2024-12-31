using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
internal class Person {
    private int? _teamID;
    private Team? _team;
    public int PersonID { get; set; }
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
    public SkillFlag SkillFlag { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public string? NameOverride { get; set; }
    public string FullName {
        get {
            if (!string.IsNullOrWhiteSpace(NameOverride))
                return NameOverride;
            return $"{LastName}, {FirstName}";
        }
    }
    public Person() { }
    public Person (object[] values) {
        PersonID = (int)values[0];
        LastName = (string)values[1];
        FirstName = (string)values[2];
        _teamID = values[3] == DBNull.Value ? null : (int)values[3];
        SkillFlag.SetValue((uint)values[4]);
        IsActive = (int)values[5] == 1;
        IsAvailable = (int)values[6] == 1;
    }
    public Person Clone() {
        Person clone = new() {
            PersonID = PersonID, 
            LastName = LastName,
            FirstName = FirstName,
            Team = Team,
            SkillFlag = SkillFlag,
            IsActive = IsActive,
            IsAvailable = IsAvailable
        };
        return clone;
    }
}
