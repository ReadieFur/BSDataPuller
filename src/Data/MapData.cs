using System.Reflection;
using DataPuller.Attributes;
using Newtonsoft.Json;

#nullable enable
namespace DataPuller.Data
{
    public class MapData : AData
    {
        #region Singleton
        /// <summary>
        /// The singleton instance that DataPuller writes to.
        /// </summary>
        [JsonIgnore] public static readonly MapData Instance = new();
        #endregion

        #region Properties
        #region Level
        /// <summary></summary>
        /// <remarks>
        /// This can remain <see href="false"/> even if <see cref="LevelFailed"/> is <see href="true"/>,
        /// when <see cref="Modifiers.NoFailOn0Energy"/> is <see href="true"/>.
        /// </remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool InLevel { get; internal set; }
        
        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool LevelPaused { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool LevelFinished { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool LevelFailed { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool LevelQuit { get; internal set; }
        #endregion

        #region Map
        /// <summary>The hash ID for the current map.</summary>
        /// <remarks><see href="null"/> if the hash could not be determined (e.g. if the map is not a custom level).</remarks>
        /// <value>Default is <see href="null"/>.</value>
        public string? Hash { get; internal set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>The name of the current map.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string SongName { get; internal set; }

        /// <summary>The sub-name of the current map.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string SongSubName { get; internal set; }

        /// <summary>The author of the song.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string SongAuthor { get; internal set; }

        /// <summary>The mapper of the current chart.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string Mapper { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary></summary>
        /// <remarks><see href="null"/> if the BSR key could not be obtained.</remarks>
        /// <value>Default is <see href="null"/>.</value>
        public string? BSRKey { get; internal set; }

        /// <summary></summary>
        /// <remarks><see href="null"/> if the cover image could not be obtained.</remarks>
        /// <value>Default is <see href="null"/>.</value>
        public string? CoverImage { get; internal set; }

        /// <summary>The duration of the map in seconds.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int Duration { get; internal set; }
        #endregion

        #region Difficulty
#pragma warning disable CS8618
        /// <summary>The type of map.</summary>
        /// <remarks>i.e. Standard, 360, OneSaber, etc.</remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string MapType { get; internal set; }

        /// <summary>The map's environment.</summary>
        /// <remarks>i.e. TheSecondEnvironment, WeaveEnvironment, etc.</remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string Environment { get; internal set; }

        /// <summary>The standard difficulty label of the map.</summary>
        /// <remarks>i.e. Easy, Normal, Hard, etc.</remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        [DefaultValue("")]
        public string Difficulty { get; internal set; }
#pragma warning restore CS8618

        /// <summary>The custom difficulty label set by the mapper.</summary>
        /// <remarks><see href="null"/> if there is none.</remarks>
        /// <value>Default is <see cref="string.Empty"/>.</value>
        public string? CustomDifficultyLabel { get; internal set; }

        /// <summary>The beats per minute of the current map.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int BPM { get; internal set; }

        /// <summary>The note jump speed of the current map.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public double NJS { get; internal set; }

#pragma warning disable CS8618
        /// <summary>The modifiers selected by the player for the current level.</summary>
        /// <remarks>i.e. No fail, No arrows, Ghost notes, etc.</remarks>
        /// <value>Default is <see cref="Data.Modifiers"/>.</value>
        [DefaultValueT<Modifiers>]
        public Modifiers Modifiers { get; internal set; }

#pragma warning restore CS8618
        /// <summary>The score multiplier set by the users selection of modifiers.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="1.0"/>.</value>
        [DefaultValue(1.0f)]
        public float ModifiersMultiplier { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        public bool PracticeMode { get; internal set; }

        /// <summary>The modifiers selected by the user that are specific to practice mode.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="Data.PracticeModeModifiers"/>.</value>
#pragma warning disable CS8618
        [DefaultValueT<PracticeModeModifiers>]
        public PracticeModeModifiers PracticeModeModifiers { get; internal set; }

        /// <summary>The amount Play Points this map is worth.</summary>
        /// <remarks><see href="0"/> if the map is unranked or the value was undetermined.</remarks>
        /// <value>Default is <see href="0"/>.</value>
#pragma warning restore CS8618
        public double PP { get; internal set; }

        /// <summary></summary>
        /// <remarks><see href="0"/> if the value was undetermined.</remarks>
        /// <value>Default is <see href="0"/>.</value>
        public double Star { get; internal set; }
        #endregion

        #region Misc
        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="UnityEngine.Application.version"/>.</value>
        [JsonProperty]
        public static readonly string GameVersion = UnityEngine.Application.version;

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value><see cref="System.Version"/>.</value>
        [JsonProperty]
        public static readonly string PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="false"/>.</value>
        [DefaultValue(false)]
        public bool IsMultiplayer { get; internal set; }
        
        /// <summary>The maximum number of players that can join the current lobby.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        [DefaultValue(0)]
        public int MultiplayerLobbyMaxSize { get; internal set; }
        
        /// <summary>The number of players connected to the current lobby.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        [DefaultValue(0)]
        public int MultiplayerLobbyCurrentSize { get; internal set; }

        /// <summary>The previous local record set by the player for this map specific mode and difficulty.</summary>
        /// <remarks><see href="0"/> if the map variant hasn't never been played before.</remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int PreviousRecord { get; internal set; }

        /// <summary>The BSR key fore the last played map.</summary>
        /// <remarks><para>
        /// <see href="null"/> if there was no previous map or the previous maps BSR key was undetermined.<br/>
        /// This value won't be updated if the current map is the same as the last.
        /// </para></remarks>
        /// <value>Default is <see href="null"/>.</value>
        public string? PreviousBSR { get; internal set; }
        #endregion
        #endregion
    }
}
