using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using ValkyrieTools;

namespace Assets.Scripts.Quest.Events
{
    // Event control class
    public class ValkyrieEvent
    {
        public Game game;
        public Assets.Scripts.Content.QuestComponent.EventQuestComponent QEventQuestComponent;
        public bool cancelable;

        // Create event from Quest data
        public ValkyrieEvent(string name)
        {
            game = Game.Get();
            if (game.quest.qd.components.ContainsKey(name))
            {
                QEventQuestComponent = game.quest.qd.components[name] as Assets.Scripts.Content.QuestComponent.EventQuestComponent;
            }
        }

        // Get the text to display for the event
        virtual public string GetText()
        {
            string text = QEventQuestComponent.text.Translate(true);

            // Find and replace {q:element with the name of the
            // element

            text = ReplaceComponentText(text);

            // Find and replace rnd:hero with a hero
            // replaces all occurances with the one hero

            Hero h = game.quest.GetRandomHero();
            if (text.Contains("{rnd:hero"))
            {
                h.selected = true;
            }
            text = text.Replace("{rnd:hero}", h.heroData.name.Translate());

            // Random heroes can have custom lookups
            if (text.StartsWith("{rnd:hero:"))
            {
                HeroData hero = h.heroData;
                int start = "{rnd:hero:".Length;
                if (!hero.ContainsTrait("male"))
                {
                    if (text[start] == '{')
                    {
                        start = text.IndexOf("}", start);
                    }
                    start = text.IndexOf(":{", start) + 1;
                    if (text[start] == '{')
                    {
                        start = text.IndexOf("}", start);
                    }
                    start = text.IndexOf(":", start) + 1;
                }
                int next = start;
                if (text[next] == '{')
                {
                    next = text.IndexOf("}", next);
                }
                next = text.IndexOf(":{", next) + 1;
                int end = next;
                if (text[end] == '{')
                {
                    end = text.IndexOf("}", end);
                }
                end = text.IndexOf(":", end);
                if (end < 0) end = text.Length - 1;
                string toReplace = text.Substring(next, end - next);
                text = new StringKey(text.Substring(start, (next - start) - 1)).Translate();
                text = text.Replace(toReplace, hero.name.Translate());
            }

            // Fix new lines and replace symbol text with special characters
            return EventManager.OutputSymbolReplace(text).Replace("\\n", "\n");
        }

        public static string ReplaceComponentText(string input)
        {
            string toReturn = input;
            if (toReturn.Contains("{c:"))
            {
                Regex questItemRegex = new Regex("{c:(((?!{).)*?)}");
                string replaceFrom;
                string componentName;
                string componentText;
                foreach (Match oneVar in questItemRegex.Matches(toReturn))
                {
                    replaceFrom = oneVar.Value;
                    componentName = oneVar.Groups[1].Value;
                    QuestComponent component;
                    if (Game.Get().quest.qd.components.TryGetValue(componentName, out component))
                    {
                        componentText = getComponentText(component);
                        toReturn = toReturn.Replace(replaceFrom, componentText);
                    }
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Get text related with selected component
        /// </summary>
        /// <param name="component">component to get text</param>
        /// <returns>extracted text</returns>
        public static string getComponentText(QuestComponent component)
        {
            Game game = Game.Get();
            if (component is TileQuestComponent)
            {
                // Replaced with the name of the Tile
                return game.cd.tileSides[((TileQuestComponent) component).tileSideName].name.Translate();
            }
            else if (component is CustomMonsterQuestComponent)
            {
                // Replaced with the custom nonster name
                return ((CustomMonsterQuestComponent) component).monsterName.Translate();
            }
            else if (component is SpawnQuestComponent)
            {
                if (!game.quest.monsterSelect.ContainsKey(component.sectionName))
                {
                    return component.sectionName;
                }

                // Replaced with the text shown in the spawn
                string monsterName = game.quest.monsterSelect[component.sectionName];
                if (monsterName.StartsWith("Custom"))
                {
                    return ((CustomMonsterQuestComponent) game.quest.qd.components[monsterName]).monsterName
                        .Translate();
                }
                else
                {
                    return game.cd.monsters[game.quest.monsterSelect[component.sectionName]].name.Translate();
                }
            }
            else if (component is EventQuestComponent)
            {
                if (!game.quest.heroSelection.ContainsKey(component.sectionName) ||
                    game.quest.heroSelection[component.sectionName].Count == 0)
                {
                    return component.sectionName;
                }

                return game.quest.heroSelection[component.sectionName][0].heroData.name.Translate();
            }
            else if (component is QItemQuestComponent)
            {
                if (!game.quest.itemSelect.ContainsKey(component.sectionName))
                {
                    return component.sectionName;
                }

                // Replaced with the first element in the list
                return game.cd.items[game.quest.itemSelect[component.sectionName]].name.Translate();
            }
            else
            {
                return component.sectionName;
            }
        }

        public static explicit operator ValkyrieEvent(QuestComponent v)
        {
            throw new NotImplementedException();
        }

        public List<DialogWindow.EventButton> GetButtons()
        {
            List<DialogWindow.EventButton> buttons = new List<DialogWindow.EventButton>();

            // Determine if no buttons should be displayed
            if (!ButtonsPresent())
            {
                return buttons;
            }

            for (int i = 0; i < QEventQuestComponent.buttons.Count; i++)
            {
                buttons.Add(new DialogWindow.EventButton(QEventQuestComponent.buttons[i], QEventQuestComponent.buttonColors[i]));
            }
            return buttons;
        }

        // Is the confirm button present?
        public bool ButtonsPresent()
        {
            // If the event can't be canceled it must have buttons
            if (!QEventQuestComponent.cancelable) return true;
            // Check if any of the next events are enabled
            foreach (List<string> l in QEventQuestComponent.nextEvent)
            {
                foreach (string s in l)
                {
                    // Check if the event doesn't exists - Quest fault
                    if (!game.quest.eManager.events.ContainsKey(s))
                    {
                        string questToTransition = game.quest.originalPath + Path.DirectorySeparatorChar + s;
                        if (game.quest.fromSavegame)
                        {
                            questToTransition = ContentData.ValkyrieLoadQuestPath + Path.DirectorySeparatorChar + s;
                        }
                        if (File.Exists(questToTransition))
                        {
                            game.quest.eManager.events.Add(s, new StartQuestValkyrieEvent(s));
                            return true;
                        }
                        else
                        {
                            ValkyrieDebug.Log("Warning: Missing event called: " + s);
                            game.quest.log.Add(new LogEntry("Warning: Missing event called: " + s, true));
                            return false;
                        }
                    }
                    if (!game.quest.eManager.events[s].Disabled()) return true;
                }
            }
            // Nothing valid, no buttons
            return false;
        }

        // Is this event disabled?
        virtual public bool Disabled()
        {
            if (game.debugTests)
                ValkyrieDebug.Log("Event test " + QEventQuestComponent.sectionName + " result is : " + game.quest.vars.Test(QEventQuestComponent.tests));

            // check if condition is valid, and if there is something to do in this event (see #916)
            return (!game.quest.vars.Test(QEventQuestComponent.tests));
        }
    }
}
