using System.Collections.Generic;
using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.Quest
{
    // Class for holding current hero status
    public class Hero
    {
        // This can be null if not selected
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;
        //  Heros are in a list so they need ID... maybe at some point this can move to an array
        public int id = 0;
        // Used for events that can select or highlight heros
        public bool selected;
        public string className = "";
        public string hybridClass = "";
        public List<string> skills;
        public int xp = 0;

        // Constuct with content hero data and an index for hero
        public Hero(HeroData h, int i)
        {
            heroData = h;
            id = i;
            skills = new List<string>();
        }

        // Construct with saved data
        public Hero(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["defeated"], out defeated);
            int.TryParse(data["id"], out id);
            if (data.ContainsKey("class"))
            {
                string[] classes = data["class"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                className = classes[0];
                if (classes.Length > 1)
                {
                    hybridClass = classes[1];
                }
            }

            Game game = Game.Get();
            // Saved content reference, look it up
            if (data.ContainsKey("type"))
            {
                foreach (KeyValuePair<string, HeroData> hd in game.cd.heroes)
                {
                    if (hd.Value.sectionName.Equals(data["type"]))
                    {
                        heroData = hd.Value;
                    }
                }
            }
            skills = new List<string>();
            if (data.ContainsKey("skills"))
            {
                skills.AddRange(data["skills"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
            }
            if (data.ContainsKey("xp"))
            {
                int.TryParse(data["xp"], out xp);
            }
        }

        public int AvailableXP()
        {
            Game game = Game.Get();
            int aXP = xp + Mathf.RoundToInt(game.quest.vars.GetValue("$%xp"));
            foreach (string s in skills)
            {
                aXP -= game.cd.skills[s].xp;
            }
            return aXP;
        }

        // Save hero to string for saves/undo
        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            string r = "[Hero" + id + "]" + nl;
            r += "id=" + id + nl;
            r += "activated=" + activated + nl;
            r += "defeated=" + defeated + nl;
            if (className.Length > 0)
            {
                r += "class=" + className + " " + hybridClass + nl;
            }
            if (heroData != null)
            {
                r += "type=" + heroData.sectionName + nl;
            }
            if (skills.Count > 0)
            {
                r += "skills=";
                foreach (string s in skills)
                {
                    r += s + ' ';
                }
                r += nl;
            }

            if (xp != 0)
            {
                r += "xp=" + xp + nl;
            }

            return r + nl;
        }
    }
}
