using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Super class for all content loaded from content packs
    public class GenericData
    {
        // name from section title or data
        public StringKey name = StringKey.NULL;

        // sets from which this belogs (expansions)
        public List<string> sets;

        // section name
        public string sectionName;

        // List of traits
        public string[] traits;

        // Path to image
        public string image;

        // priority for duplicates
        public int priority;

        // for sub classes to set type
        public static string type = "";

        public GenericData()
        {
        }

        // generic constructor gets common things
        public GenericData(string name_ini, Dictionary<string, string> content, string path, string type)
        {
            sectionName = name_ini;
            sets = new List<string>();

            // Has the name been specified?
            if (content.ContainsKey("name"))
            {
                name = new StringKey(content["name"]);
            }
            else
            {
                name = new StringKey(null, name_ini.Substring(type.Length));
            }

            priority = 0;
            if (content.ContainsKey("priority"))
            {
                int.TryParse(content["priority"], out priority);
            }

            if (content.ContainsKey("traits"))
            {
                traits = content["traits"].Split(" ".ToCharArray());
            }
            else // No traits is a valid condition
            {
                traits = new string[0];
            }

            // If image specified it is relative to the path of the ini file
            // absolute paths are not supported
            // resolve optional images like image, image2, image3 and so on
            int count = 0;
            while (true)
            {
                string key = "image" + (count > 0 ? (count + 1).ToString() : "");
                if (!content.ContainsKey(key))
                {
                    image = ""; // No image is a valid condition
                    break;
                }

                if (content[key].StartsWith("{import}"))
                {
                    image = Path.Combine(ContentData.ImportPath(), content[key].Substring(9));
                }
                else
                {
                    image = Path.Combine(path, content[key]);
                }

                if (ContentData.ResolveTextureFile(image) != null)
                {
                    break;
                }

                count++;
            }
        }

        // Does the component contain a trait?
        public bool ContainsTrait(string trait)
        {
            foreach (string s in traits)
            {
                if (trait.Equals(s))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
