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
    private readonly List<Skill> _slotTypes = [];
    private Dictionary<int, string> _teams = [];
    private bool _inserting = false;
    private Person? _selectedPerson;
    public PersonManagement() {
        InitializeComponent();
    }
    #region Database Interaction
    private void AddNewPerson() {
        Person newPerson = new() {
            PersonID = -1,
            LastName = LastnameTextbox.Text,
            FirstName = FirstnameTextbox.Text,
            Team = new() {
                TeamID = (int)TeamCombobox.SelectedValue!
            },
            Active = ActiveCheckbox.Checked,
            Available = AvailableCheckbox.Checked,
            Skills = []
        };
        foreach (object skill in SkillsListbox.CheckedItems) {
            newPerson.Skills.Add(_slotTypes.First(s => s.Equals(skill)));
        }

        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.Connection = Program.conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertPerson";
        cmd.Parameters.AddRange(newPerson.GetSqlParameters()[1..]);
        newPerson.PersonID = (int)cmd.ExecuteScalar();

        cmd.CommandText = "dbo.SetPersonSkill";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@IsSet", SqlDbType.Bit);
        cmd.Parameters["@PersonID"].Value = newPerson.PersonID;
        foreach (Skill st in _slotTypes) {
            cmd.Parameters["@SkillID"].Value = st.SkillID;
            cmd.Parameters["@IsSet"].Value = newPerson.Skills.Contains(st);
            _ = cmd.ExecuteNonQuery();
        }

        RefreshBox(newPerson.PersonID);
    }
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
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml(reader.GetString(2))
            });
        }
        reader.Close();
    }
    private void LoadPersonInfo(int personID) {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPersonData";
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = personID;
        cmd.Connection = Program.conn;
        using SqlDataAdapter adapter = new(cmd);
        DataSet ds = new();
        adapter.Fill(ds);
        DataTable person = ds.Tables[0];
        DataTable skills = ds.Tables[1];
        if (person.Rows.Count == 0 || person.Rows[0].ItemArray is null)
            return;
        _selectedPerson = new Person(person.Rows[0].ItemArray!);
        if (skills.Rows.Count == 0)
            return;
        foreach (DataRow row in skills.Rows) {
            if (row[0] is null || row[1] is null) continue;
            Skill newSkill = new() {
                SkillID = (int)row[0],
                Description = (string)row[1]
            };
            if (row[2] is not null)
                newSkill.SetSlotColor((string)row[2]);
            _selectedPerson.AddSkill(newSkill);
        }
    }
    private void LoadTeams() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetTeams";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        _teams = [];
        _teams.Add(-1, "No Team Selected");
        if (reader.Read()) {
            _teams.Add(reader.GetInt32(0), reader.GetString(1));
        }
        reader.Close();
    }
    private void RefreshBox(int? selectedValue = null) {
        GetPeople();
        if (_people.Count == 0)
            return;
        EmployeeListbox.DataSource = new BindingSource(_people.ListboxDatasource, null);
        EmployeeListbox.DisplayMember = "Value";
        EmployeeListbox.ValueMember = "Key";
        EmployeeListbox.Refresh();
        if (selectedValue != null) {
            EmployeeListbox.SelectedValue = selectedValue;
        }
    }
    private void UpdatePerson() {
        if (_selectedPerson is null)
            return;
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.Connection = Program.conn;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdatePerson";
        cmd.Parameters.AddRange(_selectedPerson.GetSqlParameters());
        _ = cmd.ExecuteScalar();

        cmd.CommandText = "dbo.SetPersonSkill";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@IsSet", SqlDbType.Bit);
        cmd.Parameters["@PersonID"].Value = _selectedPerson.PersonID;
        foreach (Skill st in _slotTypes) {
            cmd.Parameters["@SkillID"].Value = st.SkillID;
            cmd.Parameters["@IsSet"].Value = _selectedPerson.Skills.Contains(st);
            _ = cmd.ExecuteNonQuery();
        }

        RefreshBox(_selectedPerson.PersonID);
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
        TeamCombobox.SelectedValue = _selectedPerson.TeamID;
        ActiveCheckbox.Checked = _selectedPerson.Active;
        AvailableCheckbox.Checked = _selectedPerson.Available;
        SkillsListbox.ClearSelected();
        for (int i = 0; i < SkillsListbox?.Items.Count; i++) {
            if (_selectedPerson.Skills.Any(s => s.SkillID == ((Skill?)SkillsListbox?.Items[i])?.SkillID))
                SkillsListbox.SetItemChecked(i, true);
            else
                SkillsListbox.SetItemChecked(i, false);
        }
    }
    private void NewPersonButton_Click(object sender, EventArgs e) {
        _inserting = true;
        _selectedPerson = null;
        FirstnameTextbox.Text = "";
        LastnameTextbox.Text = "";
        TeamCombobox.SelectedValue = -1;
        ActiveCheckbox.Checked = true;
        AvailableCheckbox.Checked = true;
        for (int i = 0; i < SkillsListbox?.Items.Count; ++i) {
            SkillsListbox.SetItemChecked(i, false);
        }
    }
    private void SaveButton_Click(object sender, EventArgs e) {
        if (!_inserting && _selectedPerson is not null) {
            _selectedPerson.LastName = LastnameTextbox.Text;
            _selectedPerson.FirstName = FirstnameTextbox.Text;
            _selectedPerson.Team = new() {
                TeamID = (int)TeamCombobox.SelectedValue!
            };
            _selectedPerson.Active = ActiveCheckbox.Checked;
            _selectedPerson.Available = AvailableCheckbox.Checked;
            _selectedPerson.Skills = [];
            foreach (object skill in SkillsListbox.CheckedItems) {
                _selectedPerson.Skills.Add(_slotTypes.First(s => s.Equals(skill)));
            }
            UpdatePerson();
        } else if (_inserting) {
            AddNewPerson();
            _inserting = false;
        }
    }
    #endregion
}
