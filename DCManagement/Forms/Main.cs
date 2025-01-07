using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCManagement.Forms {
    public partial class Main : Form {
        public Main() {
            InitializeComponent();
        }

        private void LocationButton_Click(object sender, EventArgs e) {
            LocationManagement locationManagement = new();
            locationManagement.Show();
        }

        private void PersonButton_Click(object sender, EventArgs e) {
            PersonManagement personManagement = new();
            personManagement.Show();
        }

        private void TeamButton_Click(object sender, EventArgs e) {
            TeamManagement teamManagement = new();
            teamManagement.Show();
        }

        private void DailyButton_Click(object sender, EventArgs e) {
            DailyAssignment dailyAssignment = new();
            dailyAssignment.Show();
        }

        private void button1_Click(object sender, EventArgs e) {
            Test test = new();
            test.Show();
        }
    }
}
