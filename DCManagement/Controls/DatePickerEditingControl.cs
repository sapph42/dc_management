namespace DCManagement.Controls; 
public class DatePickerEditingControl : DateTimePicker, IDataGridViewEditingControl {
    DataGridView? dataGridView;
    private bool valueChanged = false;
    int rowIndex;
    public DatePickerEditingControl() {
        Format = DateTimePickerFormat.Short;
    }
    public object EditingControlFormattedValue {
        get {
            return Value.ToShortDateString();
        }
        set {
            if (value is string stringVal) {
                try {
                    Value = DateTime.Parse(stringVal);
                } catch { }
                Value = DateTime.Today;
            } else if (value is DateTime dateTimeVal)
                Value = dateTimeVal;
            else
                Value = DateTime.Today;
        }
    }
    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
        return EditingControlFormattedValue;
    }
    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
        Font = dataGridViewCellStyle.Font;
        CalendarForeColor = dataGridViewCellStyle.ForeColor;
        CalendarMonthBackground = dataGridViewCellStyle.BackColor;
    }
    public int EditingControlRowIndex {
        get => rowIndex;
        set => rowIndex = value;
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "It looks ugly here")]
    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey) {
        switch (key & Keys.KeyCode) {
            case Keys.Left:
            case Keys.Up:
            case Keys.Down:
            case Keys.Right:
            case Keys.Home:
            case Keys.End:
            case Keys.PageDown:
            case Keys.PageUp:
                return true;
            default:
                return !dataGridViewWantsInputKey;
        }
    }
    public void PrepareEditingControlForEdit(bool selectAll) { }
    public bool RepositionEditingControlOnValueChange => false;
    public DataGridView? EditingControlDataGridView {
        get => dataGridView;
        set => dataGridView = value;
    }
    public bool EditingControlValueChanged {
        get => valueChanged;
        set => valueChanged = value;
    }
    public Cursor EditingPanelCursor  => base.Cursor;
    protected override void OnValueChanged(EventArgs eventargs) {
        valueChanged = true;
        EditingControlDataGridView?.NotifyCurrentCellDirty(true);
        base.OnValueChanged(eventargs);
    }
}
