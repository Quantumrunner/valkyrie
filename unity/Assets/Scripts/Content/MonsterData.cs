using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    // Class for Hero specific data
    public class MonsterData : GenericData
    {
        public StringKey info = new StringKey(null, "-", false);
        public string imagePlace;
        public static new string type = "Monster";
        public string[] activations;
        public float healthBase = 0;
        public float healthPerHero = 0;
        public int horror = 0;
        public int awareness = 0;

        // This constuctor only exists for the Quest version of this class to use to do nothing
        public MonsterData()
        {
        }

        public MonsterData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
            // Get usage info
            if (content.ContainsKey("info"))
            {
                info = new StringKey(content["info"]);
            }

            if (content.ContainsKey("imageplace"))
            {
                if (content["imageplace"].IndexOf("{import}") == 0)
                {
                    imagePlace = ContentData.ImportPath() + content["imageplace"].Substring(8);
                }
                else
                {
                    imagePlace = path + Path.DirectorySeparatorChar + content["imageplace"];
                }
            }
            else // No image is a valid condition
            {
                imagePlace = image;
            }

            activations = new string[0];
            if (content.ContainsKey("activation"))
            {
                activations = content["activation"].Split(' ');
            }

            if (content.ContainsKey("health"))
            {
                float.TryParse(content["health"], out healthBase);
            }

            if (content.ContainsKey("healthperhero"))
            {
                float.TryParse(content["healthperhero"], out healthPerHero);
            }

            if (content.ContainsKey("horror"))
            {
                int.TryParse(content["horror"], out horror);
            }

            if (content.ContainsKey("awareness"))
            {
                int.TryParse(content["awareness"], out awareness);
            }
        }

        virtual public IEnumerable<string> GetAttackTypes()
        {
            HashSet<string> toReturn = new HashSet<string>();
            foreach (KeyValuePair<string, AttackData> kv in Game.Get().cd.investigatorAttacks)
            {
                if (ContainsTrait(kv.Value.target))
                {
                    toReturn.Add(kv.Value.attackType);
                }
            }

            return toReturn;
        }

        virtual public StringKey GetRandomAttack(string type)
        {
            List<AttackData> validAttacks = new List<AttackData>();
            foreach (AttackData ad in Game.Get().cd.investigatorAttacks.Values)
            {
                if (ad.attackType.Equals(type))
                {
                    if (traits.Length == 0)
                    {
                        ValkyrieDebug.Log("Monster with no traits, this should not happen");
                        validAttacks.Add(ad);
                    }
                    else if (traits.Contains(ad.target))
                    {
                        validAttacks.Add(ad);
                    }
                }
            }

            return validAttacks[Random.Range(0, validAttacks.Count)].text;
        }
    }
}
