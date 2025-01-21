using DCManagement.Classes;
using DCManagement.Resources;
using System.DirectoryServices.AccountManagement;

namespace DCManagement.Forms;
public partial class Main : Form {
    public Main() {
        InitializeComponent();
        using PrincipalContext context = new(ContextType.Domain, "MHS");
        UserPrincipal user = UserPrincipal.Current;
        bool IsUser = true;
        var clinicGroup = GroupPrincipal.FindByIdentity(context, GlobalResources.ClinicGroup);
        var personnelGroup = GroupPrincipal.FindByIdentity(context, GlobalResources.PersonnelGroup);
        var assignmentGroup = GroupPrincipal.FindByIdentity(context, GlobalResources.AssignmentGroup);
        var readonlyGroup = GroupPrincipal.FindByIdentity(context, GlobalResources.ReadOnlyGroup);
        if (clinicGroup.GetMembers(true).Any(member => member.Sid.Equals(user.Sid)))
            LocationButton.Visible = true;
        if (personnelGroup.GetMembers(true).Any(member => member.Sid.Equals(user.Sid))) {
            PersonButton.Visible = true;
            TeamButton.Visible = true;
        }
        if (assignmentGroup.GetMembers(true).Any(member => member.Sid.Equals(user.Sid))) {
            DailyButton.Visible = true;
            BackupButton.Visible = true;
            IsUser = false;
        }
        if (readonlyGroup.GetMembers(true).Any(member => member.Sid.Equals(user.Sid))) {
            ReadOnlyButton.Visible = true;
            if (IsUser)
                ReadOnlyButton.Location = new Point(118, 118);
        }
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
        Cursor.Current = Cursors.WaitCursor;
        DailyAssignment dailyAssignment = new() {
            IsReadOnly = false
        };
        dailyAssignment.Show();
    }

    private void BackupButton_Click(object sender, EventArgs e) {
        DataManagement data = new(Program.Source);
        data.BackupSqlToSqlite();
    }

    private void ReadOnlyButton_Click(object sender, EventArgs e) {
        Cursor.Current = Cursors.WaitCursor;
        DailyAssignment dailyAssignment = new() {
            IsReadOnly = true
        };
        dailyAssignment.Show();
    }
}
