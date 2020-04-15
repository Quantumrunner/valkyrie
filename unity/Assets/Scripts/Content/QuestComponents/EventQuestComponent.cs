using System.Collections.Generic;
using Assets.Scripts.Quest.VariableTests;

namespace Assets.Scripts.Content.QuestComponents
{
    // Events are used to create dialogs that control the Quest
    public class EventQuestComponent : QuestComponent
    {
        new public static string type = "Event";

        public bool display = true;
        public List<StringKey> buttons;
        public List<string> buttonColors;
        public string trigger = "";
        public List<List<string>> nextEvent;
        public string heroListName = "";
        public int minHeroes = 0;
        public int maxHeroes = 0;
        public string[] addComponents;
        public string[] removeComponents;
        public bool cancelable = false;
        public bool highlight = false;
        public bool randomEvents = false;
        public bool minCam = false;
        public bool maxCam = false;
        public int quota = 0;
        public string quotaVar = "";
        public string audio = "";
        public List<string> music;

        public string text_key { get { return genKey("text"); } }

        virtual public StringKey text { get { return genQuery("text"); } }

        // Create a new event with name (editor)
        public EventQuestComponent(string s) : base(s)
        {
            source = "events.ini";
            display = false;
            typeDynamic = type;
            nextEvent = new List<List<string>>();
            buttons = new List<StringKey>();
            buttonColors = new List<string>();
            addComponents = new string[0];
            removeComponents = new string[0];
            operations = new List<VarOperation>();
            tests = new VarTests();
            minCam = false;
            maxCam = false;
            music = new List<string>();
        }

        // Create event from ini data
        public EventQuestComponent(string name, Dictionary<string, string> data, string path, int format) : base(name, data, path, format)
        {
            typeDynamic = type;

            if (data.ContainsKey("display"))
            {
                bool.TryParse(data["display"], out display);
            }

            // Should the target location by highlighted?
            if (data.ContainsKey("highlight"))
            {
                bool.TryParse(data["highlight"], out highlight);
            }

            nextEvent = new List<List<string>>();
            buttons = new List<StringKey>();
            buttonColors = new List<string>();

            int buttonCount = 0;
            if (data.ContainsKey("buttons"))
            {
                int.TryParse(data["buttons"], out buttonCount);
            }

            // Displayed events must have a button
            if (display && buttonCount == 0)
            {
                buttonCount = 1;
            }

            for (int buttonNum = 1; buttonNum <= buttonCount; buttonNum++)
            {
                buttons.Add(genQuery("button" + buttonNum));
                if (data.ContainsKey("event" + buttonNum) && (data["event" + buttonNum].Trim().Length > 0))
                {
                    nextEvent.Add(new List<string>(data["event" + buttonNum].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)));
                }
                else
                {
                    nextEvent.Add(new List<string>());
                }

                if (data.ContainsKey("buttoncolor" + buttonNum) && display)
                {
                    buttonColors.Add(data["buttoncolor" + buttonNum]);
                }
                else
                {
                    buttonColors.Add("white");
                }
            }

            // Heros from another event can be hilighted
            if (data.ContainsKey("hero"))
            {
                heroListName = data["hero"];
            }

            // Success quota
            if (data.ContainsKey("quota"))
            {
                int.TryParse(data["quota"], out quota);
                if (data["quota"].Length > 0 && !char.IsNumber(data["quota"][0]))
                {
                    quotaVar = data["quota"];
                }
            }

            // minimum heros required to be selected for event
            if (data.ContainsKey("minhero"))
            {
                int.TryParse(data["minhero"], out minHeroes);
            }

            // maximum heros selectable for event (0 disables)
            if (data.ContainsKey("maxhero"))
            {
                int.TryParse(data["maxhero"], out maxHeroes);
            }

