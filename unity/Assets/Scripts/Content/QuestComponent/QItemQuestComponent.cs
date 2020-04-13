using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // Scenario starting item
    public class QItemQuestComponent : QuestComponent
    {
        new public static string type = "QItemQuestComponent";
        public string[] itemName;
        public string[] traits;
        public string[] traitpool;
        public bool starting = false;
        public string inspect = "";

        // Create new (editor)
        public QItemQuestComponent(string s) : base(s)
        {
            source = "items.ini";
            typeDynamic = type;
            itemName = new string[0];
            traits = new string[1];
            traitpool = new string[0];
            traits[0] = "weapon";
        }

        // Create from ini data
        public QItemQuestComponent(string name, Dictionary<string, string> data, string path) : base(name, data, path)
        {
            typeDynamic = type;
            if (data.ContainsKey("itemname"))
            {
                itemName = data["itemname"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                itemName = new string[0];
            }

            if (data.ContainsKey("starting"))
            {
                bool.TryParse(data["starting"], out starting);
            }
            else
            {
                // Depreciated (Format 3)
                starting = true;
            }

            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                traits = new string[0];
            }
            if (data.ContainsKey("traitpool"))
            {
                traitpool = data["traitpool"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                traitpool = new string[0];
            }
            if (data.ContainsKey("inspect"))
            {
                inspect = data["inspect"];
            }
        }

        // Save to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (itemName.Length > 0)
            {
                r += "itemname=";
                foreach (string s in itemName)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            // Depreciated (Format 3) - To make default false
            r += "starting=" + starting + nl;

            if (traits.Length > 0)
            {
                r += "traits=";
                foreach (string s in traits)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (traitpool.Length > 0)
            {
                r += "traitpool=";
                foreach (string s in traitpool)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (inspect.Length > 0)
            {
                r += "inspect=" + inspect + nl;
            }
            return r;
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            if (inspect.Equals(oldName))
            {
                inspect = newName;
            }
            for (int i = 0; i < itemName.Length; i++)
            {
                if (itemName[i].Equals(oldName))
                {
                    itemName[i] = newName;
                }
            }
            // If any were replaced with "", remove them
            itemName = RemoveFromArray(itemName, "");
        }
    }
}
