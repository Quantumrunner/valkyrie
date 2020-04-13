using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // Tokens are events that are tied to a token placed on the board
    public class Token : Event
    {
        new public static string type = "Token";
        public int rotation = 0;
        public string tokenName;

        // Create new with name (used by editor)
        public Token(string s) : base(s)
        {
            source = "tokens.ini";
            locationSpecified = true;
            typeDynamic = type;
            tokenName = "TokenSearch";
            cancelable = true;
        }

        // Create from ini data
        public Token(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, Quest.currentFormat)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            // Tokens don't have conditions
            tests = null;

            tokenName = "";
            if (data.ContainsKey("type"))
            {
                tokenName = data["type"];
            }
            // Get rotation if specified
            if (data.ContainsKey("rotation"))
            {
                int.TryParse(data["rotation"], out rotation);
            }
        }

        // Save to string (for editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "type=" + tokenName + nl;
            if (rotation != 0)
            {
                r += "rotation=" + rotation + nl;
            }
            return r;
        }
    }
}
