using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using UnityEngine;

namespace Assets.Scripts.GameTypes
{
    // Things for IA
    public class IAGameType : GameType
    {
        public override string DataDirectory()
        {
            return ContentDataBase.ContentPath() + "IA/";
        }

        public override StringKey HeroName()
        {
            return new StringKey("val", "IA_HERO_NAME");
        }

        public override StringKey HeroesName()
        {
            return new StringKey("val", "IA_HEROES_NAME");
        }

        public override StringKey QuestName()
        {
            return new StringKey("val", "IA_QUEST_NAME");
        }

        public override Font GetFont()
        {
            return (Font)Resources.Load("Fonts/Gara_Scenario_Desc");
        }

        public override Font GetHeaderFont()
        {
            return (Font)Resources.Load("Fonts/Windl");
        }

        public override Font GetSymbolFont()
        {
            return (Font)Resources.Load("Fonts/Gara_Scenario_Desc");
        }

        public override int MaxHeroes()
        {
            return 4;
        }

        public override int DefaultHeroes()
        {
            return 4;
        }

        public override bool DisplayHeroes()
        {
            return true;
        }

        // Tiles imported from RtL have 105 pixels per square (each 1 inch)
        public override float TilePixelPerSquare()
        {
            return 105;
        }

        public override string TypeName()
        {
            return "IA";
        }

        public override bool TileOnGrid()
        {
            return true;
        }

        public override bool DisplayMorale()
        {
            return true;
        }

        public override float SelectionRound()
        {
            return 1f;
        }

        public override float TileRound()
        {
            return 1f;
        }

        public override bool MonstersGrouped()
        {
            return false;
        }
    }
}
