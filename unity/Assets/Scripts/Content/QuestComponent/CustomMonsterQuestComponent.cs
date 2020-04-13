using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Assets.Scripts.Content.QuestComponent
{
    // Monster defined in the Quest
    public class CustomMonsterQuestComponent : Content.QuestComponent.QuestComponent
    {
        new public static string type = "CustomMonsterQuestComponent";
        // A bast type is used for default values
        public string baseMonster = "";
        public string imagePath = "";
        public string imagePlace = "";
        public string[] activations;
        public string[] traits;
        public string path = "";
        public float healthBase = 0;
        public float healthPerHero = 0;
        public bool healthDefined = false;
        public string evadeEvent = "";
        public string horrorEvent = "";
        public int horror = 0;
        public bool horrorDefined = false;
        public int awareness = 0;
        public bool awarenessDefined = false;
        public Dictionary<string, List<StringKey>> investigatorAttacks = new Dictionary<string, List<StringKey>>();

        public string monstername_key { get { return genKey("monstername"); } }
        public string info_key { get { return genKey("info"); } }

        public StringKey monsterName { get { return genQuery("monstername"); } }
        public StringKey info { get { return genQuery("info"); } }

        // Create new with name (editor)
        public CustomMonsterQuestComponent(string s) : base(s)
        {
            source = "monsters.ini";
            LocalizationRead.updateScenarioText(monstername_key, sectionName);
            LocalizationRead.updateScenarioText(info_key, "-");
            activations = new string[0];
            traits = new string[0];
            typeDynamic = type;
        }

        // Create from ini data
        public CustomMonsterQuestComponent(string iniName, Dictionary<string, string> data, string pathIn) : base(iniName, data, pathIn)
        {
            typeDynamic = type;
            path = Path.GetDirectoryName(pathIn);
            // Get base derived monster type
            if (data.ContainsKey("base"))
            {
                baseMonster = data["base"];
            }

            traits = new string[0];
            if (data.ContainsKey("traits"))
            {
                traits = data["traits"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imagePath = value != null ? value.Replace('\\', '/') : value;
            }

            imagePlace = imagePath;
            if (data.ContainsKey("imageplace"))
            {
                imagePlace = data["imageplace"];
            }

            activations = new string[0];
            if (data.ContainsKey("activation"))
            {
                activations = data["activation"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }

            if (data.ContainsKey("health"))
            {
                healthDefined = true;
                float.TryParse(data["health"], out healthBase);
            }
            if (data.ContainsKey("healthperhero"))
            {
                healthDefined = true;
                float.TryParse(data["healthperhero"], out healthPerHero);
            }

            if (data.ContainsKey("evadeevent"))
            {
                evadeEvent = data["evadeevent"];
            }
            if (data.ContainsKey("horrorevent"))
            {
                horrorEvent = data["horrorevent"];
            }

            if (data.ContainsKey("horror"))
            {
                horrorDefined = true;
                int.TryParse(data["horror"], out horror);
            }
            if (data.ContainsKey("awareness"))
            {
                awarenessDefined = true;
                int.TryParse(data["awareness"], out awareness);
            }

            if (data.ContainsKey("attacks"))
            {
                foreach (string typeEntry in data["attacks"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    string typeE = typeEntry;
                    int typeCount = 1;
                    int typeSeparator = typeEntry.IndexOf(':');
                    if (typeSeparator >= 0)
                    {
                        typeE = typeEntry.Substring(0, typeSeparator);
                        int.TryParse(typeEntry.Substring(typeSeparator + 1), out typeCount);
                    }

                    if (!investigatorAttacks.ContainsKey(typeE))
                    {
                        investigatorAttacks.Add(typeE, new List<StringKey>());
                    }

                    for (int i = 1; i <= typeCount; i++)
                    {
                        investigatorAttacks[typeE].Add(genQuery("Attack_" + typeE + "_" + i));
                    }
                }
            }
        }

        // get path of monster image
        public string GetImagePath()
        {
            if (imagePath.Length == 0)
            {
                // this will use the base monster type
                return "";
            }
            return path + Path.DirectorySeparatorChar + imagePath;
        }
        public string GetImagePlacePath()
        {
            if (imagePlace.Length == 0)
            {
                // this will use the base monster type
                return "";
            }
            return path + Path.DirectorySeparatorChar + imagePlace;
        }

        // Save to string (editor)
        override public string ToString()
        {
            StringBuilder r = new StringBuilder().Append(base.ToString());

            if (baseMonster.Length > 0)
            {
                r.Append("base=").AppendLine(baseMonster);
            }
            if (traits.Length > 0)
            {
                r.Append("traits=").AppendLine(string.Join(" ", traits));
            }
            if (imagePath.Length > 0)
            {
                r.Append("image=").AppendLine(imagePath);
            }
            if (imagePlace.Length > 0)
            {
                r.Append("imageplace=").AppendLine(imagePlace);
            }
            if (activations.Length > 0)
            {
                r.Append("activation=").AppendLine(string.Join(" ", activations));
            }
            if (healthDefined)
            {
                r.Append("health=").AppendLine(healthBase.ToString());
                r.Append("healthperhero=").AppendLine(healthPerHero.ToString());
            }
            if (evadeEvent.Length > 0)
            {
                r.Append("evadeevent=").AppendLine(evadeEvent);
            }
            if (horrorEvent.Length > 0)
            {
                r.Append("horrorevent=").AppendLine(horrorEvent);
            }

            if (horrorDefined)
            {
                r.Append("horror=").AppendLine(horror.ToString());
            }
            if (awarenessDefined)
            {
                r.Append("awareness=").AppendLine(awareness.ToString());
            }

            if (investigatorAttacks.Count > 0)
            {
                string attacksLine = "attacks=";
                foreach (string type in investigatorAttacks.Keys)
                {
                    attacksLine += type + ':' + investigatorAttacks[type].Count + " ";
                }
                r.AppendLine(attacksLine.Substring(0, attacksLine.Length - 1));
            }

            return r.ToString();
        }

        // When changing the name placement event need to update in array
        override public void ChangeReference(string oldName, string newName)
        {
            for (int i = 0; i < activations.Length; i++)
            {
                if (activations[i].Equals(oldName))
                {
                    activations[i] = newName;
                }
                else
                {
                    if (oldName.Equals("ActivationQuestComponent" + activations[i]))
                    {
                        if (newName.IndexOf("ActivationQuestComponent") == 0)
                        {
                            activations[i] = newName.Substring("ActivationQuestComponent".Length);
                        }
                        if (newName.Length == 0)
                        {
                            activations[i] = "";
                        }
                    }
                }
            }
            // If any were replaced with "", remove them
            activations = RemoveFromArray(activations, "");

            if (evadeEvent.Equals(oldName))
            {
                evadeEvent = newName;
            }
            if (horrorEvent.Equals(oldName))
            {
                horrorEvent = newName;
            }
        }
    }
}
