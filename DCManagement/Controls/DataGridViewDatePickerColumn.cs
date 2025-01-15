namespace DCManagement.Controls; 
public class DataGridViewDatePickerColumn : DataGridViewColumn {
    public DataGridViewDatePickerColumn() : base(new DataGridViewDatePickerCell()) { }
    public override DataGridViewCell CellTemplate { 
        get => base.CellTemplate; 
        set {
            if (value is not null && !value.GetType().IsAssignableFrom(typeof(DataGridViewDatePickerCell)))
                throw new InvalidCastException("Must be a DataGridViewDatePickerCell");
            base.CellTemplate = value;
        }
    }
}
