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
        public static string BSRKey { get; internal set; }
        public static string coverImage { get; internal set; }
        public static int Length { get; internal set; }

        //Difficulty
        public static int Difficulty { get; internal set; }
        public static int BPM { get; internal set; }
        public static double NJS { get; internal set; }
        public static Dictionary<string, bool> Modifiers { get; internal set; } = new Dictionary<string, bool>();
        public static bool PraticeMode { get; internal set; }
        public static Dictionary<string, float> PraticeModeModifiers { get; internal set; } = new Dictionary<string, float>();

        //Misc
        public static int PreviousRecord { get; internal set; }
        public static string PreviousBSR { get; internal set; }

        public class JsonData
        {
            //Map
            public string SongName = StaticData.SongName;
            public string SongSubName = StaticData.SongSubName;
            public string SongAuthor = StaticData.SongAuthor;
            public string Mapper = StaticData.Mapper;
            public string BSRKey = StaticData.BSRKey;
            public string coverImage = StaticData.coverImage;
            public int Length = StaticData.Length;

            //Difficulty
            public int Difficulty = StaticData.Difficulty;
            public int BPM = StaticData.BPM;
            public double NJS = StaticData.NJS;
            public Dictionary<string, bool> Modifiers = StaticData.Modifiers;
            public bool PraticeMode = StaticData.PraticeMode;
            public Dictionary<string, float> PraticeModeModifiers = StaticData.PraticeModeModifiers;

            //Misc
            public int PreviousRecord = StaticData.PreviousRecord;
            public string PreviousBSR = StaticData.PreviousBSR;

        }

        public static void Reset()
        {
            //Map Info
            SongName = default;
            SongSubName = default;
            SongAuthor = default;
            Mapper = default;
            BSRKey = default;
            coverImage = default;
            Length = default;

            //Difficult Info
            Difficulty = default;
            BPM = default;
            NJS = default;
            Modifiers = new Dictionary<string, bool>();
            PraticeMode = default;
            PraticeModeModifiers = new Dictionary<string, float>();

            //Misc
            PreviousRecord = default;
        }
    }
}
