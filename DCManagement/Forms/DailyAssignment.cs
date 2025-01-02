using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Reflection.Metadata.BlobBuilder;

namespace DCManagement.Forms {
    public partial class DailyAssignment : Form {
        private List<SlotType> _slotTypes = [];
        private PersonCollection _people = [];
        private List<Team> _teams = [];
        private Team _float = new() { 
            TeamID = 9999
        };
        private Dictionary<int, List<Person>> _floatBySkill = [];
        public DailyAssignment() {
            InitializeComponent();
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
        private Location GetLocationData(int LocationID) {
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
        private Person GetPersonData(int PersonID) {
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
                SlotType newSkill = new() {
                    SlotTypeID = (int)dataRow[0],
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
                    SlotTypeID = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    SlotColor = ColorTranslator.FromHtml(reader.GetString(2))
                });
            }
            reader.Close();
        }
        private List<Team> GetTeamData() {
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
                slots.Add(
                    new() {
                        SlotID = reader.GetInt32(0),
                        SlotSkill = _slotTypes.First(st => st.SlotTypeID == reader.GetInt32(1)),
                        MinQty = reader.GetInt32(2),
                        GoalQty = reader.GetInt32(3)
                    }
                );
            }
            reader.Close();
            return slots;
        }
        private void MoveUnassignedToFloat() {
            for (int i = 0; i < _teams.Count; i++) {
                Team team = _teams[i];
                if (!team.FillIfNoLead)
                    continue;
                if (team.TeamLead is not null && team.TeamLead.IsAvailable)
                    continue;

                for (int j = 0; j < team.Slots.Count; j++) {
                    Slot slot = team.Slots[j];
                    for (int k = 0; k < slot.Assigned.Count; k++) {
                        Person floater = slot.Assigned[k];
                        floater.Team = _float;
                        int slotType = slot.SlotSkill.SlotTypeID;
                        if (_floatBySkill.ContainsKey(slotType))
                            _floatBySkill[slotType].Add(floater);
                        else
                            _floatBySkill.Add(slotType, [floater]);
                    }
                }
            }
            for (int i = 0; i < _people.Count; i++) {
                Person thisPerson = _people[i];
                if (thisPerson.Team is not null || thisPerson.TeamID != -1)
                    continue;
                thisPerson.Team = _float;
                //TODO FloatBySkill is dumb and won't work because people have multiple skills. Re do it.
            }
        }
        private void FillMinimumSlots() {
            //First try and fill all teams to minimum staffing from float
            for (int i = 0; i < _teams.Count; i++) {
                Team team = _teams[i];
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
                    continue;
                if (team.Slots.AtMinimum)
                    continue;
                foreach (var kvp in team.Slots.MinimumNeeded()) {
                    if (kvp.Value <= 0)
                        continue;
                    if (!_floatBySkill.ContainsKey(kvp.Key))
                        continue;
                    int available = _floatBySkill[kvp.Key].Count;
                    if (available == 0) {
                        _floatBySkill.Remove(kvp.Key);
                        continue;
                    }
                    int willTake = Math.Min(available, kvp.Value);
                    for (int j = 0; j < willTake; j++) {
                        Person thisPerson = _floatBySkill[kvp.Key][j];
                        _floatBySkill[kvp.Key].Remove(thisPerson);
                        Slot slot = team.Slots.First(s => s.SlotSkill.SlotTypeID == kvp.Key);
                        slot.Assigned.Add(thisPerson);
                    }
                }
            }
            if (_teams.Count(t => !t.Slots.AtMinimum) == 0)
                return;
            //Now scan for teams that are over their minimum count and have available unlocked members
            //If found, swap to the team that needs them more
            for (int i = 0; i < _teams.Count; i++) {
                Team team = _teams[i];
                if (!team.FillIfNoLead && team.TeamLead is not null && team.TeamLead.IsAvailable)
                    continue;
                if (team.Slots.AtMinimum)
                    continue;
                foreach (var kvp in team.Slots.MinimumNeeded()) {
                    if (kvp.Value <= 0)
                        continue;
                    Team donorTeam = _teams.First(t => t.Slots.AvailableSlots.Contains(kvp.Key));
                    Slot donorTeamSlot = donorTeam.Slots.First(s => s.SlotSkill.SlotTypeID == kvp.Key);
                    Person donorPerson = donorTeamSlot.Assigned.Random();
                    donorTeamSlot.Assigned.Remove(donorPerson);
                    Slot recipientTeamSlot = team.Slots.First(s => s.SlotSkill.SlotTypeID == kvp.Key);
                    recipientTeamSlot.Assigned.Add(donorPerson);
                }
            }
            if (_teams.Count(t => !t.Slots.AtMinimum) == 0)
                return;
            //TODO Generate some kind of alert that not all teams could be filled
        }
        private void DailyAssignment_Load(object sender, EventArgs e) {
            GetSlotTypes();
            _teams = GetTeamData();

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
                    int slotTypeID = slot.SlotSkill.SlotTypeID;
                    slot.SlotSkill = _slotTypes[slotTypeID];
                    slot.Assigned = GetDefaultSlotAssignments((int)team.TeamID, slotTypeID);
                    team.Slots[j] = slot;
                }
                _teams[i] = team;
            }
            MoveUnassignedToFloat();
        }
    }
}
