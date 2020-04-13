using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // MPlaces are used to position individual monsters
    public class MPlace : QuestComponent
    {
        public bool master = false;
        new public static string type = "MPlace";
        public bool rotate = false;

        // Create a new mplace with name (editor)
        public MPlace(string s) : base(s)
        {
            source = "mplaces.ini";
            locationSpecified = true;
            typeDynamic = type;
        }

        // Load mplace from ini data
        public MPlace(string name, Dictionary<string, string> data, string path) : base(name, data, path)
        {
            // Must have a location
            locationSpecified = true;
            typeDynamic = type;
            master = false;
            if (data.ContainsKey("master"))
            {
                bool.TryParse(data["master"], out master);
            }
            if (data.ContainsKey("rotate"))
            {
                bool.TryParse(data["rotate"], out rotate);
            }
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();
            if (master)
            {
                r += "master=true" + nl;
            }
            if (rotate)
            {
                r += "rotate=true" + nl;
            }
            return r;
        }
    }
}
