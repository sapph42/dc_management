using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes;
[Flags]
internal enum SkillFlag : uint {
    None = 0,
    Dentist = 1,
    DentalAssistant = 2,
    EFDA = 4,
    RDH = 8,
    Sterilization = 16,
    MSA = 32,
    NCO = 64,
    PracticeManager = 128,
    NCOIC = 256,
    OIC = 512,
    PurchaseCoord = 1024,
    Supply = 2048,
    IT = 4096
}
