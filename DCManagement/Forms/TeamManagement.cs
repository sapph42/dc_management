using DCManagement.Classes;

namespace DCManagement.Forms;
public partial class TeamManagement : Form {
    #region Fields
    private DataManagement _data;
    private Dictionary<int, string> _teams = [];
    private LocationCollection _locations = [];
    private PersonCollection _people = [];
    private Team? _selectedTeam;
    private bool _insert = false;
    private bool _boxDirty = false;
    #endregion
    public TeamManagement() {
        InitializeComponent();
        _data = new(Program.Source);
    }
    #region Internal Helper Functions
    #region Database Interaction
    private void LoadTeams() {
        _teams = [];
        foreach (Team team in _data.GetTeams(true)) {
            _teams.Add((int)team.TeamID!, team.TeamName);
        }
    }
    private void GetLocations() {
        _locations = [];
        _locations.Add(new() {
            LocID = -1,
            Name = "No Location Assigned"
        });
        _locations.AddRangeUnsafe(_data.GetLocCollection());
    }
    private void AddNewTeam() {
        int newTeam = _data.AddNewTeam(
            TeamNameTextbox.Text,
            (int)(LeadCombobox.SelectedValue ?? -1),
            (int)(LocationCombobox.SelectedValue ?? -1),
            FillCheckbox.Checked,
            ActiveCheckbox.Checked,
            ClinicalCheckbox.Checked
        );
        _insert = false;
        RefreshBox(newTeam);
    }
    private void UpdateTeam() {
        if (_selectedTeam is null)
            return;
        _data.UpdateTeam(_selectedTeam);
        if (_boxDirty)
            RefreshBox(_selectedTeam.TeamID);
    }
    #endregion
    private void RefreshBox(int? selectedValue = null) {
        LoadTeams();
        if (_teams.Count == 0)
            return;
        _boxDirty = false;
        TeamListbox.DataSource = new BindingSource(_teams.OrderBy(kvp => kvp.Value), null);
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
        _people = _data.GetPersonList();
        LocationCombobox.DataSource = new BindingSource(_locations.ListboxDatasource.OrderBy(kvp => kvp.Value), null);
        LocationCombobox.DisplayMember = "Value";
        LocationCombobox.ValueMember = "Key";
        LocationCombobox.BindingContext = new();
        LeadCombobox.DataSource = new BindingSource(_people.ListboxDatasource.OrderBy(kvp => kvp.Value), null);
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
        ClinicalCheckbox.Checked = true;
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
            _selectedTeam.Clinical = ClinicalCheckbox.Checked;
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
        _selectedTeam = _data.GetTeam(teamID);
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