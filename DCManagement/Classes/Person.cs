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
    public List<SlotType> Skills { get; set; } = [];
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
    public bool AssignmentLocked { get; set; } = false;
    public Person() { }
    public Person (object[] values) {
        PersonID = (int)values[0];
        LastName = (string)values[1];
        FirstName = (string)values[2];
        _teamID = values[3] == DBNull.Value ? null : (int)values[3];
        IsActive = (bool)values[4];
        IsAvailable = (bool)values[5] ;
    }
    public void AddSkill(SlotType value) {
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
            IsActive = IsActive,
            IsAvailable = IsAvailable
        };
        return clone;
    }
    public List<int> GetSkillIDs() {
        if (Skills.Count > 0)
            return Skills.Select(s => s.SlotTypeID).ToList();
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
            Value = IsActive
        };
        coll[5] = new SqlParameter() {
            ParameterName = "@Available",
            SqlDbType = SqlDbType.Bit,
            Value = IsAvailable
        };
        return coll;
    }
    public void SetSkills(IEnumerable<SlotType> values) {
        Skills.Clear();
        Skills.AddRange(values);
    }
}
