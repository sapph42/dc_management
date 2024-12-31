namespace DCManagement.Forms {
    partial class SlotAssignment {
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
            FormLabel = new Label();
            SlotsDGV = new DataGridView();
            SlotIDColumn = new DataGridViewTextBoxColumn();
            SlotTypeColumn = new DataGridViewComboBoxColumn();
            MinQtyColumn = new DataGridViewTextBoxColumn();
            GoalQtyColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)SlotsDGV).BeginInit();
            SuspendLayout();
            // 
            // FormLabel
            // 
            FormLabel.AutoSize = true;
            FormLabel.Location = new Point(12, 9);
            FormLabel.Name = "FormLabel";
            FormLabel.Size = new Size(142, 15);
            FormLabel.TabIndex = 0;
            FormLabel.Text = "Slot Assignment for Team";
            // 
            // SlotsDGV
            // 
            SlotsDGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            SlotsDGV.Columns.AddRange(new DataGridViewColumn[] { SlotIDColumn, SlotTypeColumn, MinQtyColumn, GoalQtyColumn });
            SlotsDGV.Location = new Point(12, 27);
            SlotsDGV.Name = "SlotsDGV";
            SlotsDGV.Size = new Size(381, 161);
            SlotsDGV.TabIndex = 1;
            SlotsDGV.CellEndEdit += SlotsDGV_CellEndEdit;
            SlotsDGV.CellLeave += SlotsDGV_CellLeave;
            SlotsDGV.RowEnter += SlotsDGV_RowEnter;
            SlotsDGV.RowLeave += SlotsDGV_RowLeave;
            SlotsDGV.UserAddedRow += SlotsDGV_UserAddedRow;
            SlotsDGV.UserDeletedRow += SlotsDGV_UserDeletedRow;
            // 
            // SlotIDColumn
            // 
            SlotIDColumn.HeaderText = "SlotID";
            SlotIDColumn.Name = "SlotIDColumn";
            SlotIDColumn.Visible = false;
            // 
            // SlotTypeColumn
            // 
            SlotTypeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            SlotTypeColumn.HeaderText = "Slot Type";
            SlotTypeColumn.MinimumWidth = 150;
            SlotTypeColumn.Name = "SlotTypeColumn";
            SlotTypeColumn.Width = 150;
            // 
            // MinQtyColumn
            // 
            MinQtyColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            MinQtyColumn.HeaderText = "Minimum Qty";
            MinQtyColumn.Name = "MinQtyColumn";
            MinQtyColumn.Width = 107;
            // 
            // GoalQtyColumn
            // 
            GoalQtyColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            GoalQtyColumn.HeaderText = "Goal Qty";
            GoalQtyColumn.Name = "GoalQtyColumn";
            GoalQtyColumn.Width = 78;
            // 
            // SlotAssignment
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(402, 271);
            Controls.Add(SlotsDGV);
            Controls.Add(FormLabel);
            Name = "SlotAssignment";
            Text = "SlotAssignment";
            Load += SlotAssignment_Load;
            ((System.ComponentModel.ISupportInitialize)SlotsDGV).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label FormLabel;
        private DataGridView SlotsDGV;
        private DataGridViewTextBoxColumn SlotIDColumn;
        private DataGridViewComboBoxColumn SlotTypeColumn;
        private DataGridViewTextBoxColumn MinQtyColumn;
        private DataGridViewTextBoxColumn GoalQtyColumn;
    }
}