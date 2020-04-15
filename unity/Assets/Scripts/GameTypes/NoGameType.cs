using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using UnityEngine;

namespace Assets.Scripts.GameTypes
{
    // NoGameType exists for management reasons
    // Perhaps this should be the base and others inherit from this to simplify this class?
    public class NoGameType : GameType
    {
        public override string DataDirectory()
        {
            return ContentDataBase.ContentPath();
        }

        public override StringKey HeroName()
        {
            return new StringKey("val", "D2E_HERO_NAME");
        }

        public override StringKey HeroesName()
        {
            return new StringKey("val", "D2E_HEROES_NAME");
        }

        public override StringKey QuestName()
        {
            return new StringKey("val", "D2E_QUEST_NAME");
        }

        public override Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public override Font GetHeaderFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public override Font GetSymbolFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public override int MaxHeroes()
        {
            return 0;
        }

        public override int DefaultHeroes()
        {
            return 0;
        }

        public override bool DisplayHeroes()
        {
            return true;
        }

        public override float TilePixelPerSquare()
        {
            return 1f;
        }

        public override string TypeName()
        {
            return "";
        }

        public override bool TileOnGrid()
        {
            return true;
        }

        public override bool DisplayMorale()
        {
            return false;
        }

        // Number of squares for snap of objects in editor
        public override float SelectionRound()
        {
            return 1f;
        }

        // Number of squares for snap of tiles in editor
        public override float TileRound()
        {
            return 1f;
        }

        public override bool MonstersGrouped()
        {
            return true;
        }
    }
}
