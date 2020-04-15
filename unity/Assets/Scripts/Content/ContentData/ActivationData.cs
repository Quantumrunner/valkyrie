using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content.ContentData
{
    // Class for Activation specific data
    public class ActivationData : GenericData
    {
        public StringKey ability = new StringKey(null, "-", false);
        public StringKey minionActions = StringKey.NULL;
        public StringKey masterActions = StringKey.NULL;
        public StringKey moveButton = StringKey.NULL;
        public StringKey move = StringKey.NULL;
        public static new string type = "MonsterActivation";
        public bool masterFirst = false;
        public bool minionFirst = false;

        public ActivationData()
        {
        }

        public ActivationData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
            // Get ability
            if (content.ContainsKey("ability"))
            {
                ability = new StringKey(content["ability"]);
            }

            // Get minion activation info
            if (content.ContainsKey("minion"))
            {
                minionActions = new StringKey(content["minion"]);
            }

            // Get master activation info
            if (content.ContainsKey("master"))
            {
                masterActions = new StringKey(content["master"]);
            }

            if (content.ContainsKey("movebutton"))
            {
                moveButton = new StringKey(content["movebutton"]);
            }

            if (content.ContainsKey("move"))
            {
                move = new StringKey(content["move"]);
            }

            if (content.ContainsKey("masterfirst"))
            {
                bool.TryParse(content["masterfirst"], out masterFirst);
            }

            if (content.ContainsKey("minionfirst"))
            {
                bool.TryParse(content["minionfirst"], out minionFirst);
            }
        }
    }
}
