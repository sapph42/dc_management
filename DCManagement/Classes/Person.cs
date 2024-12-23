using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
internal class Person {
    public int? PersonID { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public Team? Team { get; set; }
    public SkillFlag SkillFlag { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public Person() { }
}
