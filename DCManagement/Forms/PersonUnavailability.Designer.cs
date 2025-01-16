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
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        NameLabel = new Label();
        UnavailabilityDGV = new DataGridView();
        RecordIDColumn = new DataGridViewTextBoxColumn();
        PersonIDColumn = new DataGridViewTextBoxColumn();
        StartDateColumn = new DataGridViewTextBoxColumn();
        EndDateColumn = new DataGridViewTextBoxColumn();
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
        UnavailabilityDGV.RowEnter += UnavailabilityDGV_RowEnter;
        UnavailabilityDGV.RowLeave += UnavailabilityDGV_RowLeave;
        UnavailabilityDGV.UserAddedRow += UnavailabilityDGV_UserAddedRow;
        UnavailabilityDGV.UserDeletedRow += UnavailabilityDGV_UserDeletedRow;
        // 
        // RecordIDColumn
        // 
        RecordIDColumn.DataPropertyName = "RecordID";
        RecordIDColumn.HeaderText = "RecordID";
        RecordIDColumn.Name = "RecordIDColumn";
        RecordIDColumn.Visible = false;
        // 
        // PersonIDColumn
        // 
        PersonIDColumn.DataPropertyName = "PersonID";
        PersonIDColumn.HeaderText = "PersonID";
        PersonIDColumn.Name = "PersonIDColumn";
        PersonIDColumn.Visible = false;
        // 
        // StartDateColumn
        // 
        StartDateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        StartDateColumn.DataPropertyName = "StartDate";
        dataGridViewCellStyle1.Format = "d";
        dataGridViewCellStyle1.NullValue = "Enter A Date";
        StartDateColumn.DefaultCellStyle = dataGridViewCellStyle1;
        StartDateColumn.HeaderText = "Start Date";
        StartDateColumn.Name = "StartDateColumn";
        StartDateColumn.Width = 83;
        // 
        // EndDateColumn
        // 
        EndDateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        EndDateColumn.DataPropertyName = "EndDate";
        dataGridViewCellStyle2.Format = "d";
        dataGridViewCellStyle2.NullValue = "Enter A Date";
        EndDateColumn.DefaultCellStyle = dataGridViewCellStyle2;
        EndDateColumn.HeaderText = "End Date";
        EndDateColumn.Name = "EndDateColumn";
        EndDateColumn.Width = 79;
        // 
        // PersonUnavailability
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(327, 244);
        Controls.Add(UnavailabilityDGV);
        Controls.Add(NameLabel);
        Name = "PersonUnavailability";
        Text = "PersonUnavailability";
        Load += PersonUnavailability_Load;
        ((System.ComponentModel.ISupportInitialize)UnavailabilityDGV).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion

    private Label NameLabel;
    private DataGridView UnavailabilityDGV;
    private DataGridViewTextBoxColumn RecordIDColumn;
    private DataGridViewTextBoxColumn PersonIDColumn;
    private DataGridViewTextBoxColumn StartDateColumn;
    private DataGridViewTextBoxColumn EndDateColumn;
}