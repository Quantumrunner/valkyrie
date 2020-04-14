using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Quest.Events
{
    public class StartQuestValkyrieEvent : ValkyrieEvent
    {
        public string name;

        public StartQuestValkyrieEvent(string n) : base(n)
        {
            name = n;
        }

        override public bool Disabled()
        {
            return false;
        }
    }
}
