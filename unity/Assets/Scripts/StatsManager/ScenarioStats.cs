namespace Assets.Scripts.StatsManager
{
    [System.Serializable]
    public struct ScenarioStats
    {
        /* We gather the following information for each scenario:
         *    scenario_name
         *    scenario_play_count  
         *    scenario_avg_rating  from 1 (awful) to 1O (amazing)
         *    scenario_avg_duration  in minutes
         *    scenario_avg_win_ratio from 0 to 1 (in percent)
         */
        public string scenario_name;
        public int scenario_play_count;
        public float scenario_avg_rating;
        public float scenario_avg_duration;
        public float scenario_avg_win_ratio;
    }
}
