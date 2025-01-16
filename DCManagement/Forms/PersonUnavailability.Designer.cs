using DCManagement.Controls;

namespace DCManagement.Forms; 
partial class PersonUnavailability {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        NameLabel = new Label();
        UnavailabilityDGV = new DataGridView();
        RecordIDColumn = new DataGridViewTextBoxColumn();
        PersonIDColumn = new DataGridViewTextBoxColumn();
        StartDateColumn = new DataGridViewDatePickerColumn();
        EndDateColumn = new DataGridViewDatePickerColumn();
        ((System.ComponentModel.ISupportInitialize)UnavailabilityDGV).BeginInit();
        SuspendLayout();
        // 
        // NameLabel
        // 
        NameLabel.AutoSize = true;
        NameLabel.Location = new Point(12, 9);
        NameLabel.Name = "NameLabel";
        NameLabel.Size = new Size(121, 15);
        NameLabel.TabIndex = 0;
        NameLabel.Text = "Unavailable Dates for ";
        // 
        // UnavailabilityDGV
        // 
        UnavailabilityDGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        UnavailabilityDGV.Columns.AddRange(new DataGridViewColumn[] { RecordIDColumn, PersonIDColumn, StartDateColumn, EndDateColumn });
        UnavailabilityDGV.Location = new Point(12, 45);
        UnavailabilityDGV.Name = "UnavailabilityDGV";
        UnavailabilityDGV.Size = new Size(257, 150);
        UnavailabilityDGV.TabIndex = 1;
        UnavailabilityDGV.CellEndEdit += UnavailabilityDGV_CellEndEdit;
        UnavailabilityDGV.CellLeave += UnavailabilityDGV_CellLeave;
        UnavailabilityDGV.CellValidating += UnavailabilityDGV_CellValidating;
        UnavailabilityDGV.RowEnter += UnavailabilityDGV_RowEnter;
        UnavailabilityDGV.RowLeave += UnavailabilityDGV_RowLeave;
        UnavailabilityDGV.UserAddedRow += UnavailabilityDGV_UserAddedRow;
        UnavailabilityDGV.UserDeletedRow += UnavailabilityDGV_UserDeletedRow;
        // 
        // RecordIDColumn
        // 
        RecordIDColumn.HeaderText = "RecordID";
        RecordIDColumn.Name = "RecordIDColumn";
        RecordIDColumn.Visible = false;
        // 
        // PersonIDColumn
        // 
        PersonIDColumn.HeaderText = "PersonID";
        PersonIDColumn.Name = "PersonIDColumn";
        PersonIDColumn.Visible = false;
        // 
        // StartDateColumn
        // 
        StartDateColumn.HeaderText = "Start Date";
        StartDateColumn.Name = "StartDateColumn";
        StartDateColumn.Visible = true;
        // 
        // EndDateColumn
        // 
        EndDateColumn.HeaderText = "End Date";
        EndDateColumn.Name = "EndDateColumn";
        EndDateColumn.Visible = true;
        // 
        // PersonUnavailability
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(UnavailabilityDGV);
        Controls.Add(NameLabel);
        Name = "PersonUnavailability";
        Text = "PersonUnavailability";
        Load += PersonUnavailability_Load;
        ((System.ComponentModel.ISupportInitialize)UnavailabilityDGV).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private void UnavailabilityDGV_CellLeave1(object sender, DataGridViewCellEventArgs e) {
        throw new NotImplementedException();
    }

    #endregion

    private Label NameLabel;
    private DataGridView UnavailabilityDGV;
    private DataGridViewTextBoxColumn RecordIDColumn;
    private DataGridViewTextBoxColumn PersonIDColumn;
    private DataGridViewDatePickerColumn StartDateColumn;
    private DataGridViewDatePickerColumn EndDateColumn;
}