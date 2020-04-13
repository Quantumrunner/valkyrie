using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // Quest defined Monster activation
    public class ActivationQuestComponent : QuestComponent
    {
        new public static string type = "ActivationQuestComponent";
        public bool minionFirst = false;
        public bool masterFirst = false;

        public string ability_key { get { return genKey("ability"); } }
        public string minion_key { get { return genKey("minion"); } }
        public string master_key { get { return genKey("master"); } }
        public string movebutton_key { get { return genKey("movebutton"); } }
        public string move_key { get { return genKey("move"); } }

        public StringKey ability { get { return genQuery("ability"); } }
        public StringKey minionActions { get { return genQuery("minion"); } }
        public StringKey masterActions { get { return genQuery("master"); } }
        public StringKey moveButton { get { return genQuery("movebutton"); } }
        public StringKey move { get { return genQuery("move"); } }

        // Create new (editor)
        public ActivationQuestComponent(string s) : base(s)
        {
            source = "monsters.ini";
            LocalizationRead.updateScenarioText(ability_key, "-");
            typeDynamic = type;
        }

        // Create from ini data
        public ActivationQuestComponent(string name, Dictionary<string, string> data, string path) : base(name, data, path)
        {
            typeDynamic = type;
            if (data.ContainsKey("minionfirst"))
            {
                bool.TryParse(data["minionfirst"], out minionFirst);
            }
            if (data.ContainsKey("masterfirst"))
            {
                bool.TryParse(data["masterfirst"], out masterFirst);
            }
        }

        // Save to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (minionFirst)
            {
                r += "minionfirst=" + minionFirst.ToString() + nl;
            }
            if (masterFirst)
            {
                r += "masterfirst=" + masterFirst.ToString() + nl;
            }
            return r;
        }
    }
}
