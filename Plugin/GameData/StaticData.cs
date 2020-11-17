using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataPuller.GameData
{
    class StaticData
    {
        public static event Action<string> Update;
        public static void Send()
        {
            MapEvents.previous = new JsonData();
            Update(JsonConvert.SerializeObject(MapEvents.previous, Formatting.Indented));
        }

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
        public static Dictionary<string, bool> Modifiers { get; internal set; } = new Dictionary<string, bool>();
        public static bool PracticeMode { get; internal set; }
        public static Dictionary<string, float> PracticeModeModifiers { get; internal set; } = new Dictionary<string, float>();
        public static double PP { get; internal set; }
        public static double Star { get; internal set; }

        //Misc
        public static int PreviousRecord { get; internal set; }
        public static string PreviousBSR { get; internal set; }
        /*public static MapColor ColLeft { get; internal set; }
        public static MapColor ColRight { get; internal set; }
        public static MapColor EnvLeft { get; internal set; }
        public static MapColor EnvRight { get; internal set; }
        public static MapColor EnvLeft2 { get; internal set; }
        public static MapColor EnvRight2 { get; internal set; }
        public static MapColor ObstacleColor { get; internal set; }*/

        public class JsonData
        {
            public string GameVersion = UnityEngine.Application.version;
            public string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Map
            public string Hash = StaticData.Hash;
            public string SongName = StaticData.SongName;
            public string SongSubName = StaticData.SongSubName;
            public string SongAuthor = StaticData.SongAuthor;
            public string Mapper = StaticData.Mapper;
            public string BSRKey = StaticData.BSRKey;
            public string coverImage = StaticData.coverImage;
            public int Length = StaticData.Length;
            public double TimeScale = StaticData.TimeScale;

            //Difficulty
            public string MapType = StaticData.MapType;
            public string Difficulty = StaticData.Difficulty;
            public string CustomDifficultyLabel = StaticData.CustomDifficultyLabel;
            public int BPM = StaticData.BPM;
            public double NJS = StaticData.NJS;
            public Dictionary<string, bool> Modifiers = StaticData.Modifiers;
            public bool PracticeMode = StaticData.PracticeMode;
            public Dictionary<string, float> PracticeModeModifiers = StaticData.PracticeModeModifiers;
            public double PP = StaticData.PP;
            public double Star = StaticData.Star;

            //Misc

            public int PreviousRecord = StaticData.PreviousRecord;
            public string PreviousBSR = StaticData.PreviousBSR;
            /*public MapColor ColLeft = StaticData.ColLeft;
            public MapColor ColRight = StaticData.ColRight;
            public MapColor EnvLeft = StaticData.EnvLeft;
            public MapColor EnvRight = StaticData.EnvRight;
            public MapColor EnvLeft2 = StaticData.EnvLeft2;
            public MapColor EnvRight2 = StaticData.EnvRight2;
            public MapColor ObstacleColor = StaticData.ObstacleColor;*/
        }

        public static void Reset()
        {
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
            PracticeMode = default;
            PracticeModeModifiers = new Dictionary<string, float>();
            PP = default;
            Star = default;

            //Misc
            PreviousRecord = default;
            //PreviousBSR = default;
            /*ColLeft = default;
            ColRight = default;
            EnvLeft = default;
            EnvRight = default;
            EnvLeft2 = default;
            EnvRight2 = default;
            ObstacleColor = default;*/
        }
    }
}
