using DCManagement.Classes;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Math.EC.Endo;
using System.ComponentModel;
using System.Data;

namespace DCManagement.Forms; 
public partial class PersonUnavailability : Form {
    public required Person Person;
    private List<Unavailability> unavailabilities = [];
    private readonly DataManagement _data = new(Program.Source);
    private bool _rowLeaving = false;
    private bool _isRowDirty = false;
    private bool _inserting = false;
    private DateTime _pendingStart = DateTime.MinValue;
    private DateTime _pendingEnd = DateTime.MinValue;
    public PersonUnavailability(Person person) {
        Person = person;
        InitializeComponent();
    }
    public PersonUnavailability() {
        InitializeComponent();
    }
    private void PersonUnavailability_Load(object sender, EventArgs e) {
        unavailabilities = [.. _data.GetUnavailableData(Person).OrderBy(u => u.StartDate)];
        NameLabel.Text += Person.FullName;
        foreach (var unavail in unavailabilities) {
            UnavailabilityDGV.Rows.Add([unavail.RecordID, Person.PersonID, unavail.StartDate, unavail.EndDate]);
        }
    }
    private void UnavailabilityDGV_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
        UnavailabilityDGV.Rows[e.RowIndex].ErrorText = string.Empty;
        if (!_rowLeaving)
            return;
        var row = UnavailabilityDGV.Rows[e.RowIndex];
        if (!_isRowDirty)
            return;
        if (_inserting) {
            if (string.IsNullOrEmpty((string)row.Cells[2].Value) || string.IsNullOrEmpty((string)row.Cells[3].Value)) {
                row.ErrorText = "Must fill out all fields for new row!";
                return;
            }
            DateTime startDate;
            DateTime endDate;
            if (row.Cells[2].Value is DateTime && row.Cells[3].Value is DateTime) {
                startDate = (DateTime)row.Cells[2].Value;
                endDate = (DateTime)row.Cells[3].Value;
            } else if (DateTime.TryParse((string)row.Cells[2].Value, out DateTime startOut) && 
                DateTime.TryParse((string)row.Cells[3].Value, out DateTime endOut)) {
                startDate = startOut;
                endDate = endOut;
            } else {
                row.ErrorText = "Must enter a valid date string";
                return;
            }

            var unavailability = _data.SetUnavailability(Person, startDate, endDate);
            unavailabilities.Add(unavailability);
            _inserting = false;
            _isRowDirty = false;
            return;
        } else {
            int recordID = int.Parse(UnavailabilityDGV.Rows[e.RowIndex].Cells["RecordIDColumn"].Value.ToString()!);
            int personID = int.Parse(UnavailabilityDGV.Rows[e.RowIndex].Cells["PersonIDColumn"].Value.ToString()!);
            Unavailability thisUnav = unavailabilities.First(u => u.RecordID == recordID);
            unavailabilities.Remove(thisUnav);
            DateTime startDate;
            DateTime endDate;
            if (row.Cells[2].Value is DateTime && row.Cells[3].Value is DateTime) {
                startDate = (DateTime)row.Cells[2].Value;
                endDate = (DateTime)row.Cells[3].Value;
            } else if (DateTime.TryParse((string)row.Cells[2].Value, out DateTime startOut) &&
                DateTime.TryParse((string)row.Cells[3].Value, out DateTime endOut)) {
                startDate = startOut;
                endDate = endOut;
            } else {
                row.ErrorText = "Must enter a valid date string";
                return;
            }

            thisUnav.StartDate = startDate;
            thisUnav.EndDate = endDate;
            thisUnav = _data.SetUnavailabiliy(Person, thisUnav);
            unavailabilities.Add(thisUnav);
            _isRowDirty = false;
            _rowLeaving = false;
        }
    }
    private void UnavailabilityDGV_CellLeave(object sender, DataGridViewCellEventArgs e) {
        _isRowDirty = _isRowDirty || UnavailabilityDGV.IsCurrentCellDirty;
    }
    private void UnavailabilityDGV_RowEnter(object sender, DataGridViewCellEventArgs e) {
        _rowLeaving = false;
        _isRowDirty = false;
        _inserting = false;
    }
    private void UnavailabilityDGV_RowLeave(object sender, DataGridViewCellEventArgs e) {
        _rowLeaving = true;
    }
    private void UnavailabilityDGV_UserAddedRow(object sender, DataGridViewRowEventArgs e) {
        _inserting = true;
        _isRowDirty = true;
    }
    private void UnavailabilityDGV_UserDeletedRow(object sender, DataGridViewRowEventArgs e) {
        var row = e.Row;
        int recordID = (int)row.Cells["RecordIDColumn"].Value;
        Unavailability thisUnav = unavailabilities.First(u => u.RecordID == recordID);
        unavailabilities.Remove(thisUnav);
        _data.DeleteUnavailability(thisUnav);
    }
}
