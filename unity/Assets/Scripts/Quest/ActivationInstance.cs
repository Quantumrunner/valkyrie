using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content;
using Assets.Scripts.Quest.Events;

namespace Assets.Scripts.Quest
{
    // Activation instance is requresd to track variables in the activation
    public class ActivationInstance
    {
        public ActivationData ad;
        // String is populated on creation of the activation
        public string effect;
        public string move;
        public string minionActions;
        public string masterActions;

        // Construct activation
        public ActivationInstance(ActivationData contentActivation, string monsterName)
        {
            ad = contentActivation;
            minionActions = EventManager.OutputSymbolReplace(ad.minionActions.Translate().Replace("{0}", monsterName));
            masterActions = EventManager.OutputSymbolReplace(ad.masterActions.Translate().Replace("{0}", monsterName));

            // Fill in hero, monster names
            // Note: Random hero selection is NOT kept on load/undo FIXME
            if (Game.Get().gameType is MoMGameType)
            {
                effect = ad.ability.Translate().Replace("{0}", monsterName);
                move = ad.move.Translate().Replace("{0}", monsterName);
                move = EventManager.OutputSymbolReplace(move).Replace("\\n", "\n");
            }
            else
            {
                effect = ad.ability.Translate().Replace("{0}", Game.Get().quest.GetRandomHero().heroData.name.Translate());
                effect = effect.Replace("{1}", monsterName);
            }
            // Fix new lines
            effect = EventManager.OutputSymbolReplace(effect).Replace("\\n", "\n");
        }
    }
}
