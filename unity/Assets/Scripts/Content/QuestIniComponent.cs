using System.Collections.Generic;
using System.IO;
using System.Text;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    // Quest ini component has special data
    public class QuestIniComponent
    {
        public static int minumumFormat = 4;
        // Increment during changes, and again at release
        public static int currentFormat = 13;
        public int format = 0;
        public bool hidden = false;
        public bool valid = false;
        public string path = "";
        // Quest type (MoM, D2E)
        public string type;
        // Content packs required for Quest
        public string[] packs;
        // Default language for the text
        public string defaultLanguage = "English";
        //Default music will be played when starting the Quest
        public bool defaultMusicOn = false;
        // raw localization dictionary
        public DictionaryI18n localizationDict = null;

        public string image = "";

        public int minHero = 2;
        public int maxHero = 5;
        public float difficulty = 0;
        public int lengthMin = 0;
        public int lengthMax = 0;

        // -- data for initial scenario listing
        // CRC32 of valkyrie package
        public string version = "";
        // languages availables with scenario name <"English", "The Fall of House Lynch">
        public Dictionary<string, string> languages_name = null;
        // languages availables with synopsys name <"English", "This is the synopsys">
        public Dictionary<string, string> languages_synopsys = null;
        // languages availables with synopsys name <"English", "Authors in one line">
        public Dictionary<string, string> languages_authors_short = null;
        // URL of the package
        public string package_url = "";
        // latest_update date if file is stored on Github
        public System.DateTime latest_update;
        // is package available locally
        public bool downloaded = false;
        public bool update_available = false;

        // -- data inside translation file (for unzipped Quest)
        public string name_key { get { return "quest.name"; } }
        public string description_key { get { return "quest.description"; } }
        public string synopsys_key { get { return "quest.synopsys"; } }
        public string authors_key { get { return "quest.authors"; } }
        public string authors_short_key { get { return "quest.authors_short"; } }

        public StringKey name { get { return new StringKey("qst", name_key); } }
        public StringKey description { get { return new StringKey("qst", description_key); } }
        public StringKey synopsys { get { return new StringKey("qst", synopsys_key); } }
        public StringKey authors { get { return new StringKey("qst", authors_key); } }
        public StringKey authors_short { get { return new StringKey("qst", authors_short_key); } }

        // Create from path
        public QuestIniComponent(string pathIn)
        {
            path = pathIn;
            if (path.EndsWith("\\") || path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            Dictionary<string, string> iniData = IniRead.ReadFromIni(path + Path.DirectorySeparatorChar + "Quest.ini", "Quest");
            if (iniData == null)
            {
                ValkyrieDebug.Log("Could not load the Quest.ini file in " + path + Path.DirectorySeparatorChar + "Quest.ini");
                valid = false;
                return;
            }

            // do not parse the content of a Quest from another game type
            if (iniData.ContainsKey("type") && iniData["type"] != Game.Get().gameType.TypeName())
            {
                valid = false;
                return;
            }

            //Read the localization data
            Dictionary<string, string> localizationData = IniRead.ReadFromIni(path + Path.DirectorySeparatorChar + "Quest.ini", "QuestText");

            localizationDict = new DictionaryI18n(defaultLanguage);
            foreach (string file in localizationData.Keys)
            {
                localizationDict.AddDataFromFile(path + '/' + file);
            }

            valid = Populate(iniData);
        }

        // Create from ini data
        public QuestIniComponent(Dictionary<string, string> iniData)
        {
            if (LocalizationRead.dicts.ContainsKey("qst"))
            {
                localizationDict = LocalizationRead.dicts["qst"];
            }
            else
            {
                localizationDict = new DictionaryI18n(new string[1] { ".," + Game.Get().currentLang }, defaultLanguage);
            }
            valid = Populate(iniData);
        }

        /// <summary>
        /// Create from ini data
        /// </summary>
        /// <param name="iniData">ini data to populate Quest</param>
        /// <returns>true if the Quest is valid</returns>
        public bool Populate(Dictionary<string, string> iniData)
        {
            if (iniData.ContainsKey("format"))
            {
                int.TryParse(iniData["format"], out format);
            }

            if (format > currentFormat || format < minumumFormat)
            {
                return false;
            }

            type = "";
            if (iniData.ContainsKey("type"))
            {
                type = iniData["type"];
            }

            if (iniData.ContainsKey("packs"))
            {
                packs = iniData["packs"].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                // Depreciated Format 5
                List<string> newPacks = new List<string>();
                foreach (string s in packs)
                {
                    if (s.Equals("MoM1E"))
                    {
                        newPacks.Add("MoM1ET");
                        newPacks.Add("MoM1EI");
                        newPacks.Add("MoM1EM");
                    }
                    else if (s.Equals("FA"))
                    {
                        newPacks.Add("FAT");
                        newPacks.Add("FAI");
                        newPacks.Add("FAM");
                    }
                    else if (s.Equals("CotW"))
                    {
                        newPacks.Add("CotWT");
                        newPacks.Add("CotWI");
                        newPacks.Add("CotWM");
                    }
                    else
                    {
                        newPacks.Add(s);
                    }
                }
                packs = newPacks.ToArray();
            }
            else
            {
                packs = new string[0];
            }

            if (iniData.ContainsKey("defaultlanguage"))
            {
                defaultLanguage = iniData["defaultlanguage"];
                localizationDict.defaultLanguage = defaultLanguage;
            }

            if (iniData.ContainsKey("defaultmusicon"))
            {
                bool.TryParse(iniData["defaultmusicon"], out defaultMusicOn);
            }
            else
            {
                defaultMusicOn = true;
            }

            if (iniData.ContainsKey("hidden"))
            {
                bool.TryParse(iniData["hidden"], out hidden);
            }

            if (iniData.ContainsKey("minhero"))
            {
                int.TryParse(iniData["minhero"], out minHero);
            }
            if (minHero < 1) minHero = 1;

            maxHero = Game.Get().gameType.DefaultHeroes();
            if (iniData.ContainsKey("maxhero"))
            {
                int.TryParse(iniData["maxhero"], out maxHero);
            }
            if (maxHero > Game.Get().gameType.MaxHeroes())
            {
                maxHero = Game.Get().gameType.MaxHeroes();
            }

            if (iniData.ContainsKey("difficulty"))
            {
                float.TryParse(iniData["difficulty"], out difficulty);
            }

            if (iniData.ContainsKey("lengthmin"))
            {
                int.TryParse(iniData["lengthmin"], out lengthMin);
            }
            if (iniData.ContainsKey("lengthmax"))
            {
                int.TryParse(iniData["lengthmax"], out lengthMax);
            }

            if (iniData.ContainsKey("image"))
            {
                string value = iniData["image"];
                image = value != null ? value.Replace('\\', '/') : value;
            }

            // parse data for scenario explorer
            version = "";
            if (iniData.ContainsKey("version"))
            {
                version = iniData["version"];
            }

            languages_name = new Dictionary<string, string>();
            if (iniData.ContainsKey("name." + defaultLanguage))
            {
                foreach (KeyValuePair<string, string> kv in iniData)
                {
                    if (kv.Key.Contains("name."))
                    {
                        languages_name.Add(kv.Key.Substring(5), kv.Value);
                    }
                }
            }

            languages_synopsys = new Dictionary<string, string>();
            if (iniData.ContainsKey("synopsys." + defaultLanguage))
            {
                foreach (KeyValuePair<string, string> kv in iniData)
                {
                    if (kv.Key.Contains("synopsys."))
                    {
                        languages_synopsys.Add(kv.Key.Substring(9), kv.Value);
                    }
                }
            }

            languages_authors_short = new Dictionary<string, string>();
            if (iniData.ContainsKey("authors_short." + defaultLanguage))
            {
                foreach (KeyValuePair<string, string> kv in iniData)
                {
                    if (kv.Key.Contains("authors_short."))
                    {
                        languages_authors_short.Add(kv.Key.Substring(14), kv.Value);
                    }
                }
            }

            package_url = "";
            if (iniData.ContainsKey("url"))
            {
                package_url = iniData["url"];
            }

            latest_update = new System.DateTime(0);
            if (iniData.ContainsKey("latest_update"))
            {
                System.DateTime.TryParse(iniData["latest_update"], out latest_update);
            }

            return true;
        }


        // Save to string (editor)
        override public string ToString()
        {
            string nl = System.Environment.NewLine;
            StringBuilder r = new StringBuilder();
            r.AppendLine("[Quest]");
            r.Append("format=").AppendLine(currentFormat.ToString());
            r.Append("hidden=").AppendLine(hidden.ToString());
            r.Append("type=").AppendLine(Game.Get().gameType.TypeName());
            r.Append("defaultlanguage=").AppendLine(defaultLanguage);
            r.Append("defaultmusicon=").AppendLine(defaultMusicOn.ToString());
            if (packs.Length > 0)
            {
                r.Append("packs=");
                r.AppendLine(string.Join(" ", packs));
            }

            if (minHero != 2)
            {
                r.Append("minhero=").AppendLine(minHero.ToString());
            }
            if (maxHero != Game.Get().gameType.DefaultHeroes())
            {
                r.Append("maxhero=").AppendLine(maxHero.ToString());
            }

            if (difficulty != 0)
            {
                r.Append("difficulty=").AppendLine(difficulty.ToString());
            }

            if (lengthMin != 0)
            {
                r.Append("lengthmin=").AppendLine(lengthMin.ToString());
            }
            if (lengthMax != 0)
            {
                r.Append("lengthmax=").AppendLine(lengthMax.ToString());
            }
            if (image.Length > 0)
            {
                r.Append("image=").AppendLine(image);
            }

            if (version != "")
            {
                r.Append("version=").AppendLine(version);
            }

            foreach (KeyValuePair<string, string> kv in languages_name)
            {
                r.Append("name." + kv.Key + "=").AppendLine(kv.Value);
            }

            foreach (KeyValuePair<string, string> kv in languages_synopsys)
            {
                r.Append("synopsys." + kv.Key + "=").AppendLine(kv.Value);
            }

            foreach (KeyValuePair<string, string> kv in languages_authors_short)
            {
                r.Append("authors_short." + kv.Key + "=").AppendLine(kv.Value);
            }

            return r.ToString();
        }

        public List<string> GetMissingPacks(List<string> selected)
        {
            List<string> r = new List<string>();
            foreach (string s in packs)
            {
                if (!selected.Contains(s))
                {
                    r.Add(s);
                }
            }
            return r;
        }

        public string GetShortAuthor()
        {
            string authors_short_translation = "";

            // if languages_authors_short is available, we are online in scenarios explorer and don't have access to .txt files yet
            if (Game.Get().questsList.quest_list_mode == QuestListMode.ONLINE)
            {
                if (languages_authors_short.Count != 0)
                {
                    // Try to get current language
                    if (!languages_authors_short.TryGetValue(Game.Get().currentLang, out authors_short_translation))
                    {
                        // Try to get default language
                        if (!languages_authors_short.TryGetValue(defaultLanguage, out authors_short_translation))
                        {
                            // if not translated, returns unknown
                            authors_short_translation = new StringKey("val", "AUTHORS_UNKNOWN").Translate();
                        }
                    }
                }
            }
            else
            {
                authors_short_translation = authors_short.Translate(true);
            }

            if (authors_short_translation == "")
            {
                authors_short_translation = new StringKey("val", "AUTHORS_UNKNOWN").Translate();
            }

            if (authors_short_translation.Length > 80)
                authors_short_translation = authors_short_translation.Substring(0, 75) + "(...)";

            return authors_short_translation;
        }
    }
}
