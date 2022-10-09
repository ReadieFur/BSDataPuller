using System.Collections.Generic;
using System.Reflection;
using DataPuller.Attributes;
using DataPuller.Core;
using Newtonsoft.Json;

#nullable enable
namespace DataPuller.Data
{
    public class MapData : AData
    {
        //Singleton.
        [JsonIgnore] public static readonly MapData Instance = new();

        [JsonProperty] public static readonly string GameVersion = UnityEngine.Application.version;
        [JsonProperty] public static readonly string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        //Level
        public bool InLevel { get; internal set; }
        public bool LevelPaused { get; internal set; }
        public bool LevelFinished { get; internal set; }
        public bool LevelFailed { get; internal set; }
        public bool LevelQuit { get; internal set; }

        //Map
        public string? Hash { get; internal set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [DefaultValue("")] public string SongName { get; internal set; }
        [DefaultValue("")] public string SongSubName { get; internal set; }
        [DefaultValue("")] public string SongAuthor { get; internal set; }
        [DefaultValue("")] public string Mapper { get; internal set; }
#pragma warning restore CS8618
        public string? BSRKey { get; internal set; }
        public string? CoverImage { get; internal set; }
        public int Length { get; internal set; }
        public double TimeScale { get; internal set; }

        //Difficulty
#pragma warning disable CS8618
        public string MapType { get; internal set; }
        public string Difficulty { get; internal set; }
#pragma warning restore CS8618
        public string? CustomDifficultyLabel { get; internal set; }
        public int BPM { get; internal set; }
        public double NJS { get; internal set; }
#pragma warning disable CS8618
        [DefaultValueT<Modifiers>] public Modifiers Modifiers { get; internal set; }
#pragma warning restore CS8618
        [DefaultValue(1.0f)] public float ModifiersMultiplier { get; internal set; }
        public bool PracticeMode { get; internal set; }
#pragma warning disable CS8618
        [DefaultValueT<PracticeModeModifiers>] public PracticeModeModifiers PracticeModeModifiers { get; internal set; }
#pragma warning restore CS8618
        public double PP { get; internal set; }
        public double Star { get; internal set; }

        //Misc
        [DefaultValue(false)] public bool IsMultiplayer { get; internal set; }
        public int PreviousRecord { get; internal set; }
        public string? PreviousBSR { get; internal set; }
    }
}
