using System.Collections.Generic;

namespace Assets.Scripts.Content.ContentData
{
    // Class for tile specific data
    public class TileSideData : GenericData
    {
        public float top = 0;
        public float left = 0;
        public float pxPerSquare;
        public float aspect = 0;
        public string reverse = "";
        public static new string type = "TileSide";

        public TileSideData(string name, Dictionary<string, string> content, string path) : base(name, content, path,
            type)
        {
            // Get location of top left square in tile image, default 0
            if (content.ContainsKey("top"))
            {
                float.TryParse(content["top"], out top);
            }

            if (content.ContainsKey("left"))
            {
                float.TryParse(content["left"], out left);
            }

            // pixel per D2E square (inch) of image
            if (content.ContainsKey("pps"))
            {
                if (content["pps"].StartsWith("*"))
                {
                    float.TryParse(content["pps"].Remove(0, 1), out pxPerSquare);
                    pxPerSquare *= Game.Get().gameType.TilePixelPerSquare();
                }
                else
                {
                    float.TryParse(content["pps"], out pxPerSquare);
                }
            }
            else
            {
                pxPerSquare = Game.Get().gameType.TilePixelPerSquare();
            }

            // Some MoM tiles have crazy aspect
            if (content.ContainsKey("aspect"))
            {
                float.TryParse(content["aspect"], out aspect);
            }

            // Other side used for validating duplicate use
            if (content.ContainsKey("reverse"))
            {
                reverse = content["reverse"];
            }
        }
    }
}
