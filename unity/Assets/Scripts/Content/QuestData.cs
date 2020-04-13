using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content;
using Assets.Scripts.Content.QuestComponent;
using ValkyrieTools;

// Class to manage all static data for the current Quest
public class QuestData
{
    // All components in the Quest
    public Dictionary<string, QuestComponent> components;

    // Custom activations
    public Dictionary<string, ActivationData> questActivations;

    // List of ini files containing Quest data
    List<string> iniFiles;
    // List of localization files containing Quest texts
    List<string> localizationFiles;

    // Location of the Quest.ini file
    public string questPath = "";

    // Dictionary of items to rename on reading
    public Dictionary<string, string> rename;

    // Data from 'Quest' section
    public Assets.Scripts.Content.QuestIniComponent QuestIniComponent;

    Game game;

    // Create from Quest loader entry
    public QuestData(Assets.Scripts.Content.QuestIniComponent q)
    {
        questPath = q.path + Path.DirectorySeparatorChar + "Quest.ini";
        LoadQuestData();
    }

    // Read all data files and populate components for Quest
    public QuestData(string path)
    {
        questPath = path;
        LoadQuestData();
    }
    
    // Populate data
    public void LoadQuestData()
    {
        ValkyrieDebug.Log("Loading Quest from: \"" + questPath + "\"" + System.Environment.NewLine);
        game = Game.Get();

        components = new Dictionary<string, QuestComponent>();
        questActivations = new Dictionary<string, ActivationData>();

        // Read the main Quest file
        IniData questIniData = IniRead.ReadFromIni(questPath);
        // Failure to read Quest is fatal
        if(questIniData == null)
        {
            ValkyrieDebug.Log("Failed to load Quest from: \"" + questPath + "\"");
            Application.Quit();
        }

        // List of data files
        iniFiles = new List<string>();
        localizationFiles = new List<string>();
        // The main data file is included
        iniFiles.Add("Quest.ini");

        // Quest is a special component, must be in Quest.ini
        if (questIniData.Get("Quest") == null)
        {
            ValkyrieDebug.Log("Error: Quest section missing from Quest.ini");
            return;
        }
        QuestIniComponent = new Assets.Scripts.Content.QuestIniComponent(questIniData.Get("Quest"));

        // Find others (no addition files is not fatal)
        if (questIniData.Get("QuestData") != null)
        {
            foreach (string file in questIniData.Get("QuestData").Keys)
            {
                if (file != null && file.Length > 0)
                {
                    // path is relative to the main file (absolute not supported)
                    iniFiles.Add(file);
                }
            }
        }
        else
        {
            ValkyrieDebug.Log("No QuestData extra files");
        }

        // Find Localization texts
        if (questIniData.Get("QuestText") != null)
        {
            foreach (string file in questIniData.Get("QuestText").Keys)
            {
                if (file != null && file.Length > 0)
                {
                    // path is relative to the main file (absolute not supported)
                    localizationFiles.Add(Path.GetDirectoryName(questPath) + Path.DirectorySeparatorChar + file);
                }
            }
        }
        else
        {
            ValkyrieDebug.Log("No QuestText extra files");
        }

        // Reset scenario dict
        DictionaryI18n qstDict = new DictionaryI18n(game.currentLang);
        foreach (string file in localizationFiles)
        {
            qstDict.AddDataFromFile(file);
        }
        LocalizationRead.AddDictionary("qst", qstDict);

        foreach (string f in iniFiles)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(questPath), f);
            // Read each file
            questIniData = IniRead.ReadFromIni(fullPath);
            // Failure to read a file is fatal
            if (questIniData == null)
            {
                ValkyrieDebug.Log("Unable to read Quest file: \"" + fullPath + "\"");
                Application.Quit();
            }

            rename = new Dictionary<string, string>();
            // Loop through all ini sections
            foreach (KeyValuePair<string, Dictionary<string, string>> section in questIniData.data)
            {
                // Add the section to our Quest data
                AddData(section.Key, section.Value, f);
            }

            // Update all references to this component
            foreach (QuestComponent qc in components.Values)
            {
                foreach (KeyValuePair<string, string> kv in rename)
                {
                    qc.ChangeReference(kv.Key, kv.Value);
                    LocalizationRead.dicts["qst"].RenamePrefix(kv.Key + ".", kv.Value + ".");
                }
            }
        }
    }

    // Add a section from an ini file to the Quest data.  Duplicates are not allowed
    void AddData(string name, Dictionary<string, string> content, string source)
    {
        // Fatal error on duplicates
        if(components.ContainsKey(name))
        {
            ValkyrieDebug.Log("Duplicate component in Quest: " + name);
            Application.Quit();
        }

        // Check for known types and create
        if (name.IndexOf(TileQuestComponent.type) == 0)
        {
            TileQuestComponent c = new TileQuestComponent(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(DoorQuestComponent.type) == 0)
        {
            DoorQuestComponent c = new DoorQuestComponent(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(TokenQuestComponent.type) == 0)
        {
            TokenQuestComponent c = new TokenQuestComponent(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(UiQuestComponent.type) == 0)
        {
            UiQuestComponent c = new UiQuestComponent(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(EventQuestComponent.type) == 0)
        {
            EventQuestComponent c = new EventQuestComponent(name, content, source, QuestIniComponent.format);
            components.Add(name, c);
        }
        if (name.IndexOf(SpawnQuestComponent.type) == 0)
        {
            SpawnQuestComponent c = new SpawnQuestComponent(name, content, game, source);
            components.Add(name, c);
        }
        if (name.IndexOf(MPlaceQuestComponent.type) == 0)
        {
            MPlaceQuestComponent c = new MPlaceQuestComponent(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(QItemQuestComponent.type) == 0)
        {
            QItemQuestComponent c = new QItemQuestComponent(name, content, source);
            components.Add(name, c);
        }
        // Depreciated (format 3)
        if (name.IndexOf("StartingItem") == 0)
        {
            string fixedName = "QItem" + name.Substring("StartingItem".Length);
            QItemQuestComponent c = new QItemQuestComponent(fixedName, content, source);
            components.Add(fixedName, c);
        }
        if (name.IndexOf(Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent.type) == 0)
        {
            Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent c = new Assets.Scripts.Content.QuestComponent.PuzzleQuestComponent(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(CustomMonsterQuestComponent.type) == 0)
        {
            CustomMonsterQuestComponent c = new CustomMonsterQuestComponent(name, content, source);
            components.Add(name, c);
        }
        if (name.IndexOf(ActivationQuestComponent.type) == 0)
        {
            ActivationQuestComponent c = new ActivationQuestComponent(name, content, source);
            components.Add(name, c);
        }
        // If not known ignore
    }
}