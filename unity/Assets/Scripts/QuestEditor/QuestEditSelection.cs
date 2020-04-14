using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using ValkyrieTools;

public class QuestEditSelection
{
    public Dictionary<string, Assets.Scripts.Content.QuestIniComponent> questList;

    // Create a pack with list of quests to edit
    public QuestEditSelection()
    {
        Game game = Game.Get();

        // clear list of local quests to make sure we take the latest changes
        game.questsList.UnloadLocalQuests();

        // Get list of unpacked Quest in user location (editable)
        // TODO: open/save in packages
        questList = QuestLoader.GetUserUnpackedQuests();

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Heading
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, 21);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, Assets.Scripts.Content.QuestIniComponent> q in questList)
        {
            string key = q.Key;
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
            string translation = q.Value.name.Translate();

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
            ui.SetButton(delegate { Selection(key); });
            ui.SetBGColor(Color.white);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Main menu
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Cancel);
        new UIElementBorder(ui, Color.red);
        // Delete a user Quest
        ui = new UIElement();
        ui.SetLocation((UIScaler.GetRight() * 3 / 8) - 4, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.DELETE, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Delete);
        new UIElementBorder(ui, Color.red);
        // Copy a Quest
        ui = new UIElement();
        ui.SetLocation((UIScaler.GetRight() * 5 / 8) - 4, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.COPY);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Copy);
        new UIElementBorder(ui);
        // Create a new Quest
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetRight(-9), UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.NEW);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(NewQuest);
        new UIElementBorder(ui);
    }

    public void Cancel()
    {
        Game game = Game.Get();
        // All content data has been loaded by editor, cleanup everything
        game.cd = new ContentData(game.gameType.DataDirectory());
        // Load the base content - pack will be loaded later if required
        game.cd.LoadContentID("");

        Destroyer.MainMenu();
    }

    // Change the dialog to a delete dialog
    public void Delete()
    {
        questList = QuestLoader.GetUserUnpackedQuests();
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Header
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT_TO_DELETE",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, 21);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, Assets.Scripts.Content.QuestIniComponent> q in questList)
        {
            string key = q.Key;
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
            string translation = q.Value.name.Translate();

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetButton(delegate { Delete(key); });
            ui.SetBGColor(Color.red);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Back to edit list
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CancelDelete);
        new UIElementBorder(ui, Color.red);
    }

    // Delete Quest
    public void Delete(string key)
    {
        try
        {
            Directory.Delete(key, true);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Failed to delete Quest: " + key);
        }
        new QuestEditSelection();
    }

    public void CancelCopy()
    {
        new QuestEditSelection();
    }

    public void CancelDelete()
    {
        new QuestEditSelection();
    }

    // List of quests to copy
    public void Copy()
    {
        // Can copy all quests, not just user
        questList = QuestLoader.GetQuests(true);
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Header
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT_TO_COPY",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 5, UIScaler.GetWidthUnits() - 2f, 21);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, Assets.Scripts.Content.QuestIniComponent> q in questList)
        {
            string key = q.Key;
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", q.Value.name), Color.black);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetButton(delegate { Copy(key); });
            ui.SetBGColor(Color.white);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Back to edit selection
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CancelCopy);
        new UIElementBorder(ui, Color.red);
    }

    // Copy a Quest
    public void Copy(string key)
    {
        Game game = Game.Get();
        string dataLocation = Game.AppData() + Path.DirectorySeparatorChar + Game.Get().gameType.TypeName() + "/Editor";
        if (!Directory.Exists(dataLocation))
        {
            Directory.CreateDirectory(dataLocation);
        }

        // Find a new unique directory name
        int i = 1;
        while (Directory.Exists(dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i;

        // Fully extract this scenario before copy if this is a package
        if (Path.GetExtension(Path.GetFileName(key)) == ".valkyrie")
        {
            // extract the full package
            QuestLoader.ExtractSinglePackageFull(ContentData.DownloadPath() + Path.DirectorySeparatorChar + Path.GetFileName(key));
        }

        // Copy files
        try
        {
            DirectoryCopy(key, targetLocation, true);
            // read new Quest file
            string[] questData = File.ReadAllLines(targetLocation + "/Quest.ini");

            // Search for Quest section
            bool questFound = false;
            for (i = 0; i < questData.Length; i++)
            {
                if (questData[i].Equals("[Quest]"))
                {
                    // Inside Quest section
                    questFound = true;
                }
                if (questFound && questData[i].IndexOf("name=") == 0)
                {
                    // Add copy to name
                    questFound = false;
                    questData[i] = questData[i] + " (Copy)";
                }
            }
            // Write back to ini file
            File.WriteAllLines(targetLocation + "/Quest.ini", questData);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Error: Failed to copy Quest.");
            Application.Quit();
        }
        // Back to selection
        new QuestEditSelection();
    }

    // Copy a directory
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    // Create a new Quest
    public void NewQuest()
    {
        Game game = Game.Get();
        string dataLocation = Game.AppData() + Path.DirectorySeparatorChar + Game.Get().gameType.TypeName() + "/Editor";
        if (!Directory.Exists(dataLocation))
        {
            Directory.CreateDirectory(dataLocation);
        }

        // Find an available unique directory name
        int i = 1;
        while (Directory.Exists(dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i;

        try
        {
            Directory.CreateDirectory(targetLocation);

            List<string> questData = new List<string>();

            // Create basic Quest info
            questData.Add("[Quest]");
            questData.Add("type=" + game.gameType.TypeName());
            questData.Add("format=" + Assets.Scripts.Content.QuestIniComponent.currentFormat);
            questData.Add("defaultlanguage=" + game.currentLang); 
            questData.Add("");
            questData.Add("[QuestText]");
            questData.Add("Localization."+ game.currentLang +".txt");

            // Write Quest file
            File.WriteAllLines(targetLocation + "/Quest.ini", questData.ToArray());

            // Create new dictionary
            DictionaryI18n newScenarioDict = new DictionaryI18n(new string[1] { ".," + game.currentLang }, game.currentLang);

            // Add Quest name to dictionary
            newScenarioDict.AddEntry("Quest.name", game.gameType.QuestName().Translate() + " " + i);
            // Add Quest description to dictionary
            newScenarioDict.AddEntry("Quest.description", game.gameType.QuestName().Translate() + " " + i + "...");

            // Generate localization file
            Dictionary<string,List<string>> localization_files = newScenarioDict.SerializeMultiple();

            foreach (string oneLang in localization_files.Keys)
            {
                // Write localization file
                File.WriteAllText(
                    targetLocation + "/Localization." + oneLang + ".txt",
                    string.Join(System.Environment.NewLine, localization_files[oneLang].ToArray()));
            }
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Error: Failed to create new Quest: " + e.Message);
            Application.Quit();
        }
        // Back to edit selection
        new QuestEditSelection();
    }

    // Select a Quest for editing
    public void Selection(string key)
    {
        ValkyrieDebug.Log("INFO: Select Quest " + key + " for editing");

        Game game = Game.Get();

        // Remove all current components
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        if (game.quest!=null) game.quest.RemoveAll();

        game.audioControl.StopMusic();

        // Fetch all of the Quest data
        ValkyrieDebug.Log("Selecting Quest: " + key + System.Environment.NewLine);
        ValkyrieDebug.Log("Starting Editor" + System.Environment.NewLine);
        QuestEditor.Begin(questList[key].path);
    }
}
