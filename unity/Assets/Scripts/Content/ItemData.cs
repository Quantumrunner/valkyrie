using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for Item specific data
    public class ItemData : GenericData
    {
        public static new string type = "Item";
        public bool unique = false;
        public int price = 0;
        public int minFame = -1;
        public int maxFame = -1;

        public ItemData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            if (name.IndexOf("ItemUnique") == 0)
            {
                unique = true;
            }

            if (content.ContainsKey("price"))
            {
                int.TryParse(content["price"], out price);
            }

            if (content.ContainsKey("minfame"))
            {
                minFame = Fame(content["minfame"]);
            }

            if (content.ContainsKey("maxfame"))
            {
                maxFame = Fame(content["maxfame"]);
            }
        }

        public static int Fame(string name)
        {
            if (name.Equals("insignificant")) return 1;
            if (name.Equals("noteworthy")) return 2;
            if (name.Equals("impressive")) return 3;
            if (name.Equals("celebrated")) return 4;
            if (name.Equals("heroic")) return 5;
            if (name.Equals("legendary")) return 6;
            return 0;
        }
    }
}
