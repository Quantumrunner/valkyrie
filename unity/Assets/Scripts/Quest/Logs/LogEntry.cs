using UnityEngine;

namespace Assets.Scripts.Quest.Logs
{
    public class LogEntry
    {
        string entry;
        bool valkyrie = false;
        bool editor = false;

        public LogEntry(string e, bool editorIn = false, bool valkyrieIn = false)
        {
            entry = e;
            editor = editorIn;
            valkyrie = valkyrieIn;
        }

        public LogEntry(string type, string e)
        {
            if (type.IndexOf("valkyrie") == 0)
            {
                valkyrie = true;
            }
            if (type.IndexOf("editor") == 0)
            {
                editor = true;
            }
            entry = e;
        }

        public string ToString(int id)
        {
            string r = "";
            if (valkyrie)
            {
                r += "valkyrie" + id + "=";
            }
            else if (editor)
            {
                r += "editor" + id + "=";
            }
            else
            {
                r += "Quest" + id + "=";
            }
            r += entry.Replace("\n", "\\n") + System.Environment.NewLine;
            return r;
        }

        public string GetEntry(bool editorSet = false)
        {
            if (valkyrie && !Application.isEditor)
            {
                return "";
            }
            if (editor && !editorSet)
            {
                return "";
            }
            return entry.Replace("\\n", "\n") + "\n\n";
        }
    }
}
