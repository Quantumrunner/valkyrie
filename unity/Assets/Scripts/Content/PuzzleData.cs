using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Puzzle images
    public class PuzzleData : GenericData
    {
        public static new string type = "Puzzle";

        public PuzzleData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
        }
    }
}
