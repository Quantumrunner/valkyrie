using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Audio
    public class AudioData : GenericData
    {
        public static new string type = "Audio";
        public string file = "";

        public AudioData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            if (content.ContainsKey("file"))
            {
                if (content["file"].IndexOf("{import}") == 0)
                {
                    file = ContentData.ImportPath() + content["file"].Substring(8);
                }
                else
                {
                    file = path + Path.DirectorySeparatorChar + content["file"];
                }
            }
        }
    }
}
