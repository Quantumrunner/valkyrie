using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.HttpManagement;
using Assets.Scripts.Quest;
using Assets.Scripts.Quest.Heroes;
using UnityEngine;

namespace Assets.Scripts.StatsManager
{

    /*
     * Form : https://goo.gl/forms/jrC9oKh8EPMMdO2l2
     * 
     * Post URL: <form action="https://docs.google.com/forms/u/1/d/e/1FAIpQLSfiFPuQOTXJI54LI-WNvn1K6qCkM5xErxJdUUJRhCZthaIqcA/formResponse" target="_self" method="POST" id="mG61Hd">
     *
     * JSON : https://drive.google.com/uc?id=1lEhwFWrryzNH6DUMbte37G1p22SyDhu9&export=download
     * 
     */
    
// This class provides functions to submit game stats download game stats
    public class StatsManager
    {

        // stats for all scenario, downloaded from JSON
        public Stats_JSONobject stats_json;
        public Dictionary<string, ScenarioStats> scenarios_stats = null;

        public bool error_download = false;
        public string error_download_description = "";
        public bool download_ongoing = false;

        // stats for current scenario, to be submitted
        private PublishedGameStats gameStats = null;

        public void PrepareStats(string victory, int rating, string comments)
        {
            if (gameStats == null) gameStats = new PublishedGameStats();

            gameStats.Reset();

            gameStats.victory = victory;

            if (rating == 0)
                gameStats.rating = "not set";
            else
                gameStats.rating = rating.ToString();

            gameStats.comments = comments;

            Game game = Game.Get();
            Quest.Quest quest = game.quest;

            // Quest filename is the unique id
            gameStats.scenario_name = Path.GetFileName(game.quest.originalPath);

            // language is required to see the quality of translations
            gameStats.language_selected = game.currentLang;

            // Get number of heroes
            foreach (Hero h in quest.heroes)
            {
                if (h.heroData != null)
                {
                    gameStats.players_count++;
                    // remove leading 'hero' before hero name
                    gameStats.investigators_list += h.heroData.sectionName.Remove(0, 4) + ";";
                }
            }

            // Get the list of events
            if (quest.eventList != null)
            {
                foreach (string event_name in game.quest.eventList)
                {
                    gameStats.events_list += event_name + ";";
                }
            }
            else
            {
                gameStats.events_list = "no event (old save?)";
            }

            // max cell size of Google sheet is 50k characters
            if (gameStats.events_list.Length > 50000)
            {
                gameStats.events_list.Remove(88 + (gameStats.events_list.Length - 50000), 50000);
                gameStats.events_list =
                    "---Beginning of event list is not included to avoid exceeding google sheet max size---" +
                    gameStats.events_list;
            }

            // Get the list of vars
            if (quest.vars != null)
            {
                foreach (KeyValuePair<string, float> kv in quest.vars.vars)
                {
                    if (kv.Value != 0)
                    {
                        gameStats.vars_list += kv.Key + "=" + kv.Value.ToString() + ";";
                    }
                }
            }
            else
            {
                gameStats.events_list = "no vars (old save?)";
            }

            // max cell size of Google sheet is 50k characters
            if (gameStats.vars_list.Length > 50000)
            {
                gameStats.vars_list.Remove(88 + (gameStats.vars_list.Length - 50000), 50000);
                gameStats.vars_list =
                    "---Beginning of vars list is not included to avoid exceeding google sheet max size---" +
                    gameStats.events_list;
            }

            if (quest.duration >= 0)
            {
                TimeSpan current_duration = System.DateTime.UtcNow.Subtract(quest.start_time);
                gameStats.duration = quest.duration + (int) current_duration.TotalMinutes;
            }
            else
            {
                gameStats.duration = 0;
            }

            gameStats.quest_name = quest.qd.QuestIniComponent.name.Translate();

        }



        // send data to google forms
        public void PublishData()
        {
            WWWForm formFields = new WWWForm();

            formFields.AddField("entry.1875990408", gameStats.scenario_name);
            formFields.AddField("entry.989998412", gameStats.quest_name);
            formFields.AddField("entry.84574628", gameStats.victory);
            formFields.AddField("entry.227102998", gameStats.rating);
            formFields.AddField("entry.2125749314", gameStats.comments);
            formFields.AddField("entry.170795919", gameStats.duration.ToString());
            formFields.AddField("entry.376629889", gameStats.players_count.ToString());
            formFields.AddField("entry.1150567176", gameStats.investigators_list);
            formFields.AddField("entry.2106598722", gameStats.language_selected);
            formFields.AddField("entry.1047979960", gameStats.events_list);
            formFields.AddField("entry.571357850", gameStats.vars_list);

            // submit async
            HTTPManager.Upload(
                "https://docs.google.com/forms/u/1/d/e/1FAIpQLSfiFPuQOTXJI54LI-WNvn1K6qCkM5xErxJdUUJRhCZthaIqcA/formResponse?hl=en",
                formFields,
                StatsUpload_callback);
        }

        private void StatsUpload_callback(string data, bool error)
        {
            // todo : do something ?
        }

        // Download JSON
        public void DownloadStats()
        {
            download_ongoing = true;
            HTTPManager.Get("https://drive.google.com/uc?id=1lEhwFWrryzNH6DUMbte37G1p22SyDhu9&export=download",
                StatsDownload_callback);
        }

        private void StatsDownload_callback(string data, bool error, System.Uri uri)
        {
            download_ongoing = false;

            if (error)
            {
                error_download = true;
                error_download_description = data;
                return;
            }

            stats_json = JsonUtility.FromJson<Stats_JSONobject>(data);
            scenarios_stats = new Dictionary<string, ScenarioStats>();

            if (stats_json == null)
                Debug.Log("ERROR: Stat file is empty\n");

            // one entry per scenario
            foreach (ScenarioStats stats in stats_json.scenarios_stats)
            {
                scenarios_stats[stats.scenario_name] = stats;
            }
        }

    }
}