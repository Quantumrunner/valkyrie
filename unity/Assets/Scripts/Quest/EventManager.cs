using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI.Screens;
using ValkyrieTools;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Assets.Scripts.Content.QuestComponent;

// Class for managing Quest events
public class EventManager
{
    // A dictionary of available events
    public Dictionary<string, Event> events;

    // events should display monster image when not null
    public Quest.Monster monsterImage;
    // events should display monster health if true
    public bool monsterHealth = false;

    // Stack of events to be triggered
    public Stack<Event> eventStack;

    public Game game;

    // Event currently open
    public Event currentEvent;

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

        events = new Dictionary<string, Event>();
        eventStack = new Stack<Event>();

        // Find Quest events
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is Assets.Scripts.Content.QuestComponent.Event)
            {
                // If the event is a monster type cast it
                if (kv.Value is Spawn)
                {
                    events.Add(kv.Key, new MonsterEvent(kv.Key));
                }
                else
                {
                    events.Add(kv.Key, new Event(kv.Key));
                }
            }
        }

        // Add game content perils as available events
        foreach (KeyValuePair<string, PerilData> kv in game.cd.perils)
        {
            events.Add(kv.Key, new Peril(kv.Key));
        }

        if (data != null)
        {
            if (data.ContainsKey("queue"))
            {
                foreach (string s in data["queue"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                {
                    eventStack.Push(events[s]);
                }
            }
            if (data.ContainsKey("monsterimage"))
            {
                monsterImage = Quest.Monster.GetMonster(data["monsterimage"]);
            }
            if (data.ContainsKey("monsterhealth"))
            {
                bool.TryParse(data["monsterhealth"], out monsterHealth);
            }
            if (data.ContainsKey("currentevent") && game.quest.activeShop != data["currentevent"])
            {
                currentEvent = events[data["currentevent"]];
                ResumeEvent();
            }
        }
    }

    // Queue all events by trigger, optionally start
    public void EventTriggerType(string type, bool trigger=true)
    {
        foreach (KeyValuePair<string, Event> kv in events)
        {
            if (kv.Value.QEvent != null && kv.Value.QEvent.trigger.Equals(type))
            {
                QueueEvent(kv.Key, trigger);
            }
        }
    }

    // Queue event, optionally trigger next event
    public void QueueEvent(string name, bool trigger=true)
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
                events.Add(name, new StartQuestEvent(name));
            }
            else
            {
                ValkyrieDebug.Log("Warning: Missing event called: " + name);
                game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + name, true));
                return;
            }
        }

        // Don't queue disabled events
        if (events[name].Disabled()) return;

        // Place this on top of the stack
        eventStack.Push(events[name]);

        // IF there is a current event trigger if specified
        if (currentEvent == null && trigger)
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
        Event e = eventStack.Pop();
        currentEvent = e;

        // Move to another Quest
        if (e is StartQuestEvent)
        {
            // This loads the game
            game.quest.ChangeQuest((e as StartQuestEvent).name);
            return;
        }

        // Event may have been disabled since added
        if (e.Disabled())
        {
            currentEvent = null;
            TriggerEvent();
            return;
        }

        // Play audio
        if (game.cd.audio.ContainsKey(e.QEvent.audio))
        {
            game.audioControl.Play(game.cd.audio[e.QEvent.audio].file);
        }
        else if (e.QEvent.audio.Length > 0)
        {
            game.audioControl.Play(Quest.FindLocalisedMultimediaFile(e.QEvent.audio, Path.GetDirectoryName(game.quest.qd.questPath)));
        }

        // Set Music
        if (e.QEvent.music.Count > 0)
        {
            List<string> music = new List<string>();
            foreach (string s in e.QEvent.music)
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
                game.quest.music = new List<string>(e.QEvent.music);
            }
        }

        // Perform var operations
        game.quest.vars.Perform(e.QEvent.operations);
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
        if (e is MonsterEvent)
        {
            MonsterEvent qe = (MonsterEvent)e;

            qe.MonsterEventSelection();

            // Is this type new?
            Quest.Monster oldMonster = null;
            foreach (Quest.Monster m in game.quest.monsters)
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
                game.quest.monsters.Add(new Quest.Monster(qe));
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
                oldMonster.healthMod = Mathf.RoundToInt(qe.qMonster.uniqueHealthBase + (Game.Get().quest.GetHeroCount() * qe.qMonster.uniqueHealthHero));
            }

            // Display the location(s)
            if (qe.QEvent.locationSpecified && e.QEvent.display)
            {
                game.tokenBoard.AddMonster(qe);
            }
        }

        // Highlight a space on the board
        if (e.QEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.QEvent);
        }

        // Is this a shop?
        List<string> itemList = new List<string>();
        foreach (string s in e.QEvent.addComponents)
        {
            if (s.IndexOf("QItem") == 0)
            {
                // Fix #998
                if (game.gameType.TypeName() == "MoM" && itemList.Count==1)
                {
                    ValkyrieDebug.Log("WARNING: only one QItem can be used in event " + e.QEvent.sectionName + ", ignoring other items");
                    break;
                }
                itemList.Add(s);
            }
        }
        // Add board components
        game.quest.Add(e.QEvent.addComponents, itemList.Count > 1);
        // Remove board components
        game.quest.Remove(e.QEvent.removeComponents);

        // Move camera
        if (e.QEvent.locationSpecified && !(e.QEvent is Ui))
        {
            CameraController.SetCamera(e.QEvent.location);
        }

        if (e.QEvent is Assets.Scripts.Content.QuestComponent.Puzzle)
        {
            Assets.Scripts.Content.QuestComponent.Puzzle p = e.QEvent as Assets.Scripts.Content.QuestComponent.Puzzle;
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
        if (e.QEvent.minCam)
        {
            CameraController.SetCameraMin(e.QEvent.location);
        }
        if (e.QEvent.maxCam)
        {
            CameraController.SetCameraMax(e.QEvent.location);
        }

        // Is this a shop?
        if (itemList.Count > 1 && !game.quest.boardItems.ContainsKey("#shop"))
        {
            game.quest.boardItems.Add("#shop", new ShopInterface(itemList, Game.Get(), e.QEvent.sectionName));
            game.quest.ordered_boardItems.Add("#shop");
        }
        else if (!e.QEvent.display)
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
        Event e = currentEvent;
        if (e is MonsterEvent)
        {
            // Display the location(s)
            if (e.QEvent.locationSpecified && e.QEvent.display)
            {
                game.tokenBoard.AddMonster(e as MonsterEvent);
            }
        }

        // Highlight a space on the board
        if (e.QEvent.highlight)
        {
            game.tokenBoard.AddHighlight(e.QEvent);
        }

        if (e.QEvent is Assets.Scripts.Content.QuestComponent.Puzzle)
        {
            Assets.Scripts.Content.QuestComponent.Puzzle p = e.QEvent as Assets.Scripts.Content.QuestComponent.Puzzle;
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
        EndEvent(currentEvent.QEvent, state);
    }

    // Event ended
    public void EndEvent(Assets.Scripts.Content.QuestComponent.Event eventData, int state=0)
    {
        // Get list of next events
        List<string> eventList = new List<string>();
        if (eventData.nextEvent.Count > state)
        {
            eventList = eventData.nextEvent[state];
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
                    events.Add(s, new StartQuestEvent(s));
                    enabledEvents.Add(s);
                }
                else
                {
                    ValkyrieDebug.Log("Warning: Missing event called: " + s);
                    game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + s, true));
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

            if( Path.GetFileName(game.quest.originalPath).StartsWith("EditorScenario") 
             || !Path.GetFileName(game.quest.originalPath).EndsWith(".valkyrie") )
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

        currentEvent = null;
        // Are there any events?
        if (enabledEvents.Count > 0)
        {
            // Are we picking at random?
            if (eventData.randomEvents)
            {
                // Add a random event
                game.quest.eManager.QueueEvent(enabledEvents[UnityEngine.Random.Range(0, enabledEvents.Count)], false);
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
            if (game.quest.phase == Quest.MoMPhase.monsters)
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

    // Event control class
    public class Event
    {
        public Game game;
        public Assets.Scripts.Content.QuestComponent.Event QEvent;
        public bool cancelable;

        // Create event from Quest data
        public Event(string name)
        {
            game = Game.Get();
            if (game.quest.qd.components.ContainsKey(name))
            {
                QEvent = game.quest.qd.components[name] as Assets.Scripts.Content.QuestComponent.Event;
            }
        }

        // Get the text to display for the event
        virtual public string GetText()
        {
            string text = QEvent.text.Translate(true);

            // Find and replace {q:element with the name of the
            // element

            text = ReplaceComponentText(text);

            // Find and replace rnd:hero with a hero
            // replaces all occurances with the one hero

            Quest.Hero h = game.quest.GetRandomHero();
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
            return OutputSymbolReplace(text).Replace("\\n", "\n");
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
            switch (component.GetType().Name)
            {
                case "Event":
                    if(!game.quest.heroSelection.ContainsKey(component.sectionName) || game.quest.heroSelection[component.sectionName].Count == 0)
                    {
                        return component.sectionName;
                    }
                    return game.quest.heroSelection[component.sectionName][0].heroData.name.Translate();
                case "Tile":
                    // Replaced with the name of the Tile
                    return game.cd.tileSides[((Tile)component).tileSideName].name.Translate();
                case "CustomMonster":
                    // Replaced with the custom nonster name
                    return ((CustomMonster)component).monsterName.Translate();
                case "Spawn":
                    if (!game.quest.monsterSelect.ContainsKey(component.sectionName))
                    {
                        return component.sectionName;
                    }
                    // Replaced with the text shown in the spawn
                    string monsterName = game.quest.monsterSelect[component.sectionName];
                    if (monsterName.StartsWith("Custom")) {
                        return ((CustomMonster)game.quest.qd.components[monsterName]).monsterName.Translate();
                    } else {
                        return game.cd.monsters[game.quest.monsterSelect[component.sectionName]].name.Translate();
                    }
                case "QItem":
                    if (!game.quest.itemSelect.ContainsKey(component.sectionName))
                    {
                        return component.sectionName;
                    }
                    // Replaced with the first element in the list
                    return game.cd.items[game.quest.itemSelect[component.sectionName]].name.Translate();
                default:
                    return component.sectionName;
            }
        }

        public static explicit operator Event(QuestComponent v)
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

            for (int i = 0; i < QEvent.buttons.Count; i++)
            {
                buttons.Add(new DialogWindow.EventButton(QEvent.buttons[i], QEvent.buttonColors[i]));
            }
            return buttons;
        }

        // Is the confirm button present?
        public bool ButtonsPresent()
        {
            // If the event can't be canceled it must have buttons
            if (!QEvent.cancelable) return true;
            // Check if any of the next events are enabled
            foreach (List<string> l in QEvent.nextEvent)
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
                            game.quest.eManager.events.Add(s, new StartQuestEvent(s));
                            return true;
                        }
                        else
                        {
                            ValkyrieDebug.Log("Warning: Missing event called: " + s);
                            game.quest.log.Add(new Quest.LogEntry("Warning: Missing event called: " + s, true));
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
                ValkyrieDebug.Log("Event test " + QEvent.sectionName + " result is : " + game.quest.vars.Test(QEvent.tests));

            // check if condition is valid, and if there is something to do in this event (see #916)
            return (!game.quest.vars.Test(QEvent.tests));
        }
    }

    public class StartQuestEvent : Event
    {
        public string name;

        public StartQuestEvent(string n) : base(n)
        {
            name = n;
        }

        override public bool Disabled()
        {
            return false;
        }
    }

    // Monster event extends event for adding monsters
    public class MonsterEvent : Event
    {
        public Spawn qMonster;
        public MonsterData cMonster;

        public MonsterEvent(string name) : base(name)
        {
            // cast the monster event
            qMonster = QEvent as Spawn;

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
                cMonster = new QuestMonster(game.quest.qd.components[t] as CustomMonster);
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
            return new StringKey(qMonster.uniqueTitle,"{type}",cMonster.name.fullKey);
        }
    }

    // Peril extends event
    public class Peril : Event
    {
        public PerilData cPeril;

        public Peril(string name) : base(name)
        {
            // Event is pulled from content data not Quest data
            QEvent = game.cd.perils[name] as Assets.Scripts.Content.QuestComponent.Event;
            cPeril = QEvent as PerilData;
        }
    }


    public override string ToString()
    {
        //Game game = Game.Get();
        string nl = System.Environment.NewLine;
        // General Quest state block
        string r = "[EventManager]" + nl;
        r += "queue=";
        foreach (Event e in eventStack.ToArray())
        {
            r += e.QEvent.sectionName + " ";
        }
        r += nl;
        if (currentEvent != null)
        {
            r += "currentevent=" + currentEvent.QEvent.sectionName + nl;
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
                output = output.Replace(statement, game.quest.vars.GetValue(statement.Substring(5, statement.Length - 6)).ToString());
                //find next random tag
                index = output.IndexOf("{var:");
            }
        }
        catch (System.Exception)
        {
            game.quest.log.Add(new Quest.LogEntry("Warning: Invalid var clause in text: " + input, true));
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
        Dictionary<string, string> toReturn = new Dictionary<string, string>(CHARS_MAP[Game.Get().gameType.TypeName()]);
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

    public static Dictionary<string, Dictionary<string, string>> CHARS_MAP = new Dictionary<string, Dictionary<string, string>>
    {
        { "D2E", new Dictionary<string,string>()
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
        { "MoM", new Dictionary<string,string>()
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
        { "IA", new Dictionary<string,string>()
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

    public static Dictionary<string, Dictionary<string, string>> CHAR_PACKS_MAP = new Dictionary<string, Dictionary<string, string>>
    {
        { "D2E", new Dictionary<string,string>()
        },
        { "MoM", new Dictionary<string,string>()
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
        { "IA", new Dictionary<string,string>()
            {
                {"{SWI01}", ""},
            }
        }
    };
}

