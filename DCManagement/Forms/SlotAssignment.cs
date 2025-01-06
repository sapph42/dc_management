using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DCManagement.Forms;
public partial class SlotAssignment : Form {
    #region Fields
    private readonly Team _team;
    private readonly List<Skill> _skills = [];

    private bool _isRowDirty = false;
    private bool _inserting = false;
    private bool _rowLeaving = false;
    #endregion
    #region Properties
    public TeamSlots Slots = [];
    #endregion
    public SlotAssignment(Team team) {
        InitializeComponent();
        _team = team;
        FormLabel.Text = $"Slot Assignment for Team {team.TeamName}";
    }
    #region Database Interaction
    private void GetSkills() {
        using SqlConnection conn = new(Program.SqlConnectionString);;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetSkills";
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            _skills.Add(new() {
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml("#" + reader.GetString(2)),
                Priority = reader.GetInt32(3)
            });
        }
        reader.Close();
    }
    private void GetTeamSlots() {
        using SqlConnection conn = new(Program.SqlConnectionString);;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT SlotID, SkillID, MinQty, GoalQty FROM dbo.GetTeamSlots(@TeamID)";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = _team.TeamID;
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Skill thisSkill = _skills.First(st => st.SkillID == reader.GetInt32(1));
            Slots.Add(
                new() {
                    SlotID = reader.GetInt32(0),
                    SkillID = thisSkill.SkillID,
                    Description = thisSkill.Description,
                    SlotColor = thisSkill.SlotColor,
                    Priority = thisSkill.Priority,
                    MinQty = reader.GetInt32(2),
                    GoalQty = reader.GetInt32(3)
                }
            );
        }
        reader.Close();
    }
    private int InsertNewSlot(DataGridViewRow row) {
        using SqlConnection conn = new(Program.SqlConnectionString);;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertTeamSlot";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@MinQty", SqlDbType.Int);
        cmd.Parameters.Add("@GoalQty", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = _team.TeamID;
        cmd.Parameters["@SkillID"].Value = row.Cells[1].Value;
        cmd.Parameters["@MinQty"].Value = row.Cells[2].Value;
        cmd.Parameters["@GoalQty"].Value = row.Cells[3].Value;
        cmd.Connection = conn;
        conn.Open();
        return (int)cmd.ExecuteScalar();
    }
    private int UpdateSlot(DataGridViewRow row) {
        using SqlConnection conn = new(Program.SqlConnectionString);;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdateTeamSlot";
        cmd.Parameters.Add("@SlotID", SqlDbType.Int);
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@MinQty", SqlDbType.Int);
        cmd.Parameters.Add("@GoalQty", SqlDbType.Int);
        cmd.Parameters["@SlotID"].Value = row.Cells[0].Value;
        cmd.Parameters["@TeamID"].Value = _team.TeamID;
        cmd.Parameters["@SkillID"].Value = row.Cells[1].Value;
        cmd.Parameters["@MinQty"].Value = row.Cells[2].Value;
        cmd.Parameters["@GoalQty"].Value = row.Cells[3].Value;
        cmd.Connection = conn;
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
    private static bool DeleteSlot(int slotID) {
        using SqlConnection conn = new(Program.SqlConnectionString);;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.DeleteTeamSlot";
        cmd.Parameters.Add("@SlotID", SqlDbType.Int);
        cmd.Parameters["@SlotID"].Value = slotID;
        cmd.Connection = conn;
        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }
    #endregion
    #region Form Event Handlers
    private void SlotAssignment_Load(object sender, EventArgs e) {
        GetSkills();
        GetTeamSlots();
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
            var newSlotID = InsertNewSlot(row);
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
        if (UpdateSlot(row) == 0)
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
        if (DeleteSlot(slotID)) {
            Slots.Remove(slotID);
        }
    }
    #endregion
}
