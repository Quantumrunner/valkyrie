using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Content.QuestComponents
{
    // Doors are like tokens but placed differently and have different defaults
    public class DoorQuestComponent : EventQuestComponent
    {
        new public static string type = "Door";
        public int rotation = 0;
        public GameObject gameObject;
        public string colourName = "white";

        // Create new with name (used by editor)
        public DoorQuestComponent(string s) : base(s)
        {
            source = "door.ini";
            locationSpecified = true;
            typeDynamic = type;
            cancelable = true;
        }

        // Create from ini data
        public DoorQuestComponent(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, QuestIniComponent.currentFormat)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Doors are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }

            // color is only supported as a hexadecimal "#RRGGBB" format
            if (data.ContainsKey("color"))
            {
                colourName = data["color"];
            }
        }

        // Save to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!colourName.Equals("white"))
            {
                r += "color=" + colourName + nl;
            }
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }
}
