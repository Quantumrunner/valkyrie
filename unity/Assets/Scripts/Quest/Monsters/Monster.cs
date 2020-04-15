using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.Content.ContentData;
using Assets.Scripts.Content.QuestComponents;
using Assets.Scripts.Quest.Events;
using UnityEngine;

namespace Assets.Scripts.Quest.Monsters
{
    // Class for holding current monster status
    public class Monster
    {
        // Content Data
        public MonsterData monsterData;

        // Spawn event name used later for defeated triggers
        public string spawnEventName;

        // What dulpicate number is the monster?
        public int duplicate = 0;

        // State
        public bool activated = false;
        public bool minionStarted = false;
        public bool masterStarted = false;
        // Accumulated damage
        public int damage = 0;

        // Quest specific info
        public bool unique = false;
        public StringKey uniqueText = StringKey.NULL;
        public StringKey uniqueTitle = StringKey.NULL;

        public int healthMod = 0;

        // Activation is reset each round so that master/minion use the same data and forcing doesn't re roll
        // Note that in RtL forcing activation WILL reroll the selected activation
        public ActivationInstance currentActivation;

        // Initialise from monster event
        // When an event adds a monster group this is called
        public Monster(MonsterValkyrieEvent monsterValkyrieEvent)
        {
            monsterData = monsterValkyrieEvent.cMonster;
            spawnEventName = monsterValkyrieEvent.qMonster.sectionName;
            unique = monsterValkyrieEvent.qMonster.unique;
            uniqueTitle = monsterValkyrieEvent.GetUniqueTitle();
            uniqueText = monsterValkyrieEvent.qMonster.uniqueText;
            healthMod = Mathf.RoundToInt(monsterValkyrieEvent.qMonster.uniqueHealthBase + (Game.Get().quest.GetHeroCount() * monsterValkyrieEvent.qMonster.uniqueHealthHero));

            Game game = Game.Get();
            HashSet<int> dupe = new HashSet<int>();
            foreach (Monster m in game.quest.monsters)
            {
                string active_monster = "";
                string new_monster = "";

                // also check for custom monster base type
                if (m.monsterData.sectionName.IndexOf("CustomMonster") == 0)
                    active_monster = ((m.monsterData) as QuestMonster).derivedType;
                else
                    active_monster = m.monsterData.sectionName;

                if (monsterData.sectionName.IndexOf("CustomMonster") == 0)
                    new_monster = ((monsterData) as QuestMonster).derivedType;
                else
                    new_monster = monsterData.sectionName;

                if (active_monster == new_monster || m.duplicate != 0)
                {
                    dupe.Add(m.duplicate);
                }
            }

            while (dupe.Contains(duplicate))
            {
                duplicate++;
            }
        }

        // Create new activation
        public void NewActivation(ActivationData contentActivation)
        {
            currentActivation = new ActivationInstance(contentActivation, monsterData.name.Translate());
        }

        // Construct from save data
        public Monster(Dictionary<string, string> data)
        {
            bool.TryParse(data["activated"], out activated);
            bool.TryParse(data["minionStarted"], out minionStarted);
            bool.TryParse(data["masterStarted"], out masterStarted);
            bool.TryParse(data["unique"], out unique);
            int.TryParse(data["damage"], out damage);
            int.TryParse(data["duplicate"], out duplicate);

            if (data.ContainsKey("healthmod"))
            {
                int.TryParse(data["healthmod"], out healthMod);
            }

            uniqueText = new StringKey(data["uniqueText"]);
            uniqueTitle = new StringKey(data["uniqueTitle"]);

            // Support old saves (deprectiated)
            if (data["type"].IndexOf("UniqueMonster") == 0)
            {
                data["type"] = "CustomMonster" + data["type"].Substring("UniqueMonster".Length);
            }

            // Find base type
            Game game = Game.Get();
            if (game.cd.monsters.ContainsKey(data["type"]))
            {
                monsterData = game.cd.monsters[data["type"]];
            }
            // Check if type is a special Quest specific type
            if (game.quest.qd.components.ContainsKey(data["type"]) && game.quest.qd.components[data["type"]] is CustomMonsterQuestComponent)
            {
                monsterData = new QuestMonster(game.quest.qd.components[data["type"]] as CustomMonsterQuestComponent);
            }

            // If we have already selected an activation find it
            if (data.ContainsKey("activation"))
            {
                ActivationData saveActivation = null;
                if (game.cd.activations.ContainsKey(data["activation"]))
                {
                    saveActivation = game.cd.activations[data["activation"]];
                }
                // Activations can be specific to the Quest
                if (game.quest.qd.components.ContainsKey(data["activation"]))
                {
                    saveActivation = new QuestActivation(game.quest.qd.components[data["activation"]] as ActivationQuestComponent);
                }
                currentActivation = new ActivationInstance(saveActivation, monsterData.name.Translate());
            }
        }

        public string GetIdentifier()
        {
            return monsterData.sectionName + ":" + duplicate;
        }

        public static Monster GetMonster(string identifier)
        {
            Game game = Game.Get();
            string[] parts = identifier.Split(':');
            if (parts.Length != 2) return null;
            int d = 0;
            int.TryParse(parts[1], out d);
            foreach (Monster m in game.quest.monsters)
            {
                if (m.monsterData.sectionName.Equals(parts[0]) && m.duplicate == d)
                {
                    return m;
                }
            }
            return null;
        }

        public int GetHealth()
        {
            return Mathf.RoundToInt(monsterData.healthBase + (Game.Get().quest.GetHeroCount() * monsterData.healthPerHero)) + healthMod;
        }

        // Save monster data to string
        override public string ToString()
        {
            string nl = System.Environment.NewLine;

            // Section name must be unique
            string r = "[Monster" + monsterData.sectionName + duplicate + "]" + nl;
            r += "activated=" + activated + nl;
            r += "type=" + monsterData.sectionName + nl;
            r += "minionStarted=" + minionStarted + nl;
            r += "masterStarted=" + masterStarted + nl;
            r += "unique=" + unique + nl;
            r += "uniqueText=" + uniqueText + nl;
            r += "uniqueTitle=" + uniqueTitle + nl;
            r += "damage=" + damage + nl;
            r += "duplicate=" + duplicate + nl;
            r += "healthmod=" + healthMod + nl;
            // Save the activation (currently doesn't save the effect string)
            if (currentActivation != null)
            {
                r += "activation=" + currentActivation.ad.sectionName + nl;
            }
            return r;
        }
    }
}
