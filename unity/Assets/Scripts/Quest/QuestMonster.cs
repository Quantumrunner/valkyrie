using Assets.Scripts.Content;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content.ContentData;
using Assets.Scripts.Content.QuestComponents;
using Assets.Scripts.Quest.Events;
using UnityEngine;

namespace Assets.Scripts.Quest
{
    // A monster Quest class that is defined by the Quest
    public class QuestMonster : MonsterData
    {
        public bool useMonsterTypeActivations = false;
        public CustomMonsterQuestComponent CMonsterQuestComponent;
        public string derivedType = "";

        // Construct with Quest data
        public QuestMonster(CustomMonsterQuestComponent qm) : base()
        {
            Game game = Game.Get();
            CMonsterQuestComponent = qm;

            // Get base derived monster type
            MonsterData baseObject = null;

            // Check for content data monster defined as base
            if (game.cd.monsters.ContainsKey(qm.baseMonster))
            {
                // Set base monster type
                derivedType = qm.baseMonster;
                baseObject = game.cd.monsters[qm.baseMonster];
            }

            // If name not set use base type
            if (!qm.monsterName.KeyExists() && baseObject != null)
            {
                name = baseObject.name;
            }
            else
            {
                name = qm.monsterName;
            }

            // Initialise sets
            sets = new List<string>();

            // define data
            sectionName = qm.sectionName;
            priority = 0;

            // Read traits from Quest data or base type
            traits = qm.traits;
            if (traits.Length == 0 && baseObject != null)
            {
                traits = baseObject.traits;
            }

            // Read info from Quest data or base type
            info = new StringKey(null, EventManager.OutputSymbolReplace(qm.info.Translate()), false);
            if (!qm.info.KeyExists() && baseObject != null)
            {
                info = baseObject.info;
            }

            image = qm.GetImagePath();
            if (image.Length == 0)
            {
                if (baseObject != null)
                {
                    image = baseObject.image;
                }
            }
            else
            {
                image = Path.GetDirectoryName(game.quest.qd.questPath) + Path.DirectorySeparatorChar + image;
            }

            // Read placement image from Quest data or base type
            imagePlace = qm.GetImagePlacePath();
            if (imagePlace.Length == 0)
            {
                if (baseObject != null)
                {
                    imagePlace = baseObject.image;
                }
                else
                {
                    imagePlace = image;
                }
            }
            else
            {
                imagePlace = Path.GetDirectoryName(game.quest.qd.questPath) + Path.DirectorySeparatorChar + imagePlace;
            }

            // Read activations  from Quest data or base type
            activations = qm.activations;
            if (activations.Length == 0 && baseObject != null)
            {
                useMonsterTypeActivations = true;
            }

            healthBase = qm.healthBase;
            healthPerHero = qm.healthPerHero;
            if (!qm.healthDefined && baseObject != null)
            {
                healthBase = baseObject.healthBase;
                healthPerHero = baseObject.healthPerHero;
            }

            horror = qm.horror;
            if (!qm.horrorDefined && baseObject != null)
            {
                horror = baseObject.horror;
            }

            awareness = qm.awareness;
            if (!qm.awarenessDefined && baseObject != null)
            {
                awareness = baseObject.awareness;
            }
        }

        override public StringKey GetRandomAttack(string type)
        {
            if (!CMonsterQuestComponent.investigatorAttacks.ContainsKey(type))
            {
                return base.GetRandomAttack(type);
            }

            List<StringKey> attackOptions = CMonsterQuestComponent.investigatorAttacks[type];
            return attackOptions[Random.Range(0, attackOptions.Count)];
        }
    }
}