using DataPuller.Client;

namespace DataPuller.Controllers
{
    class SwingRatingCounterDidFinishController : ISaberSwingRatingCounterDidFinishReceiver
    {
        private NoteCutInfo noteCutInfo;

        public SwingRatingCounterDidFinishController(NoteCutInfo _noteCutInfo)
        {
            noteCutInfo = _noteCutInfo;
        }

        public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter)
        {
            ScoreModel.RawScoreWithoutMultiplier(saberSwingRatingCounter, noteCutInfo.cutDistanceToCenter, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore);
            LiveData.BlockHitScore = new int[] { beforeCutRawScore, afterCutRawScore, cutDistanceRawScore };
            noteCutInfo.swingRatingCounter.UnregisterDidFinishReceiver(this);
        }
    }
}
