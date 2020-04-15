using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Horror Checks
    public class HorrorData : GenericData
    {
        public static new string type = "Horror";

        // Evade text
        public StringKey text = StringKey.NULL;
        public string monster = "";

        public HorrorData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
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
