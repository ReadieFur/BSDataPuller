using DataPuller.Client;
using HarmonyLib;

namespace DataPuller
{
    [HarmonyPatch(typeof(BeatmapObjectExecutionRatingsRecorder), "HandleScoringForNoteDidFinish")]
    internal class HandleScoringForNoteDidFinishPatch
    {
        static void Postfix(ScoringElement scoringElement)
        {
            if (scoringElement != null)
            {
                GoodCutScoringElement goodCutScoringElement;
                if ((goodCutScoringElement = (scoringElement as GoodCutScoringElement)) != null)
                {
                    LiveData.Combo++;
                    LiveData.BlockHitScore = new int[] {goodCutScoringElement.cutScoreBuffer.beforeCutScore, goodCutScoringElement.cutScoreBuffer.afterCutScore, goodCutScoringElement.cutScoreBuffer.centerDistanceCutScore};
                    LiveData.Score += goodCutScoringElement.cutScore * goodCutScoringElement.multiplier;
                    LiveData.MaxScore += goodCutScoringElement.maxPossibleCutScore * goodCutScoringElement.multiplier;
                }
            }
        }
    }
}
