using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
internal class Team {
    public int? TeamID { get; set; }
    public Person? TeamLead { get; set; }
    public Location? PrimaryLocation { get; set; }
    public bool FillIfNoLead { get; set; } = true;
    public bool Active { get; set; } = true;
    public string TeamName { get; set; }
    public Location? CurrentAssignment { get; set; }
    public Team() {
        TeamName = string.Empty;
    }
    public Team (string TeamName) {
        this.TeamName = TeamName;
    }
}
