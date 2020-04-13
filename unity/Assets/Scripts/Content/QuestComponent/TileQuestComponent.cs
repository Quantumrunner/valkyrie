using System.Collections.Generic;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Content.QuestComponent
{
    // Class for TileQuestComponent components (use TileSide content data)
    public class TileQuestComponent : QuestComponent
    {
        new public static string type = "TileQuestComponent";
        public int rotation = 0;
        public string tileSideName;

        // Create new with name (used by editor)
        public TileQuestComponent(string s) : base(s)
        {
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            source = "tiles.ini";
            foreach (KeyValuePair<string, TileSideData> kv in game.cd.tileSides)
            {
                tileSideName = kv.Key;
                break;
            }
        }

        // Create tileQuestComponent from ini data
        public TileQuestComponent(string name, Dictionary<string, string> data, string path) : base(name, data, path)
        {
            // Tiles must have a location
            locationSpecified = true;
            typeDynamic = type;
            // Get rotation if specified
            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }

            // Find the tileside that is used
            if (data.ContainsKey("side"))
            {
                tileSideName = data["side"];
            }
            else
            {
                // Fatal if missing
                ValkyrieDebug.Log("Error: No TileSide specified in Quest component: " + name);
                Application.Quit();
            }
        }

        // Save to ini string (used by editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "side=" + tileSideName + nl;
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }
}
