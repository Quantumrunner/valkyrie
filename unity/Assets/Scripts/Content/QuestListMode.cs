using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Current mode to get Quest list
    // Default is local, it changes when file has been downloaded
    // It can also be set by user
    public enum QuestListMode
    {
        ONLINE,
        LOCAL,
        DOWNLOADING,
        ERROR_DOWNLOAD
    };
}
