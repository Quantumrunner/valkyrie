using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content;

namespace Assets.Scripts.Quest.Events
{
    // Peril extends event
    public class PerilValkyrieEvent : ValkyrieEvent
    {
        public PerilData cPeril;

        public PerilValkyrieEvent(string name) : base(name)
        {
            // Event is pulled from content data not Quest data
            QEventQuestComponent = game.cd.perils[name] as Assets.Scripts.Content.QuestComponent.EventQuestComponent;
            cPeril = QEventQuestComponent as PerilData;
        }
    }
}
