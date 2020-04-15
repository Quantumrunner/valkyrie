using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using UnityEngine;

namespace Assets.Scripts.GameTypes
{
    public class MoMGameType : GameType
    {
        public override string DataDirectory()
        {
            return ContentDataBase.ContentPath() + "MoM/";
        }

        public override StringKey HeroName()
        {
            return new StringKey("val", "MOM_HERO_NAME");
        }

        public override StringKey HeroesName()
        {
            return new StringKey("val", "MOM_HEROES_NAME");
        }

        public override StringKey QuestName()
        {
            return new StringKey("val", "MOM_QUEST_NAME");
        }

        public override Font GetFont()
        {
            return (Font)Resources.Load("Fonts/MADGaramondPro");
        }

        public override Font GetHeaderFont()
        {
            return (Font)Resources.Load("Fonts/OldNewspaperTypes");
        }

        public override Font GetSymbolFont()
        {
            return (Font)Resources.Load("Fonts/MADGaramondPro");
        }

        public override int MaxHeroes()
        {
            return 10;
        }

        public override int DefaultHeroes()
        {
            return 5;
        }

        public override bool DisplayHeroes()
        {
            return false;
        }

        public override float TilePixelPerSquare()
        {
            // the base side of the tile is 1024 pixels, we are having 3.5 'squares' (3.5 inches) in this
            // These squares are the same size as D2E squares
            if (Application.platform == RuntimePlatform.Android)
                return 512f / 3.5f;
            return 1024f / 3.5f;
        }

        public override string TypeName()
        {
            return "MoM";
        }

        public override bool TileOnGrid()
        {
            return false;
        }

        public override bool DisplayMorale()
        {
            return false;
        }

        // Number of squares for snap of objects in editor
        public override float SelectionRound()
        {
            return 1.75f;
        }

        // Number of squares for snap of tiles in editor
        public override float TileRound()
        {
            return 3.5f;
        }

        public override bool MonstersGrouped()
        {
            return false;
        }
    }
}
