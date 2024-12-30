namespace DCManagement.Forms {
    partial class LocationManagement {
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
            SetClinicFloorplanTSMI = new ToolStripMenuItem();
            DrawNewLocationTSMI = new ToolStripMenuItem();
            MouseCoordTSMI = new ToolStripMenuItem();
            TeamTooltip = new ToolTip(components);
            ContextMenu = new ContextMenuStrip(components);
            RenameLocationToolStripMenuItem = new ToolStripMenuItem();
            DeleteLocationToolStripMenuItem = new ToolStripMenuItem();
            MoveLocationToolStripMenuItem = new ToolStripMenuItem();
            EditMenu.SuspendLayout();
            ContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // EditMenu
            // 
            EditMenu.Items.AddRange(new ToolStripItem[] { EditTSMI, MouseCoordTSMI });
            EditMenu.Location = new Point(0, 0);
            EditMenu.Name = "EditMenu";
            EditMenu.Size = new Size(1109, 24);
            EditMenu.TabIndex = 0;
            EditMenu.Text = "Edit";
            // 
            // EditTSMI
            // 
            EditTSMI.DropDownItems.AddRange(new ToolStripItem[] { SetClinicFloorplanTSMI, DrawNewLocationTSMI });
            EditTSMI.Name = "EditTSMI";
            EditTSMI.Size = new Size(39, 20);
            EditTSMI.Text = "Edit";
            // 
            // SetClinicFloorplanTSMI
            // 
            SetClinicFloorplanTSMI.Name = "SetClinicFloorplanTSMI";
            SetClinicFloorplanTSMI.Size = new Size(177, 22);
            SetClinicFloorplanTSMI.Text = "Set Clinic Floorplan";
            SetClinicFloorplanTSMI.Click += SetClinicFloorplanToolStripMenuItem_Click;
            // 
            // DrawNewLocationTSMI
            // 
            DrawNewLocationTSMI.Name = "DrawNewLocationTSMI";
            DrawNewLocationTSMI.Size = new Size(177, 22);
            DrawNewLocationTSMI.Text = "Draw New Location";
            DrawNewLocationTSMI.Click += DrawNewLocationToolStripMenuItem_Click;
            // 
            // MouseCoordTSMI
            // 
            MouseCoordTSMI.Name = "MouseCoordTSMI";
            MouseCoordTSMI.Size = new Size(26, 20);
            MouseCoordTSMI.Text = "X";
            // 
            // TeamTooltip
            // 
            TeamTooltip.Active = false;
            // 
            // ContextMenu
            // 
            ContextMenu.Items.AddRange(new ToolStripItem[] { RenameLocationToolStripMenuItem, DeleteLocationToolStripMenuItem, MoveLocationToolStripMenuItem });
            ContextMenu.Name = "ContextMenu";
            ContextMenu.Size = new Size(181, 92);
            ContextMenu.Opening += ContextMenu_Opening;
            // 
            // RenameLocationToolStripMenuItem
            // 
            RenameLocationToolStripMenuItem.Name = "RenameLocationToolStripMenuItem";
            RenameLocationToolStripMenuItem.Size = new Size(180, 22);
            RenameLocationToolStripMenuItem.Text = "Rename Location";
            RenameLocationToolStripMenuItem.Click += RenameLocationToolStripMenuItem_Click;
            // 
            // DeleteLocationToolStripMenuItem
            // 
            DeleteLocationToolStripMenuItem.Name = "DeleteLocationToolStripMenuItem";
            DeleteLocationToolStripMenuItem.Size = new Size(180, 22);
            DeleteLocationToolStripMenuItem.Text = "Delete Location";
            DeleteLocationToolStripMenuItem.Click += DeleteLocationToolStripMenuItem_Click;
            // 
            // MoveLocationToolStripMenuItem
            // 
            MoveLocationToolStripMenuItem.Name = "MoveLocationToolStripMenuItem";
            MoveLocationToolStripMenuItem.Size = new Size(180, 22);
            MoveLocationToolStripMenuItem.Text = "Move Location";
            MoveLocationToolStripMenuItem.Click += MoveLocationToolStripMenuItem_Click;
            // 
            // LocationManagement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1109, 620);
            ContextMenuStrip = ContextMenu;
            Controls.Add(EditMenu);
            MainMenuStrip = EditMenu;
            Name = "LocationManagement";
            Text = "LocationManagement";
            FormClosing += LocationManagement_FormClosing;
            Load += LocationManagement_Load;
            Click += LocationManagement_Click;
            MouseDown += LocationManagement_MouseDown;
            MouseMove += LocationManagement_MouseMove;
            MouseUp += LocationManagement_MouseUp;
            EditMenu.ResumeLayout(false);
            EditMenu.PerformLayout();
            ContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip EditMenu;
        private ToolStripMenuItem EditTSMI;
        private ToolStripMenuItem SetClinicFloorplanTSMI;
        private ToolStripMenuItem DrawNewLocationTSMI;
        private ToolStripMenuItem MouseCoordTSMI;
        private ToolTip TeamTooltip;
        private ContextMenuStrip ContextMenu;
        private ToolStripMenuItem RenameLocationToolStripMenuItem;
        private ToolStripMenuItem DeleteLocationToolStripMenuItem;
        private ToolStripMenuItem MoveLocationToolStripMenuItem;
    }
}