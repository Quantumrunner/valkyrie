using System.Collections.Generic;

namespace Assets.Scripts.Content.ContentData
{
    // Holding class for contentpack data
    public class ContentPack
    {
        public string name;
        public string image;
        public string description;
        public string id;
        public string type;
        public List<string> iniFiles;
        public Dictionary<string, List<string>> localizationFiles;
        public List<string> clone;
    }
}
