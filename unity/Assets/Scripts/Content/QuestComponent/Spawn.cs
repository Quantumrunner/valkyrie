using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // Spawn items are monster group placement events
    public class Spawn : Event
    {
        new public static string type = "Spawn";
        // Array of placements by hero count
        public string[][] placement;
        public bool unique = false;
        public float uniqueHealthBase = 0;
        public float uniqueHealthHero = 0;
        public string[] mTypes;
        public string[] mTraitsRequired;
        public string[] mTraitsPool;

        public string uniquetitle_key { get { return genKey("uniquetitle"); } }
        public string uniquetext_key { get { return genKey("uniquetext"); } }

        public StringKey uniqueTitle { get { return genQuery("uniquetitle"); } }
        public StringKey uniqueText { get { return genQuery("uniquetext"); } }

        // Create new with name (used by editor)
        public Spawn(string s) : base(s)
        {
            source = "spawns.ini";
            // Location defaults to specified
            locationSpecified = true;
            typeDynamic = type;
            Game game = Game.Get();
            mTypes = new string[1];
            // This gets the first type available, because we need something
            foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
            {
                mTypes[0] = kv.Key;
                break;
            }
            mTraitsRequired = new string[0];
            mTraitsPool = new string[0];

            // Initialise array
            placement = new string[game.gameType.MaxHeroes() + 1][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
            }
        }

        // Create from ini data
        public Spawn(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
        {
            typeDynamic = type;
            // First try to a list of specific types
            if (data.ContainsKey("monster"))
            {
                mTypes = data["monster"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                mTypes = new string[0];
            }

            // A list of traits to match
            mTraitsRequired = new string[0];
            if (data.ContainsKey("traits"))
            {
                mTraitsRequired = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            // A list of traits to match
            mTraitsPool = new string[0];
            if (data.ContainsKey("traitpool"))
            {
                mTraitsPool = data["traitpool"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            // Array of placements by hero count
            placement = new string[game.gameType.MaxHeroes() + 1][];
            for (int i = 0; i < placement.Length; i++)
            {
                placement[i] = new string[0];
                if (data.ContainsKey("placement" + i))
                {
                    placement[i] = data["placement" + i].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                }
            }

            if (data.ContainsKey("unique"))
            {
                bool.TryParse(data["unique"], out unique);
            }
            if (data.ContainsKey("uniquehealth"))
            {
                float.TryParse(data["uniquehealth"], out uniqueHealthBase);
            }
            if (data.ContainsKey("uniquehealthhero"))
            {
                float.TryParse(data["uniquehealthhero"], out uniqueHealthHero);
            }
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            base.ChangeReference(oldName, newName);

            for (int j = 0; j < placement.Length; j++)
            {
                for (int i = 0; i < placement[j].Length; i++)
                {
                    // Placement used is being renamed
                    if (placement[j][i].Equals(oldName))
                    {
                        placement[j][i] = newName;
                    }
                }
                // If any were replaced with "", remove them
                placement[j] = RemoveFromArray(placement[j], "");
            }

            for (int i = 0; i < mTypes.Length; i++)
            {
                // Placement used is being renamed
                if (mTypes[i].Equals(oldName) && oldName.IndexOf("Monster") != 0)
                {
                    mTypes[i] = newName;
                }
            }
            // If any were replaced with "", remove them
            mTypes = RemoveFromArray(mTypes, "");
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (mTypes.Length > 0)
            {
                r += "monster=";
                foreach (string s in mTypes)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (mTraitsRequired.Length > 0)
            {
                r += "traits=";
                foreach (string s in mTraitsRequired)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (mTraitsPool.Length > 0)
            {
                r += "traitpool=";
                foreach (string s in mTraitsPool)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            for (int i = 0; i < placement.Length; i++)
            {
                if (placement[i].Length > 0)
                {
                    r += "placement" + i + "=";
                    foreach (string s in placement[i])
                    {
                        r += s + " ";
                    }
                    r = r.Substring(0, r.Length - 1) + nl;
                }
            }
            if (unique)
            {
                r += "unique=true" + nl;
                r += "uniquehealth=" + uniqueHealthBase + nl;
                r += "uniquehealthhero=" + uniqueHealthHero + nl;
            }
            if (uniqueHealthBase != 0 && !unique)
            {
                r += "uniquehealth=" + uniqueHealthBase + nl;
            }
            if (uniqueHealthHero != 0 && !unique)
            {
                r += "uniquehealthhero=" + uniqueHealthHero + nl;
            }
            return r;
        }
    }
}
