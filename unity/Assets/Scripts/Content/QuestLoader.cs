using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.GameTypes;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    // Class for getting lists of Quest with details
    public class QuestLoader
    {

        // Return a dictionary of all available quests
        public static Dictionary<string, Assets.Scripts.Content.QuestIniComponent> GetQuests(bool getHidden = false)
        {
            Dictionary<string, Assets.Scripts.Content.QuestIniComponent> quests =
                new Dictionary<string, Assets.Scripts.Content.QuestIniComponent>();

            Game game = Game.Get();
            // Look in the user application data directory
            string dataLocation = Game.AppData();
            mkDir(dataLocation);
            mkDir(Assets.Scripts.Content.ContentData.ContentDataBase.DownloadPath());

            // Get a list of downloaded Quest not packed
            List<string> questDirectories = GetUnpackedQuests(Assets.Scripts.Content.ContentData.ContentDataBase.DownloadPath());

            // Extract only required files from downloaded packages 
            ExtractPackages(Assets.Scripts.Content.ContentData.ContentDataBase.DownloadPath());

            // Get the list of extracted packages
            questDirectories.AddRange(GetUnpackedQuests(Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath));

            // Add the list of editor Quest
            if (game.gameType is MoMGameType)
            {
                dataLocation += "/MoM/Editor";
            }

            if (game.gameType is D2EGameType)
            {
                dataLocation += "/D2E/Editor";
            }

            if (game.gameType is IAGameType)
            {
                dataLocation += "/IA/Editor";
            }

            questDirectories.AddRange(GetUnpackedQuests(dataLocation));

            // Go through all directories
            foreach (string p in questDirectories)
            {
                // load Quest
                Assets.Scripts.Content.QuestIniComponent q = new Assets.Scripts.Content.QuestIniComponent(p);
                // Check Quest is valid and of the right type
                if (q.valid && q.type.Equals(game.gameType.TypeName()))
                {
                    // Is the Quest hidden?
                    if (!q.hidden || getHidden)
                    {
                        // Add Quest to Quest list
                        quests.Add(p, q);
                    }
                }
            }

            // Return list of available quests
            return quests;
        }

        // Return a single Quest, Quest name is without file extension
        public static Assets.Scripts.Content.QuestIniComponent GetSingleQuest(string questName, bool getHidden = false)
        {
            Assets.Scripts.Content.QuestIniComponent questIniComponent = null;

            Game game = Game.Get();
            // Look in the user application data directory
            string dataLocation = Game.AppData();
            mkDir(dataLocation);
            mkDir(Assets.Scripts.Content.ContentData.ContentDataBase.DownloadPath());

            string path = Assets.Scripts.Content.ContentData.ContentDataBase.DownloadPath() + Path.DirectorySeparatorChar + questName + ".valkyrie";
            QuestLoader.ExtractSinglePackagePartial(path);

            // load Quest
            Assets.Scripts.Content.QuestIniComponent q =
                new Assets.Scripts.Content.QuestIniComponent(Path.Combine(Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath,
                    Path.GetFileName(path)));
            // Check Quest is valid and of the right type
            if (q.valid && q.type.Equals(game.gameType.TypeName()))
            {
                // Is the Quest hidden?
                if (!q.hidden || getHidden)
                {
                    // Add Quest to Quest list
                    questIniComponent = q;
                }
            }

            // Return list of available quests
            return questIniComponent;
        }

        // Return list of quests available in the user path (includes packages)
        public static Dictionary<string, Assets.Scripts.Content.QuestIniComponent> GetUserQuests()
        {
            Dictionary<string, Assets.Scripts.Content.QuestIniComponent> quests =
                new Dictionary<string, Assets.Scripts.Content.QuestIniComponent>();

            // Read user application data for quests
            string dataLocation = Game.AppData();
            mkDir(dataLocation);
            List<string> questDirectories = GetUnpackedQuests(dataLocation);

            // Read extracted packages
            questDirectories.AddRange(GetUnpackedQuests(Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath));

            // go through all found quests
            foreach (string p in questDirectories)
            {
                // read Quest
                Assets.Scripts.Content.QuestIniComponent q = new Assets.Scripts.Content.QuestIniComponent(p);
                // Check if valid and correct type
                if (q.valid && q.type.Equals(Game.Get().gameType.TypeName()))
                {
                    quests.Add(p, q);
                }
            }

            return quests;
        }

        // Return list of quests available in the user path unpackaged (editable)
        public static Dictionary<string, Assets.Scripts.Content.QuestIniComponent> GetUserUnpackedQuests()
        {
            var quests = new Dictionary<string, Assets.Scripts.Content.QuestIniComponent>();

            // Read user application data for quests
            string dataLocation = Game.AppData();
            mkDir(dataLocation);
            List<string> questDirectories = GetUnpackedQuests(dataLocation);

            string tempPath = Assets.Scripts.Content.ContentData.ContentDataBase.TempPath;
            string gameType = Game.Get().gameType.TypeName();
            // go through all found quests
            foreach (string p in questDirectories)
            {
                // Android stores the temp path in the data dir, we don't want the extracted scenarios from there
                if (p.StartsWith(tempPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // read Quest
                var q = new Assets.Scripts.Content.QuestIniComponent(p);
                // Check if valid and correct type
                if (q.valid && q.type.Equals(gameType))
                {
                    quests.Add(p, q);
                }
            }

            return quests;
        }

        // Get list of directories with quests at a path (unpacked quests)
        public static List<string> GetUnpackedQuests(string path)
        {
            List<string> quests = new List<string>();

            if (!Directory.Exists(path))
            {
                return quests;
            }

            // Get all directories at path
            List<string> questDirectories = DirList(path);
            foreach (string p in questDirectories)
            {
                // All packs must have a Quest.ini, otherwise ignore
                if (File.Exists(p + "/Quest.ini"))
                {
                    quests.Add(p);
                }
            }

            return quests;
        }

        /// <summary>
        /// Fully extract one single package, before starting a Quest, and save package filename
        /// </summary>
        /// <param name="path">path of the file to extract</param>
        public static void ExtractSinglePackageFull(string path)
        {
            // Extract into temp
            string tempValkyriePath = Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath;
            mkDir(tempValkyriePath);

            string extractedPath = Path.Combine(tempValkyriePath, Path.GetFileName(path));
            ZipManager.Extract(extractedPath, path, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);
        }

        /// <summary>
        /// Partial extract of a single package, before listing the savegames
        /// Only the Quest.ini and translations needs to be extracted to validate Quest and get its name
        /// </summary>
        /// <param name="path">path of the file to extract</param>
        public static void ExtractSinglePackagePartial(string path)
        {
            // Extract into temp
            string extractedPath = Path.Combine(Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath, Path.GetFileName(path));
            ZipManager.Extract(extractedPath, path, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC);
        }

        /// <summary>
        /// Partial extract of all package in a directory, to list quests,  and save package filename
        /// </summary>
        /// <param name="path">path of the directory containing .valkyrie package</param>
        public static void ExtractPackages(string path)
        {
            // Find all packages at path
            string[] archives = Directory.GetFiles(path, "*.valkyrie", SearchOption.AllDirectories);

            // Extract all packages
            foreach (string f in archives)
            {
                string extractedPath = Path.Combine(Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath, Path.GetFileName(f));
                ZipManager.Extract(extractedPath, f, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC);
            }
        }

        // Attempt to create a directory
        public static void mkDir(string p)
        {
            if (!Directory.Exists(p))
            {
                try
                {
                    Directory.CreateDirectory(p);
                }
                catch (System.Exception)
                {
                    ValkyrieDebug.Log("Error: Unable to create directory: " + p);
                    Application.Quit();
                }
            }
        }

        // Return a list of directories at a path (recursive)
        public static List<string> DirList(string path)
        {
            return DirList(path, new List<string>());
        }

        // Add to list of directories at a path (recursive)
        public static List<string> DirList(string path, List<string> l)
        {
            List<string> list = new List<string>(l);

            foreach (string s in Directory.GetDirectories(path))
            {
                list = DirList(s, list);
                list.Add(s);
            }

            return list;
        }

        public static void CleanTemp()
        {
            // Nothing to do if no temporary files
            string tempValkyriePath = Assets.Scripts.Content.ContentData.ContentDataBase.TempValyriePath;
            if (!Directory.Exists(tempValkyriePath))
            {
                return;
            }

            try
            {
                Directory.Delete(tempValkyriePath, true);
            }
            catch
            {
                ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
            }
        }
    }
}