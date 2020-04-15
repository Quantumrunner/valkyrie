using System.Collections.Generic;

namespace Assets.Scripts.Content.QuestComponents
{
    // Puzzle component
    public class PuzzleQuestComponent : EventQuestComponent
    {
        new public static string type = "Puzzle";
        public string puzzleClass = "slide";
        public string skill = "{observation}";
        public int puzzleLevel = 4;
        public int puzzleAltLevel = 3;
        public string puzzleSolution = "";
        public string imageType = "";

        // Create a new puzzle with name (editor)
        public PuzzleQuestComponent(string s) : base(s)
        {
            source = "puzzles.ini";
            typeDynamic = type;
            nextEvent.Add(new List<string>());
            buttonColors.Add("white");
            buttons.Add(genQuery("button1"));
            LocalizationRead.updateScenarioText(genKey("button1"),
                new StringKey("val", "PUZZLE_GUESS").Translate());
        }

        // Construct from ini data
        public PuzzleQuestComponent(string name, Dictionary<string, string> data, string path) : base(name, data, path, QuestIniComponent.currentFormat)
        {
            typeDynamic = type;

            if (data.ContainsKey("class"))
            {
                puzzleClass = data["class"];
            }
            if (data.ContainsKey("image"))
            {
                string value = data["image"];
                imageType = value != null ? value.Replace('\\', '/') : value;
            }
            if (data.ContainsKey("skill"))
            {
                skill = data["skill"];
            }
            if (data.ContainsKey("puzzlelevel"))
            {
                int.TryParse(data["puzzlelevel"], out puzzleLevel);
            }
            if (data.ContainsKey("puzzlealtlevel"))
            {
                int.TryParse(data["puzzlealtlevel"], out puzzleAltLevel);
            }
            if (data.ContainsKey("puzzlesolution"))
            {
                puzzleSolution = data["puzzlesolution"];
            }
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = base.ToString();

            if (!puzzleClass.Equals("slide"))
            {
                r += "class=" + puzzleClass + nl;
            }
            if (!skill.Equals("{observation}"))
            {
                r += "skill=" + skill + nl;
            }
            if (!imageType.Equals(""))
            {
                r += "image=" + imageType + nl;
            }
            if (puzzleLevel != 4)
            {
                r += "puzzlelevel=" + puzzleLevel + nl;
            }
            if (puzzleAltLevel != 3)
            {
                r += "puzzlealtlevel=" + puzzleAltLevel + nl;
            }
            if (!puzzleSolution.Equals(""))
            {
                r += "puzzlesolution=" + puzzleSolution + nl;
            }
            return r;
        }
    }
}
