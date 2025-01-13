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
            button1 = new Button();
            BackupButton = new Button();
            SuspendLayout();
            // 
            // LocationButton
            // 
            LocationButton.Location = new Point(41, 53);
            LocationButton.Name = "LocationButton";
            LocationButton.Size = new Size(75, 23);
            LocationButton.TabIndex = 0;
            LocationButton.Text = "Location";
            LocationButton.UseVisualStyleBackColor = true;
            LocationButton.Click += LocationButton_Click;
            // 
            // PersonButton
            // 
            PersonButton.Location = new Point(167, 63);
            PersonButton.Name = "PersonButton";
            PersonButton.Size = new Size(75, 23);
            PersonButton.TabIndex = 1;
            PersonButton.Text = "Person";
            PersonButton.UseVisualStyleBackColor = true;
            PersonButton.Click += PersonButton_Click;
            // 
            // TeamButton
            // 
            TeamButton.Location = new Point(306, 68);
            TeamButton.Name = "TeamButton";
            TeamButton.Size = new Size(75, 23);
            TeamButton.TabIndex = 2;
            TeamButton.Text = "Team";
            TeamButton.UseVisualStyleBackColor = true;
            TeamButton.Click += TeamButton_Click;
            // 
            // DailyButton
            // 
            DailyButton.Location = new Point(448, 76);
            DailyButton.Name = "DailyButton";
            DailyButton.Size = new Size(75, 23);
            DailyButton.TabIndex = 3;
            DailyButton.Text = "Daily";
            DailyButton.UseVisualStyleBackColor = true;
            DailyButton.Click += DailyButton_Click;
            // 
            // button1
            // 
            button1.Location = new Point(356, 228);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Test";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // BackupButton
            // 
            BackupButton.Location = new Point(524, 194);
            BackupButton.Name = "BackupButton";
            BackupButton.Size = new Size(75, 23);
            BackupButton.TabIndex = 5;
            BackupButton.Text = "Backup Database";
            BackupButton.UseVisualStyleBackColor = true;
            BackupButton.Click += BackupButton_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(BackupButton);
            Controls.Add(button1);
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
        private Button button1;
        private Button BackupButton;
    }
}