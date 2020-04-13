using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;
using ValkyrieTools;
using System.IO;
using System.Text;
using Assets.Scripts.Content.QuestComponent;

// Quest editor static helper class
public class QuestEditor {

    // start editing a Quest
    public static void Begin(string path)
    {
        Game game = Game.Get();
        game.editMode = true;

        new MenuButton();
        new ToolsButton();

        // re-read Quest data
        Reload(path);
    }

    // Reload a Quest from file
    public static void Reload(string path)
    {
        Destroyer.Dialog();

        Game game = Game.Get();

        // Remove all current components
        if (game.quest != null)   game.quest.RemoveAll();

        // Clean up everything marked as 'editor'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.EDITOR))
            Object.Destroy(go);

        // Read from file
        game.quest = new Quest(new Assets.Scripts.Content.Quest(path));

        // Is this needed?
        game.quest.RemoveAll();

        // Add all components to the Quest
        foreach (KeyValuePair<string, QuestComponent> kv in game.quest.qd.components)
        {
            game.quest.Add(kv.Key);
        }
        // Set all components to mostly transparent
        game.quest.ChangeAlphaAll();

        // Create a new QED
        game.qed = new QuestEditorData(game.qed);
    }

    // Save the Quest
    public static void Save()
    {
        Game game = Game.Get();
        // Add a comment at the start of the Quest with the editor version

        StringBuilder questData = new StringBuilder()
            .Append("; Saved by version: ")
            .AppendLine(game.version);

        // Save Quest meta content to a string
        questData.AppendLine(game.quest.qd.Quest.ToString());

        // Write to disk
        try
        {
            // first we serialize dictionary to know the available languages
            if (LocalizationRead.selectDictionary("qst") != null)
            {
                Dictionary<string, List<string>> localization_files =
                    LocalizationRead.dicts["qst"].SerializeMultiple();

                // Append to the end of the content file the languages files
                questData.AppendLine().AppendLine("[QuestText]");

                foreach (string language in localization_files.Keys)
                {
                    questData.AppendLine("Localization." + language + ".txt");
                    File.WriteAllText(
                        Path.GetDirectoryName(game.quest.qd.questPath) + "/Localization." + language + ".txt",
                        string.Join(System.Environment.NewLine, localization_files[language].ToArray()));
                }

                questData.AppendLine();
            }
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Error: Failed to save Quest in editor.");
            Application.Quit();
        }

        questData.AppendLine("[QuestData]");

        Dictionary<string, StringBuilder> fileData = new Dictionary<string, StringBuilder>();

        foreach (QuestComponent qc in game.quest.qd.components.Values)
        {
            string source = qc.source;
            if (source.Length == 0)
            {
                source = "Quest.ini";
            }

            if (!fileData.ContainsKey(source))
            {
                StringBuilder thisFile = new StringBuilder();
                if (!source.Equals("Quest.ini"))
                {
                    thisFile.Append("; Saved by version: ").AppendLine(game.version);
                    questData.AppendLine(source);
                }
                fileData.Add(source, thisFile);
            }
            if (!(qc is PerilData))
            {
                fileData[source].AppendLine().Append(qc);
            }
        }

        if (fileData.ContainsKey("Quest.ini"))
        {
            fileData["Quest.ini"] = questData.Append(fileData["Quest.ini"]);
        }
        else
        {
            fileData.Add("Quest.ini", questData);
        }

        foreach (KeyValuePair<string, StringBuilder> kv in fileData)
        {
            string outFile = Path.Combine(Path.GetDirectoryName(game.quest.qd.questPath), kv.Key);
           // Write to disk
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outFile));
                File.WriteAllText(outFile, kv.Value.ToString());
            }
            catch (System.Exception)
            {
                ValkyrieDebug.Log("Error: Failed to write to " + outFile + ", components lost");
                Application.Quit();
            }
        }

        Destroyer.Dialog();
    }

    /// <summary>
    /// Find each stringkey in dictionary inside the ini file. If doesn't appear, it is unused, can be removed
    /// </summary>
    /// <param name="localization_file"></param>
    /// <param name="ini_file"></param>
    public static void removeUnusedStringKeys(List<string> localization_file, string ini_file)
    {
        // Search each line except first one
        for (int pos = localization_file.Count - 1; pos > 0; pos--)
        {
            string key = "{qst:" + localization_file[pos].Split(',')[0] + "}";
            if (!ini_file.Contains(key))
            {
                localization_file.RemoveAt(pos);
            }
        }
    }
}
