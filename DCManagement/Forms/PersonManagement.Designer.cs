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
            NewPersonButton = new Button();
            SuspendLayout();
            // 
            // EmployeeListbox
            // 
            EmployeeListbox.FormattingEnabled = true;
            EmployeeListbox.ItemHeight = 15;
            EmployeeListbox.Location = new Point(12, 12);
            EmployeeListbox.Name = "EmployeeListbox";
            EmployeeListbox.Size = new Size(148, 394);
            EmployeeListbox.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Location = new Point(166, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(622, 424);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Person Data";
            // 
            // NewPersonButton
            // 
            NewPersonButton.Location = new Point(12, 413);
            NewPersonButton.Name = "NewPersonButton";
            NewPersonButton.Size = new Size(148, 23);
            NewPersonButton.TabIndex = 2;
            NewPersonButton.Text = "New Person";
            NewPersonButton.UseVisualStyleBackColor = true;
            // 
            // PersonManagement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(NewPersonButton);
            Controls.Add(groupBox1);
            Controls.Add(EmployeeListbox);
            Name = "PersonManagement";
            Text = "Person Management";
            ResumeLayout(false);
        }

        #endregion

        private ListBox EmployeeListbox;
        private GroupBox groupBox1;
        private Button NewPersonButton;
    }
}