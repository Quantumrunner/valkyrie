using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content.ContentData
{
    // Class for Class specific data
    public class SkillData : GenericData
    {
        public static new string type = "Skill";
        public int xp = 0;

        public SkillData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            // Get archetype
            if (content.ContainsKey("xp"))
            {
                int.TryParse(content["xp"], out xp);
            }
        }
    }
}
