using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataPuller
{
    public class LevelInfo
    {
        public static event Action<string> jsonUpdated;
        public static void eventJsonUpdated()
        {
            Task.Run(() => { jsonUpdated(JsonConvert.SerializeObject(new NonStaticPublicLevelInfo(), Formatting.Indented));});
        }

        //Level
        public static bool InLevel { get; internal set; }
        public static bool LevelPaused { get; internal set; }
        public static bool LevelFinished { get; internal set; }
        public static bool LevelFailed { get; internal set; }
        public static bool LevelQuit { get; internal set; }

        //Map
        public static string SongName { get; internal set; }
        public static string SongSubName { get; internal set; }
        public static string SongAuthor { get; internal set; }
        public static string Mapper { get; internal set; }
        public static string BSRKey { get; internal set; }
        public static int BPM { get; internal set; }
        public static string coverImage { get; internal set; }
        public static int Length { get; internal set; }
        public static int PreviousRecord { get; internal set; }

        //Difficulty
        public static int Difficulty { get; internal set; }
        public static double NJS { get; internal set; }

        //Score
        public static bool FullCombo { get; internal set; } = true;
        public static int Score { get; internal set; }
        public static int Combo { get; internal set; }
        public static int Misses { get; internal set; }
        public static double Accuracy { get; internal set; } = 100;
        public static List<int> BlockHitScores { get; internal set; } = new List<int>();
        public static double PlayerHealth { get; set; } = 0.5;

        //Modifiers/Pratice Mode
        public static Dictionary<string, bool> Modifiers { get; internal set; } = new Dictionary<string, bool>();
        public static bool PraticeMode { get; internal set; }
        public static Dictionary<string, float> PraticeModeModifiers { get; internal set; } = new Dictionary<string, float>();

        //Misc
        public static int Timer { get; internal set; }
        public static string PreviousBSR { get; internal set; }
    }

    //For JSON
    public class NonStaticPublicLevelInfo
    {
        //Level
        public bool InLevel = LevelInfo.InLevel;
        public bool LevelPaused = LevelInfo.LevelPaused;
        public bool LevelFinished = LevelInfo.LevelFinished;
        public bool LevelFailed = LevelInfo.LevelFailed;
        public bool LevelQuit = LevelInfo.LevelQuit;

        //Map
        public string SongName = LevelInfo.SongName;
        public string SongSubName = LevelInfo.SongSubName;
        public string SongAuthor = LevelInfo.SongAuthor;
        public string Mapper = LevelInfo.Mapper;
        public string BSRKey = LevelInfo.BSRKey;
        public int BPM = LevelInfo.BPM;
        public string coverImage = LevelInfo.coverImage;
        public int Length = LevelInfo.Length;
        public int PreviousRecord = LevelInfo.PreviousRecord;

        //Difficulty
        public int Difficulty = LevelInfo.Difficulty;
        public double NJS = LevelInfo.NJS;

        //Score
        public bool FullCombo = LevelInfo.FullCombo;
        public int Score = LevelInfo.Score;
        public int Combo = LevelInfo.Combo;
        public int Misses = LevelInfo.Misses;
        public double Accuracy = LevelInfo.Accuracy;
        public List<int> BlockHitScores = LevelInfo.BlockHitScores;
        public double PlayerHealth = LevelInfo.PlayerHealth;

        //Modifiers/Pratice Mode
        public Dictionary<string, bool> Modifiers = LevelInfo.Modifiers;
        public bool PraticeMode = LevelInfo.PraticeMode;
        public Dictionary<string, float> PraticeModeModifiers = LevelInfo.PraticeModeModifiers;

        //Misc
        public int Timer = LevelInfo.Timer;
        public string PreviousBSR = LevelInfo.PreviousBSR;
    }
}
