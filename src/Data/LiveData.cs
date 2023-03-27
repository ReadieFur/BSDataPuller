using System;
using DataPuller.Attributes;
using Newtonsoft.Json;

#nullable enable
namespace DataPuller.Data
{
    public class LiveData : AData
    {
        #region Singleton
        /// <summary>
        /// The singleton instance that DataPuller writes to.
        /// </summary>
        [JsonIgnore] public static readonly LiveData Instance = new();
        #endregion

        #region Overrides
        [Obsolete("Use 'Send(ELiveDataEventTriggers)' instead.", true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        internal override void Send()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            Send(ELiveDataEventTriggers.Unknown);
        }

        public void Send(ELiveDataEventTriggers triggerType = ELiveDataEventTriggers.Unknown)
        {
            EventTrigger = triggerType;
            base.Send();
            lastSendTime = DateTime.MinValue;
        }
        #endregion

        [JsonIgnore] public DateTime lastSendTime = DateTime.MinValue;

        #region Properties
        #region Score
        /// <summary>The current raw score.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int Score { get; internal set; }

        /// <summary>The current score with the player selected multipliers applied.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int ScoreWithMultipliers { get; internal set; }

        /// <summary>The maximum possible raw score for the current number of cut notes.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int MaxScore { get; internal set; }

        /// <summary>The maximum possible score with the player selected multipliers applied for the current number of cut notes.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int MaxScoreWithMultipliers { get; internal set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>The <see cref="string"/> rank label for the current score.</summary>
        /// <remarks>i.e. SS, S, A, B, etc.</remarks>
        /// <value>Default is <see href="SSS"/>.</value>
        [DefaultValue("SSS")]
        public string Rank { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="true"/>.</value>
        [DefaultValue(true)]
        public bool FullCombo { get; internal set; }

        /// <summary>The total number of notes spawned since the start position of the song until the current position in the song.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int NotesSpawned { get; internal set; }

        /// <summary>The current note cut combo count without error.</summary>
        /// <remarks>Resets back to <see href="0"/> when the player: misses a note, hits a note incorrectly, takes damage or hits a bomb.</remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int Combo { get; internal set; }

        /// <summary>The total number of missed and incorrectly hit notes since the start position of the song until the current position in the song.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        public int Misses { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="100"/>.</value>
        [DefaultValue(100)]
        public double Accuracy { get; internal set; }

        /// <summary>The individual scores for the last hit note.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="Data.SBlockHitScore"/>.</value>
        [DefaultValueT<SBlockHitScore>]
        public SBlockHitScore BlockHitScore { get; internal set; }

        /// <summary></summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="50"/>.</value>
        [DefaultValue(50)]
        public double PlayerHealth { get; internal set; }

        /// <summary>The colour of note that was last hit.</summary>
        /// <remarks><see cref="ColorType.None"/> if no note was previously hit or a bomb was hit.</remarks>
        /// <value>Default is <see cref="ColorType.None"/>.</value>
        [DefaultValue(ColorType.None)]
        public ColorType ColorType { get; internal set; }
        #endregion

        #region Misc
        /// <summary>The total amount of time in seconds since the start of the map.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see href="0"/>.</value>
        [DefaultValue(0)]
        public int TimeElapsed { get; internal set; }

        /// <summary>The event that caused the update trigger to be fired.</summary>
        /// <remarks></remarks>
        /// <value>Default is <see cref="ELiveDataEventTriggers.Unknown"/>.</value>
        [DefaultValue(ELiveDataEventTriggers.Unknown)]
        public ELiveDataEventTriggers EventTrigger { get; internal set; }
        #endregion
        #endregion
    }
}
