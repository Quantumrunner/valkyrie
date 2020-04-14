using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using ValkyrieTools;

namespace Assets.Scripts.Quest.Events
{
    // Monster event extends event for adding monsters
    public class MonsterValkyrieEvent : ValkyrieEvent
    {
        public SpawnQuestComponent qMonster;
        public MonsterData cMonster;

        public MonsterValkyrieEvent(string name) : base(name)
        {
            // cast the monster event
            qMonster = QEventQuestComponent as SpawnQuestComponent;

            // monsters are generated on the fly to avoid duplicate for D2E when using random
        }

        public void MonsterEventSelection()
        {
            if (!game.quest.RuntimeMonsterSelection(qMonster.sectionName))
            {
                ValkyrieDebug.Log("Warning: Monster type unknown in event: " + qMonster.sectionName);
                return;
            }
            string t = game.quest.monsterSelect[qMonster.sectionName];
            if (game.quest.qd.components.ContainsKey(t))
            {
                cMonster = new QuestMonster(game.quest.qd.components[t] as CustomMonsterQuestComponent);
            }
            else
            {
                cMonster = game.cd.monsters[t];
            }
        }

        // Event text
        override public string GetText()
        {
            // Monster events have {type} replaced with the selected type
            return base.GetText().Replace("{type}", cMonster.name.Translate());
        }

        // Unique monsters can have a special name
        public StringKey GetUniqueTitle()
        {
            // Default to Master {type}
            if (qMonster.uniqueTitle.KeyExists())
            {
                return new StringKey("val", "MONSTER_MASTER_X", cMonster.name);
            }
            return new StringKey(qMonster.uniqueTitle, "{type}", cMonster.name.fullKey);
        }
    }
}
