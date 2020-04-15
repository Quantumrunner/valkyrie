using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using Assets.Scripts.Content.QuestComponents;

namespace Assets.Scripts.Quest.Events
{
    // Peril extends event
    public class PerilValkyrieEvent : ValkyrieEvent
    {
        public PerilData cPeril;

        public PerilValkyrieEvent(string name) : base(name)
        {
            // Event is pulled from content data not Quest data
            QEventQuestComponent = game.cd.perils[name] as EventQuestComponent;
            cPeril = QEventQuestComponent as PerilData;
        }
    }
}
