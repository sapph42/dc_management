using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace DCManagement.Forms;
public partial class DailyAssignment : Form {
    private List<Skill> _slotTypes = [];
    private PersonCollection _people = [];
    private List<Team> _teams = [];
    private Team _float = new() {
        TeamID = 9999
    };
    private AvailablePeople _availablePeople = new();
    private Floorplan _floorplan;
    private Size _maxSize;
    private List<Label> _labels = [];
    public DailyAssignment() {
        InitializeComponent();
        _floorplan = new Floorplan() {
            Locations = LocationCollection.GetLocations(),
        };
        _maxSize = new() {
            Height = ClientSize.Height,
            Width = ClientSize.Width
        };
        _floorplan.LoadFloorplan();
        ResizeForm();
        BackgroundImage = _floorplan.ImageWithLocations;
    }
    #region Data Methods
    private void FillGoalSlots(int SkillID) {
        //First try and fill all teams to goal staffing from float
        List<Team> teamsToFill = [.. _teams.Where(t => !t.HasGoalStaffing() && t.HighestPriorityNeed() == SkillID)];
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
                Team donorTeam = _teams.First(t => t.Slots.AvailableSlotsForGoal.Contains(kvp.Key));
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
        List<Team> teamsToFill = [.. _teams.Where(t => !t.HasMinimumStaffing() && t.HighestPriorityNeed() == SkillID)];
        for (int i = 0; i < teamsToFill.Count; i++) {
            Team team = teamsToFill[i];
            if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.Available)
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
                Team donorTeam = _teams.First(t => t.Slots.AvailableSlots.Contains(kvp.Key));
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
    private List<Person> GetDefaultSlotAssignments(int TeamID, int SlotTypeID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetDefaultSlotAssignments";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters.Add("@SlotType", SqlDbType.Int);
        cmd.Parameters["@SlotType"].Value = SlotTypeID;
        cmd.Connection = Program.conn;
        List<Person> members = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            if (!_people.ContainsKey(personID))
                _people.Add(GetPersonData(personID));
            members.Add(_people[personID]);
        }
        reader.Close();
        return members;
    }
    private static Location GetLocationData(int LocationID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location WHERE LocID=@LocID";
        cmd.Parameters.Add("@LocID", SqlDbType.Int);
        cmd.Parameters["@LocID"].Value = LocationID;
        cmd.Connection = Program.conn;
        Location loc = new();
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            loc = new(row);
        }
        reader.Close();
        return loc;
    }
    private static Person GetPersonData(int PersonID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPersonData";
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = PersonID;
        cmd.Connection = Program.conn;
        using SqlDataAdapter adapter = new(cmd);
        DataSet ds = new();
        adapter.Fill(ds);
        DataTable personData = ds.Tables[0];
        DataTable skills = ds.Tables[1];
        if (personData.Rows.Count == 0 || personData.Rows[0].ItemArray is null)
            throw new ArgumentException("No such person exists");
        Person person = new(personData.Rows[0].ItemArray!);
        if (skills.Rows.Count == 0)
            return person;
        foreach (DataRow dataRow in skills.Rows) {
            if (dataRow[0] is null || dataRow[1] is null) continue;
            Skill newSkill = new() {
                SkillID = (int)dataRow[0],
                Description = (string)dataRow[1]
            };
            if (dataRow[2] is not null)
                newSkill.SetSlotColor((string)dataRow[2]);
            person.AddSkill(newSkill);
        }
        return person;
    }
    private void GetSlotTypes() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetSlotTypes";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            _slotTypes.Add(new() {
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml(reader.GetString(2))
            });
        }
        reader.Close();
    }
    private static List<Team> GetTeamData() {
        List<Team> teams = [];
        PersonCollection people = [];
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT TeamID, TeamName, TeamLead, PrimaryLocation, FillIfNoLead FROM Team WHERE Active=1";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            teams.Add(new Team(row));
        }
        reader.Close();
        return teams;
    }
    private TeamSlots GetTeamSlots(int TeamID) {
        TeamSlots slots = [];
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT SlotID, SlotType, MinQty, GoalQty FROM dbo.GetTeamSlots(@TeamID)";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Skill thisSkill = _slotTypes.First(st => st.SkillID == reader.GetInt32(1));
            slots.Add(
                new() {
                    SlotID = reader.GetInt32(0),
                    SkillID = thisSkill.SkillID,
                    Description = thisSkill.Description,
                    SlotColor = thisSkill.SlotColor,
                    MinQty = reader.GetInt32(2),
                    GoalQty = reader.GetInt32(3)
                }
            );
        }
        reader.Close();
        return slots;
    }
    private void MoveUnassignedToFloat() {
        //First lets look at teams that have available personnel
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            // If FillIfNoLead is false, personnel are never available
            if (!team.FillIfNoLead)
                continue;
            // If there is a TeamLead who is Active and Available, its members are not available
            if (team.TeamLead is not null && team.TeamLead.Available && team.TeamLead.Active)
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
            Person thisPerson = _people[i];
            if (thisPerson.Team is not null || thisPerson.TeamID != -1)
                continue;
            if (thisPerson.Team is null)
                _float.AssignPerson(thisPerson, thisPerson.Skills.First().SkillID);
            else
                thisPerson.Team.ReassignPerson(thisPerson, _float, thisPerson.Skills.First().SkillID);
            thisPerson.Team = _float;
            _availablePeople.People.Add(thisPerson);
        }
    }
    private void PrepTeams() {
        for (int i = 0; i < _teams.Count; i++) {
            Team team = _teams[i];
            if (team.TeamLeadID == -1)
                continue;
            if (!team.Active)
                continue;
            Person lead = GetPersonData(team.TeamLeadID);
            _people.Add(lead);
            team.TeamLead = lead;
            if (team.LocationID == -1) {
                team.PrimaryLocation = GetLocationData(team.LocationID);
                team.CurrentAssignment = team.PrimaryLocation;
            }
            team.Slots = GetTeamSlots((int)team.TeamID!);
            for (int j = 0; j < team.Slots.Count; j++) {
                Slot slot = team.Slots[j];
                int slotTypeID = slot.SkillID;
                slot.AssignSkillProperties(_slotTypes[slotTypeID]);
                slot.Assigned = GetDefaultSlotAssignments((int)team.TeamID, slotTypeID);
                team.Slots[j] = slot;
            }
            _teams[i] = team;
        }
    }
    #endregion
    private void ResizeForm() {
        Size imageSize = _floorplan.ImageSize;
        float aspectRatio = (float)imageSize.Width / (float)imageSize.Height;
        if (imageSize.Width <= _maxSize.Width && imageSize.Height <= _maxSize.Height) {
            Size = imageSize;
            BackgroundImageLayout = ImageLayout.Center;
        } else {
            if (aspectRatio >= 1) {
                Size = new Size() {
                    Width = _maxSize.Width,
                    Height = (int)(_maxSize.Width / aspectRatio)
                };
            } else {
                Size = new Size() {
                    Width = (int)(_maxSize.Height * aspectRatio),
                    Height = _maxSize.Height
                };
            }
            BackgroundImageLayout = ImageLayout.Stretch;
        }
        _floorplan.ClientSize = this.ClientSize;
    }
    #region Form Event Handlers
    private void DailyAssignment_DragDrop(object sender, DragEventArgs e) {
        if (e.Data is null)
            return;
        Label? label = e.Data.GetData(typeof(Label)) as Label;
        if (label is null)
            return;
        object? tag = label.Tag;
        if (tag is null) 
            return;
        Point dropPoint = PointToClient(new Point(e.X, e.Y));
        Location? loc = _floorplan.Locations.FindByPoint(dropPoint);
        if (loc is null)
            return;
        Team? team = _teams.Where(t => t.CurrentAssignment == loc).FirstOrDefault();
        if (team is null && tag is Team) {
            team = tag as Team;
            team!.CurrentAssignment = loc;
            foreach (var slotLabel in team.Slots.SelectMany(s => s.Assigned).Select(p => p.Label)) {
                DeleteLables(team);
            }
            CreateLabels(team);
        } else if (team is not null && tag is Person person) {
            Slot? slot = team.HighestPriorityMatch(person);
            if (slot is null)
                return;
            team.AssignPerson(person, slot.SkillID, true);
            person.AssignmentLocked = true;
            DeleteLables(team);
            CreateLabels(team);
        }
    }
    private void DailyAssignment_DragOver(object sender, DragEventArgs e) {
        e.Effect = DragDropEffects.Move;
    }
    private void DailyAssignment_Load(object sender, EventArgs e) {
        GetSlotTypes();
        _teams = GetTeamData();
        PrepTeams();
        MoveUnassignedToFloat();
        List<Skill> activeSkills = _teams
            .Select(t => t.Slots)
            .Select(ts => ts
                .ToSlot()
                .Cast<Skill>())
            .SelectMany(l => l)
            .OrderBy(s => s.Priority)
            .ToList();
        foreach (var skill in activeSkills) {
            FillMinimumSlots(skill.SkillID);
        }
        foreach (var skill in activeSkills) {
            FillGoalSlots(skill.SkillID);
        }
        AddLabels();
    }
    #endregion
    #region Label Event Handlers
    private void SlotLabel_MouseDown(object? sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Left) {
            Label? label = sender as Label;
            if (label is not null)
                DoDragDrop(label, DragDropEffects.Move);
        }
    }

    #endregion
    #region Draw People
    private LabelPattern DetermineTeamPattern(Team team) {
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
        Rectangle rect = team.CurrentAssignment?.Rect ?? team.PrimaryLocation!.Rect;
        int centerX = (rect.Left + rect.Width) / 2;
        int topY = rect.Top + 10;
        Point lastLabelLoc = new(0, topY - 16);
        switch (team.LabelPattern) {
            case LabelPattern.None:
                return;
            case LabelPattern.Single:
            case LabelPattern.SingleAsst:
            case LabelPattern.MultiStacked:
            case LabelPattern.SingleAsstwEFDA:
                if (team.TeamLead is not null && team.TeamLead.Available) {
                    team.TeamLead.Label = new() {
                        Text = team.TeamLead.LastName,
                        AutoSize = true,
                        BackColor = team.TeamLead.Skills.OrderByDescending(s => s.Priority).First().SlotColor,
                        Tag = team
                    };
                    lastLabelLoc = new() {
                        X = centerX + (team.TeamLead.Label.Width / 2),
                        Y = lastLabelLoc.Y + 16
                    };
                    team.TeamLead.Label.Location = lastLabelLoc;
                    _labels.Add(team.TeamLead.Label);
                    foreach (var slot in team.Slots) {
                        foreach (var person in slot.Assigned.Except([team.TeamLead])) {
                            if (person is null) continue;
                            person.Label = new() {
                                Text = person.LastName,
                                AutoSize = true,
                                BackColor = slot.SlotColor,
                                Tag = person
                            };
                            lastLabelLoc = new() {
                                X = centerX + (person.Label.Width / 2),
                                Y = lastLabelLoc.Y + 16
                            };
                            person.Label.Location = lastLabelLoc;
                            _labels.Add(person.Label);
                        }
                    }
                } else {
                    foreach (var slot in team.Slots) {
                        foreach (var person in slot.Assigned) {
                            if (person is null) continue;
                            person.Label = new() {
                                Text = person.LastName,
                                AutoSize = true,
                                BackColor = slot.SlotColor,
                                Tag = person
                            };
                            lastLabelLoc = new() {
                                X = centerX + (person.Label.Width / 2),
                                Y = lastLabelLoc.Y + 16
                            };
                            person.Label.Location = lastLabelLoc;
                            _labels.Add(person.Label);
                        }
                    }
                }
                break;
            case LabelPattern.DualAsst:
            case LabelPattern.DualAsstPlus:
            case LabelPattern.DualAsstwEFDA:
            case LabelPattern.DualAsstwEFDAPlus:
                team.TeamLead!.Label = new() {
                    Text = team.TeamLead.LastName,
                    AutoSize = true,
                    BackColor = team.TeamLead.Skills.OrderByDescending(s => s.Priority).First().SlotColor,
                    Tag = team
                };
                team.TeamLead.Label.MouseDown += SlotLabel_MouseDown;
                lastLabelLoc = new() {
                    X = centerX + (team.TeamLead.Label.Width / 2),
                    Y = lastLabelLoc.Y + 16
                };
                team.TeamLead.Label.Location = lastLabelLoc;
                _labels.Add(team.TeamLead.Label);
                var assistants = team
                    .Slots
                    .Where(s => s.Description == "Dental Assistant")
                    .SelectMany(s => s.Assigned)
                    .Except([team.TeamLead])
                    .ToArray();
                var efAssistant = team
                    .Slots
                    .Where(s => s.Description == "EFDA")
                    .SelectMany(s => s.Assigned)
                    .Except([team.TeamLead])
                    .Except(assistants)
                    .First();
                assistants[0].Label = new() {
                    Text = assistants[0].LastName,
                    AutoSize = true,
                    BackColor = _slotTypes.Where(s => s.Description == "Dental Assistant").First().SlotColor,
                    Tag = assistants[0]
                };
                assistants[1].Label = new() {
                    Text = assistants[1].LastName,
                    AutoSize = true,
                    BackColor = _slotTypes.Where(s => s.Description == "Dental Assistant").First().SlotColor,
                    Tag = assistants[1]
                };
                int totalWidth = assistants[0].Label.Width + assistants[1].Label.Width;
                int startX = centerX - (totalWidth / 2);
                assistants[0].Label!.Location = new Point(startX, lastLabelLoc.Y + 16);
                lastLabelLoc = new Point(startX + assistants[0].Label!.Width, lastLabelLoc.Y + 16);
                assistants[1].Label!.Location = lastLabelLoc;
                _labels.Add(assistants[0].Label);
                _labels.Add(assistants[1].Label);
                if (efAssistant is not null) {
                    efAssistant.Label = new() {
                        Text = efAssistant.LastName,
                        AutoSize = true,
                        BackColor = _slotTypes.Where(s => s.Description == "EFDA").First().SlotColor,
                        Tag = efAssistant
                    };
                    lastLabelLoc = new Point(centerX + efAssistant.Label.Width, lastLabelLoc.Y + 16);
                    efAssistant.Label.Location = lastLabelLoc;
                    _labels.Add(efAssistant.Label);
                }
                foreach (var assistant in assistants[2..]) {
                    assistant.Label = new() {
                        Text = assistant.LastName,
                        AutoSize = true,
                        BackColor = _slotTypes.Where(s => s.Description == "Dental Assistant").First().SlotColor,
                        Tag = assistant
                    };
                    lastLabelLoc = new Point(centerX + assistant.Label.Width, lastLabelLoc.Y + 16);
                    assistant.Label.Location = lastLabelLoc;
                    _labels.Add(assistant.Label);
                }
                break;
            default:
                return;
        }
        SuspendLayout();
        foreach (var slotLabel in team.Slots.SelectMany(s => s.Assigned).Select(p => p.Label)) {
            slotLabel.MouseDown += SlotLabel_MouseDown;
            Controls.Add(slotLabel);
        }
        ResumeLayout();
    }
    private void DeleteLables(Team team) {
        SuspendLayout();
        foreach (var slotLabel in team.Slots.SelectMany(s => s.Assigned).Select(p => p.Label)) {
            _labels.Remove(slotLabel);
            Controls.Remove(slotLabel);
        }
        ResumeLayout();
    }
    private void AddLabels() {
        foreach (var team in _teams) {
            team.LabelPattern = DetermineTeamPattern(team);
            CreateLabels(team);
        }
    }
    #endregion
}
