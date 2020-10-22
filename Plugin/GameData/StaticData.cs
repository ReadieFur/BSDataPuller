using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataPuller.GameData
{
    class StaticData
    {
        public static event Action<string> Update;
        public static void Send() { Update(JsonConvert.SerializeObject(new JsonData(), Formatting.Indented)); }

        //Map
        public static string SongName { get; internal set; }
        public static string SongSubName { get; internal set; }
        public static string SongAuthor { get; internal set; }
        public static string Mapper { get; internal set; }
        public static string BsrKey { get; internal set; }
        public static string CoverImage { get; internal set; }
        public static int Length { get; internal set; }

        //Difficulty
        public static int Difficulty { get; internal set; }
        public static int Bpm { get; internal set; }
        public static double Njs { get; internal set; }
        public static Dictionary<string, bool> Modifiers { get; internal set; } = new Dictionary<string, bool>();
        public static bool PracticeMode { get; internal set; }
        public static Dictionary<string, float> PracticeModeModifiers { get; internal set; } = new Dictionary<string, float>();

        //Misc
        public static int PreviousRecord { get; internal set; }
        public static string PreviousBsr { get; internal set; }

        public class JsonData
        {
            //Map
            public string SongName = StaticData.SongName;
            public string SongSubName = StaticData.SongSubName;
            public string SongAuthor = StaticData.SongAuthor;
            public string Mapper = StaticData.Mapper;
            public string BsrKey = StaticData.BsrKey;
            public string CoverImage = StaticData.CoverImage;
            public int Length = StaticData.Length;

            //Difficulty
            public int Difficulty = StaticData.Difficulty;
            public int Bpm = StaticData.Bpm;
            public double Njs = StaticData.Njs;
            public Dictionary<string, bool> Modifiers = StaticData.Modifiers;
            public bool PracticeMode = StaticData.PracticeMode;
            public Dictionary<string, float> PracticeModeModifiers = StaticData.PracticeModeModifiers;

            //Misc
            public int PreviousRecord = StaticData.PreviousRecord;
            public string PreviousBsr = StaticData.PreviousBsr;

        }

        public static void Reset()
        {
            //Map Info
            SongName = default;
            SongSubName = default;
            SongAuthor = default;
            Mapper = default;
            BsrKey = default;
            CoverImage = default;
            Length = default;

            //Difficult Info
            Difficulty = default;
            Bpm = default;
            Njs = default;
            Modifiers = new Dictionary<string, bool>();
            PracticeMode = default;
            PracticeModeModifiers = new Dictionary<string, float>();

            //Misc
            PreviousRecord = default;
        }
    }
}
