using DCManagement.Classes;
using DCManagement.Forms;
using DCManagement.Resources;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace DCManagement;

internal static class Program {

    public static DataSource Source;
    static bool IsHostnameAvailable(string hostname) {
        // Step 1: DNS Resolution
        try {
            Dns.GetHostEntry(hostname);
        } catch (SocketException) {
            return false; // Hostname does not resolve
        }

        // Step 2: Ping Test
        try {
            using Ping ping = new();
            PingReply reply = ping.Send(hostname, 1000); // 1-second timeout
            if (reply.Status != IPStatus.Success) {
                return false; // Ping failed
            }
        } catch (PingException) {
            return false; // Ping failed
        }

        // Step 3: Port Check (Optional)
        try {
            using TcpClient client = new();
            client.Connect(hostname, 1433); // Try port 80 (HTTP)
            return true; // Successfully connected
        } catch (SocketException) {
            return false; // Port is not available
        }
    }

    [STAThread]
    static void Main() {
        if (!string.IsNullOrWhiteSpace(GlobalResources.Server) || IsHostnameAvailable(GlobalResources.Server))
            Source = DataSource.SQL;
        else
            Source = DataSource.SQLite;
        ApplicationConfiguration.Initialize();
        Application.Run(new Main());
    }
}