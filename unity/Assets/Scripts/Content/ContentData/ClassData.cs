using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content.ContentData
{
    // Class for Class specific data
    public class ClassData : GenericData
    {
        public string archetype = "warrior";
        public string hybridArchetype = "";
        public static new string type = "Class";
        public List<string> items;

        public ClassData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            // Get archetype
            if (content.ContainsKey("archetype"))
            {
                archetype = content["archetype"];
            }

            // Get hybridArchetype
            if (content.ContainsKey("hybridarchetype"))
            {
                hybridArchetype = content["hybridarchetype"];
            }

            // Get starting item
            items = new List<string>();
            if (content.ContainsKey("items"))
            {
                items.AddRange(content["items"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
