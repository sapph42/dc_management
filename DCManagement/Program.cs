using DCManagement.Forms;
using DCManagement.Resources;
using Microsoft.Data.SqlClient;

namespace DCManagement;

internal static class Program {

    public static string SqlConnectionString = "";

    [STAThread]
    static void Main() {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        SqlConnectionStringBuilder csb = new() {
            DataSource = GlobalResources.Server,
            InitialCatalog = GlobalResources.Database,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 30,
            CommandTimeout = 5
        };
        SqlConnectionString = csb.ConnectionString;

        ApplicationConfiguration.Initialize();
        Application.Run(new TeamManagement());
    }
}