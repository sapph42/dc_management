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

namespace DCManagement.Forms;
public partial class TeamManagement : Form {
    #region Fields

    private Dictionary<int, string> _teams = [];
    private LocationCollection _locations = [];
    private PersonCollection _people = [];
    private Team? _selectedTeam;
    private bool _insert = false;
    private bool _boxDirty = false;
    #endregion
    public TeamManagement() {
        Program.conn = new(Program.SqlConnectionString);
        InitializeComponent();
    }
    #region Internal Helper Functions
    #region Database Interaction
    private void LoadTeams() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetTeams";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        _teams = [];
        if (reader.Read()) {
            _teams.Add(reader.GetInt32(0), reader.GetString(1));
        }
        reader.Close();
    }
    private void GetLocations() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location";
        cmd.Connection = Program.conn;
        _locations = [];
        _locations.Add(new() {
            LocID = -1,
            Name = "No Team Assigned"
        });
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            _locations.Add(new Location(row));
        }
        reader.Close();
    }
    private void GetPeople() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPeople";
        cmd.Connection = Program.conn;
        _people = [];
        _people.Add(new() {
            PersonID = -1,
            NameOverride = "No Team Lead"
        });
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[7];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            _people.Add(new Person(row));
        }
        reader.Close();
    }
    private void LoadTeamInfo(int teamID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT TeamID, TeamName, TeamLead, PrimaryLocation, FillIfNoLead, Active FROM dbo.GetTeamInfo(@TeamID)";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = teamID;
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            _selectedTeam = new Team(row);
        }
        reader.Close();
    }
    private void AddNewTeam() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.Connection = Program.conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertTeam";
        cmd.Parameters.Add("@Name", SqlDbType.VarChar);
        cmd.Parameters.Add("@Lead", SqlDbType.Int);
        cmd.Parameters.Add("@LocID", SqlDbType.Int);
        cmd.Parameters.Add("@Fill", SqlDbType.Bit);
        cmd.Parameters.Add("@Active", SqlDbType.Bit);
        cmd.Parameters["@Name"].Value = TeamNameTextbox.Text;
        cmd.Parameters["@Lead"].Value = LeadCombobox.SelectedValue;
        cmd.Parameters["@LocID"].Value = LocationCombobox.SelectedValue;
        cmd.Parameters["@Fill"].Value = FillCheckbox.Checked ? 1 : 0;
        cmd.Parameters["@Active"].Value = ActiveCheckbox.Checked ? 1 : 0;
        int newTeam = (int)cmd.ExecuteScalar();
        _insert = false;
        RefreshBox(newTeam);
    }
    private void UpdateTeam() {
        if (_selectedTeam is null)
            return;
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.Connection = Program.conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdateTeam";
        cmd.Parameters.AddRange(_selectedTeam.GetSqlParameters());
        _ = cmd.ExecuteScalar();
        if (_boxDirty)
            RefreshBox(_selectedTeam.TeamID);
    }
    #endregion
    private void RefreshBox(int? selectedValue = null) {
        LoadTeams();
        if (_teams.Count == 0)
            return;
        _boxDirty = false;
        TeamListbox.DataSource = new BindingSource(_teams, null);
        TeamListbox.DisplayMember = "Value";
        TeamListbox.ValueMember = "Key";
        TeamListbox.Refresh();
        if (selectedValue != null) {
            TeamListbox.SelectedValue = selectedValue;
        }
    }
    #endregion
    #region Form Event Handlers
    private void TeamManagement_Load(object sender, EventArgs e) {
        GetLocations();
        GetPeople();
        LocationCombobox.DataSource = new BindingSource(_locations.ListboxDatasource, null);
        LocationCombobox.DisplayMember = "Value";
        LocationCombobox.ValueMember = "Key";
        LocationCombobox.BindingContext = new();
        LeadCombobox.DataSource = new BindingSource(_people.ListboxDatasource, null);
        LeadCombobox.DisplayMember = "Value";
        LeadCombobox.ValueMember = "Key";
        LeadCombobox.BindingContext = new();
        RefreshBox();
    }
    #endregion
    #region Control Event Handlers
    private void NewTeamButton_Click(object sender, EventArgs e) {
        _selectedTeam = null;
        TeamNameTextbox.Text = "";
        LocationCombobox.SelectedValue = -1;
        LeadCombobox.SelectedValue = -1;
        FillCheckbox.Checked = true;
        ActiveCheckbox.Checked = true;
        SlotButton.Enabled = false;
        _insert = true;
    }
    private void SaveButton_Click(object sender, EventArgs e) {
        if (_insert)
            AddNewTeam();
        else {
            if (_selectedTeam is null)
                return;
            _selectedTeam.TeamName = TeamNameTextbox.Text;
            _selectedTeam.LocationID = (int?)LocationCombobox.SelectedValue ?? -1;
            _selectedTeam.TeamLeadID = (int?)LeadCombobox.SelectedValue ?? -1;
            _selectedTeam.FillIfNoLead = FillCheckbox.Checked;
            _selectedTeam.Active = ActiveCheckbox.Checked;
            if (_selectedTeam.TeamName != TeamListbox.GetItemText(TeamListbox.SelectedItem))
                _boxDirty = true;
            if (!ActiveCheckbox.Checked)
                _boxDirty = true;
            UpdateTeam();
        }
        SlotButton.Enabled = true;
    }
    private void SlotButton_Click(object sender, EventArgs e) {
        if (_selectedTeam is null)
            return;
        SlotAssignment slotAssignment = new(_selectedTeam);
        slotAssignment.ShowDialog();
    }
    private void TeamListbox_SelectedIndexChanged(object sender, EventArgs e) {
        int teamID = ((KeyValuePair<int, string>)TeamListbox.Items[TeamListbox.SelectedIndex]).Key;
        LoadTeamInfo(teamID);
        if (_selectedTeam is null)
            return;
        TeamNameTextbox.Text = _selectedTeam.TeamName;
        LocationCombobox.SelectedValue = _selectedTeam.LocationID;
        LeadCombobox.SelectedValue = _selectedTeam.TeamLeadID;
        FillCheckbox.Checked = _selectedTeam.FillIfNoLead;
        ActiveCheckbox.Checked = _selectedTeam.Active;
        _insert = false;
    }
    #endregion
}