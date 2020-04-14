using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.GameTypes;
using Assets.Scripts.UI.Screens;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Quest.Events
{
    // Class for managing Quest events
    public class EventManager
    {
        // A dictionary of available events
        public Dictionary<string, ValkyrieEvent> events;

        // events should display monster image when not null
        public Monster monsterImage;

        // events should display monster health if true
        public bool monsterHealth = false;

        // Stack of events to be triggered
        public Stack<ValkyrieEvent> eventStack;

        public Game game;

        // Event currently open
        public ValkyrieEvent CurrentValkyrieEvent;

        public EventManager()
        {
            Init(null);
        }

        public EventManager(Dictionary<string, string> data)
        {
            Init(data);
        }

        public void Init(Dictionary<string, string> data)
        {
            game = Game.Get();

            // This is filled out later but is required for loading saves
            game.quest.eManager = this;

            events = new Dictionary<string, ValkyrieEvent>();
            eventStack = new Stack<ValkyrieEvent>();

            // Find Quest events
            foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
            {
                if (kv.Value is Assets.Scripts.Content.QuestComponent.EventQuestComponent)
                {
                    // If the event is a monster type cast it
                    if (kv.Value is SpawnQuestComponent)
                    {
                        events.Add(kv.Key, new MonsterValkyrieEvent(kv.Key));
                    }
                    else
                    {
                        events.Add(kv.Key, new ValkyrieEvent(kv.Key));
                    }
                }
            }

            // Add game content perils as available events
            foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
            {
                events.Add(kv.Key, new PerilValkyrieEvent(kv.Key));
            }

            if (data != null)
            {
                if (data.ContainsKey("queue"))
                {
                    foreach (string s in data["queue"]
                        .Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        eventStack.Push(events[s]);
                    }
                }

                if (data.ContainsKey("monsterimage"))
                {
                    monsterImage = Monster.GetMonster(data["monsterimage"]);
                }

                if (data.ContainsKey("monsterhealth"))
                {
                    bool.TryParse(data["monsterhealth"], out monsterHealth);
                }

                if (data.ContainsKey("currentevent") && game.quest.activeShop != data["currentevent"])
                {
                    CurrentValkyrieEvent = events[data["currentevent"]];
                    ResumeEvent();
                }
            }
        }

        // Queue all events by trigger, optionally start
        public void EventTriggerType(string type, bool trigger = true)
        {
            foreach (KeyValuePair<string, ValkyrieEvent> kv in events)
            {
                if (kv.Value.QEventQuestComponent != null && kv.Value.QEventQuestComponent.trigger.Equals(type))
                {
                    QueueEvent(kv.Key, trigger);
                }
            }
        }

        // Queue event, optionally trigger next event
        public void QueueEvent(string name, bool trigger = true)
        {
            // Check if the event doesn't exists - Quest fault
            if (!events.ContainsKey(name))
            {
                string questToTransition = game.quest.originalPath + Path.DirectorySeparatorChar + name;
                if (game.quest.fromSavegame)
                {
                    questToTransition = ContentData.ValkyrieLoadQuestPath + Path.DirectorySeparatorChar + name;
                }

                if (File.Exists(questToTransition))
                {
                    events.Add(name, new StartQuestValkyrieEvent(name));
                }
                else
                {
                    ValkyrieDebug.Log("Warning: Missing event called: " + name);
                    game.quest.log.Add(new LogEntry("Warning: Missing event called: " + name, true));
                    return;
                }
            }

            // Don't queue disabled events
            if (events[name].Disabled()) return;

            // Place this on top of the stack
            eventStack.Push(events[name]);

            // IF there is a current event trigger if specified
            if (CurrentValkyrieEvent == null && trigger)
            {
                TriggerEvent();
            }
        }

        // Trigger next event in stack
        public void TriggerEvent()
        {
            Game game = Game.Get();
            // First check if things need to be added to the queue at end round
            if (game.roundControl.CheckNewRound()) return;

            // No events to trigger
            if (eventStack.Count == 0) return;

            // Get the next event
            ValkyrieEvent e = eventStack.Pop();
            CurrentValkyrieEvent = e;

            // Move to another Quest
            if (e is StartQuestValkyrieEvent)
            {
                // This loads the game
                game.quest.ChangeQuest((e as StartQuestValkyrieEvent).name);
                return;
            }

            // Event may have been disabled since added
            if (e.Disabled())
            {
                CurrentValkyrieEvent = null;
                TriggerEvent();
                return;
            }

            // Play audio
            if (game.cd.audio.ContainsKey(e.QEventQuestComponent.audio))
            {
                game.audioControl.Play(game.cd.audio[e.QEventQuestComponent.audio].file);
            }
            else if (e.QEventQuestComponent.audio.Length > 0)
            {
                game.audioControl.Play(Quest.FindLocalisedMultimediaFile(e.QEventQuestComponent.audio,
                    Path.GetDirectoryName(game.quest.qd.questPath)));
            }

            // Set Music
            if (e.QEventQuestComponent.music.Count > 0)
            {
                List<string> music = new List<string>();
                foreach (string s in e.QEventQuestComponent.music)
                {
                    if (game.cd.audio.ContainsKey(s))
                    {
                        music.Add(game.cd.audio[s].file);
                    }
                    else
                    {
                        music.Add(Quest.FindLocalisedMultimediaFile(s, Path.GetDirectoryName(game.quest.qd.questPath)));
                    }
                }

                game.audioControl.PlayMusic(music);
                if (music.Count > 1)
                {
                    game.quest.music = new List<string>(e.QEventQuestComponent.music);
                }
            }

            // Perform var operations
            game.quest.vars.Perform(e.QEventQuestComponent.operations);
            // Update morale change
            if (game.gameType is D2EGameType)
            {
                game.quest.AdjustMorale(0);
            }

            if (game.quest.vars.GetValue("$restock") == 1)
            {
                game.quest.GenerateItemSelection();
            }

            // If a dialog window is open we force it closed (this shouldn't happen)
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
                UnityEngine.Object.Destroy(go);

            // If this is a monster event then add the monster group
            if (e is MonsterValkyrieEvent)
            {
                MonsterValkyrieEvent qe = (MonsterValkyrieEvent) e;

                qe.MonsterEventSelection();

                // Is this type new?
                Monster oldMonster = null;
                foreach (Monster m in game.quest.monsters)
                {
                    if (m.monsterData.sectionName.Equals(qe.cMonster.sectionName))
                    {
                        // Matched existing monster
                        oldMonster = m;
                    }
                }

                // Add the new type
                if (!game.gameType.MonstersGrouped() || oldMonster == null)
                {
                    game.quest.monsters.Add(new Monster(qe));
                    game.monsterCanvas.UpdateList();
                    // Update monster var
                    game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);
                }
                // There is an existing group, but now it is unique
                else if (qe.qMonster.unique)
                {
                    oldMonster.unique = true;
                    oldMonster.uniqueText = qe.qMonster.uniqueText;
                    oldMonster.uniqueTitle = qe.GetUniqueTitle();
                    oldMonster.healthMod =
                        Mathf.RoundToInt(qe.qMonster.uniqueHealthBase +
                                         (Game.Get().quest.GetHeroCount() * qe.qMonster.uniqueHealthHero));
                }

                // Display the location(s)
                if (qe.QEventQuestComponent.locationSpecified && e.QEventQuestComponent.display)
                {
                    game.tokenBoard.AddMonster(qe);
                }
            }

            // Highlight a space on the board
            if (e.QEventQuestComponent.highlight)
            {
                game.tokenBoard.AddHighlight(e.QEventQuestComponent);
            }

            // Is this a shop?
            List<string> itemList = new List<string>();
            foreach (string s in e.QEventQuestComponent.addComponents)
            {
                if (s.IndexOf("QItem") == 0)
                {
                    // Fix #998
                    if (game.gameType.TypeName() == "MoM" && itemList.Count == 1)
                    {
                        ValkyrieDebug.Log("WARNING: only one QItem can be used in event " +
                                          e.QEventQuestComponent.sectionName + ", ignoring other items");
                        break;
                    }

                    itemList.Add(s);
                }
            }

            // Add board components
            game.quest.Add(e.QEventQuestComponent.addComponents, itemList.Count > 1);
            // Remove board components
            game.quest.Remove(e.QEventQuestComponent.removeComponents);

            // Move camera
            if (e.QEventQuestComponent.locationSpecified && !(e.QEventQuestComponent is UiQuestComponent))
            {
                CameraController.SetCamera(e.QEventQuestComponent.location);
            }

            if (e.QEventQuestComponent is Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent)
            {
                Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent p =
                    e.QEventQuestComponent as Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent;
                if (p.puzzleClass.Equals("slide"))
                {
                    new PuzzleSlideWindow(e);
                }

                if (p.puzzleClass.Equals("code"))
                {
                    new PuzzleCodeWindow(e);
                }

                if (p.puzzleClass.Equals("image"))
                {
                    new PuzzleImageWindow(e);
                }

                if (p.puzzleClass.Equals("tower"))
                {
                    new PuzzleTowerWindow(e);
                }

                return;
            }

            // Set camera limits
            if (e.QEventQuestComponent.minCam)
            {
                CameraController.SetCameraMin(e.QEventQuestComponent.location);
            }

            if (e.QEventQuestComponent.maxCam)
            {
                CameraController.SetCameraMax(e.QEventQuestComponent.location);
            }

            // Is this a shop?
            if (itemList.Count > 1 && !game.quest.boardItems.ContainsKey("#shop"))
            {
                game.quest.boardItems.Add("#shop",
                    new ShopInterface(itemList, Game.Get(), e.QEventQuestComponent.sectionName));
                game.quest.ordered_boardItems.Add("#shop");
            }
            else if (!e.QEventQuestComponent.display)
            {
                // Only raise dialog if there is text, otherwise auto confirm
                EndEvent();
            }
            else
            {
                if (monsterImage != null)
                {
                    MonsterDialogMoM.DrawMonster(monsterImage, true);
                    if (monsterHealth)
                    {
                    }
                }

                new DialogWindow(e);
            }
        }

        public void ResumeEvent()
        {
            ValkyrieEvent e = CurrentValkyrieEvent;
            if (e is MonsterValkyrieEvent)
            {
                // Display the location(s)
                if (e.QEventQuestComponent.locationSpecified && e.QEventQuestComponent.display)
                {
                    game.tokenBoard.AddMonster(e as MonsterValkyrieEvent);
                }
            }

            // Highlight a space on the board
            if (e.QEventQuestComponent.highlight)
            {
                game.tokenBoard.AddHighlight(e.QEventQuestComponent);
            }

            if (e.QEventQuestComponent is Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent)
            {
                Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent p =
                    e.QEventQuestComponent as Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent;
                if (p.puzzleClass.Equals("slide"))
                {
                    new PuzzleSlideWindow(e);
                }

                if (p.puzzleClass.Equals("code"))
                {
                    new PuzzleCodeWindow(e);
                }

                if (p.puzzleClass.Equals("image"))
                {
                    new PuzzleImageWindow(e);
                }

                return;
            }

            new DialogWindow(e);
        }

        // Event ended
        public void EndEvent(int state = 0)
        {
            EndEvent(CurrentValkyrieEvent.QEventQuestComponent, state);
        }

        // Event ended
        public void EndEvent(Assets.Scripts.Content.QuestComponent.EventQuestComponent eventQuestComponentData,
            int state = 0)
        {
            // Get list of next events
            List<string> eventList = new List<string>();
            if (eventQuestComponentData.nextEvent.Count > state)
            {
                eventList = eventQuestComponentData.nextEvent[state];
            }

            // Only take enabled events from list
            List<string> enabledEvents = new List<string>();
            foreach (string s in eventList)
            {
                // Check if the event doesn't exists - Quest fault
                if (!events.ContainsKey(s))
                {
                    string questToTransition = game.quest.originalPath + Path.DirectorySeparatorChar + s;
                    if (game.quest.fromSavegame)
                    {
                        questToTransition = ContentData.ValkyrieLoadQuestPath + Path.DirectorySeparatorChar + s;
                    }

                    if (File.Exists(questToTransition))
                    {
                        events.Add(s, new StartQuestValkyrieEvent(s));
                        enabledEvents.Add(s);
                    }
                    else
                    {
                        ValkyrieDebug.Log("Warning: Missing event called: " + s);
                        game.quest.log.Add(new LogEntry("Warning: Missing event called: " + s, true));
                    }
                }
                else if (!game.quest.eManager.events[s].Disabled())
                {
                    enabledEvents.Add(s);
                }
            }

            // Has the Quest ended?
            if (game.quest.vars.GetValue("$end") != 0)
            {
                game.quest.questHasEnded = true;

                if (Path.GetFileName(game.quest.originalPath).StartsWith("EditorScenario")
                    || !Path.GetFileName(game.quest.originalPath).EndsWith(".valkyrie"))
                {
                    // do not show score screen for scenario with a non customized name, or if the scenario is not a package (most probably a test)
                    Destroyer.MainMenu();
                }
                else
                {
                    new EndGameScreen();
                }

                return;
            }

            CurrentValkyrieEvent = null;
            // Are there any events?
            if (enabledEvents.Count > 0)
            {
                // Are we picking at random?
                if (eventQuestComponentData.randomEvents)
                {
                    // Add a random event
                    game.quest.eManager.QueueEvent(enabledEvents[UnityEngine.Random.Range(0, enabledEvents.Count)],
                        false);
                }
                else
                {
                    // Add the first valid event
                    game.quest.eManager.QueueEvent(enabledEvents[0], false);
                }
            }

            // Add any custom triggered events
            AddCustomTriggers();

            if (eventStack.Count == 0)
            {
                monsterImage = null;
                monsterHealth = false;
                if (game.quest.phase == MoMPhase.monsters)
                {
                    game.roundControl.MonsterActivated();
                    return;
                }
            }

            // Trigger a stacked event
            TriggerEvent();
        }

        public void AddCustomTriggers()
        {
            foreach (KeyValuePair<string, float> kv in game.quest.vars.GetPrefixVars("@"))
            {
                if (kv.Value > 0)
                {
                    game.quest.vars.SetValue(kv.Key, 0);
                    EventTriggerType("Var" + kv.Key.Substring(1), false);
                }
            }

            foreach (KeyValuePair<string, float> kv in game.quest.vars.GetPrefixVars("$@"))
            {
                if (kv.Value > 0)
                {
                    game.quest.vars.SetValue(kv.Key, 0);
                    EventTriggerType("Var" + "$" + kv.Key.Substring(2), false);
                }
            }
        }








        public override string ToString()
        {
            //Game game = Game.Get();
            string nl = System.Environment.NewLine;
            // General Quest state block
            string r = "[EventManager]" + nl;
            r += "queue=";
            foreach (ValkyrieEvent e in eventStack.ToArray())
            {
                r += e.QEventQuestComponent.sectionName + " ";
            }

            r += nl;
            if (CurrentValkyrieEvent != null)
            {
                r += "currentevent=" + CurrentValkyrieEvent.QEventQuestComponent.sectionName + nl;
            }

            if (monsterImage != null)
            {
                r += "monsterimage=" + monsterImage.GetIdentifier() + nl;
            }

            if (monsterHealth)
            {
                r += "monsterhealth=" + monsterHealth.ToString() + nl;
            }

            return r;
        }

        /// <summary>
        /// Replace symbol markers with special characters to be shown in Quest
        /// </summary>
        /// <param name="input">text to show</param>
        /// <returns></returns>
        public static string OutputSymbolReplace(string input)
        {
            string output = input;
            Game game = Game.Get();

            // Fill in variable data
            try
            {
                // Find first random number tag
                int index = output.IndexOf("{var:");
                // loop through event text
                while (index != -1)
                {
                    // find end of tag
                    string statement = output.Substring(index, output.IndexOf("}", index) + 1 - index);
                    // Replace with variable data
                    output = output.Replace(statement,
                        game.quest.vars.GetValue(statement.Substring(5, statement.Length - 6)).ToString());
                    //find next random tag
                    index = output.IndexOf("{var:");
                }
            }
            catch (System.Exception)
            {
                game.quest.log.Add(new LogEntry("Warning: Invalid var clause in text: " + input, true));
            }

            foreach (var conversion in GetCharacterMap(false, true))
            {
                output = output.Replace(conversion.Key, conversion.Value);
            }

            return output;
        }

        /// <summary>
        /// Replace symbol markers with special characters to be stored in editor
        /// </summary>
        /// <param name="input">text to store</param>
        /// <returns></returns>
        public static string InputSymbolReplace(string input)
        {
            string output = input;

            foreach (var conversion in GetCharacterMap(false, true))
            {
                output = output.Replace(conversion.Value, conversion.Key);
            }

            return output;
        }

        public static Dictionary<string, string> GetCharacterMap(bool addRnd = false, bool addPacks = false)
        {
            if (!CHARS_MAP.ContainsKey(Game.Get().gameType.TypeName()))
            {
                return null;
            }

            Dictionary<string, string> toReturn =
                new Dictionary<string, string>(CHARS_MAP[Game.Get().gameType.TypeName()]);
            if (addRnd)
            {
                toReturn.Add("{rnd:hero}", "Rnd");
            }

            if (addPacks)
            {
                foreach (var entry in CHAR_PACKS_MAP[Game.Get().gameType.TypeName()])
                {
                    toReturn.Add(entry.Key, entry.Value);
                }
            }

            return toReturn;
        }

        public static Dictionary<string, Dictionary<string, string>> CHARS_MAP =
            new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "D2E", new Dictionary<string, string>()
                    {
                        {"{heart}", "≥"},
                        {"{fatigue}", "∏"},
                        {"{might}", "∂"},
                        {"{will}", "π"},
                        {"{action}", "∞"},
                        {"{knowledge}", "∑"},
                        {"{awareness}", "μ"},
                        {"{shield}", "≤"},
                        {"{surge}", "±"},
                    }
                },
                {
                    "MoM", new Dictionary<string, string>()
                    {
                        {"{will}", ""},
                        {"{action}", ""},
                        {"{strength}", ""},
                        {"{agility}", ""},
                        {"{lore}", ""},
                        {"{influence}", ""},
                        {"{observation}", ""},
                        {"{success}", ""},
                        {"{clue}", ""},
                    }
                },
                {
                    "IA", new Dictionary<string, string>()
                    {
                        {"{action}", ""},
                        {"{wound}", ""},
                        {"{surge}", ""},
                        {"{attack}", ""},
                        {"{strain}", ""},
                        {"{tech}", ""},
                        {"{insight}", ""},
                        {"{strength}", ""},
                        {"{block}", ""},
                        {"{evade}", ""},
                        {"{dodge}", ""},
                    }
                }
            };

        public static Dictionary<string, Dictionary<string, string>> CHAR_PACKS_MAP =
            new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "D2E", new Dictionary<string, string>()
                },
                {
                    "MoM", new Dictionary<string, string>()
                    {
                        {"{MAD01}", ""},
                        {"{MAD06}", ""},
                        {"{MAD09}", ""},
                        {"{MAD20}", ""},
                        {"{MAD21}", ""},
                        {"{MAD22}", ""},
                        {"{MAD23}", ""},
                        {"{MAD25}", ""},
                        {"{MAD26}", ""},
                        {"{MAD27}", ""},
                        {"{MAD28}", ""},
                    }
                },
                {
                    "IA", new Dictionary<string, string>()
                    {
                        {"{SWI01}", ""},
                    }
                }
            };
    }
}