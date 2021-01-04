using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataPuller.GameData
{
    class LiveData
    {
        public static DateTime LastSend = DateTime.Now;

        public static event Action<string> Update;
        public static void Send()
        {
            Update(JsonConvert.SerializeObject(new JsonData(), Formatting.None));
            LastSend = DateTime.Now;
        }

        //Level
        public static bool InLevel { get; internal set; }
        public static bool LevelPaused { get; internal set; }
        public static bool LevelFinished { get; internal set; }
        public static bool LevelFailed { get; internal set; }
        public static bool LevelQuit { get; internal set; }

        //Score
        public static int Score { get; internal set; }
        public static int ScoreWithMultipliers { get; internal set; }
        public static int MaxScore { get; internal set; }
        public static int MaxScoreWithMultipliers { get; internal set; }
        public static string Rank { get; internal set; } = "";
        public static bool FullCombo { get; internal set; } = true;
        public static int Combo { get; internal set; }
        public static int Misses { get; internal set; }
        public static double Accuracy { get; internal set; } = 100;
        public static List<int> BlockHitScores { get; internal set; } = new List<int>();
        public static double PlayerHealth { get; internal set; } = 50;

        //Misc
        public static int TimeElapsed { get; internal set; } = 0;

        public class JsonData
        {
            //Level
            public bool InLevel = LiveData.InLevel;
            public bool LevelPaused = LiveData.LevelPaused;
            public bool LevelFinished = LiveData.LevelFinished;
            public bool LevelFailed = LiveData.LevelFailed;
            public bool LevelQuit = LiveData.LevelQuit;

            //Score
            public int Score = LiveData.Score;
            public int ScoreWithMultipliers = LiveData.ScoreWithMultipliers;
            public int MaxScore = LiveData.MaxScore;
            public int MaxScoreWithMultipliers = LiveData.MaxScoreWithMultipliers;
            public string Rank = LiveData.Rank;
            public bool FullCombo = LiveData.FullCombo;
            public int Combo = LiveData.Combo;
            public int Misses = LiveData.Misses;
            public double Accuracy = LiveData.Accuracy;
            public List<int> BlockHitScores = LiveData.BlockHitScores;
            public double PlayerHealth = LiveData.PlayerHealth;

            //Misc
            public int TimeElapsed = LiveData.TimeElapsed;
        }

        public static void Reset()
        {
            //Level Info
            LevelPaused = default;
            LevelFinished = default;
            LevelFailed = default;
            LevelQuit = default;

            //Score Info
            FullCombo = true;
            Score = default;
            ScoreWithMultipliers = default;
            MaxScore = default;
            MaxScoreWithMultipliers = default;
            Rank = "";
            Combo = default;
            Misses = default;
            Accuracy = 100;
            BlockHitScores = new List<int>();
            PlayerHealth = 50;

            //Misc
            TimeElapsed = 0;
    }
    }
}
