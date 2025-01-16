using System.Diagnostics;

namespace DCManagement.Controls; 
public class DataGridViewDatePickerCell : DataGridViewTextBoxCell {
    public DataGridViewDatePickerCell() : base() {
        Style.Format = "d";
    }
    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
        Debug.Assert(DataGridView is not null &&
            DataGridView.EditingPanel is not null &&
            DataGridView.EditingControl is not null);
        Debug.Assert(!ReadOnly);
        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
        DatePickerEditingControl? ctl = DataGridView.EditingControl as DatePickerEditingControl;
        Debug.Assert(ctl is not null);
        ctl.ShowUpDown = false;
        ctl.Format = DateTimePickerFormat.Custom;
        ctl.CustomFormat = "yyyy-MM-dd";
        ctl.MinDate = DateTime.Parse("2025-01-01");
        ctl.MaxDate = DateTime.Today.AddDays(365);
        if (Value is null)
            ctl!.Value = (DateTime)DefaultNewRowValue;
        else
            ctl!.Value = (DateTime)Value;
    }
    public override Type EditType => typeof(DatePickerEditingControl);
    public override Type ValueType => typeof(DateTime);
    public override object DefaultNewRowValue => DateTime.Today;
}
