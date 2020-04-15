using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.GameTypes
{
    // Things for D2E
    public class D2EGameType : GameType
    {
        public override string DataDirectory()
        {
            return ContentData.ContentPath() + "D2E/";
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

        // There are actually two fonts, should expand to include header/text
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
            return (Font)Resources.Load("Fonts/Descent_symbol");
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
            return "D2E";
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
            return true;
        }
    }
}
