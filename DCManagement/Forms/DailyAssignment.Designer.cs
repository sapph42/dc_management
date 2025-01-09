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
            components = new System.ComponentModel.Container();
            EditMenu = new MenuStrip();
            EditTSMI = new ToolStripMenuItem();
            RefreshToolStripMenuItem = new ToolStripMenuItem();
            MouseCoordTSMI = new ToolStripMenuItem();
            AlertMessagesTSMI = new ToolStripMenuItem();
            CancelPendingActionToolStripMenuItem = new ToolStripMenuItem();
            LabelContextMenu = new ContextMenuStrip(components);
            ToggleAvailabilityToolStripMenuItem = new ToolStripMenuItem();
            EditMenu.SuspendLayout();
            LabelContextMenu.SuspendLayout();
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
            EditTSMI.DropDownItems.AddRange(new ToolStripItem[] { RefreshToolStripMenuItem });
            EditTSMI.Name = "EditTSMI";
            EditTSMI.Size = new Size(39, 20);
            EditTSMI.Text = "Edit";
            // 
            // RefreshToolStripMenuItem
            // 
            RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem";
            RefreshToolStripMenuItem.Size = new Size(113, 22);
            RefreshToolStripMenuItem.Text = "Refresh";
            RefreshToolStripMenuItem.Click += RefreshToolStripMenuItem_Click;
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
            // LabelContextMenu
            // 
            LabelContextMenu.Items.AddRange(new ToolStripItem[] { ToggleAvailabilityToolStripMenuItem });
            LabelContextMenu.Name = "LabelContextMenu";
            LabelContextMenu.Size = new Size(181, 48);
            // 
            // ToggleAvailabilityToolStripMenuItem
            // 
            ToggleAvailabilityToolStripMenuItem.Name = "ToggleAvailabilityToolStripMenuItem";
            ToggleAvailabilityToolStripMenuItem.Size = new Size(180, 22);
            ToggleAvailabilityToolStripMenuItem.Text = "Toggle Availability";
            ToggleAvailabilityToolStripMenuItem.Click += ToggleAvailabilityToolStripMenuItem_Click;
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
            LabelContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip EditMenu;
        private ToolStripMenuItem EditTSMI;
        private ToolStripMenuItem MouseCoordTSMI;
        private ToolStripMenuItem AlertMessagesTSMI;
        private ToolStripMenuItem CancelPendingActionToolStripMenuItem;
        private ToolStripMenuItem RefreshToolStripMenuItem;
        private ContextMenuStrip LabelContextMenu;
        private ToolStripMenuItem ToggleAvailabilityToolStripMenuItem;
    }
}