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
    private readonly SqlConnection conn;
    private Dictionary<int, string> _teams = [];
    private LocationCollection _locations = [];
    #endregion
    public TeamManagement() {
        conn = new(Program.SqlConnectionString);
        InitializeComponent();
    }
    #region Internal Helper Functions
    #region Database Interaction
    private void ConnOpen() {
        if (conn.State != ConnectionState.Open)
            conn.Open();
    }
    private void LoadTeams() {
        ConnOpen();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetTeams";
        cmd.Connection = conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        _teams = [];
        if (reader.Read()) {
            _teams.Add(reader.GetInt32(0), reader.GetString(1));
        }
        reader.Close();
    }
    private void GetLocations() {
        ConnOpen();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location";
        cmd.Connection = conn;
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
    #endregion
    private void RefreshBox() {
        TeamListbox.DataSource = new BindingSource(_teams, null);
        TeamListbox.DisplayMember = "Value";
        TeamListbox.ValueMember = "Key";
    }
    #endregion
    #region Form Event Handlers
    private void TeamManagement_Load(object sender, EventArgs e) {
        LoadTeams();
        if (_teams.Count > 0)
            RefreshBox();
        GetLocations();
        LocationCombobox.DataSource = new BindingSource(_locations, null);
        LocationCombobox.DisplayMember = "Value.Name";
        LocationCombobox.ValueMember = "Key";
    }
    #endregion
    #region Control Event Handlers
    private void TeamListbox_SelectedIndexChanged(object sender, EventArgs e) {
        int teamID = (int)TeamListbox.Items[TeamListbox.SelectedIndex];
    }
    #endregion
}