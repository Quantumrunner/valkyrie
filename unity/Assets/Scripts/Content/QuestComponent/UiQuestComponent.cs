using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponent
{
    // Ui is an image/button that is displayed to the user
    public class UiQuestComponent : EventQuestComponent
    {
        new public static string type = "UI";
        public string imageName = "";
        public bool verticalUnits = false;
        public int hAlign = 0;
        public int vAlign = 0;
        public float size = 1;
        public float textSize = 1;
        public string textColor = "white";
        public string textBackgroundColor = "transparent";
        public float aspect = 1;
        public bool border = false;

        public string uitext_key { get { return genKey("uitext"); } }

        public StringKey uiText { get { return genQuery("uitext"); } }

        // Create new with name (used by editor)
        public UiQuestComponent(string s) : base(s)
        {
            source = "ui.ini";
            locationSpecified = true;
            typeDynamic = type;
            cancelable = true;
        }

        // Create from ini data
        public UiQuestComponent(string name, Dictionary<string, string> data, Game game, string path) : base(name, data, path, QuestIniComponent.currentFormat)
        {
            locationSpecified = true;
            typeDynamic = type;
            // Tokens are cancelable because you can select then cancel
            cancelable = true;

            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imageName = value != null ? value.Replace('\\', '/') : value;
            }

            if (data.ContainsKey("vunits"))
            {
                bool.TryParse(data["vunits"], out verticalUnits);
            }

            if (data.ContainsKey("size"))
            {
                float.TryParse(data["size"], out size);
            }

            if (data.ContainsKey("textsize"))
            {
                float.TryParse(data["textsize"], out textSize);
            }

            if (data.ContainsKey("textaspect"))
            {
                float.TryParse(data["textaspect"], out aspect);
            }

            if (data.ContainsKey("textcolor"))
            {
                textColor = data["textcolor"];
            }

            if (data.ContainsKey("textbackgroundcolor"))
            {
                textBackgroundColor = data["textbackgroundcolor"];
            }

            if (data.ContainsKey("halign"))
            {
                if (data["halign"].Equals("left"))
                {
                    hAlign = -1;
                }
                if (data["halign"].Equals("right"))
                {
                    hAlign = 1;
                }
            }

            if (data.ContainsKey("valign"))
            {
                if (data["valign"].Equals("top"))
                {
                    vAlign = -1;
                }
                if (data["valign"].Equals("bottom"))
                {
                    vAlign = 1;
                }
            }

            if (data.ContainsKey("border"))
            {
                bool.TryParse(data["border"], out border);
            }
        }

        // Save to string (for editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            r += "image=" + imageName + nl;
            r += "size=" + size + nl;

            if (textSize != 1)
            {
                r += "textsize=" + textSize + nl;
            }

            if (!textColor.Equals("white"))
            {
                r += "textcolor=" + textColor + nl;
            }

            if (!textBackgroundColor.Equals("transparent"))
            {
                r += "textbackgroundcolor=" + textBackgroundColor + nl;
            }

            if (verticalUnits)
            {
                r += "vunits=" + verticalUnits + nl;
            }

            if (border)
            {
                r += "border=" + border + nl;
            }

            if (aspect != 1)
            {
                r += "textaspect=" + aspect + nl;
            }

            if (hAlign < 0)
            {
                r += "halign=left" + nl;
            }
            if (hAlign > 0)
            {
                r += "halign=right" + nl;
            }

            if (vAlign < 0)
            {
                r += "valign=top" + nl;
            }
            if (vAlign > 0)
            {
                r += "valign=bottom" + nl;
            }

            return r;
        }
    }
}
