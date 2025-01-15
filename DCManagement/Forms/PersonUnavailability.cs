using DCManagement.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCManagement.Forms {
    public partial class PersonUnavailability : Form {
        public required Person Person;
        private  List<Unavailability> unavailabilities;
        private DataManagement _data = new(Program.Source);
        private bool _newRowNeeded = false;
        private bool _rowLeaving = false;;
        private bool _isRowDirty = false;
        private bool _inserting = false;
        public PersonUnavailability(Person person) {
            Person = person;
            InitializeComponent();
            unavailabilities = _data.GetUnavailableData(Person).OrderBy(u => u.StartDate).ToList();
        }
        private void PersonUnavailability_Load(object sender, EventArgs e) {
            NameLabel.Text += Person.FullName;
            RecordIDColumn.DataPropertyName = "RecordID";
            PersonIDColumn.DataPropertyName = "PersonID";
            StartDateColumn.DataPropertyName = "StartDate";
            EndDateColumn.DataPropertyName = "EndDate";
            UnavailabilityDGV.DataSource = unavailabilities;
            UnavailabilityDGV.Refresh();
        }
        private void UnavailabilityDGV_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            UnavailabilityDGV.Rows[e.RowIndex].ErrorText = string.Empty;
            if (!_rowLeaving)
                return;
            var row = UnavailabilityDGV.Rows[e.RowIndex];
            if (!_isRowDirty)
                return;
            DateTime startDate = (DateTime)UnavailabilityDGV.Rows[e.RowIndex].Cells["StartDate"].Value;
            DateTime endDate = (DateTime)UnavailabilityDGV.Rows[e.RowIndex].Cells["endDate"].Value;
            if (_inserting) {
                var unavailability = _data.SetUnavailability(Person, startDate, endDate);
                unavailabilities.Add(unavailability);
                _inserting = false;
                _isRowDirty = false;
                UnavailabilityDGV.DataSource = unavailabilities.OrderBy(u => u.StartDate).ToList();
                UnavailabilityDGV.Refresh();                
                return;
            }
            int recordID = int.Parse(UnavailabilityDGV.Rows[e.RowIndex].Cells["RecordID"].Value.ToString()!);
            int personID = int.Parse(UnavailabilityDGV.Rows[e.RowIndex].Cells["PersonID"].Value.ToString()!);
            Unavailability thisUnav = unavailabilities.First(u => u.RecordID == recordID);
            unavailabilities.Remove(thisUnav);
            thisUnav.StartDate = startDate;
            thisUnav.EndDate = endDate;
            thisUnav = _data.SetUnavailabiliy(Person, thisUnav);
            unavailabilities.Add(thisUnav);
            UnavailabilityDGV.DataSource = unavailabilities.OrderBy(u => u.StartDate).ToList();
            UnavailabilityDGV.Refresh();
            _isRowDirty = false;
            _rowLeaving = false;
        }
        private void UnavailabilityDGV_CellLeave(object sender, DataGridViewCellEventArgs e) {
            _isRowDirty = _isRowDirty || UnavailabilityDGV.IsCurrentCellDirty;
        }
        private void UnavailabilityDGV_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            string name = UnavailabilityDGV.Columns[e.ColumnIndex].Name;
            switch (name) {
                case "StartDate":
                    if (UnavailabilityDGV.Rows[e.RowIndex].Cells["StartDate"].Value is DateTime sStartDate &&
                        UnavailabilityDGV.Rows[e.RowIndex].Cells["EndDate"].Value is DateTime sEndDate)
                        if (sEndDate < sStartDate && sEndDate != DateTime.Today) {
                            UnavailabilityDGV.Rows[e.RowIndex].ErrorText = "Start Date must be less than or equal to End Date";
                            e.Cancel = true;
                        }
                    break;
                case "EndDate":
                    if (UnavailabilityDGV.Rows[e.RowIndex].Cells["StartDate"].Value is DateTime eStartDate &&
                        UnavailabilityDGV.Rows[e.RowIndex].Cells["EndDate"].Value is DateTime eEndDate)
                        if (eEndDate < eStartDate) {
                            UnavailabilityDGV.Rows[e.RowIndex].ErrorText = "Start Date must be less than or equal to End Date";
                            e.Cancel = true;
                        }
                    break;
                default:
                    break;
            }
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
            int recordID = (int)row.Cells["RecordID"].Value;
            Unavailability thisUnav = unavailabilities.First(u => u.RecordID == recordID);
            unavailabilities.Remove(thisUnav);
            _data.DeleteUnavailability(thisUnav);
        }
    }
}
