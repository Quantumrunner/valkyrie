using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content.QuestComponent;

namespace Assets.Scripts.Content
{
    // Perils are content data that inherits from QuestData for reasons.
    public class PerilData : EventQuestComponent
    {
        new public static string type = "Peril";
        public int priority = 0;

        public StringKey perilText;

        override public StringKey text
        {
            get { return perilText; }
        }

        public PerilData(string name, Dictionary<string, string> data) : base(name, data, "",
            Assets.Scripts.Content.QuestIniComponent.currentFormat)
        {
            typeDynamic = type;
            if (data.ContainsKey("priority"))
            {
                int.TryParse(data["priority"], out priority);
            }

            if (data.ContainsKey("text"))
            {
                perilText = new StringKey(data["text"]);
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                if (data.ContainsKey("button" + (i + 1)))
                {
                    buttons[i] = new StringKey(data["button" + (i + 1)]);
                }
                else
                {
                    buttons[i] = StringKey.NULL;
                }
            }
        }
    }
}
