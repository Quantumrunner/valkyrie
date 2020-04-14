using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using Assets.Scripts.Quest.BoardComponents;
using Assets.Scripts.Quest.Events;
using Assets.Scripts.Save;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Quest
{

    public enum MoMPhase
    {
        investigator,
        mythos,
        monsters,
        horror
    }

    // Class to manage all data for the current Quest
    public class Quest
    {
        /// <summary>
        /// QuestData
        /// </summary>
        public QuestData qd;

        /// <summary>
        /// Current Quest Path
        /// </summary>
        public string questPath = "";

        /// <summary>
        /// Original Quest Path (required for Quest within a Quest)
        /// </summary>
        public string originalPath = "";

        /// <summary>
        /// components on the board (tiles, tokens, doors)
        /// </summary>
        public Dictionary<string, BoardComponent> boardItems;

        /// <summary>
        /// list of components on the board (tiles, tokens, doors) in the right order for savegames
        ///  we need to use an ordered list of board item, to preserve items stacking order, Dictionary do not preserve order
        /// </summary>
        public List<string> ordered_boardItems;

        /// <summary>
        /// vars for the Quest 
        /// </summary>
        public VarManager vars;

        // A list of items that have been given to the investigators
        public HashSet<string> items;

        // Dictionary of shops and items
        public Dictionary<string, List<string>> shops;

        // The open shop
        public string activeShop = "";

        // Dictionary of picked items
        public Dictionary<string, string> itemSelect;

        // Dictionary item inspect events
        public Dictionary<string, string> itemInspect;

        // A dictionary of heros that have been selected in events
        public Dictionary<string, List<Hero>> heroSelection;

        // A dictionary of puzzle state
        public Dictionary<string, Puzzle> puzzle;

        // A count of successes from events
        public Dictionary<string, int> eventQuota;

        // Event manager handles the events
        public EventManager eManager;

        /// <summary>
        /// List of heros and their status
        /// </summary>
        public List<Hero> heroes;

        /// <summary>
        /// List of active monsters and their status
        /// </summary>
        public List<Monster> monsters;

        // Stack of saved game state for undo
        public Stack<string> undo;

        // Event Log
        public List<LogEntry> log;

        // Event list
        public List<string> eventList;

        // Dictionary of picked monster types
        public Dictionary<string, string> monsterSelect;

        // game state variables
        public MoMPhase phase = MoMPhase.investigator;

        // This is true when the Quest is finished
        public bool questHasEnded = false;

        // This is true once heros are selected and the Quest is started
        public bool heroesSelected = false;

        // This is true once the first tile has been displayed
        public bool firstTileDisplayed = false;

        // This is true once the first autoSave has been done
        public bool firstAutoSaveDone = false;

        // This is true if the game is loaded from a savegame
        public bool fromSavegame = false;

        // Quest start time (or load time)
        public System.DateTime start_time;

        // Quest gameplay duration
        public int duration;

        // Default music will be played when you start the Quest
        public bool defaultMusicOn;

        // A list of music if custom music has been selected - used for save games
        public List<string> music = new List<string>();

        // Reference back to the game object
        public Game game;

        /// <summary>
        /// Find and return audio or picture file for localization. Use only for scenarios.
        /// </summary>
        /// <param name="name">Path from root folder scenario to file and File Name</param>
        /// <param name="source">Path to root scenario folder</param>
        /// <returns>Find and return audio or picture file for localization, if it exists. Otherwise return default file</returns>
        /// <remarks> 
        /// param "name" should contain name and path from root folder scenario to file, as in *.ini. Real path ".../.../[ScenarioName]/.../FileName", "name" = ".../FileName".
        /// param "source" should contain path to scenario. ".../.../[ScenarioName]".
        /// </remarks>
        public static string FindLocalisedMultimediaFile(string name, string source)
        {
            if (!Game.game.editMode && File.Exists(Path.Combine(Path.Combine(source, Game.game.currentLang), name)))
            {
                return Path.Combine(Path.Combine(source, Game.game.currentLang), name);
            }
            return Path.Combine(source, name);
        }

        // Construct a new Quest from Quest data
        public Quest(Assets.Scripts.Content.QuestIniComponent q)
        {
            game = Game.Get();

            // This happens anyway but we need it to be here before the following code is executed
            game.quest = this;

            // Static data from the Quest file
            qd = new QuestData(q);
            questPath = Path.GetDirectoryName(qd.questPath);
            originalPath = Path.GetDirectoryName(qd.questPath);

            // Initialise data
            boardItems = new Dictionary<string, BoardComponent>();
            ordered_boardItems = new List<string>();
            vars = new VarManager();
            items = new HashSet<string>();
            shops = new Dictionary<string, List<string>>();
            itemSelect = new Dictionary<string, string>();
            itemInspect = new Dictionary<string, string>();
            monsters = new List<Monster>();
            heroSelection = new Dictionary<string, List<Hero>>();
            puzzle = new Dictionary<string, Puzzle>();
            eventQuota = new Dictionary<string, int>();
            undo = new Stack<string>();
            log = new List<LogEntry>();
            eventList = new List<string>();
            monsterSelect = new Dictionary<string, string>();

            start_time = System.DateTime.UtcNow;
            duration = 0;
            defaultMusicOn = q.defaultMusicOn;

            GenerateItemSelection();
            eManager = new EventManager();

            // Populate null hero list, these can then be selected as hero types
            heroes = new List<Hero>();
            for (int i = 1; i <= qd.QuestIniComponent.maxHero; i++)
            {
                heroes.Add(new Hero(null, i));
            }

            // Set Quest vars for selected expansions
            foreach (string s in game.cd.GetLoadedPackIDs())
            {
                if (s.Length > 0)
                {
                    vars.SetValue("#" + s, 1);
                }
            }
            // Depreciated support for Quest formats < 6
            if (game.cd.GetLoadedPackIDs().Contains("MoM1ET") && game.cd.GetLoadedPackIDs().Contains("MoM1EI") && game.cd.GetLoadedPackIDs().Contains("MoM1EM"))
            {
                vars.SetValue("#MoM1E", 1);
            }
            if (game.cd.GetLoadedPackIDs().Contains("CotWT") && game.cd.GetLoadedPackIDs().Contains("CotWI") && game.cd.GetLoadedPackIDs().Contains("CotWM"))
            {
                vars.SetValue("#CotW", 1);
            }
            if (game.cd.GetLoadedPackIDs().Contains("FAT") && game.cd.GetLoadedPackIDs().Contains("FAI") && game.cd.GetLoadedPackIDs().Contains("FAM"))
            {
                vars.SetValue("#FA", 1);
            }

            vars.SetValue("#round", 1);
        }

        public void GenerateItemSelection()
        {
            // Clear shops
            shops = new Dictionary<string, List<string>>();

            // Clear item matches
            itemSelect = new Dictionary<string, string>();

            // Determine fame level
            int fame = 1;
            if (vars.GetValue("$%fame") >= vars.GetValue("$%famenoteworthy")) fame = 2;
            if (vars.GetValue("$%fame") >= vars.GetValue("$%fameimpressive")) fame = 3;
            if (vars.GetValue("$%fame") >= vars.GetValue("$%famecelebrated")) fame = 4;
            if (vars.GetValue("$%fame") >= vars.GetValue("$%fameheroic")) fame = 5;
            if (vars.GetValue("$%fame") >= vars.GetValue("$%famelegendary")) fame = 6;

            // Determine monster types
            bool progress = false;
            bool force = false;
            bool done = false;
            while (!done)
            {
                progress = false;
                foreach (KeyValuePair<string, QuestComponent> kv in qd.components)
                {
                    QItemQuestComponent qItemQuestComponent = kv.Value as QItemQuestComponent;
                    if (qItemQuestComponent != null)
                    {
                        progress |= AttemptItemMatch(qItemQuestComponent, fame, force);
                        if (progress && force) force = false;
                    }
                }
                if (!progress && !force)
                {
                    force = true;
                }
                else if (!progress && force)
                {
                    done = true;
                }
            }
            vars.SetValue("$restock", 0);
        }


        public bool AttemptItemMatch(QItemQuestComponent qItemQuestComponent, int fame, bool force = true)
        {
            if (itemSelect.ContainsKey(qItemQuestComponent.sectionName))
            {
                return false;
            }
            if ((qItemQuestComponent.traitpool.Length + qItemQuestComponent.traits.Length) == 0)
            {
                foreach (string t in qItemQuestComponent.itemName)
                {
                    if (itemSelect.ContainsKey(t))
                    {
                        itemSelect.Add(qItemQuestComponent.sectionName, itemSelect[t]);
                        return true;
                    }
                    if (t.IndexOf("QItem") == 0)
                    {
                        return false;
                    }
                    // Item type might exist in content packs
                    else if (game.cd.items.ContainsKey(t))
                    {
                        itemSelect.Add(qItemQuestComponent.sectionName, t);
                        return true;
                    }
                }
            }
            else
            {
                // List of exclusions
                List<string> exclude = new List<string>();
                foreach (string t in qItemQuestComponent.itemName)
                {
                    if (itemSelect.ContainsKey(t))
                    {
                        exclude.Add(itemSelect[t]);
                    }
                    else if (t.IndexOf("QItem") == 0 && !force)
                    {
                        return false;
                    }
                    else
                    {
                        exclude.Add(t);
                    }
                }

                // add user inventory in exclude list
                exclude.AddRange(items);

                // we don't want duplicate in the shop
                exclude.AddRange(itemSelect.Values.ToList());

                // Start a list of matches
                List<string> list = new List<string>();
                foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
                {
                    bool next = false;

                    foreach (string t in qItemQuestComponent.traits)
                    {
                        // Does the item have this trait?
                        if (!kv.Value.ContainsTrait(t))
                        {
                            // Trait missing, exclude monster
                            next = true;
                        }
                    }
                    if (next) continue;

                    foreach (string t in exclude)
                    {
                        if (t.Equals(kv.Key))
                            next = true;
                    }
                    if (next) continue;


                    if (kv.Value.minFame > 0)
                    {
                        if (kv.Value.minFame > fame) continue;
                        if (kv.Value.maxFame < fame) continue;
                    }

                    // Must have one of these traits
                    bool oneFound = (qItemQuestComponent.traitpool.Length == 0);
                    foreach (string t in qItemQuestComponent.traitpool)
                    {
                        // Does the item have this trait?
                        if (kv.Value.ContainsTrait(t))
                        {
                            oneFound = true;
                        }
                    }

                    // item has all traits
                    if (oneFound)
                    {
                        list.Add(kv.Key);
                    }
                }

                if (list.Count == 0)
                {
                    game.quest.log.Add(new LogEntry("Warning: Unable to find an item for QItem: " + qItemQuestComponent.sectionName, true));
                    return false;
                }

                // Pick item at random from candidates
                itemSelect.Add(qItemQuestComponent.sectionName, list[Random.Range(0, list.Count)]);
                return true;
            }
            return false;
        }

        public bool RuntimeMonsterSelection(string spawn_name)
        {
            // Determine monster types
            if (qd.components.ContainsKey(spawn_name))
            {
                SpawnQuestComponent qs = qd.components[spawn_name] as SpawnQuestComponent;
                return AttemptMonsterMatch(qs);
            }
            else
            {
                return false;
            }
        }

        public bool AttemptMonsterMatch(SpawnQuestComponent spawnQuestComponent, bool force = true)
        {
            if (monsterSelect.ContainsKey(spawnQuestComponent.sectionName))
            {
                return true;
            }
            if ((spawnQuestComponent.mTraitsPool.Length + spawnQuestComponent.mTraitsRequired.Length) == 0)
            {
                foreach (string t in spawnQuestComponent.mTypes)
                {
                    if (monsterSelect.ContainsKey(t))
                    {
                        monsterSelect.Add(spawnQuestComponent.sectionName, monsterSelect[t]);
                        return true;
                    }
                    if (t.IndexOf("Spawn") == 0)
                    {
                        return false;
                    }
                    string monster = t;
                    if (monster.IndexOf("Monster") != 0 && monster.IndexOf("CustomMonster") != 0)
                    {
                        monster = "Monster" + monster;
                    }
                    // Monster type might be a unique for this Quest
                    if (game.quest.qd.components.ContainsKey(monster))
                    {
                        monsterSelect.Add(spawnQuestComponent.sectionName, monster);
                        return true;
                    }
                    // Monster type might exist in content packs, 'Monster' is optional
                    else if (game.cd.monsters.ContainsKey(monster))
                    {
                        monsterSelect.Add(spawnQuestComponent.sectionName, monster);
                        return true;
                    }
                }
            }
            else
            {
                // List of exclusions
                List<string> exclude = new List<string>();
                foreach (string t in spawnQuestComponent.mTypes)
                {
                    if (monsterSelect.ContainsKey(t))
                    {
                        exclude.Add(monsterSelect[t]);
                    }
                    else if (t.IndexOf("Spawn") == 0 && !force)
                    {
                        return false;
                    }
                    else
                    {
                        exclude.Add(t);
                    }
                }

                if (game.gameType.TypeName() == "D2E")
                {
                    // monster already selected
                    foreach (KeyValuePair<string, string> entry in monsterSelect)
                    {
                        if (!exclude.Contains(entry.Value))
                        {
                            exclude.Add(entry.Value);
                        }
                    }
                    // monster already present in the board
                    foreach (Monster entry in monsters)
                    {
                        if (!exclude.Contains(entry.monsterData.sectionName))
                        {
                            exclude.Add(entry.monsterData.sectionName);
                        }
                    }
                }

                // Start a list of matches
                List<string> list = new List<string>();
                foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
                {
                    bool allFound = true;
                    foreach (string t in spawnQuestComponent.mTraitsRequired)
                    {
                        // Does the monster have this trait?
                        if (!kv.Value.ContainsTrait(t))
                        {
                            // Trait missing, exclude monster
                            allFound = false;
                        }
                    }

                    // Must have one of these traits
                    bool oneFound = (spawnQuestComponent.mTraitsPool.Length == 0);
                    foreach (string t in spawnQuestComponent.mTraitsPool)
                    {
                        // Does the monster have this trait?
                        if (kv.Value.ContainsTrait(t))
                        {
                            oneFound = true;
                        }
                    }

                    bool excludeBool = false;
                    foreach (string t in exclude)
                    {
                        if (t.Equals(kv.Key)) excludeBool = true;
                    }

                    // Monster has all traits
                    if (allFound && oneFound && !excludeBool)
                    {
                        list.Add(kv.Key);
                    }
                }

                foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
                {
                    CustomMonsterQuestComponent cm = kv.Value as CustomMonsterQuestComponent;
                    if (cm == null) continue;

                    MonsterData baseMonster = null;
                    string[] traits = cm.traits;
                    // Check for content data monster defined as base
                    if (game.cd.monsters.ContainsKey(cm.baseMonster))
                    {
                        baseMonster = game.cd.monsters[cm.baseMonster];
                        if (traits.Length == 0)
                        {
                            traits = baseMonster.traits;
                        }
                    }

                    bool allFound = true;
                    foreach (string t in spawnQuestComponent.mTraitsRequired)
                    {
                        // Does the monster have this trait?
                        if (!InArray(traits, t))
                        {
                            // Trait missing, exclude monster
                            allFound = false;
                        }
                    }

                    // Must have one of these traits
                    bool oneFound = (spawnQuestComponent.mTraitsPool.Length == 0);
                    foreach (string t in spawnQuestComponent.mTraitsPool)
                    {
                        // Does the monster have this trait?
                        if (InArray(traits, t))
                        {
                            oneFound = true;
                        }
                    }

                    bool excludeBool = false;
                    foreach (string t in exclude)
                    {
                        if (t.Equals(kv.Key)) excludeBool = true;
                    }

                    // Monster has all traits
                    if (allFound && oneFound && !excludeBool)
                    {
                        list.Add(kv.Key);
                    }
                }

                if (list.Count == 0)
                {
                    ValkyrieDebug.Log("Error: Unable to find monster of traits specified in event: " + spawnQuestComponent.sectionName);
                    game.quest.log.Add(new LogEntry("Error: Unable to find monster of traits specified in spawn event: " + spawnQuestComponent.sectionName, true));
                    return false;
                }

                // Pick monster at random from candidates
                monsterSelect.Add(spawnQuestComponent.sectionName, list[Random.Range(0, list.Count)]);
                return true;
            }
            return false;
        }

        public static bool InArray(string[] array, string item)
        {
            foreach (string s in array)
            {
                if (s.Equals(item)) return true;
            }
            return false;
        }

        // Construct a Quest from save data string
        // Used for undo
        public Quest(string save)
        {
            LoadQuest(IniRead.ReadFromString(save));
        }

        // Construct a Quest from save iniData
        public Quest(IniData saveData)
        {
            LoadQuest(saveData);
        }

        public void ChangeQuest(string path)
        {
            Game game = Game.Get();

            // Clean up everything marked as 'board'
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.BOARD))
            {
                // do not destroy Quest Ui panel in case we are using it again :
                // findGameObject would find it as "Actual object destruction is always delayed until after the current Update loop" (see issue #820)
                if (go.name != "QuestUICanvas")
                    Object.Destroy(go);
            }
            game.tokenBoard.tc.Clear();

            phase = MoMPhase.investigator;
            game.cc.gameObject.transform.position = new Vector3(0, 0, -8);

            game.cc.minLimit = false;
            game.cc.maxLimit = false;

            // Set static Quest data
            if (path.StartsWith("\\") || path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1);
            }

            string questToTransition = game.quest.originalPath + Path.DirectorySeparatorChar + path;
            if (game.quest.fromSavegame)
            {
                questToTransition = ContentData.ValkyrieLoadQuestPath + Path.DirectorySeparatorChar + path;
            }

            qd = new QuestData(questToTransition);
            // set questPath but do not set original path, as we are loading from within a Quest here.
            questPath = Path.GetDirectoryName(qd.questPath);

            vars.TrimQuest();

            undo = new Stack<string>();

            // Initialise data
            boardItems = new Dictionary<string, BoardComponent>();
            ordered_boardItems = new List<string>();
            monsters = new List<Monster>();
            heroSelection = new Dictionary<string, List<Hero>>();
            puzzle = new Dictionary<string, Puzzle>();
            eventQuota = new Dictionary<string, int>();
            undo = new Stack<string>();
            monsterSelect = new Dictionary<string, string>();
            itemSelect = new Dictionary<string, string>();
            itemInspect = new Dictionary<string, string>();
            shops = new Dictionary<string, List<string>>();
            activeShop = "";

            GenerateItemSelection();
            eManager = new EventManager();

            // Set Quest vars for selected expansions
            foreach (string s in game.cd.GetLoadedPackIDs())
            {
                if (s.Length > 0)
                {
                    vars.SetValue("#" + s, 1);
                }
            }
            vars.SetValue("#round", 1);

            // Set Quest flag based on hero count
            int heroCount = 0;
            foreach (Hero h in heroes)
            {
                h.activated = false;
                h.defeated = false;
                h.selected = false;
                if (h.heroData != null)
                {
                    heroCount++;
                    // Create variable to value 1 for each selected Hero
                    game.quest.vars.SetValue("#" + h.heroData.sectionName, 1);
                }
            }
            game.quest.vars.SetValue("#heroes", heroCount);

            List<string> music = new List<string>();
            foreach (AudioData ad in game.cd.audio.Values)
            {
                if (ad.ContainsTrait("Quest")) music.Add(ad.file);
            }
            game.audioControl.PlayDefaultQuestMusic(music);

            // Update the screen
            game.monsterCanvas.UpdateList();
            game.heroCanvas.UpdateStatus();

            // when starting a new Quest, reset round countroller
            game.roundControl.Reset();

            // Start round events
            eManager.EventTriggerType("StartRound", false);
            // Start the Quest (top of stack)
            eManager.EventTriggerType("EventStart", false);
            eManager.TriggerEvent();
            SaveManager.Save(0);
        }

        // Read save data
        public void LoadQuest(IniData saveData)
        {
            game = Game.Get();

            // This happens anyway but we need it to be here before the following code is executed (also needed for loading saves)
            game.quest = this;

            fromSavegame = true;

            // Set static Quest data
            qd = new QuestData(saveData.Get("Quest", "path"));

            // Start Quest music
            List<string> music = new List<string>();
            if (saveData.Get("Music") != null)
            {
                music = new List<string>(saveData.Get("Music").Values);
                List<string> toPlay = new List<string>();
                foreach (string s in music)
                {
                    if (game.cd.audio.ContainsKey(s))
                    {
                        toPlay.Add(game.cd.audio[s].file);
                    }
                    else
                    {
                        toPlay.Add(FindLocalisedMultimediaFile(s, Path.GetDirectoryName(qd.questPath)));
                    }
                }
                game.audioControl.PlayMusic(toPlay);
            }
            else
            {
                foreach (AudioData ad in game.cd.audio.Values)
                {
                    if (ad.ContainsTrait("Quest")) music.Add(ad.file);
                }
                game.audioControl.PlayDefaultQuestMusic(music);
            }

            // Get state
            bool.TryParse(saveData.Get("Quest", "heroesSelected"), out heroesSelected);
            bool horror;
            bool.TryParse(saveData.Get("Quest", "horror"), out horror);
            if (horror)
            {
                phase = MoMPhase.horror;
            }

            // Set camera
            float camx, camy, camz;
            float.TryParse(saveData.Get("Quest", "camx"), out camx);
            float.TryParse(saveData.Get("Quest", "camy"), out camy);
            float.TryParse(saveData.Get("Quest", "camz"), out camz);
            game.cc.gameObject.transform.position = new Vector3(camx, camy, camz);

            game.cc.minLimit = false;
            if (saveData.Get("Quest", "camxmin").Length > 0)
            {
                game.cc.minLimit = true;
                int.TryParse(saveData.Get("Quest", "camxmin"), out game.cc.minPanX);
                int.TryParse(saveData.Get("Quest", "camymin"), out game.cc.minPanY);
            }

            game.cc.maxLimit = false;
            if (saveData.Get("Quest", "camxmax").Length > 0)
            {
                game.cc.maxLimit = true;
                int.TryParse(saveData.Get("Quest", "camxmax"), out game.cc.maxPanX);
                int.TryParse(saveData.Get("Quest", "camymax"), out game.cc.maxPanY);
            }

            shops = new Dictionary<string, List<string>>();
            if (saveData.Get("Shops") != null)
            {
                foreach (KeyValuePair<string, string> kv in saveData.Get("Shops"))
                {
                    List<string> shopList = new List<string>();
                    foreach (string s in kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        shopList.Add(s);
                    }
                    shops.Add(kv.Key, shopList);
                }
            }

            activeShop = "";
            if (saveData.Get("ActiveShop") != null)
            {
                foreach (KeyValuePair<string, string> kv in saveData.Get("ActiveShop"))
                {
                    activeShop = kv.Key;
                }
            }

            // Restore event log
            log = new List<LogEntry>();
            foreach (KeyValuePair<string, string> kv in saveData.Get("Log"))
            {
                log.Add(new LogEntry(kv.Key, kv.Value));
            }

            // Restore event list (do nothing if empty: '??' is here to avoid null exception)
            eventList = new List<string>();
            foreach (KeyValuePair<string, string> kv in saveData.Get("EventList") ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                eventList.Add(kv.Value);
            }

            // Set start time to now
            start_time = System.DateTime.UtcNow;
            // get previous duration, if not present, we are using an old savegame so do not use the duration
            if (!int.TryParse(saveData.Get("Quest", "duration"), out duration))
            {
                duration = -1;
            }

            Dictionary<string, string> saveVars = saveData.Get("Vars");
            vars = new VarManager(saveVars);

            itemSelect = saveData.Get("SelectItem");
            if (itemSelect == null)
            {
                // Support old saves
                itemSelect = new Dictionary<string, string>();
                GenerateItemSelection();
            }

            itemInspect = saveData.Get("ItemInspect");
            if (itemInspect == null)
            {
                itemInspect = new Dictionary<string, string>();
            }

            // Set items
            items = new HashSet<string>();
            Dictionary<string, string> saveItems = saveData.Get("Items");
            foreach (KeyValuePair<string, string> kv in saveItems)
            {
                items.Add(kv.Key);
            }

            originalPath = saveData.Get("Quest", "originalpath");
            questPath = saveData.Get("Quest", "path");

            monsterSelect = saveData.Get("SelectMonster");
            if (monsterSelect == null)
            {
                // Support old saves
                monsterSelect = new Dictionary<string, string>();
            }

            // Clear all tokens
            game.tokenBoard.Clear();
            // Clean up everything marked as 'board'
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.BOARD))
                Object.Destroy(go);

            // Repopulate items on the baord
            boardItems = new Dictionary<string, BoardComponent>();
            ordered_boardItems = new List<string>();
            Dictionary<string, string> saveBoard = saveData.Get("Board");
            foreach (KeyValuePair<string, string> kv in saveBoard)
            {
                string boardItem = kv.Key;
                if (boardItem[0] == '\\')
                {
                    boardItem = boardItem.Substring(1);
                }
                if (boardItem.IndexOf("Door") == 0)
                {
                    boardItems.Add(boardItem, new DoorBoardComponent(qd.components[boardItem] as Assets.Scripts.Content.QuestComponent.DoorQuestComponent, game));
                    ordered_boardItems.Add(boardItem);
                }
                if (boardItem.IndexOf("Token") == 0)
                {
                    boardItems.Add(boardItem, new TokenBoardComponent(qd.components[boardItem] as Assets.Scripts.Content.QuestComponent.TokenQuestComponent, game));
                    ordered_boardItems.Add(boardItem);
                }
                if (boardItem.IndexOf("Tile") == 0)
                {
                    boardItems.Add(boardItem, new TileBoardComponent(qd.components[boardItem] as Assets.Scripts.Content.QuestComponent.TileQuestComponent, game));
                    ordered_boardItems.Add(boardItem);
                }
                if (boardItem.IndexOf("Ui") == 0)
                {
                    boardItems.Add(boardItem, new UIBoardComponent(qd.components[boardItem] as UiQuestComponent, game));
                    ordered_boardItems.Add(boardItem);
                }
                if (boardItem.IndexOf("#shop") == 0)
                {
                    boardItems.Add(boardItem, new ShopInterface(new List<string>(), Game.Get(), activeShop));
                    ordered_boardItems.Add(boardItem);
                }
            }

            // Clean undo stack (we don't save undo stack)
            // When performing undo this is replaced later
            undo = new Stack<string>();

            // Fetch hero state
            heroes = new List<Hero>();
            monsters = new List<Monster>();
            int heroCount = 0;
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
            {
                if (kv.Key.IndexOf("Hero") == 0 && kv.Key.IndexOf("HeroSelection") != 0)
                {
                    heroCount++;
                    heroes.Add(new Hero(kv.Value));
                }
            }
            game.quest.vars.SetValue("#heroes", heroCount);

            // Monsters must be after heros because some activations refer to heros
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
            {
                if (kv.Key.IndexOf("Monster") == 0)
                {
                    monsters.Add(new Monster(kv.Value));
                }
            }

            // Restore hero selections
            heroSelection = new Dictionary<string, List<Hero>>();
            Dictionary<string, string> saveSelection = saveData.Get("HeroSelection");
            foreach (KeyValuePair<string, string> kv in saveSelection)
            {
                // List of selected heroes
                string[] selectHeroes = kv.Value.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                List<Hero> heroList = new List<Hero>();

                foreach (string s in selectHeroes)
                {
                    foreach (Hero h in heroes)
                    {
                        // Match hero id
                        int id;
                        int.TryParse(s, out id);
                        if (id == h.id)
                        {
                            heroList.Add(h);
                        }
                    }
                }
                // Add this selection
                heroSelection.Add(kv.Key, heroList);
            }

            puzzle = new Dictionary<string, Puzzle>();
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in saveData.data)
            {
                if (kv.Key.IndexOf("PuzzleSlide") == 0)
                {
                    puzzle.Add(kv.Key.Substring("PuzzleSlide".Length, kv.Key.Length - "PuzzleSlide".Length), new PuzzleSlide(kv.Value));
                }
                if (kv.Key.IndexOf("PuzzleCode") == 0)
                {
                    puzzle.Add(kv.Key.Substring("PuzzleCode".Length, kv.Key.Length - "PuzzleCode".Length), new PuzzleCode(kv.Value));
                }
                if (kv.Key.IndexOf("PuzzleImage") == 0)
                {
                    puzzle.Add(kv.Key.Substring("PuzzleImage".Length, kv.Key.Length - "PuzzleImage".Length), new PuzzleImage(kv.Value));
                }
                if (kv.Key.IndexOf("PuzzleTower") == 0)
                {
                    puzzle.Add(kv.Key.Substring("PuzzleTower".Length, kv.Key.Length - "PuzzleTower".Length), new PuzzleTower(kv.Value));
                }
            }
            // Restore event quotas
            eventQuota = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> kv in saveData.Get("EventQuota"))
            {
                int value;
                int.TryParse(kv.Value, out value);
                eventQuota.Add(kv.Key, value);
            }

            eManager = new EventManager(saveData.Get("EventManager"));

            // Update the screen
            game.stageUI = new NextStageButton();
            game.monsterCanvas.UpdateList();
            game.heroCanvas.UpdateStatus();
        }

        // Save to the undo stack
        // This is called on user actions (such as defeated monsters, heros activated)
        public void Save()
        {
            undo.Push(ToString());
        }

        // Load from the undo stack
        public void Undo()
        {
            // Nothing to undo
            if (undo.Count == 0) return;
            // Load the old state.  This will also set game.Quest
            Quest oldQuest = new Quest(undo.Pop());
            // Transfer the undo stack to the loaded state
            oldQuest.undo = undo;
        }

        // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
        // Delay is used if we can't raise the nomorale event at this point (need to recall this later)
        public void AdjustMorale(int m, bool delay = false)
        {
            Game game = Game.Get();

            float morale = vars.GetValue("$%morale") + m;
            vars.SetValue("$%morale", morale);

            // Test for no morale ending
            if (morale < 0)
            {
                morale = 0;
                game.moraleDisplay.Update();
                // Hold loss during activation
                if (delay)
                {
                    morale = -1;
                    return;
                }
                eManager.EventTriggerType("NoMorale");
            }
            game.moraleDisplay.Update();
        }

        // Function to return a hero at random
        public Hero GetRandomHero()
        {
            // We have to create a list with the null heroes trimmed
            List<Hero> hList = new List<Hero>();
            foreach (Hero h in heroes)
            {
                if (h.heroData != null)
                {
                    hList.Add(h);
                }
            }
            return hList[Random.Range(0, hList.Count)];
        }

        public int GetHeroCount()
        {
            int count = 0;
            foreach (Hero h in heroes)
            {
                if (h.heroData != null)
                {
                    count++;
                }
            }
            return count;
        }

        // Add a list of components (token, tile, etc)
        public void Add(string[] names, bool shop = false)
        {
            foreach (string s in names)
            {
                Add(s, shop);
            }
        }

        // Add a component to the board
        public void Add(string name, bool shop = false)
        {
            // Check that the component is valid
            if (!game.quest.qd.components.ContainsKey(name))
            {
                ValkyrieDebug.Log("Error: Unable to create missing Quest component: " + name);
                Application.Quit();
            }
            // Add to active list
            QuestComponent qc = game.quest.qd.components[name];

            if (boardItems.ContainsKey(name)) return;

            // Add to board
            if (qc is Assets.Scripts.Content.QuestComponent.TileQuestComponent)
            {
                boardItems.Add(name, new TileBoardComponent((Assets.Scripts.Content.QuestComponent.TileQuestComponent)qc, game));
                ordered_boardItems.Add(name);
            }
            if (qc is Assets.Scripts.Content.QuestComponent.DoorQuestComponent)
            {
                boardItems.Add(name, new DoorBoardComponent((Assets.Scripts.Content.QuestComponent.DoorQuestComponent)qc, game));
                ordered_boardItems.Add(name);
            }
            if (qc is Assets.Scripts.Content.QuestComponent.TokenQuestComponent)
            {
                boardItems.Add(name, new TokenBoardComponent((Assets.Scripts.Content.QuestComponent.TokenQuestComponent)qc, game));
                ordered_boardItems.Add(name);
            }
            if (qc is UiQuestComponent)
            {
                boardItems.Add(name, new UIBoardComponent((UiQuestComponent)qc, game));
                ordered_boardItems.Add(name);
            }
            if (qc is QItemQuestComponent && !shop)
            {
                if (itemSelect.ContainsKey(name))
                {
                    items.Add(itemSelect[name]);
                    if (((QItemQuestComponent)qc).inspect.Length > 0)
                    {
                        if (game.quest.itemInspect.ContainsKey(itemSelect[name]))
                        {
                            game.quest.itemInspect.Remove(itemSelect[name]);
                        }
                        game.quest.itemInspect.Add(itemSelect[name], ((QItemQuestComponent)qc).inspect);
                    }
                }
            }
        }

        // Remove a list of active components
        public void Remove(string[] names)
        {
            foreach (string s in names)
            {
                Remove(s);
            }
        }

        // Remove an activate component
        public void Remove(string name)
        {
            if (boardItems.ContainsKey(name))
            {
                boardItems[name].Remove();
                boardItems.Remove(name);
                ordered_boardItems.Remove(name);
            }
            if (monsterSelect.ContainsKey(name))
            {
                List<Monster> toRemove = new List<Monster>();
                foreach (Monster m in monsters)
                {
                    if (m.monsterData.sectionName.Equals(monsterSelect[name]))
                    {
                        toRemove.Add(m);
                    }
                }

                foreach (Monster m in toRemove)
                {
                    monsters.Remove(m);
                    game.monsterCanvas.UpdateList();
                    game.quest.vars.SetValue("#monsters", game.quest.monsters.Count);
                }
            }
            if (itemSelect.ContainsKey(name) && items.Contains(itemSelect[name]))
            {
                items.Remove(itemSelect[name]);
                if (itemInspect.ContainsKey(itemSelect[name]))
                {
                    itemInspect.Remove(itemSelect[name]);
                }
            }
            if (name.Equals("#monsters"))
            {
                monsters.Clear();
                game.monsterCanvas.UpdateList();
                game.quest.vars.SetValue("#monsters", 0);
            }
            if (name.Equals("#boardcomponents"))
            {
                foreach (BoardComponent bc in boardItems.Values)
                {
                    bc.Remove();
                }
                boardItems.Clear();
                ordered_boardItems.Clear();
            }
        }

        public bool UIItemsPresent()
        {
            foreach (BoardComponent c in boardItems.Values)
            {
                if (c is UIBoardComponent) return true;
                if (c is ShopInterface) return true;
            }
            return false;
        }

        // Remove all active components
        public void RemoveAll()
        {
            foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
            {
                kv.Value.Remove();
            }

            boardItems.Clear();
            ordered_boardItems.Clear();
        }

        // Change the transparency of a component on the board
        public void ChangeAlpha(string name, float alpha)
        {
            // Check is component is active
            if (!boardItems.ContainsKey(name)) return;
            boardItems[name].SetVisible(alpha);
        }

        // Change the transparency of all board components
        public void ChangeAlphaAll()
        {
            foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
            {
                kv.Value.SetVisible(game.editorTransparency);
            }
        }

        public void Update()
        {
            foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
            {
                kv.Value.UpdateAlpha(Time.deltaTime);
            }
        }

        // Save the Quest state to a string for save games and undo
        override public string ToString()
        {
            //Game game = Game.Get();
            string nl = System.Environment.NewLine;
            // General Quest state block
            string r = "[Quest]" + nl;

            r += "time=" + System.DateTime.Now.ToString() + nl;

            // Current game duration + duration of previous game session before loading
            if (duration >= 0)
            {
                System.TimeSpan current_duration = System.DateTime.UtcNow.Subtract(start_time);
                r += "duration=" + (int)(this.duration + current_duration.TotalMinutes) + nl;
            }
            else
            {
                // if previous duration is invalid, we are using an old savegame do not try to calculate anything
                r += "duration=" + (-1) + nl;
            }

            // Save valkyrie version
            r += "valkyrie=" + game.version + nl;

            r += "path=" + qd.questPath + nl;
            r += "originalpath=" + originalPath + nl;
            r += "questname=" + qd.QuestIniComponent.name.Translate() + nl;

            if (phase == MoMPhase.horror)
            {
                r += "horror=true" + nl;
            }
            else
            {
                r += "horror=false" + nl;
            }
            r += "heroesSelected=" + heroesSelected + nl;
            r += "camx=" + game.cc.gameObject.transform.position.x + nl;
            r += "camy=" + game.cc.gameObject.transform.position.y + nl;
            r += "camz=" + game.cc.gameObject.transform.position.z + nl;
            if (game.cc.minLimit)
            {
                r += "camxmin=" + game.cc.minPanX + nl;
                r += "camymin=" + game.cc.minPanY + nl;
            }
            if (game.cc.maxLimit)
            {
                r += "camxmax=" + game.cc.maxPanX + nl;
                r += "camymax=" + game.cc.maxPanY + nl;
            }

            r += "[Packs]" + nl;
            foreach (string pack in game.cd.GetLoadedPackIDs())
            {
                r += pack + nl;
            }

            r += "[Board]" + nl;
            // we need to use an ordered list of board item, to preserve items stacking order, Dictionary do not preserve order
            // foreach (KeyValuePair<string, BoardComponent> kv in boardItems)
            foreach (string item in ordered_boardItems)
            {
                // Hack to prevent items from being treated as comments
                r += '\\' + item + nl;
            }

            r += vars.ToString();

            r += "[Items]" + nl;
            foreach (string s in items)
            {
                r += s + nl;
            }

            // Hero selection is a list of selections, each with space separated hero lists
            r += "[HeroSelection]" + nl;
            foreach (KeyValuePair<string, List<Hero>> kv in heroSelection)
            {
                r += kv.Key + "=";
                foreach (Hero h in kv.Value)
                {
                    r += h.id + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            // Save event quotas
            r += "[EventQuota]" + nl;
            foreach (KeyValuePair<string, int> kv in eventQuota)
            {
                r += kv.Key + "=" + kv.Value + nl;
            }

            // Save hero state
            foreach (Hero h in heroes)
            {
                r += h.ToString();
            }

            // Save monster state
            foreach (Monster m in monsters)
            {
                r += m.ToString();
            }

            foreach (KeyValuePair<string, Puzzle> kv in puzzle)
            {
                r += kv.Value.ToString(kv.Key);
            }

            r += "[Log]" + nl;
            int i = 0;
            foreach (LogEntry e in log)
            {
                r += e.ToString(i++);
            }

            r += "[EventList]" + nl;
            i = 0;
            foreach (string eventName in eventList)
            {
                r += "Event" + i + "=" + eventName + nl;
                i++;
            }

            r += "[SelectMonster]" + nl;
            foreach (KeyValuePair<string, string> kv in monsterSelect)
            {
                r += kv.Key + "=" + kv.Value + nl;
            }

            r += "[SelectItem]" + nl;
            foreach (KeyValuePair<string, string> kv in itemSelect)
            {
                r += kv.Key + "=" + kv.Value + nl;
            }

            r += "[ItemInspect]" + nl;
            foreach (KeyValuePair<string, string> kv in itemInspect)
            {
                r += kv.Key + "=" + kv.Value + nl;
            }

            if (activeShop.Length > 0)
            {
                r += "[ActiveShop]" + nl;
                r += activeShop + nl;
            }

            r += eManager.ToString();

            r += "[Shops]" + nl;
            foreach (KeyValuePair<string, List<string>> kv in shops)
            {
                r += kv.Key + "=";
                foreach (string s in kv.Value)
                {
                    r += s + " ";
                }
                r = r.Substring(0, r.Length - 1) + nl;
            }

            if (music.Count > 0)
            {
                r += "[Music]" + nl;
                for (int j = 0; j < music.Count; j++)
                {
                    r += "track" + j + "=" + music[j] + nl;
                }
                r += nl;
            }

            return r;
        }
    }
}




