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
public partial class PersonManagement : Form {
    private PersonCollection _people = [];
    private readonly List<SlotType> _slotTypes = [];
    private Dictionary<int, string> _teams = [];
    private bool _boxDirty = false;
    private Person? _selectedPerson;
    public PersonManagement() {
        InitializeComponent();
    }
    #region Database Interaction
    private void GetPeople() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPeople";
        cmd.Connection = Program.conn;
        _people = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[7];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            _people.Add(new Person(row));
        }
        reader.Close();
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
    private void LoadPersonInfo(int personID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT PersonID, LastName, FirstName, TeamID, Active, Available FROM dbo.GetPersonInfo(@PersonID)";
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = personID;
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            _selectedPerson = new Person(row);
        }
        reader.Close();
    }
    private void LoadTeams() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetTeams";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        _teams = [];
        _teams.Add(-1,"No Team Selected");
        if (reader.Read()) {
            _teams.Add(reader.GetInt32(0), reader.GetString(1));
        }
        reader.Close();
    }
    private void RefreshBox(int? selectedValue = null) {
        GetPeople();
        if (_people.Count == 0)
            return;
        _boxDirty = false;
        EmployeeListbox.DataSource = new BindingSource(_people.ListboxDatasource, null);
        EmployeeListbox.DisplayMember = "Value";
        EmployeeListbox.ValueMember = "Key";
        EmployeeListbox.Refresh();
        if (selectedValue != null) {
            EmployeeListbox.SelectedValue = selectedValue;
        }
    }
    #endregion
    #region Form Event Handlers
    private void PersonManagement_Load(object sender, EventArgs e) {
        LoadTeams();
        GetSlotTypes();
        TeamCombobox.DataSource = new BindingSource(_teams, null);
        TeamCombobox.DisplayMember = "Value";
        TeamCombobox.ValueMember = "Key";
        TeamCombobox.BindingContext = new();
        SkillsListbox.DataSource = new BindingSource(_slotTypes, null);
        SkillsListbox.DisplayMember = "Description";
        SkillsListbox.ValueMember = "SlotTypeID";
        RefreshBox();
    }
    #endregion
    #region Control Event Handlers
    private void EmployeeListbox_SelectedIndexChanged(object sender, EventArgs e) {
        int personID = ((KeyValuePair<int, string>)EmployeeListbox.Items[EmployeeListbox.SelectedIndex]).Key;
        LoadPersonInfo(personID);
        if (_selectedPerson is null)
            return;
        LastnameTextbox.Text = _selectedPerson.LastName;
        FirstnameTextbox.Text = _selectedPerson.FirstName;
        TeamCombobox.SelectedValue = _selectedPerson.Team?.TeamID;
        ActiveCheckbox.Checked = _selectedPerson.IsActive;
        AvailableCheckbox.Checked = _selectedPerson.IsAvailable;
        SkillsListbox.ClearSelected();
        for (int i = 0; i < SkillsListbox?.Items.Count; i++) {
            if (_selectedPerson.Skills.Any(s => s.SlotSkill.Description == SkillsListbox?.Items[i].ToString()))
                SkillsListbox.SetItemChecked(i, true);
            else
                SkillsListbox.SetItemChecked(i, false);
        }
    }
    private void NewPersonButton_Click(object sender, EventArgs e) {
        _selectedPerson = null;
        FirstnameTextbox.Text = "";
        LastnameTextbox.Text = "";
        TeamCombobox.SelectedValue = -1;
    }
    #endregion
}
