using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Investigator Evades
    public class EvadeData : GenericData
    {
        public static new string type = "Evade";

        // Evade text
        public StringKey text = StringKey.NULL;
        public string monster = "";

        public EvadeData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            // Get attack text
            if (content.ContainsKey("text"))
            {
                text = new StringKey(content["text"]);
            }

            // Get attack target
            if (content.ContainsKey("monster"))
            {
                monster = content["monster"];
            }
        }
    }
}
