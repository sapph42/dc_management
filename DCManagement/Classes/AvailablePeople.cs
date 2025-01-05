using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
internal class AvailablePeople {
    public List<Person> People { get; set; } = [];
    public AvailablePeople() { }
    /// <summary>
    /// Get the number of available people with a given skill.
    /// </summary>
    /// <param name="SkillTypeID">The SkillTypeID to check against</param>
    /// <returns>The count of people that have the specified SkillTypeID</returns>
    public int AvailbleSkills(int SkillTypeID) {
        return People.Count(p => p.HasSkill(SkillTypeID));
    }
    /// <summary>
    /// Retreive a random person with the given SkillTypeID, and remove them from People.
    /// </summary>
    /// <param name="SkillTypeID"></param>
    /// <returns>The selected Person</returns>
    public Person GetPerson(int SkillTypeID) {
        Person person = People.Random();
        People.Remove(person);
        return person;
    }
}
