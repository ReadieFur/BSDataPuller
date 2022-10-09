namespace DataPuller.Data
{
    public struct SBlockHitScore
    {
        /// <summary><see href="0"/> to <see href="70"/></summary>
        public int PreSwing { get; internal set; }
        /// <summary><see href="0"/> to <see href="30"/></summary>
        public int PostSwing { get; internal set; }
        /// <summary><see href="0"/> to <see href="15"/></summary>
        public int CenterSwing { get; internal set; }
    }
}
