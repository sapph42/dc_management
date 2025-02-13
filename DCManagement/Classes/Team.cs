﻿namespace DCManagement.Classes; 
public class Team : IEquatable<Team> {
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
    public bool Clinical { get; set; } = true;
    public Location? CurrentAssignment { get; set; }
    public TeamSlots Slots { get; set; } = [];
    public LabelPattern? LabelPattern { get; set; }
    public int AssignedCount {
        get => Slots.AssignedCount;
    }
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
        if (values[6] is bool clinical)
            Clinical = clinical;
        else 
            Clinical = Convert.ToBoolean(values[6]);
    }
    public void AssignPerson(Person person, bool lockOverride = false) {
        if (person.AssignmentLocked & !lockOverride)
            return;
        if (person.Team is not null &&
            person.Team.Equals(this) &&
            person.AssignedSlot is not null)
            return;
        if (person.Team is not null && person.Team.TeamLead == person) {
            person.Team.TeamLead = null;
        }
        person.Team = this;
        var firstBestSlot = Slots.OrderByDescending(s => s.Priority).Where(s => person.Skills.Contains((Skill)s)).FirstOrDefault();
        if (firstBestSlot is null)
            return;
        person.Team = this;
        Slots
            .Where(s => s.SkillID == firstBestSlot.SkillID)
            .First()
            .AssignToSlot(person);
    }
    public void AssignPerson(Person person, int skillID, bool lockOverride = false) {
        if (person.AssignmentLocked & !lockOverride)
            return;
        if (person.Team is not null &&
            person.Team.Equals(this) &&
            person.AssignedSlot is not null &&
            person.AssignedSlot.Priority > Slots.Where(s => s.SkillID == skillID).First().Priority)
            return;
        person.Team = this;
        Slots
            .Where(s => s.SkillID == skillID)
            .First()
            .AssignToSlot(person);
    }
    public void AssignPerson(Person person, int SlotID, int? skillID) {
        _ = skillID;
        person.Team = this;
        Slots.AssignByID(person, SlotID);
    }
    public bool Equals(Team? otherTeam) {
        if (otherTeam is null) return false;
        return TeamID == otherTeam.TeamID;
    }
    public override bool Equals(object? obj) => Equals(obj as Team);
    public static bool operator ==(Team? a, Team? b) {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
    public static bool operator !=(Team? a, Team? b) {
        if (a is null && b is null) return false;
        if (a is null || b is null) return true;
        return !a.Equals(b);
    }
    public override int GetHashCode() => TeamID.GetHashCode();
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
        bool wasTeamLead = false;
        if (person.Team is not null && person.Team.TeamLead == person) {
            wasTeamLead = true;
            person.Team.TeamLead = null;
        }
        if (TeamLead == person) {
            wasTeamLead = true;
            TeamLead = null;
        }
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
        if (wasTeamLead)
            newTeam.TeamLead ??= person;
    }
    public void RemovePerson(Person person) {
        Slots.RemovePerson(person);
    }
    public List<Person> UnassignAll() {
        List<Person> unassigned = [];
        Slots
            .SelectMany(s => s.Assigned)
            .ToList()
            .ForEach(p => unassigned.Add(p));
        Slots
            .ForEach(s => s.UnassignAll());
        return unassigned;
    }
    public override string ToString() {
        return TeamName;
    }
}
