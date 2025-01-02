using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes;
internal class DailyTeam : Team {
    public List<DailySlot> Members { get; set; } = [];
}
internal class DailySlot : SlotType {
    public Person? AssignedMember { get; set; }
    public bool ShouldFill = true;
    public bool MustFill = false;
}
internal class Assignment {
    public LocationCollection Locations { get; set; } = [];
    public List<DailyTeam> Teams { get; set; } = [];
    public PersonCollection People { get; set; } = [];
    public Assignment() { }
}
