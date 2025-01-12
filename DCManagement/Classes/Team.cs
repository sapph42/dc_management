﻿namespace DCManagement.Classes; 
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
            .AssignToSlot(person);
    }
    public bool Equals(Team otherTeam) {
        return TeamID == otherTeam.TeamID;
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
    public void ReassignPerson(Person person, Team newTeam, int skillID, bool OverrideLock = false, bool Lock = false, List<int>? ignoreSkills = default) {
        if (person.AssignmentLocked && !OverrideLock)
            return;
        person.Team = newTeam;
        Slots.RemovePerson(person);
        if (!person.Available || !person.Active)
            return;
        if (newTeam.Slots.Where(s => s.SkillID == skillID).FirstOrDefault<Slot>() is null) {
            try {
                ignoreSkills ??= [];
                int otherSkill = person.Skills.First(s => !ignoreSkills.Contains(skillID)).SkillID;
                ignoreSkills.Add(skillID);
                ReassignPerson(person, newTeam, otherSkill, OverrideLock, Lock, ignoreSkills);
            } catch {
                throw new Exception();
            }
        }
        newTeam
            .Slots
            .Where(s => s.SkillID == skillID)
            .First()
            .AssignToSlot(person);
        if (Lock)
            person.AssignmentLocked = true;
    }
    public void RemovePerson(Person person) {
        Slots.RemovePerson(person);
    }
    public override string ToString() {
        return TeamName;
    }
}
