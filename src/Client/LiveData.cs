using System;
using Newtonsoft.Json;

#nullable enable
namespace DataPuller.Client
{
    public static class LiveData
    {
        public static DateTime LastSend = DateTime.Now;

        public static event Action<string>? OnUpdate;
        public static void Send(ELiveDataEventTriggers? triggerType = null)
        {
            EventTrigger = triggerType;
            OnUpdate?.Invoke(JsonConvert.SerializeObject(new JsonData(), Formatting.None));
            LastSend = DateTime.Now;
            EventTrigger = null;
        }

        //Score
        public static int Score { get; internal set; }
        public static int ScoreWithMultipliers { get; internal set; }
        public static int MaxScore { get; internal set; }
        public static int MaxScoreWithMultipliers { get; internal set; }
        public static string? Rank { get; internal set; }
        public static bool FullCombo { get; internal set; } = true;
        public static int Combo { get; internal set; }
        public static int Misses { get; internal set; }
        public static double Accuracy { get; internal set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static int[] BlockHitScore { get; internal set; }
#pragma warning restore CS8618
        public static double PlayerHealth { get; internal set; }
        public static ColorType ColorType { get; internal set; }

        //Misc
        public static int TimeElapsed { get; internal set; }
        public static long unixTimestamp { get; internal set; }
        public static ELiveDataEventTriggers? EventTrigger { get; internal set; }

        public class JsonData
        {
            //Score
            public int Score = LiveData.Score;
            public int ScoreWithMultipliers = LiveData.ScoreWithMultipliers;
            public int MaxScore = LiveData.MaxScore;
            public int MaxScoreWithMultipliers = LiveData.MaxScoreWithMultipliers;
            public string? Rank = LiveData.Rank;
            public bool FullCombo = LiveData.FullCombo;
            public int Combo = LiveData.Combo;
            public int Misses = LiveData.Misses;
            public double Accuracy = LiveData.Accuracy;
            public int[] BlockHitScore = LiveData.BlockHitScore;
            public double PlayerHealth = LiveData.PlayerHealth;
            public ColorType ColorType = LiveData.ColorType;

            //Misc
            public int TimeElapsed = LiveData.TimeElapsed;
            public long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            public ELiveDataEventTriggers? EventTrigger = LiveData.EventTrigger ?? null;
        }

        public static void Reset()
        {
            //Score Info
            FullCombo = true;
            Score = default;
            ScoreWithMultipliers = default;
            MaxScore = default;
            MaxScoreWithMultipliers = default;
            Rank = null;
            Combo = default;
            Misses = default;
            Accuracy = 100;
            BlockHitScore = new int[] { 0, 0, 0 };
            PlayerHealth = 50;
            ColorType = ColorType.None;

            //Misc
            TimeElapsed = 0;
            // unixTimestamp = default;
            EventTrigger = null;
        }
    }
}
