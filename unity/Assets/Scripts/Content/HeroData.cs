using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Hero specific data
    public class HeroData : GenericData
    {
        public string archetype = "warrior";
        public static new string type = "Hero";
        public string item = "";

        public HeroData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            // Get archetype
            if (content.ContainsKey("archetype"))
            {
                archetype = content["archetype"];
            }

            // Get starting item
            if (content.ContainsKey("item"))
            {
                item = content["item"];
            }
        }
    }
}
