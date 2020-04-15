using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Content.ContentData
{
    // Class for Token data
    public class TokenData : GenericData
    {
        public int x = 0;
        public int y = 0;
        public int height = 0;

        public int width = 0;

        // 0 means token is 1 square big
        public float pxPerSquare = 0;
        public static new string type = "Token";

        public TokenData(string name, Dictionary<string, string> content, string path) : base(name, content, path, type)
        {
            init(content);
        }

        public TokenData(string name, Dictionary<string, string> content, string path, string typeIn) : base(name,
            content, path, typeIn)
        {
            init(content);
        }

        public void init(Dictionary<string, string> content)
        {

            if (Application.platform == RuntimePlatform.Android && content.ContainsKey("x_android"))
            {
                int.TryParse(content["x_android"], out x);
            }
            else if (content.ContainsKey("x"))
            {
                int.TryParse(content["x"], out x);
            }

            if (Application.platform == RuntimePlatform.Android && content.ContainsKey("y_android"))
            {
                int.TryParse(content["y_android"], out y);
            }
            else if (content.ContainsKey("y"))
            {
                int.TryParse(content["y"], out y);
            }

            // These are used to extract part of an image (atlas) for the token
            if (content.ContainsKey("height"))
            {
                int.TryParse(content["height"], out height);
            }

            if (content.ContainsKey("height"))
            {
                int.TryParse(content["width"], out width);
            }

            // pixel per D2E square (inch) of image
            if (content.ContainsKey("pps"))
            {
                float.TryParse(content["pps"], out pxPerSquare);
            }
        }

        public bool FullImage()
        {
            if (height == 0) return true;
            if (width == 0) return true;
            return false;
        }
    }
}
