using Assets.Scripts.Content;
using UnityEngine;

namespace Assets.Scripts.GameTypes
{

    // GameType manages setting that are specific to the game type
    public abstract class GameType
    {
        public abstract string DataDirectory();
        public abstract StringKey HeroName();
        public abstract StringKey HeroesName();
        public abstract StringKey QuestName();
        public abstract int MaxHeroes();
        public abstract int DefaultHeroes();
        public abstract bool DisplayHeroes();

        public abstract float TilePixelPerSquare();

        // There are actually two fonts, should expand to include header/text
        public abstract Font GetFont();
        public abstract Font GetHeaderFont();
        public abstract Font GetSymbolFont();
        public abstract string TypeName();
        public abstract bool TileOnGrid();
        public abstract bool DisplayMorale();
        public abstract float SelectionRound();
        public abstract float TileRound();
        public abstract bool MonstersGrouped();
    }
}