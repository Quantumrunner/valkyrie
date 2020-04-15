using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for tile specific data
    public class PackTypeData : GenericData
    {
        public static new string type = "PackType";

        public PackTypeData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
        }
    }
}
