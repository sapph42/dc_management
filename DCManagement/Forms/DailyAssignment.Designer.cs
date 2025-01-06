namespace DCManagement.Forms {
    partial class DailyAssignment {
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
            EditMenu = new MenuStrip();
            EditTSMI = new ToolStripMenuItem();
            SetClinicFloorplanTSMI = new ToolStripMenuItem();
            DrawNewLocationTSMI = new ToolStripMenuItem();
            MouseCoordTSMI = new ToolStripMenuItem();
            AlertMessagesTSMI = new ToolStripMenuItem();
            CancelPendingActionToolStripMenuItem = new ToolStripMenuItem();
            EditMenu.SuspendLayout();
            SuspendLayout();
            // 
            // EditMenu
            // 
            EditMenu.Items.AddRange(new ToolStripItem[] { EditTSMI, MouseCoordTSMI, AlertMessagesTSMI, CancelPendingActionToolStripMenuItem });
            EditMenu.Location = new Point(0, 0);
            EditMenu.Name = "EditMenu";
            EditMenu.Size = new Size(1596, 24);
            EditMenu.TabIndex = 1;
            EditMenu.Text = "Edit";
            // 
            // EditTSMI
            // 
            EditTSMI.DropDownItems.AddRange(new ToolStripItem[] { SetClinicFloorplanTSMI, DrawNewLocationTSMI });
            EditTSMI.Name = "EditTSMI";
            EditTSMI.Size = new Size(39, 20);
            EditTSMI.Text = "Edit";
            EditTSMI.Visible = false;
            // 
            // SetClinicFloorplanTSMI
            // 
            SetClinicFloorplanTSMI.Name = "SetClinicFloorplanTSMI";
            SetClinicFloorplanTSMI.Size = new Size(180, 22);
            SetClinicFloorplanTSMI.Text = "Set Clinic Floorplan";
            // 
            // DrawNewLocationTSMI
            // 
            DrawNewLocationTSMI.Name = "DrawNewLocationTSMI";
            DrawNewLocationTSMI.Size = new Size(180, 22);
            DrawNewLocationTSMI.Text = "Draw New Location";
            // 
            // MouseCoordTSMI
            // 
            MouseCoordTSMI.Name = "MouseCoordTSMI";
            MouseCoordTSMI.Size = new Size(26, 20);
            MouseCoordTSMI.Text = "X";
            // 
            // AlertMessagesTSMI
            // 
            AlertMessagesTSMI.Alignment = ToolStripItemAlignment.Right;
            AlertMessagesTSMI.Name = "AlertMessagesTSMI";
            AlertMessagesTSMI.Size = new Size(12, 20);
            // 
            // CancelPendingActionToolStripMenuItem
            // 
            CancelPendingActionToolStripMenuItem.Name = "CancelPendingActionToolStripMenuItem";
            CancelPendingActionToolStripMenuItem.Size = new Size(55, 20);
            CancelPendingActionToolStripMenuItem.Text = "Cancel";
            CancelPendingActionToolStripMenuItem.Visible = false;
            // 
            // DailyAssignment
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1596, 742);
            Controls.Add(EditMenu);
            Name = "DailyAssignment";
            Text = "DailyAssignment";
            Load += DailyAssignment_Load;
            DragDrop += DailyAssignment_DragDrop;
            DragOver += DailyAssignment_DragOver;
            MouseMove += DailyAssignment_MouseMove;
            EditMenu.ResumeLayout(false);
            EditMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip EditMenu;
        private ToolStripMenuItem EditTSMI;
        private ToolStripMenuItem SetClinicFloorplanTSMI;
        private ToolStripMenuItem DrawNewLocationTSMI;
        private ToolStripMenuItem MouseCoordTSMI;
        private ToolStripMenuItem AlertMessagesTSMI;
        private ToolStripMenuItem CancelPendingActionToolStripMenuItem;
    }
}