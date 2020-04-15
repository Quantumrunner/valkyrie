using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Class for images
    public class ImageData : TokenData
    {
        public static new string type = "Image";

        public ImageData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
        }
    }
}
