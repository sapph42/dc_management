namespace DCManagement.Forms {
    partial class PersonManagement {
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
            EmployeeListbox = new ListBox();
            groupBox1 = new GroupBox();
            label4 = new Label();
            SkillsListbox = new CheckedListBox();
            SaveButton = new Button();
            AvailableCheckbox = new CheckBox();
            ActiveCheckbox = new CheckBox();
            TeamCombobox = new ComboBox();
            label3 = new Label();
            FirstnameTextbox = new TextBox();
            label2 = new Label();
            LastnameTextbox = new TextBox();
            label1 = new Label();
            NewPersonButton = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // EmployeeListbox
            // 
            EmployeeListbox.FormattingEnabled = true;
            EmployeeListbox.ItemHeight = 15;
            EmployeeListbox.Location = new Point(12, 12);
            EmployeeListbox.Name = "EmployeeListbox";
            EmployeeListbox.Size = new Size(148, 304);
            EmployeeListbox.TabIndex = 0;
            EmployeeListbox.SelectedIndexChanged += EmployeeListbox_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(SkillsListbox);
            groupBox1.Controls.Add(SaveButton);
            groupBox1.Controls.Add(AvailableCheckbox);
            groupBox1.Controls.Add(ActiveCheckbox);
            groupBox1.Controls.Add(TeamCombobox);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(FirstnameTextbox);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(LastnameTextbox);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(166, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(528, 343);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Person Data";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(313, 25);
            label4.Name = "label4";
            label4.Size = new Size(33, 15);
            label4.TabIndex = 10;
            label4.Text = "Skills";
            // 
            // SkillsListbox
            // 
            SkillsListbox.FormattingEnabled = true;
            SkillsListbox.Location = new Point(313, 49);
            SkillsListbox.Name = "SkillsListbox";
            SkillsListbox.Size = new Size(193, 256);
            SkillsListbox.TabIndex = 9;
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(220, 315);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(75, 22);
            SaveButton.TabIndex = 8;
            SaveButton.Text = "Save Person";
            SaveButton.UseVisualStyleBackColor = true;
            // 
            // AvailableCheckbox
            // 
            AvailableCheckbox.AutoSize = true;
            AvailableCheckbox.Location = new Point(177, 250);
            AvailableCheckbox.Name = "AvailableCheckbox";
            AvailableCheckbox.Size = new Size(74, 19);
            AvailableCheckbox.TabIndex = 7;
            AvailableCheckbox.Text = "Available";
            AvailableCheckbox.UseVisualStyleBackColor = true;
            // 
            // ActiveCheckbox
            // 
            ActiveCheckbox.AutoSize = true;
            ActiveCheckbox.Location = new Point(11, 250);
            ActiveCheckbox.Name = "ActiveCheckbox";
            ActiveCheckbox.Size = new Size(59, 19);
            ActiveCheckbox.TabIndex = 6;
            ActiveCheckbox.Text = "Active";
            ActiveCheckbox.UseVisualStyleBackColor = true;
            // 
            // TeamCombobox
            // 
            TeamCombobox.FormattingEnabled = true;
            TeamCombobox.Location = new Point(98, 174);
            TeamCombobox.Name = "TeamCombobox";
            TeamCombobox.Size = new Size(153, 23);
            TeamCombobox.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 177);
            label3.Name = "label3";
            label3.Size = new Size(86, 15);
            label3.TabIndex = 4;
            label3.Text = "Assigned Team";
            // 
            // FirstnameTextbox
            // 
            FirstnameTextbox.Location = new Point(98, 98);
            FirstnameTextbox.Name = "FirstnameTextbox";
            FirstnameTextbox.Size = new Size(153, 23);
            FirstnameTextbox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 101);
            label2.Name = "label2";
            label2.Size = new Size(64, 15);
            label2.TabIndex = 2;
            label2.Text = "First Name";
            // 
            // LastnameTextbox
            // 
            LastnameTextbox.Location = new Point(98, 22);
            LastnameTextbox.Name = "LastnameTextbox";
            LastnameTextbox.Size = new Size(153, 23);
            LastnameTextbox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 30);
            label1.Name = "label1";
            label1.Size = new Size(63, 15);
            label1.TabIndex = 0;
            label1.Text = "Last Name";
            // 
            // NewPersonButton
            // 
            NewPersonButton.Location = new Point(12, 332);
            NewPersonButton.Name = "NewPersonButton";
            NewPersonButton.Size = new Size(148, 23);
            NewPersonButton.TabIndex = 2;
            NewPersonButton.Text = "New Person";
            NewPersonButton.UseVisualStyleBackColor = true;
            NewPersonButton.Click += NewPersonButton_Click;
            // 
            // PersonManagement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(706, 367);
            Controls.Add(NewPersonButton);
            Controls.Add(groupBox1);
            Controls.Add(EmployeeListbox);
            Name = "PersonManagement";
            Text = "Person Management";
            Load += PersonManagement_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ListBox EmployeeListbox;
        private GroupBox groupBox1;
        private Button NewPersonButton;
        private TextBox LastnameTextbox;
        private Label label1;
        private TextBox FirstnameTextbox;
        private Label label2;
        private Label label4;
        private CheckedListBox SkillsListbox;
        private Button SaveButton;
        private CheckBox AvailableCheckbox;
        private CheckBox ActiveCheckbox;
        private ComboBox TeamCombobox;
        private Label label3;
    }
}