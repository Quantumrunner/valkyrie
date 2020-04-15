using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content.ContentData
{
    // Class for Investigator Attacks
    public class AttackData : GenericData
    {
        public static new string type = "Attack";

        // Attack text
        public StringKey text = StringKey.NULL;

        // Target type (human, spirit...)
        public string target = "";

        // Attack type (heavy, unarmed)
        public string attackType = "";

        public AttackData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
            // Get attack text
            if (content.ContainsKey("text"))
            {
                text = new StringKey(content["text"]);
            }

            // Get attack target
            if (content.ContainsKey("target"))
            {
                target = content["target"];
            }

            // Get attack type
            if (content.ContainsKey("attacktype"))
            {
                attackType = content["attacktype"];
            }
        }
    }
}
