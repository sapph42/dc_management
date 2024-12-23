using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
internal class SlotType {
    public int? SlotTypeID { get; set; }
    public string? Description { get; set; }
    public SkillFlag SkillFlag { get; set;}
    public Color SlotColor { get; set; }
    public SlotType() { }
    public void SetSlotColor (string ColorHex) {
        SlotColor = ColorTranslator.FromHtml (ColorHex);
    }
}
