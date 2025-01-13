using DCManagement.Classes;

namespace DCManagement.Forms;
public partial class SlotAssignment : Form {
    #region Fields
    private readonly DataManagement _data;
    private readonly Team _team;
    private List<Skill> _skills = [];

    private bool _isRowDirty = false;
    private bool _inserting = false;
    private bool _rowLeaving = false;
    #endregion
    #region Properties
    public TeamSlots Slots = [];
    #endregion
    public SlotAssignment(Team team) {
        InitializeComponent();
        _data = new(Program.Source);
        _team = team;
        FormLabel.Text = $"Slot Assignment for Team {team.TeamName}";
    }
    #region Form Event Handlers
    private void SlotAssignment_Load(object sender, EventArgs e) {
        _skills = _data.GetSkills();
        Slots = _data.GetTeamSlots((int)_team.TeamID!, _skills);
        SlotTypeColumn.DataSource = _skills;
        SlotTypeColumn.DisplayMember = "Description";
        SlotTypeColumn.ValueMember = "SkillID";
        foreach (var slot in Slots) {
            SlotsDGV.Rows.Add([slot.SlotID, slot.SkillID, slot.MinQty, slot.GoalQty]);
        }
    }
    #endregion
    #region Control Event Handlers
    private void SlotsDGV_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
        if (!_rowLeaving)
            return;
        var row = SlotsDGV.Rows[e.RowIndex];
        if (!_isRowDirty)
            return;
        if (_inserting) {
            if ((int)row.Cells[1].Value == 0 || 
                string.IsNullOrWhiteSpace((string)row.Cells[2].Value) || 
                string.IsNullOrWhiteSpace((string)row.Cells[3].Value)) {
                row.ErrorText = "Must fill out all fields for new column!";
            } else {
                row.ErrorText = "";
            }

            var newSlotID = _data.InsertSlot(
                (int)_team.TeamID!,
                (int)row.Cells[1].Value,
                Int32.Parse((string)row.Cells[2].Value),
                Int32.Parse((string)row.Cells[3].Value)
            );


            row.Cells[0].Value = newSlotID;
            Skill thisSkill = _skills.First(st => st.SkillID == (int)row.Cells[1].Value);
            Slots.Add(new() {
                SlotID = newSlotID,
                SkillID = thisSkill.SkillID,
                Description = thisSkill.Description,
                SlotColor = thisSkill.SlotColor,
                Priority = thisSkill.Priority,
                MinQty = Int32.Parse((string)row.Cells[2].Value),
                GoalQty = Int32.Parse((string)row.Cells[3].Value)
            });
            _inserting = false;
            _isRowDirty = false;
            return;
        }
        int slotID = (int)row.Cells[0].Value;
        int result = _data.UpdateSlot(
            (int)row.Cells[0].Value,
            (int)_team.TeamID!,
            (int)row.Cells[1].Value,
            (int)row.Cells[2].Value,
            (int)row.Cells[3].Value
        );
        if (result == 0)
            return;
        for (int i = 0; i < Slots.Count; i++) {
            if (Slots[i].SlotID != slotID)
                continue;
            Skill thisSkill = _skills.First(st => st.SkillID == (int)row.Cells[1].Value);
            Slots[i] = new() {
                SlotID = slotID,
                SkillID = thisSkill.SkillID,
                Description = thisSkill.Description,
                SlotColor = thisSkill.SlotColor,
                Priority = thisSkill.Priority,
                MinQty = Int32.Parse((string)row.Cells[2].Value),
                GoalQty = Int32.Parse((string)row.Cells[3].Value)
            };
            break;
        }
        _isRowDirty = false;
        _rowLeaving = false;
    }
    private void SlotsDGV_CellLeave(object sender, DataGridViewCellEventArgs e) {
        _isRowDirty = _isRowDirty || SlotsDGV.IsCurrentCellDirty;
    }
    private void SlotsDGV_RowEnter(object sender, DataGridViewCellEventArgs e) {
        _rowLeaving = false;
        _isRowDirty = false;
        _inserting = false;
    }
    private void SlotsDGV_RowLeave(object sender, DataGridViewCellEventArgs e) {
        _rowLeaving = true;
    }
    private void SlotsDGV_UserAddedRow(object sender, DataGridViewRowEventArgs e) {
        _inserting = true;
        _isRowDirty = true;
    }
    private void SlotsDGV_UserDeletedRow(object sender, DataGridViewRowEventArgs e) {
        var row = e.Row;
        int slotID = (int)row.Cells[0].Value;
        if (_data.DeleteSlot(slotID)) {
            Slots.Remove(slotID);
        }
    }
    #endregion
}
