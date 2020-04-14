using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Quest;
using UnityEngine;

namespace Assets.Scripts.Content.QuestComponent
{
    // Super class for all Quest components
    public class QuestComponent
    {
        // Source file
        public string source = "";
        // location on the board in squares
        public Vector2 location;
        // Has a location been speficied?
        public bool locationSpecified = false;
        // type for sub classes
        public static string type = "";
        public string typeDynamic = "";
        // name of section in ini file
        public string sectionName;
        // image for display
        public UnityEngine.UI.Image image;
        // comment for developers
        public string comment = "";
        // Var tests and operations if required
        public VarTests tests = null;
        public List<VarOperation> operations = null;

        private static char DOT = '.';
        public string genKey(string element)
        {
            return new StringBuilder(sectionName).Append(DOT).Append(element).ToString();
        }

        public StringKey genQuery(string element)
        {
            return new StringKey("qst", sectionName + DOT + element);
        }

        // Create new component in editor
        public QuestComponent(string nameIn)
        {
            typeDynamic = type;
            sectionName = nameIn;
            location = Vector2.zero;
        }

        // Construct from ini data
        public QuestComponent(string nameIn, Dictionary<string, string> data, string sourceIn, int format = -1)
        {
            typeDynamic = type;
            sectionName = nameIn;
            source = sourceIn;

            // Default to 0, 0 unless specified
            location = new Vector2(0, 0);
            locationSpecified = false;
            if (data.ContainsKey("xposition"))
            {
                locationSpecified = true;
                float.TryParse(data["xposition"], out location.x);
            }

            if (data.ContainsKey("yposition"))
            {
                locationSpecified = true;
                float.TryParse(data["yposition"], out location.y);
            }
            if (data.ContainsKey("comment"))
            {
                comment = data["comment"];
            }

            operations = new List<VarOperation>();
            if (data.ContainsKey("operations"))
            {
                string[] array = data["operations"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in array)
                {
                    operations.Add(new VarOperation(s));
                }
            }

            // Backwards support for format < 8
            if (format <= 8 && sectionName.StartsWith("EventEnd"))
            {
                operations.Add(new VarOperation("$end,=,1"));
            }

            tests = new VarTests();
            if (data.ContainsKey("vartests"))
            {
                //todo load save
                string[] array = data["vartests"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in array)
                {
                    tests.Add(s);
                }
            }
            // Backwards support for conditions
            else if (data.ContainsKey("conditions"))
            {
                string[] array = data["conditions"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                foreach (string s in array)
                {
                    if (i > 0) tests.Add(new VarTestsLogicalOperator("AND"));
                    tests.Add(new VarOperation(s));
                    i++;
                }
            }
        }

        // Helper function to remove an element form an array
        public static string[] RemoveFromArray(string[] array, string element)
        {
            // Count how many elements remain
            int count = 0;
            foreach (string s in array)
            {
                if (!s.Equals(element)) count++;
            }

            // Create new array
            string[] trimArray = new string[count];

            // Index through old array, storing in new array
            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(element))
                {
                    trimArray[j++] = array[i];
                }
            }

            return trimArray;
        }

        // Used to rename components
        virtual public void ChangeReference(string oldName, string newName)
        {

        }

        // Used to delete components
        virtual public void RemoveReference(string refName)
        {
            // Rename to "" is taken to be delete
            ChangeReference(refName, "");
        }

        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            string r = "[" + sectionName + "]" + nl;
            if (locationSpecified)
            {
                r += "xposition=" + location.x + nl;
                r += "yposition=" + location.y + nl;
            }
            if (comment.Length > 0)
            {
                r += "comment=" + comment + nl;
            }

            if (operations != null && operations.Count > 0)
            {
                r += "operations=";
                foreach (VarOperation o in operations)
                {
                    r += o.ToString() + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (tests != null && tests.VarTestsComponents.Count > 0)
            {
                r += "vartests=" + tests.ToString() + nl;
            }

            return r;
        }
    }
}
