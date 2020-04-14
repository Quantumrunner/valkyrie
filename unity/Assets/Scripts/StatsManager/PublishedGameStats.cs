namespace Assets.Scripts.StatsManager
{
    public class PublishedGameStats
    {
        /* Google Form is waiting for the following datas 
         * 
         *    Scenario ID/name (automatic)
         *    Victory (manual)
         *    Your rating for this scenario (1-10)  (manual)
         *    Optional: Comments / issue report / suggestion (manual)
         *    Game duration (automatic)
         *    Number of players (automatic)
         *    List of investigators (automatic)
         *    List of Events activated (automatic)
         */

        public string scenario_name = "";
        public string quest_name = "";
        public string victory = "";
        public string rating = "";
        public string comments = "";
        public int duration = 0;
        public int players_count = 0;
        public string investigators_list = "";
        public string language_selected = "";
        public string events_list = "";
        public string vars_list = "";


        public void Reset()
        {
            scenario_name = "";
            victory = "";
            rating = "";
            comments = "";
            duration = 0;
            players_count = 0;
            investigators_list = "";
            language_selected = "";
            events_list = "";
            vars_list = "";
        }

    }
}