            // Display hidden components (space separated list)
            if (data.ContainsKey("add"))
            {
                addComponents = data["add"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                addComponents = new string[0];
            }

            // Hide components (space separated list)
            if (data.ContainsKey("remove"))
            {
                removeComponents = data["remove"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                removeComponents = new string[0];
            }

            // trigger event on condition
            if (data.ContainsKey("trigger"))
            {
                trigger = data["trigger"];
            }

            // Randomise next event setting
            if (data.ContainsKey("randomevents"))
            {
                bool.TryParse(data["randomevents"], out randomEvents);
            }
            // Randomise next event setting
            if (data.ContainsKey("mincam"))
            {
                locationSpecified = false;
                bool.TryParse(data["mincam"], out minCam);
            }
            // Randomise next event setting
            if (data.ContainsKey("maxcam"))
            {
                locationSpecified = false;
                bool.TryParse(data["maxcam"], out maxCam);
            }
            if (data.ContainsKey("audio"))
            {
                string value = data["audio"];
                audio = value != null ? value.Replace('\\', '/') : value;
            }
            music = new List<string>();
            if (data.ContainsKey("music"))
            {
                music = new List<string>(data["music"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries));
                for (int i = 0; i < music.Count; i++)
                {
                    music[i] = music[i].Replace('\\', '/');
                }
            }
        }

        // Check all references when a component name is changed
        override public void ChangeReference(string oldName, string newName)
        {
            if (sectionName.Equals(oldName) && newName != "")
            {
                for (int i = 1; i <= buttons.Count; i++)
                {
                    buttons[i - 1] = new StringKey("qst", newName + '.' + "button" + i);
                }
            }

            // hero list event is changed
            if (heroListName.Equals(oldName))
            {
                heroListName = newName;
            }
            // a next event is changed
            for (int i = 0; i < nextEvent.Count; i++)
            {
                for (int j = 0; j < nextEvent[i].Count; j++)
                {
                    if (nextEvent[i][j].Equals(oldName))
                    {
                        nextEvent[i][j] = newName;
                    }
                }
            }
            // If next event is deleted, trim array
            for (int i = 0; i < nextEvent.Count; i++)
            {
                bool removed = true;
                while (removed)
                {
                    removed = nextEvent[i].Remove("");
                }
            }

            // If CustomMonster renamed update trigger
            if (trigger.IndexOf("Defeated" + oldName) == 0)
            {
                trigger = "Defeated" + newName;
            }
            if (trigger.IndexOf("DefeatedUnique" + oldName) == 0)
            {
                trigger = "DefeatedUnique" + newName;
            }

            // component to add renamed
            for (int i = 0; i < addComponents.Length; i++)
            {
                if (addComponents[i].Equals(oldName))
                {
                    addComponents[i] = newName;
                }
            }
            // If component is deleted, trim array
            addComponents = RemoveFromArray(addComponents, "");

            // component to remove renamed
            for (int i = 0; i < removeComponents.Length; i++)
            {
                if (removeComponents[i].Equals(oldName))
                {
                    removeComponents[i] = newName;
                }
            }
            // If component is deleted, trim array
            removeComponents = RemoveFromArray(removeComponents, "");
        }

        // Save event to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!display)
            {
                r += "display=false" + nl;
            }

            if (highlight)
            {
                r += "highlight=true" + nl;
            }

            r += "buttons=" + buttons.Count + nl;

            int buttonNum = 1;
            foreach (List<string> l in nextEvent)
            {
                r += "event" + buttonNum++ + "=";
                foreach (string s in l)
                {
                    r += s + " ";
                }
                if (l.Count > 0)
                {
                    r = r.Substring(0, r.Length - 1);
                }
                r += nl;
            }

            buttonNum = 1;
            foreach (string s in buttonColors)
            {
                if (!s.Equals("white"))
                {
                    r += "buttoncolor" + buttonNum + "=\"" + s + "\"" + nl;
                }
                buttonNum++;
            }

            if (!heroListName.Equals(""))
            {
                r += "hero=" + heroListName + nl;
            }
            if (quotaVar.Length > 0)
            {
                r += "quota=" + quotaVar + nl;
            }
            else if (quota != 0)
            {
                r += "quota=" + quota + nl;
            }
            if (minHeroes != 0)
            {
                r += "minhero=" + minHeroes + nl;
            }
            if (maxHeroes != 0)
            {
                r += "maxhero=" + maxHeroes + nl;
            }
            if (addComponents.Length > 0)
            {
                r += "add=";
                foreach (string s in addComponents)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (removeComponents.Length > 0)
            {
                r += "remove=";
                foreach (string s in removeComponents)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }
            if (!trigger.Equals(""))
            {
                r += "trigger=" + trigger + nl;
            }

            if (randomEvents)
            {
                r += "randomevents=true" + nl;
            }
            // Randomise next event setting
            if (minCam)
            {
                r += "mincam=true" + nl;
            }
            if (maxCam)
            {
                r += "maxcam=true" + nl;
            }
            if (maxCam || minCam)
            {
                r += "xposition=" + location.x + nl;
                r += "yposition=" + location.y + nl;
            }

            if (audio.Length > 0)
            {
                r += "audio=" + audio + nl;
            }

            if (music.Count > 0)
            {
                r += "music=";
                foreach (string s in music)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            return r;
        }
    }
}
