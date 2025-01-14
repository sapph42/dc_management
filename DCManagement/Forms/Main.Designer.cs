namespace DCManagement.Forms {
    partial class Main {
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
            LocationButton = new Button();
            PersonButton = new Button();
            TeamButton = new Button();
            DailyButton = new Button();
            BackupButton = new Button();
            ReadOnlyButton = new Button();
            SuspendLayout();
            // 
            // LocationButton
            // 
            LocationButton.Location = new Point(32, 34);
            LocationButton.Name = "LocationButton";
            LocationButton.Size = new Size(104, 47);
            LocationButton.TabIndex = 0;
            LocationButton.Text = "Location Management";
            LocationButton.UseVisualStyleBackColor = true;
            LocationButton.Visible = false;
            LocationButton.Click += LocationButton_Click;
            // 
            // PersonButton
            // 
            PersonButton.Location = new Point(196, 34);
            PersonButton.Name = "PersonButton";
            PersonButton.Size = new Size(104, 47);
            PersonButton.TabIndex = 1;
            PersonButton.Text = "Person Management";
            PersonButton.UseVisualStyleBackColor = true;
            PersonButton.Visible = false;
            PersonButton.Click += PersonButton_Click;
            // 
            // TeamButton
            // 
            TeamButton.Location = new Point(32, 118);
            TeamButton.Name = "TeamButton";
            TeamButton.Size = new Size(104, 47);
            TeamButton.TabIndex = 2;
            TeamButton.Text = "Team Management";
            TeamButton.UseVisualStyleBackColor = true;
            TeamButton.Visible = false;
            TeamButton.Click += TeamButton_Click;
            // 
            // DailyButton
            // 
            DailyButton.Location = new Point(196, 118);
            DailyButton.Name = "DailyButton";
            DailyButton.Size = new Size(104, 47);
            DailyButton.TabIndex = 3;
            DailyButton.Text = "Daily Assignments";
            DailyButton.UseVisualStyleBackColor = true;
            DailyButton.Visible = false;
            DailyButton.Click += DailyButton_Click;
            // 
            // BackupButton
            // 
            BackupButton.Location = new Point(32, 216);
            BackupButton.Name = "BackupButton";
            BackupButton.Size = new Size(104, 47);
            BackupButton.TabIndex = 5;
            BackupButton.Text = "Backup Database";
            BackupButton.UseVisualStyleBackColor = true;
            BackupButton.Visible = false;
            BackupButton.Click += BackupButton_Click;
            // 
            // ReadOnlyButton
            // 
            ReadOnlyButton.Location = new Point(196, 216);
            ReadOnlyButton.Name = "ReadOnlyButton";
            ReadOnlyButton.Size = new Size(104, 47);
            ReadOnlyButton.TabIndex = 6;
            ReadOnlyButton.Text = "View Today's Assignments";
            ReadOnlyButton.UseVisualStyleBackColor = true;
            ReadOnlyButton.Visible = false;
            ReadOnlyButton.Click += ReadOnlyButton_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(346, 308);
            Controls.Add(ReadOnlyButton);
            Controls.Add(BackupButton);
            Controls.Add(DailyButton);
            Controls.Add(TeamButton);
            Controls.Add(PersonButton);
            Controls.Add(LocationButton);
            Name = "Main";
            Text = "Main";
            ResumeLayout(false);
        }

        #endregion

        private Button LocationButton;
        private Button PersonButton;
        private Button TeamButton;
        private Button DailyButton;
        private Button BackupButton;
        private Button ReadOnlyButton;
    }
}