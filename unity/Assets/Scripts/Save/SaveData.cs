using System.IO;
using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Save
{
    public class SaveData
    {
        public bool valid = false;
        public string quest_name;
        public System.DateTime saveTime;
        public Texture2D image;

        public SaveData(int num = 0)
        {
            Game game = Game.Get();
            if (!File.Exists(SaveManager.SaveFile(num))) return;
            try
            {
                if (!Directory.Exists(ContentData.TempValyriePath))
                {
                    Directory.CreateDirectory(ContentData.TempValyriePath);
                }

                string valkyrieLoadPath = Path.Combine(ContentData.TempValyriePath, "Preload");
                if (!Directory.Exists(valkyrieLoadPath))
                {
                    Directory.CreateDirectory(valkyrieLoadPath);
                }

                ZipManager.Extract(valkyrieLoadPath, SaveManager.SaveFile(num),
                    ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_SAVE_INI_PIC);

                image = ContentData.FileToTexture(Path.Combine(valkyrieLoadPath, "image.png"));

                string data = File.ReadAllText(Path.Combine(valkyrieLoadPath, "save.ini"));
                IniData saveData = IniRead.ReadFromString(data);

                // when loading a Quest, path should always be $TMP/load/Quest/$subquest/Quest.ini
                // Make sure it is when loading a Quest saved for the first time, as in that case it is the original load path
                string questLoadPath = Path.GetDirectoryName(saveData.Get("Quest", "path"));
                string questOriginalPath = saveData.Get("Quest", "originalpath");

                // loading a Quest saved for the first time
                if (questLoadPath.Contains(questOriginalPath))
                {
                    questLoadPath = questLoadPath.Replace(questOriginalPath, ContentData.ValkyrieLoadQuestPath);
                }

                // use preload path rather than load
                questLoadPath =
                    questLoadPath.Replace(ContentData.ValkyrieLoadPath, ContentData.ValkyriePreloadPath);
                Assets.Scripts.Content.QuestIniComponent q =
                    new Assets.Scripts.Content.QuestIniComponent(questLoadPath);
                if (!q.valid)
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " contains unsupported Quest version." +
                                      System.Environment.NewLine);
                    return;
                }

                quest_name = saveData.Get("Quest", "questname");

                if (VersionManager.VersionNewer(game.version, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from a future version." +
                                      System.Environment.NewLine);
                    return;
                }

                if (!VersionManager.VersionNewerOrEqual(SaveManager.minValkyieVersion, saveData.Get("Quest", "valkyrie")))
                {
                    ValkyrieDebug.Log("Warning: Save " + num + " is from an old unsupported version." +
                                      System.Environment.NewLine);
                    return;
                }

                saveTime = System.DateTime.Parse(saveData.Get("Quest", "time"));

                valid = true;
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log("Warning: Unable to open save file: " + SaveManager.SaveFile(num) + "\nException: " +
                                  e.ToString());
            }
        }
    }
}
