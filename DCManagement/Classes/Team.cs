using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
public class Team {
    private int? _teamLead;
    private int? _primaryLoc;
    public int? TeamID { get; set; }
    public Person? TeamLead { get; set; }
    public int TeamLeadID {
        get {
            return TeamLead?.PersonID ?? _teamLead ?? -1;
        }
        internal set {
            _teamLead = value;
        }
    }
    public Location? PrimaryLocation { get; set; }
    public int LocationID {
        get {
            return PrimaryLocation?.LocID ?? _primaryLoc ?? -1;
        }
        internal set {
            _primaryLoc = value;
        }
    }
    public bool FillIfNoLead { get; set; } = true;
    public bool Active { get; set; } = true;
    public string TeamName { get; set; }
    public Location? CurrentAssignment { get; set; }
    public TeamSlots Slots { get; set; } = [];

    public LabelPattern? LabelPattern { get; set; }
    public Team() {
        TeamName = string.Empty;
    }
    public Team (string TeamName) {
        this.TeamName = TeamName;
    }
    public Team (object[] values) {
        TeamID = (int)values[0];
        TeamName = (string)values[1];
        _teamLead = values[2]==DBNull.Value ? null : (int)values[2];
        _primaryLoc = values[3] == DBNull.Value ? null : (int)values[3];
        FillIfNoLead = (bool)values[4];
        Active = (bool)values[5];
    }
    public void AssignPerson(Person person, int skillID, bool lockOverride = false) {
        if (person.AssignmentLocked & !lockOverride)
            return;
        person.Team = this;
        Slots
            .Where(s => s.SkillID == skillID)
            .First()
            .Assigned
            .Add(person);
    }
    public SqlParameter[] GetSqlParameters() {
        var coll = new SqlParameter[6];
        coll[0] = new SqlParameter() {
            ParameterName = "@TeamID",
            SqlDbType = SqlDbType.Int,
            Value = TeamID
        };
        coll[1] = new SqlParameter() {
            ParameterName = "@Name",
            SqlDbType = SqlDbType.VarChar,
            Value = TeamName
        };
        coll[2] = new SqlParameter() {
            ParameterName = "@Lead",
            SqlDbType = SqlDbType.Int,
            Value = TeamLeadID
        };
        coll[3] = new SqlParameter() {
            ParameterName = "@LocID",
            SqlDbType = SqlDbType.Int,
            Value = LocationID
        };
        coll[4] = new SqlParameter() {
            ParameterName = "@Fill",
            SqlDbType = SqlDbType.Bit,
            Value = FillIfNoLead
        };
        coll[5] = new SqlParameter() {
            ParameterName = "@Active",
            SqlDbType = SqlDbType.Bit,
            Value = Active
        };
        return coll;
    }
    public bool HasAvailablePersonnel(bool targetGoal = false) {
        if (targetGoal) 
            return Slots.Any(s => s.HasAvailableForGoal);
        return Slots.Any(s => s.HasAvailable);
    }
    public bool HasAvailablePersonnelBySkill(int SkillTypeID) {
        return Slots.Where(s => s.SkillID == SkillTypeID && s.HasAvailable).Any();
    }
    public bool HasGoalStaffing() {
        return Slots.All(s => s.HasGoal);
    }
    public bool HasGoalStaffingBySkill(int SKillTypeID) {
        return Slots.Where(s => s.SkillID == SKillTypeID && s.HasGoal).Any();
    }
    public bool HasMinimumStaffing() {
        return Slots.All(s => s.HasMinimum);
    }
    public bool HasMinimumStaffingBySkill(int SkillTypeID) {
        return Slots.Where(s => s.SkillID == SkillTypeID && s.HasMinimum).Any();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>The highest priority SlotTypeID among SlotSkills that are below minimum </returns>
    public int? HighestPriorityNeed() {
        return Slots
            .Where(s => !s.HasMinimum)?
            .MaxBy(s => s.Priority)?
            .SkillID;
    }
    public Slot? HighestPriorityMatch(Person person) => Slots
            .Where(slot => person.Skills.Any(skill => skill.Equals((Skill)slot)))
            .OrderByDescending(slot => slot.Priority)
            .FirstOrDefault();
    public void ReassignPerson(Person person, Team newTeam, int skillID) {
        if (person.AssignmentLocked)
            return;
        person.Team = newTeam;
        Slots.RemovePerson(person);
        newTeam
            .Slots
            .Where(s => s.SkillID == skillID)
            .First()
            .Assigned
            .Add(person);
    }
}
