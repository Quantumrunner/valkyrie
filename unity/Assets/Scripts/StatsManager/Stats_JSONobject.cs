using System.Collections.Generic;

namespace Assets.Scripts.StatsManager
{
    [System.Serializable]
    public class Stats_JSONobject
    {
        public string FileGenerationDate;
        public List<ScenarioStats> scenarios_stats;
    }
}
