using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DCManagement.Forms;
public partial class DailyAssignment : Form {
    private List<Skill> _skills = [];
    private readonly PersonCollection _people = [];
    private List<Team> _teams = [];
    private Team _float = new() {
        TeamID = 9999
    };
    private Team _unavailable = new() {
        TeamID = -1
    };
    private readonly AvailablePeople _availablePeople = new();
    private readonly List<Person> _unavailablePeople = [];
    private readonly List<Team> _defunctTeams = [];
    private readonly Floorplan _floorplan;
    private Size _maxSize;
    private readonly List<Label> _labels = [];
    public DailyAssignment() {
        InitializeComponent();
        _floorplan = new Floorplan() {
            Locations = LocationCollection.GetLocations(),
            Client = this
        };
        InitializeForm(true, true);
    }
    private void InitializeForm(bool baseInit = false, bool doResize = false, bool fullInit = false) {
        _maxSize = new() {
            Height = ClientSize.Height,
            Width = ClientSize.Width
        };
        if (baseInit) {
            _floorplan.LoadFloorplan();
            if (doResize) 
                ResizeForm();
            BackgroundImage = _floorplan.ImageWithLocations;
        }
        if (fullInit) {
            ClearLabels();
            AddLabels();
            DrawUnavailable();
            DrawFloat();
        }
    }
    #region Data Methods
    private void FillGoalSlots(int SkillID) {
        //First try and fill all teams to goal staffing from float
        List<Team> teamsToFill = [.. _teams.Where(
            t => !t.HasGoalStaffing() && (
                (
                    !t.FillIfNoLead && t.TeamLead is not null
                ) ||
                (
                    t.FillIfNoLead
                )
            ) &&
            t.HighestPriorityNeed() == SkillID
        )];
        for (int i = 0; i < teamsToFill.Count; i++) {
            Team team = teamsToFill[i];
            if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.Available)
                continue;
            if (team.Slots.AtGoal)
                continue;
            foreach (var kvp in team.Slots.GoalNeeded().Where(kvp => kvp.Key == SkillID)) {
                if (kvp.Value <= 0)
                    continue;
                int available = _availablePeople.AvailbleSkills(kvp.Value);
                if (available == 0)
                    continue;
                int willTake = Math.Min(available, kvp.Value);
                for (int j = 0; j < willTake; j++) {
                    Person thisPerson = _availablePeople.GetPerson(kvp.Key);
                    if (thisPerson.Team is not null)
                        thisPerson.Team.ReassignPerson(thisPerson, team, SkillID);
                    else
                        team.AssignPerson(thisPerson, SkillID);
                    Slot slot = team.Slots.First(s => s.SkillID == kvp.Key);
                    slot.Assigned.Add(thisPerson);
                    thisPerson.Team = team;
                }
            }
        }
        if (!_teams.Any(t => !t.HasGoalStaffingBySkill(SkillID)))
            return;
        //Now scan for teams that are over their goal count and have available unlocked members
        //If found, swap to the team that needs them more
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.Available)
                continue;
            if (team.Slots.AtGoal)
                continue;
            foreach (var kvp in team.Slots.GoalNeeded().Where(kvp => kvp.Key == SkillID)) {
                if (kvp.Value <= 0)
                    continue;
                Team? donorTeam = _teams.FirstOrDefault<Team>(t => t.Slots.AvailableSlotsForGoal.Contains(kvp.Key));
                if (donorTeam is null)
                    continue;
                Slot donorTeamSlot = donorTeam.Slots.First(s => s.SkillID == kvp.Key);
                donorTeam.ReassignPerson(donorTeamSlot.Assigned.Random(), team, donorTeamSlot.SkillID);
            }
        }
        if (!_teams.Any(t => !t.HasGoalStaffingBySkill(SkillID)))
            return;
        //ThisSlotNeedsFilling
        //ThereAreAvailableMoves
        //TODO Generate some kind of alert that not all teams could be filled
    }
    private void FillMinimumSlots(int SkillID) {
        //First try and fill all teams to minimum staffing from float
        List<Team> teamsToFill = [.. _teams.Where(
            t => !t.HasMinimumStaffing() && (
                (
                    !t.FillIfNoLead && t.TeamLead is not null
                ) ||
                (
                    t.FillIfNoLead
                )
            ) &&
            t.HighestPriorityNeed() == SkillID
        )];
        for (int i = 0; i < teamsToFill.Count; i++) {
            Team team = teamsToFill[i];
            if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.Available && team.TeamLead.Active)
                continue;
            if (team.Slots.AtMinimum)
                continue;
            foreach (var kvp in team.Slots.MinimumNeeded().Where(kvp => kvp.Key == SkillID)) {
                if (kvp.Value <= 0)
                    continue;
                int available = _availablePeople.AvailbleSkills(kvp.Value);
                if (available == 0)
                    continue;
                int willTake = Math.Min(available, kvp.Value);
                for (int j = 0; j < willTake; j++) {
                    Person thisPerson = _availablePeople.GetPerson(kvp.Key);
                    if (thisPerson.Team is not null)
                        thisPerson.Team.ReassignPerson(thisPerson, team, SkillID);
                    else
                        team.AssignPerson(thisPerson, SkillID);
                    Slot slot = team.Slots.First(s => s.SkillID == kvp.Key);
                    slot.Assigned.Add(thisPerson);
                    thisPerson.Team = team;
                }
            }
        }
        if (!_teams.Any(t => !t.HasMinimumStaffingBySkill(SkillID)))
            return;
        //Now scan for teams that are over their minimum count and have available unlocked members
        //If found, swap to the team that needs them more
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.Available)
                continue;
            if (team.Slots.AtMinimum)
                continue;
            foreach (var kvp in team.Slots.MinimumNeeded().Where(kvp => kvp.Key == SkillID)) {
                if (kvp.Value <= 0)
                    continue;
                Team? donorTeam = _teams.FirstOrDefault<Team>(t => t.Slots.AvailableSlots.Contains(kvp.Key));
                if (donorTeam is null)
                    continue;
                Slot donorTeamSlot = donorTeam.Slots.First(s => s.SkillID == kvp.Key);
                donorTeam.ReassignPerson(donorTeamSlot.Assigned.Random(), team, donorTeamSlot.SkillID);
            }
        }
        if (!_teams.Any(t => !t.HasMinimumStaffingBySkill(SkillID)))
            return;
        //ThisSlotNeedsFilling
        //ThereAreAvailableMoves
        //TODO Generate some kind of alert that not all teams could be filled
    }
    private List<Person> GetDefaultSlotAssignments(int TeamID, int SkillID) {
        using SqlConnection conn = new(Program.SqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetDefaultSlotAssignments";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters["@SkillID"].Value = SkillID;
        cmd.Connection = conn;
        conn.Open();
        List<Person> members = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            if (!_people.ContainsKey(personID))
                _people.Add(Person.FromDatabase(personID));
            members.Add(_people[personID]);
        }
        reader.Close();
        return AvailablePeople.CleanUnavailable(members);
    }
    private void PrepTeams() {
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            if (!team.Active)
                continue;
            if (team.TeamLeadID != -1) {
                team.TeamLead = _people[team.TeamLeadID]; ;
            }
            if (team.PrimaryLocation is null && team.LocationID != -1) {
                team.PrimaryLocation = Classes.Location.FromDatabase(team.LocationID);
                team.CurrentAssignment = team.PrimaryLocation;
            }
            Debug.WriteLine($"{_teams[i].TeamID}:{_teams[i].TeamName} :: {_teams[i].PrimaryLocation?.Name ?? ""}");
            team.Slots = TeamSlots.FromDatabase((int)team.TeamID!, _skills);
            for (int j = 0; j < team.Slots.Count; j++) {
                Slot slot = team.Slots[j];
                int skillID = slot.SkillID;
                slot.Assigned = GetDefaultSlotAssignments((int)team.TeamID, skillID);
                Debug.WriteLine($"{_teams[i].TeamID}:{_teams[i].Slots[j].SlotID} :: {_teams[i].Slots[j].Description}");
            }
        }
        try {
            _float.Slots = TeamSlots.FromDatabase((int)_float.TeamID!, _skills);
            _unavailable.Slots = TeamSlots.FromDatabase((int)_unavailable.TeamID!, _skills);
        } catch {

        }
    }
    private List<Skill> GetActiveSkills() =>
        _teams
            .Select(t => t.Slots)
            .Select(ts => ts.ToSlot()
                            .Cast<Skill>()
            )
            .SelectMany(l => l)
            .OrderBy(s => s.Priority)
            .DistinctBy(s => s.SkillID)
            .ToList();
    #endregion
    #region Label Transactions
    private void MoveUnassignedToFloat() {
        static bool leadTeamsMissingLeads(Team t) =>
            !t.FillIfNoLead &&
            t.TeamLead is not null &&
            (!t.TeamLead.Active || !t.TeamLead.Available) &&
            t.TeamName != "Float";
        static bool unleadTeamsMissingLeads(Team t) =>
            !t.FillIfNoLead &&
            t.TeamLead is null &&
            t.TeamName != "Float";
        List<Team> defunctTeams = [.. _teams.Where(t => leadTeamsMissingLeads(t) || unleadTeamsMissingLeads(t))];
        for (int defunctIterator = 0; defunctIterator < defunctTeams.Count; defunctIterator++) {
            Team team = defunctTeams[defunctIterator];
            for (int slotIterator = 0; slotIterator < team.Slots.Count; slotIterator++) {
                Slot slot = team.Slots[slotIterator];
                for (int personIterator = 0; personIterator < slot.Assigned.Count; personIterator++) {
                    Person floater = slot.Assigned[personIterator];
                    if (!floater.Available || !floater.Active) {
                        _ = _availablePeople.People.Remove(floater);
                        continue;
                    }
                    if (floater.Team is null)
                        _float.AssignPerson(floater, slot.SkillID);
                    else
                        floater.Team.ReassignPerson(floater, _float, slot.SkillID);
                    _availablePeople.People.Add(floater);
                }
            }
            _teams.Remove(team);
            _defunctTeams.Add(team);
        }
        foreach (var person in _people.Values) {
            if (person.Team is not null && defunctTeams.Contains(person.Team)) {
                if (!person.Available || !person.Active) {
                    _ = _availablePeople.People.Remove(person);
                    continue;
                }
                person.Team.ReassignPerson(person, _float, person.Skills.OrderBy(p => p.Priority).First().SkillID);
                _availablePeople.People.Add(person);
            }
        }
        //First lets look at teams that have available personnel
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            // If FillIfNoLead is false, personnel are never available
            if (!team.FillIfNoLead)
                continue;
            // If there is a TeamLead who is Active and Available, its members are not available
            if (team.TeamLead is null || (
                    team.TeamLead is not null &&
                    team.TeamLead.Available
                    && team.TeamLead.Active)
                )
                continue;

            for (int j = 0; j < team.Slots.Count; j++) {
                Slot slot = team.Slots[j];
                for (int k = 0; k < slot.Assigned.Count; k++) {
                    Person floater = slot.Assigned[k];
                    if (floater.Team is null)
                        _float.AssignPerson(floater, slot.SkillID);
                    else
                        floater.Team.ReassignPerson(floater, _float, slot.SkillID);
                    _availablePeople.People.Add(floater);
                }
            }
        }
        //Now lets look at people that aren't assigned to a team
        for (int i = 0; i < _people.Count; i++) {
            Person thisPerson = _people.ElementAt(i).Value;
            if (thisPerson.Team is not null || thisPerson.TeamID != -1)
                continue;
            if (thisPerson.Team is null)
                _float.AssignPerson(thisPerson, thisPerson.Skills.OrderBy(s => s.Priority).First().SkillID);
            else
                thisPerson.Team.ReassignPerson(thisPerson, _float, thisPerson.Skills.OrderBy(s => s.Priority).First().SkillID);
            thisPerson.Team = _float;
            _availablePeople.People.Add(thisPerson);
        }
    }
    private void ToggleUnavailable(Person person, bool toFloat = true) {
        using SqlConnection conn = new(Program.SqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = person.PersonID;
        cmd.Connection = conn;
        conn.Open();
        if (person.Available) {
            cmd.CommandText = "dbo.UnavailableToday";
            _ = cmd.ExecuteNonQuery();
            person.Available = false;
            _unavailablePeople.Add(person);
            Team? targetTeam = person.Team;
            if (targetTeam is not null) {
                if (targetTeam.Equals(_float)) {
                    _availablePeople.People.Remove(person);
                    Controls.Remove(person.Label);
                    DrawFloat();
                } else {
                    Controls.Remove(person.Label);
                    person.RemoveFromTeam();
                    targetTeam.LabelPattern = DetermineTeamPattern(targetTeam);
                    DeleteLables(targetTeam);
                    CreateLabels(targetTeam);
                    targetTeam.RemovePerson(person);
                }
            }
            DrawUnavailable();
        } else {
            cmd.CommandText = "dbo.AvailableToday";
            _ = cmd.ExecuteNonQuery();
            _unavailablePeople.Remove(person);
            Controls.Remove(person.Label);
            _unavailable.RemovePerson(person);
            DrawUnavailable();
            if (toFloat)
                MoveToFloat(person);
        }
    }
    private void MoveToFloat(Person person) {
        person.Available = true;
        person.Team = _float;
        _availablePeople.People.Add(person);
        DrawFloat();
    }
    #endregion
    private void ResizeForm() {
        Size imageSize = _floorplan.ImageSize;
        float aspectRatio = (float)imageSize.Width / (float)imageSize.Height;
        Size adjustment = new(Width - ClientSize.Width, Height - ClientSize.Height);
        Size maxClientSize = new(_maxSize.Width - adjustment.Width, _maxSize.Height - adjustment.Height);
        if (imageSize.Width <= maxClientSize.Width && imageSize.Height <= maxClientSize.Height) {
            Size = imageSize + adjustment;
            BackgroundImageLayout = ImageLayout.Center;
        } else {
            if (aspectRatio >= 1) {
                Size = new Size() {
                    Width = maxClientSize.Width,
                    Height = (int)(maxClientSize.Width / aspectRatio)
                } + adjustment;
            } else {
                Size = new Size() {
                    Width = (int)(maxClientSize.Height * aspectRatio),
                    Height = maxClientSize.Height
                } + adjustment;
            }
            BackgroundImageLayout = ImageLayout.Stretch;
        }
    }
    #region Form Event Handlers
    private void DailyAssignment_DragDrop(object sender, DragEventArgs e) {
        if (e.Data is null)
            return;
        if (e.Data.GetData(typeof(Label)) is not Label label)
            return;
        object? tag = label.Tag;
        if (tag is null)
            return;
        Point dropPoint = PointToClient(new Point(e.X, e.Y));
        dropPoint = _floorplan.TransformCoordinatesInv(dropPoint);
        Location? loc = _floorplan.Locations.FindByPoint(dropPoint);
        if (loc is null)
            return;
        Team? newTeam = _teams.Where(t => t.CurrentAssignment is not null && t.CurrentAssignment.Equals(loc)).FirstOrDefault();
        if (newTeam is null && tag is not Team
            )
            newTeam = _defunctTeams.Where(t => t.PrimaryLocation is not null && t.PrimaryLocation.Equals(loc)).FirstOrDefault();
        if (newTeam is null) {
            if (_unavailable.LocationID == loc.LocID)
                newTeam = _unavailable;
            else if (_float.LocationID == loc.LocID)
                newTeam = _float;
        }
        Team currentTeam;
        if (newTeam is null && tag is Team defunctToValid) {
            currentTeam = defunctToValid;
            if (currentTeam.CurrentAssignment is not null && currentTeam.CurrentAssignment.Equals(loc))
                return;
            SuspendLayout();

            DeleteLables(currentTeam);
            currentTeam.CurrentAssignment = loc;
            if (_defunctTeams.Contains(currentTeam)) {
                _teams.Add(currentTeam);
            }
            _defunctTeams.Remove(currentTeam);
            //foreach (var slotLabel in team.Slots.SelectMany(s => s.Assigned).Select(p => p.Label)) {
            //    DeleteLables(team);
            //}
            CreateLabels(currentTeam);
            ResumeLayout();
        } else if (newTeam is not null && tag is Team teamToTeam) {
            currentTeam = teamToTeam;
            if (currentTeam.CurrentAssignment is not null && currentTeam.CurrentAssignment.Equals(loc))
                return;
            if (!(newTeam.Equals(_float) || newTeam.Equals(_unavailable)))
                return;
            SuspendLayout();
            for (int i = 0; i < currentTeam.Slots.Count; i++) {
                var slot = currentTeam.Slots[i];
                for (int j = 0; j < slot.Assigned.Count; j++) {
                    Person assignee = slot.Assigned[j];
                    if (assignee.Equals(currentTeam.TeamLead!)) {
                        if (newTeam.Equals(_unavailable)) {
                            ToggleUnavailable(assignee);
                            ResumeLayout();
                            continue;
                        } else if (newTeam.Equals(_float)) {
                            MoveToFloat(assignee);
                            ResumeLayout();
                            continue;
                        }
                    }
                    MoveToFloat(assignee);
                }
                slot.Assigned.Clear();
            }
            _teams.Remove(currentTeam);
            _defunctTeams.Add(currentTeam);
        } else if (newTeam is not null && tag is Person person) {
            Slot? slot = newTeam.HighestPriorityMatch(person);
            if (slot is null)
                return;
            if (person.Team is not null) {
                currentTeam = person.Team;
                bool wasFloat = false;
                if (newTeam.Equals(currentTeam))
                    return;
                SuspendLayout();
                if (person.Team.Equals(_unavailable)) {
                    ToggleUnavailable(person, newTeam.Equals(_float));
                } else if (newTeam.Equals(_unavailable)) {
                    ToggleUnavailable(person, false);
                    ResumeLayout();
                    return;
                } else if (newTeam.Equals(_float)) {
                    MoveToFloat(person);
                    ResumeLayout();
                    return;
                } else if (person.Team.Equals(_float)) {
                    _availablePeople.People.Remove(person);
                    Controls.Remove(person.Label);
                    DrawFloat();
                    wasFloat = true;
                } else {
                    DeleteLables(currentTeam);
                }
                if (_defunctTeams.Contains(newTeam)) {
                    _teams.Add(newTeam);
                } else {
                    DeleteLables(newTeam);
                }
                _defunctTeams.Remove(newTeam);
                newTeam.AssignPerson(person, slot.SkillID, true);
                person.AssignmentLocked = true;
                if (!wasFloat) {
                    currentTeam.LabelPattern = DetermineTeamPattern(currentTeam);
                    CreateLabels(currentTeam);
                }
            } else {
                DeleteLables(newTeam);
                newTeam.AssignPerson(person, slot.SkillID, true);
                person.AssignmentLocked = true;
            }
            newTeam.LabelPattern = DetermineTeamPattern(newTeam);
            CreateLabels(newTeam);
            ResumeLayout();
        }
    }
    private void DailyAssignment_DragOver(object sender, DragEventArgs e) {
        e.Effect = DragDropEffects.Move;
    }
    private void DailyAssignment_Load(object sender, EventArgs e) {
        _skills = Skill.FromDatabase();
        _teams = Team.FromDatabase();
        if (_teams.FirstOrDefault(t => t.TeamName == "Float") is not null) {
            _float = _teams.First(t => t.TeamName == "Float");
            _teams.Remove(_float);
            _float.PrimaryLocation = Classes.Location.FromDatabase(_float.LocationID);
            _float.CurrentAssignment = _float.PrimaryLocation;
        }
        if (_teams.FirstOrDefault(t => t.TeamName == "Unavailable") is not null) {
            _unavailable = _teams.First(t => t.TeamName == "Unavailable");
            _teams.Remove(_unavailable);
            _unavailable.PrimaryLocation = Classes.Location.FromDatabase(_unavailable.LocationID);
            _unavailable.CurrentAssignment = _unavailable.PrimaryLocation;
        }
        PrepTeams();
        using SqlConnection conn = new(Program.SqlConnectionString);
        using SqlCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetPeople";
        cmd.Connection = conn;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            Person person = Person.FromDatabase(personID);
            if (!person.Available) {
                person.Team = _unavailable;
                _unavailablePeople.Add(person);
            } else {
                person.Team = _teams.FirstOrDefault(t => t.TeamID == person.TeamID);
                if (person.Team is null && person.Available) {
                    person.Team = _float;
                    _availablePeople.People.Add(person);
                }
                _people.Add(person);
            }
        }
        MoveUnassignedToFloat();
        List<Skill> activeSkills = GetActiveSkills();
        foreach (var skill in activeSkills) {
            FillMinimumSlots(skill.SkillID);
        }
        foreach (var skill in activeSkills) {
            FillGoalSlots(skill.SkillID);
        }
        InitializeForm(false, false, true);
    }
    private void DailyAssignment_Resize(object sender, EventArgs e) {
        InitializeForm(true, false, true);
    }
    private void DailyAssignment_MouseMove(object sender, MouseEventArgs e) {
        MouseCoordTSMI.Text = $"{e.X},{e.Y}";
    }
    #endregion
    #region Control Event Handlers
    private void RefreshToolStripMenuItem_Click(object sender, EventArgs e) {
        foreach (var team in _teams) {
            team.LabelPattern = DetermineTeamPattern(team);
            DeleteLables(team);
            CreateLabels(team);
        }
    }
    private void ToggleAvailabilityToolStripMenuItem_Click(object sender, EventArgs e) {
        if (sender is not ToolStripMenuItem item)
            return;
        if (item.Owner is not ContextMenuStrip owner)
            return;
        if (owner.SourceControl is not Label label)
            return;
        Person target;
        if (label.Tag is Team team)
            target = team.TeamLead!;
        else
            target = (label.Tag as Person)!;
        ToggleUnavailable(target);
    }
    #endregion
    #region Label Event Handlers
    internal void SlotLabel_MouseDown(object? sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Left) {
            Label? label = sender as Label;
            if (label is not null)
                DoDragDrop(label, DragDropEffects.Move);
        }
    }
    #endregion
    #region Draw People
    private void AddLabels() {
        foreach (var team in _teams) {
            team.LabelPattern = DetermineTeamPattern(team);
            CreateLabels(team);
        }
    }
    private static LabelPattern DetermineTeamPattern(Team team) {
        if (team.TeamLead is null) {
            int assignedToTeam = team.Slots.Select(s => s.Assigned.Count).Sum();
            if (assignedToTeam == 0)
                return LabelPattern.None;
            else if (assignedToTeam == 1)
                return LabelPattern.Single;
            else
                return LabelPattern.MultiStacked;
        } else {
            List<Person> assignedToTeam = team.Slots.SelectMany(s => s.Assigned).ToList();
            int nonLeadMembers = assignedToTeam.Except([team.TeamLead]).Count();
            bool hasEFDA = team.Slots.Any(s => s.Description == "EFDA");
            if (nonLeadMembers == 0)
                return LabelPattern.Single;
            else if (nonLeadMembers == 1)
                return LabelPattern.SingleAsst;
            else {
                if (hasEFDA) {
                    if (nonLeadMembers == 2)
                        return LabelPattern.SingleAsstwEFDA;
                    else if (nonLeadMembers == 3)
                        return LabelPattern.DualAsstwEFDA;
                    else
                        return LabelPattern.DualAsstwEFDAPlus;
                } else {
                    if (nonLeadMembers == 2)
                        return LabelPattern.DualAsst;
                    else
                        return LabelPattern.DualAsstPlus;
                }
            }
        }
    }
    private void CreateLabels(Team team) {
        if (team.CurrentAssignment is null && team.PrimaryLocation is null)
            return;

        Rectangle rect = _floorplan.TransformRectangle(
            team.CurrentAssignment?.Rect ?? team.PrimaryLocation!.Rect
        );

        Point topLeft = rect.Location;
        int centerX = topLeft.X + rect.Width / 2;
        int topY = topLeft.Y + (int)(10 * _floorplan.GetScale().Y);
        int currentY = topY;
        Point lastLabelLoc = new(0, topY);
        Person teamLead;
        Color slotColor;
        switch (team.LabelPattern) {
            case LabelPattern.None:
                return;
            case LabelPattern.MultiStacked:
                SuspendLayout();
                int stackCount = team.Slots
                                     .SelectMany(s => s.Assigned)
                                     .Where(p => p is not null)
                                     .Count();
                var slots = team.Slots
                                .Select(s => new KeyValuePair<Slot, List<Person>>(s, s.Assigned))
                                .ToList();
                if (stackCount == 0)
                    break;
                int firstY = topY + 16;
                int lastY = rect.Bottom - 26;
                int spacing = (stackCount > 1) ? (lastY - firstY) / (stackCount - 1) : 0;
                for (int i = 0; i < slots.Count; i++) {
                    var slot = slots[i];
                    var slotInfo = slot.Key;
                    for (int j = 0; j < slot.Value.Count; j++) {
                        var person = slot.Value[j];
                        currentY = firstY + (spacing * (i + 1) * j);
                        person.GenerateCenteredLabelTemplate(centerX, currentY, slotInfo.SlotColor);
                        lastLabelLoc = person.Label.Location;
                        Controls.Add(person.Label);
                        _labels.Add(person.Label);
                    }
                }
                break;
            case LabelPattern.Single:
            case LabelPattern.SingleAsst:
            case LabelPattern.SingleAsstwEFDA:
                SuspendLayout();
                if (team.TeamLead is not null && team.TeamLead.Available) {
                    teamLead = team.TeamLead;
                    currentY = lastLabelLoc.Y + 16;
                    slotColor = teamLead.Skills.OrderByDescending(s => s.Priority).First().SlotColor;
                    teamLead.GenerateCenteredLabelTemplate(centerX, currentY, slotColor);
                    lastLabelLoc = teamLead.Label.Location;
                    Controls.Add(teamLead.Label);
                    _labels.Add(teamLead.Label);
                    foreach (var slot in team.Slots) {
                        foreach (var person in slot.Assigned.Except([team.TeamLead])) {
                            if (person is null) continue;

                            currentY = lastLabelLoc.Y + 16;
                            person.GenerateCenteredLabelTemplate(centerX, currentY, slotColor);
                            lastLabelLoc = person.Label.Location;
                            Controls.Add(person.Label);
                            _labels.Add(person.Label);
                        }
                    }
                } else {
                    currentY = lastLabelLoc.Y;
                    foreach (var slot in team.Slots) {
                        foreach (var person in slot.Assigned) {
                            if (person is null) continue;
                            currentY = lastLabelLoc.Y + 16;
                            person.GenerateCenteredLabelTemplate(centerX, currentY, slot.SlotColor);
                            lastLabelLoc = person.Label.Location;
                            Controls.Add(person.Label);
                            _labels.Add(person.Label);
                        }
                    }
                }
                break;
            case LabelPattern.DualAsst:
            case LabelPattern.DualAsstPlus:
            case LabelPattern.DualAsstwEFDA:
            case LabelPattern.DualAsstwEFDAPlus:
                if (team.TeamLead is null)
                    ArgumentNullException.ThrowIfNull(nameof(team.TeamLead));
                SuspendLayout();
                teamLead = team.TeamLead!;
                currentY = lastLabelLoc.Y + 16;
                slotColor = teamLead.Skills.OrderByDescending(s => s.Priority).First().SlotColor;
                teamLead.GenerateCenteredLabelTemplate(centerX, currentY, slotColor);
                lastLabelLoc = teamLead.Label.Location;
                Controls.Add(teamLead.Label);
                _labels.Add(teamLead.Label);
                slotColor = _skills.Where(s => s.Description == "Dental Assistant").First().SlotColor;
                var assistants = team
                    .Slots
                    .Where(s => s.Description == "Dental Assistant")
                    .SelectMany(s => s.Assigned)
                    .Except([teamLead])
                    .ToArray();
                var efAssistant = team
                    .Slots
                    .Where(s => s.Description == "EFDA")
                    .SelectMany(s => s.Assigned)
                    .Except([teamLead])
                    .Except(assistants)
                    .FirstOrDefault<Person>();
                assistants[0].GenerateLabelTemplate(slotColor);
                assistants[1].GenerateLabelTemplate(slotColor);
                int totalWidth = assistants[0].Label.Width + assistants[1].Label.Width + 5;
                int startX = centerX - (totalWidth / 2);
                assistants[0].Label!.Location = new Point(startX, lastLabelLoc.Y + 16);
                lastLabelLoc = new Point(startX + assistants[0].Label!.Width + 5, lastLabelLoc.Y + 16);
                assistants[1].Label!.Location = lastLabelLoc;
                Controls.Add(assistants[0].Label);
                Controls.Add(assistants[1].Label);
                _labels.Add(assistants[0].Label);
                _labels.Add(assistants[1].Label);
                if (efAssistant is not null) {
                    slotColor = _skills.Where(s => s.Description == "EFDA").First().SlotColor;
                    currentY = lastLabelLoc.Y + 16;
                    efAssistant.GenerateCenteredLabelTemplate(centerX, currentY, slotColor);
                    lastLabelLoc = efAssistant.Label.Location;
                    Controls.Add(efAssistant.Label);
                    _labels.Add(efAssistant.Label);
                }
                foreach (var assistant in assistants[2..]) {
                    slotColor = _skills.Where(s => s.Description == "Dental Assistant").First().SlotColor;
                    currentY = lastLabelLoc.Y + 16;
                    assistant.GenerateCenteredLabelTemplate(centerX, currentY, slotColor);
                    lastLabelLoc = assistant.Label.Location;
                    Controls.Add(assistant.Label);
                    _labels.Add(assistant.Label);
                }
                break;
            default:
                return;
        }

        foreach (var label in _labels) {
            label.MouseDown += SlotLabel_MouseDown;
            label.ContextMenuStrip = LabelContextMenu;
        }
        ResumeLayout();
    }
    private void DeleteLables(Team team) {
        SuspendLayout();
        team.Slots
            .SelectMany(s => s.Assigned)
            .Select(p => p.Label)
            .ToList()
            .ForEach(label => {
                _labels.Remove(label);
                Controls.Remove(label);
            });
        ResumeLayout();
    }
    private void ClearLabels() {
        Controls
            .OfType<Label>()
            .ToList()
            .ForEach(label => {
                Controls.Remove(label);
        });
    }
    private void DrawFloat() {
        const int horizontalSpacing = 5;
        const int verticalSpacing = 5;
        if (_float.PrimaryLocation is null)
            return;
        Rectangle rect = _float.PrimaryLocation.Rect;
        rect = _floorplan.TransformRectangle(rect);
        int firstX = rect.Left + (int)(10 * _floorplan.GetScale().X);
        int firstY = rect.Top + (int)(15 * _floorplan.GetScale().Y);
        int currentX = firstX;
        int currentY = firstY;
        int rowHeight = 0;
        SuspendLayout();
        foreach (var person in _availablePeople.People) {
            if (person.Label is not null && Controls.OfType<Label>().Contains(person.Label))
                Controls.Remove(person.Label);
            person.Label = new() {
                Text = $"{person.LastName}, {person.FirstName[..1]}.",
                AutoSize = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Tag = person,
                Width = 1,
                Location = new Point(0, 0)
            };
            Controls.Add(person.Label);
            person.Label.PerformLayout();
            if (currentX + person.Label.Width > rect.Right - horizontalSpacing) {
                currentX = firstX;
                currentY += rowHeight + verticalSpacing;
                rowHeight = 0;
                if (currentY + person.Label.Height > rect.Bottom)
                    break;
            }

            person.Label.Location = new() {
                X = currentX,
                Y = currentY
            };
            currentX += person.Label.Width + horizontalSpacing;
            rowHeight = Math.Max(rowHeight, person.Label.Height);
            person.Label.MouseDown += SlotLabel_MouseDown;
            person.Label.ContextMenuStrip = LabelContextMenu;
        }
        ResumeLayout();
    }
    private void DrawUnavailable() {
        const int horizontalSpacing = 5;
        const int verticalSpacing = 5;
        if (_unavailable.PrimaryLocation is null)
            return;
        Rectangle rect = _unavailable.PrimaryLocation.Rect;
        rect = _floorplan.TransformRectangle(rect);
        int firstX = rect.Left + (int)(10 * _floorplan.GetScale().X);
        int firstY = rect.Top + (int)(15 * _floorplan.GetScale().Y);
        int currentX = firstX;
        int currentY = firstY;
        int rowHeight = 0;
        SuspendLayout();
        foreach (var person in _unavailablePeople) {
            person.Team = _unavailable;
            if (person.Label is not null && Controls.OfType<Label>().Contains(person.Label))
                Controls.Remove(person.Label);
            person.Label = new() {
                Text = $"{person.LastName}, {person.FirstName[..1]}.",
                AutoSize = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Tag = person,
                Width = 1,
                Location = new Point(0, 0)
            };
            Controls.Add(person.Label);
            person.Label.PerformLayout();
            if (currentX + person.Label.Width > rect.Right - horizontalSpacing) {
                currentX = firstX;
                currentY += rowHeight + verticalSpacing;
                rowHeight = 0;
                if (currentY + person.Label.Height > rect.Bottom)
                    break;
            }

            person.Label.Location = new() {
                X = currentX,
                Y = currentY
            };
            currentX += person.Label.Width + horizontalSpacing;
            rowHeight = Math.Max(rowHeight, person.Label.Height);
            person.Label.MouseDown += SlotLabel_MouseDown;
            person.Label.ContextMenuStrip = LabelContextMenu;
        }
        ResumeLayout();
    }
    #endregion
}
