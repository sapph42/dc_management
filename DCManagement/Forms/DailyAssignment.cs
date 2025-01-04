using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DCManagement.Forms {
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
                if (team.TeamLead is not null && team.TeamLead.IsAvailable && team.TeamLead.IsActive)
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
        private void FillGoalSlots(int SkillID) {
            //First try and fill all teams to goal staffing from float
            List<Team> teamsToFill = [.. _teams.Where(t => !t.HasGoalStaffing() && t.HighestPriorityNeed() == SkillID)];
            for (int i = 0; i < teamsToFill.Count; i++) {
                Team team = teamsToFill[i];
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
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
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
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
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
                    continue;
                if (team.Slots.AtMinimum)
                    continue;
                foreach (var kvp in team.Slots.MinimumNeeded().Where(kvp => kvp.Key == SkillID)) {
                    if (kvp.Value <= 0)
                        continue;
                    int available = _availablePeople.AvailbleSkills(kvp.Value);
                    if (available== 0)
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
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
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
        }
    }
}
