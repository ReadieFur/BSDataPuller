using System;
using DataPuller.Attributes;
using Newtonsoft.Json;

#nullable enable
namespace DataPuller.Data
{
    public class LiveData : AData
    {
        //Singleton.
        [JsonIgnore] public static readonly LiveData Instance = new();

        [Obsolete("Use 'Send(ELiveDataEventTriggers)' instead.", true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        internal override void Send()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            EventTrigger = ELiveDataEventTriggers.Unknown;
            base.Send();
            lastSendTime = DateTime.MinValue;
        }

        public void Send(ELiveDataEventTriggers triggerType = ELiveDataEventTriggers.Unknown)
        {
            EventTrigger = triggerType;
            base.Send();
            lastSendTime = DateTime.MinValue;
        }

        [JsonIgnore] public DateTime lastSendTime = DateTime.MinValue;

        //Score
        public int Score { get; internal set; }
        public int ScoreWithMultipliers { get; internal set; }
        public int MaxScore { get; internal set; }
        public int MaxScoreWithMultipliers { get; internal set; }
        public string? Rank { get; internal set; }
        [DefaultValue(true)] public bool FullCombo { get; internal set; }
        public int NotesSpawned { get; internal set; }
        public int Combo { get; internal set; }
        public int Misses { get; internal set; }
        [DefaultValue(100)] public double Accuracy { get; internal set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [DefaultValue(new[] { 0, 0, 0 })] public int[] BlockHitScore { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [DefaultValue(50)] public double PlayerHealth { get; internal set; }
        [DefaultValue(ColorType.None)] public ColorType ColorType { get; internal set; }

        //Misc
        [DefaultValue(0)] public int TimeElapsed { get; internal set; }
        [DefaultValue(ELiveDataEventTriggers.Unknown)] public ELiveDataEventTriggers EventTrigger { get; internal set; }
    }
}
