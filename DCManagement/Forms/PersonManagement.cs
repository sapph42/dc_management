using DCManagement.Classes;
using System.Data;
using System.Text;

namespace DCManagement.Forms;
public partial class PersonManagement : Form {
    private PersonCollection _people = [];
    private List<Skill> _skills = [];
    private Dictionary<int, string> _teams = [];
    private bool _inserting = false;
    private Person? _selectedPerson;
    private DataManagement _data;
    public PersonManagement() {
        InitializeComponent();
        _data = new(Program.Source);
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
            newPerson.Skills.Add(_skills.First(s => s.Equals(skill)));
        }

        newPerson.PersonID = _data.AddNewPerson(newPerson);
        _data.UpdatePersonSkills(newPerson, _skills);
        RefreshBox(newPerson.PersonID);
    }
    private void LoadTeams() {
        _teams = [];
        _teams.Add(-1, "No Team Selected");
        foreach (Team team in _data.GetTeams(true)) {
            _teams.Add((int)team.TeamID!, team.TeamName);
        }
    }
    private void RefreshBox(int? selectedValue = null) {
        _people = _data.GetPersonList();
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
        _data.UpdatePerson(_selectedPerson, _skills);
        RefreshBox(_selectedPerson.PersonID);
    }
    #endregion
    #region Form Event Handlers
    private void PersonManagement_Load(object sender, EventArgs e) {
        LoadTeams();
        _skills = _data.GetSkills();
        TeamCombobox.DataSource = new BindingSource(_teams, null);
        TeamCombobox.DisplayMember = "Value";
        TeamCombobox.ValueMember = "Key";
        TeamCombobox.BindingContext = new();
        SkillsListbox.DataSource = new BindingSource(_skills, null);
        SkillsListbox.DisplayMember = "Description";
        SkillsListbox.ValueMember = "SkillID";
        RefreshBox();
    }
    #endregion
    #region Control Event Handlers
    private void EmployeeListbox_SelectedIndexChanged(object sender, EventArgs e) {
        int personID = ((KeyValuePair<int, string>)EmployeeListbox.Items[EmployeeListbox.SelectedIndex]).Key;
        _inserting = false;
        _selectedPerson = _data.GetPerson(personID);
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
                _selectedPerson.Skills.Add(_skills.First(s => s.Equals(skill)));
            }
            UpdatePerson();
        } else if (_inserting) {
            AddNewPerson();
            _inserting = false;
        }
    }
    #endregion
}
