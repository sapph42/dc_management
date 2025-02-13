﻿namespace DCManagement.Forms {
    partial class TeamManagement {
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
            label1 = new Label();
            TeamListbox = new ListBox();
            NewTeamButton = new Button();
            groupBox1 = new GroupBox();
            SlotButton = new Button();
            SaveButton = new Button();
            LocationCombobox = new ComboBox();
            LeadCombobox = new ComboBox();
            ActiveCheckbox = new CheckBox();
            FillCheckbox = new CheckBox();
            label4 = new Label();
            label3 = new Label();
            TeamNameTextbox = new TextBox();
            label2 = new Label();
            ClinicalCheckbox = new CheckBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 0;
            label1.Text = "Teams";
            // 
            // TeamListbox
            // 
            TeamListbox.FormattingEnabled = true;
            TeamListbox.ItemHeight = 15;
            TeamListbox.Location = new Point(12, 27);
            TeamListbox.Name = "TeamListbox";
            TeamListbox.Size = new Size(190, 184);
            TeamListbox.TabIndex = 1;
            TeamListbox.SelectedIndexChanged += TeamListbox_SelectedIndexChanged;
            // 
            // NewTeamButton
            // 
            NewTeamButton.Location = new Point(12, 217);
            NewTeamButton.Name = "NewTeamButton";
            NewTeamButton.Size = new Size(190, 23);
            NewTeamButton.TabIndex = 2;
            NewTeamButton.Text = "New Team";
            NewTeamButton.UseVisualStyleBackColor = true;
            NewTeamButton.Click += NewTeamButton_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ClinicalCheckbox);
            groupBox1.Controls.Add(SlotButton);
            groupBox1.Controls.Add(SaveButton);
            groupBox1.Controls.Add(LocationCombobox);
            groupBox1.Controls.Add(LeadCombobox);
            groupBox1.Controls.Add(ActiveCheckbox);
            groupBox1.Controls.Add(FillCheckbox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(TeamNameTextbox);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(219, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(495, 228);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Team Data";
            // 
            // SlotButton
            // 
            SlotButton.Location = new Point(301, 160);
            SlotButton.Name = "SlotButton";
            SlotButton.Size = new Size(115, 23);
            SlotButton.TabIndex = 15;
            SlotButton.Text = "Set Team Slots";
            SlotButton.UseVisualStyleBackColor = true;
            SlotButton.Click += SlotButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(236, 199);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(75, 23);
            SaveButton.TabIndex = 14;
            SaveButton.Text = "Save Team";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // LocationCombobox
            // 
            LocationCombobox.FormattingEnabled = true;
            LocationCombobox.Location = new Point(109, 160);
            LocationCombobox.Name = "LocationCombobox";
            LocationCombobox.Size = new Size(166, 23);
            LocationCombobox.TabIndex = 13;
            // 
            // LeadCombobox
            // 
            LeadCombobox.FormattingEnabled = true;
            LeadCombobox.Location = new Point(109, 94);
            LeadCombobox.Name = "LeadCombobox";
            LeadCombobox.Size = new Size(166, 23);
            LeadCombobox.TabIndex = 12;
            // 
            // ActiveCheckbox
            // 
            ActiveCheckbox.AutoSize = true;
            ActiveCheckbox.Location = new Point(301, 78);
            ActiveCheckbox.Name = "ActiveCheckbox";
            ActiveCheckbox.Size = new Size(59, 19);
            ActiveCheckbox.TabIndex = 11;
            ActiveCheckbox.Text = "Active";
            ActiveCheckbox.UseVisualStyleBackColor = true;
            // 
            // FillCheckbox
            // 
            FillCheckbox.AutoSize = true;
            FillCheckbox.Location = new Point(301, 22);
            FillCheckbox.Name = "FillCheckbox";
            FillCheckbox.Size = new Size(172, 19);
            FillCheckbox.TabIndex = 10;
            FillCheckbox.Text = "Fill If No Lead/Lead Missing";
            FillCheckbox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 163);
            label4.Name = "label4";
            label4.Size = new Size(97, 15);
            label4.TabIndex = 4;
            label4.Text = "Primary Location";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 94);
            label3.Name = "label3";
            label3.Size = new Size(63, 15);
            label3.TabIndex = 2;
            label3.Text = "Team Lead";
            // 
            // TeamNameTextbox
            // 
            TeamNameTextbox.Location = new Point(109, 23);
            TeamNameTextbox.Name = "TeamNameTextbox";
            TeamNameTextbox.Size = new Size(166, 23);
            TeamNameTextbox.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 26);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 0;
            label2.Text = "Team Name";
            // 
            // ClinicalCheckbox
            // 
            ClinicalCheckbox.AutoSize = true;
            ClinicalCheckbox.Location = new Point(301, 134);
            ClinicalCheckbox.Name = "ClinicalCheckbox";
            ClinicalCheckbox.Size = new Size(65, 19);
            ClinicalCheckbox.TabIndex = 16;
            ClinicalCheckbox.Text = "Clinical";
            ClinicalCheckbox.UseVisualStyleBackColor = true;
            // 
            // TeamManagement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(726, 252);
            Controls.Add(groupBox1);
            Controls.Add(NewTeamButton);
            Controls.Add(TeamListbox);
            Controls.Add(label1);
            Name = "TeamManagement";
            Text = "TeamManagement";
            Load += TeamManagement_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ListBox TeamListbox;
        private Button NewTeamButton;
        private GroupBox groupBox1;
        private TextBox TeamNameTextbox;
        private Label label2;
        private Label label4;
        private Label label3;
        private ComboBox LocationCombobox;
        private ComboBox LeadCombobox;
        private CheckBox ActiveCheckbox;
        private CheckBox FillCheckbox;
        private Button SaveButton;
        private Button SlotButton;
        private CheckBox ClinicalCheckbox;
    }
}