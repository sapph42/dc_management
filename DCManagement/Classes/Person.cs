using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
public class Person {
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
    public bool Equals(Person otherPerson) {
        return PersonID == otherPerson.PersonID;
    }
    public List<int> GetSkillIDs() {
        if (Skills.Count > 0)
            return Skills.Select(s => s.SkillID).ToList();
        return [];
    }
    public SqlParameter[] GetSqlParameters() {
        var coll = new SqlParameter[6];
        coll[0] = new SqlParameter() {
            ParameterName = "@PersonID",
            SqlDbType = SqlDbType.Int,
            Value = PersonID
        };
        coll[1] = new SqlParameter() {
            ParameterName = "@LastName",
            SqlDbType = SqlDbType.VarChar,
            Value = LastName
        };
        coll[2] = new SqlParameter() {
            ParameterName = "@FirstName",
            SqlDbType = SqlDbType.VarChar,
            Value = FirstName
        };
        coll[3] = new SqlParameter() {
            ParameterName = "@TeamID",
            SqlDbType = SqlDbType.Int,
            Value = TeamID
        };
        coll[4] = new SqlParameter() {
            ParameterName = "@Active",
            SqlDbType = SqlDbType.Bit,
            Value = Active
        };
        coll[5] = new SqlParameter() {
            ParameterName = "@Available",
            SqlDbType = SqlDbType.Bit,
            Value = Available
        };
        return coll;
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
