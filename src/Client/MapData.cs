using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataPuller.Client
{
    class MapData
    {
        public static event Action<string> Update;
        public static void Send()
        {
            MapEvents.previousStaticData = new JsonData();
            Update(JsonConvert.SerializeObject(MapEvents.previousStaticData, Formatting.None));
        }

        //Level
        public static bool InLevel { get; internal set; }
        public static bool LevelPaused { get; internal set; }
        public static bool LevelFinished { get; internal set; }
        public static bool LevelFailed { get; internal set; }
        public static bool LevelQuit { get; internal set; }

        //Map
        public static string Hash { get; internal set; }
        public static string SongName { get; internal set; }
        public static string SongSubName { get; internal set; }
        public static string SongAuthor { get; internal set; }
        public static string Mapper { get; internal set; }
        public static string BSRKey { get; internal set; }
        public static string coverImage { get; internal set; }
        public static int Length { get; internal set; }
        public static double TimeScale { get; internal set; }

        //Difficulty
        public static string MapType { get; internal set; }
        public static string Difficulty { get; internal set; }
        public static string CustomDifficultyLabel { get; internal set; }
        public static int BPM { get; internal set; }
        public static double NJS { get; internal set; }
        public static Dictionary<string, bool> Modifiers { get; internal set; }
        public static float ModifiersMultiplier { get; internal set; }
        public static bool PracticeMode { get; internal set; }
        public static Dictionary<string, float> PracticeModeModifiers { get; internal set; }
        public static double PP { get; internal set; }
        public static double Star { get; internal set; }

        //Misc
        public static bool IsMultiplayer { get; internal set; }
        public static int PreviousRecord { get; internal set; }
        public static string PreviousBSR { get; internal set; }

        public class JsonData
        {
            public string GameVersion = UnityEngine.Application.version;
            public string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Level
            public bool InLevel = MapData.InLevel;
            public bool LevelPaused = MapData.LevelPaused;
            public bool LevelFinished = MapData.LevelFinished;
            public bool LevelFailed = MapData.LevelFailed;
            public bool LevelQuit = MapData.LevelQuit;

            //Map
            public string Hash = MapData.Hash;
            public string SongName = MapData.SongName;
            public string SongSubName = MapData.SongSubName;
            public string SongAuthor = MapData.SongAuthor;
            public string Mapper = MapData.Mapper;
            public string BSRKey = MapData.BSRKey;
            public string coverImage = MapData.coverImage;
            public int Length = MapData.Length;
            public double TimeScale = MapData.TimeScale;

            //Difficulty
            public string MapType = MapData.MapType;
            public string Difficulty = MapData.Difficulty;
            public string CustomDifficultyLabel = MapData.CustomDifficultyLabel;
            public int BPM = MapData.BPM;
            public double NJS = MapData.NJS;
            public Dictionary<string, bool> Modifiers = MapData.Modifiers;
            public float ModifiersMultiplier = MapData.ModifiersMultiplier;
            public bool PracticeMode = MapData.PracticeMode;
            public Dictionary<string, float> PracticeModeModifiers = MapData.PracticeModeModifiers;
            public double PP = MapData.PP;
            public double Star = MapData.Star;

            //Misc
            public bool IsMultiplayer = MapData.IsMultiplayer;
            public int PreviousRecord = MapData.PreviousRecord;
            public string PreviousBSR = MapData.PreviousBSR;
        }

        public static void Reset()
        {
            //Level Info
            InLevel = default;
            LevelPaused = default;
            LevelFinished = default;
            LevelFailed = default;
            LevelQuit = default;

            //Map Info
            Hash = default;
            SongName = default;
            SongSubName = default;
            SongAuthor = default;
            Mapper = default;
            BSRKey = default;
            coverImage = default;
            CustomDifficultyLabel = default;
            Length = default;
            TimeScale = default;

            //Difficulty Info
            MapType = default;
            Difficulty = default;
            BPM = default;
            NJS = default;
            Modifiers = new Dictionary<string, bool>();
            ModifiersMultiplier = 1;
            PracticeMode = default;
            PracticeModeModifiers = new Dictionary<string, float>();
            PP = default;
            Star = default;

            //Misc
            IsMultiplayer = false;
            PreviousRecord = default;
        }
    }
}
