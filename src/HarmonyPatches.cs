using DataPuller.Data;
using HarmonyLib;

#nullable enable
namespace DataPuller
{
    [HarmonyPatch(typeof(BeatmapObjectExecutionRatingsRecorder), "HandleScoringForNoteDidFinish")]
    internal class HandleScoringForNoteDidFinishPatch
    {
        static void Postfix(ScoringElement scoringElement)
        {
            if (scoringElement != null)
            {
                if (scoringElement is GoodCutScoringElement goodCutScoringElement)
                {
                    LiveData.Instance.Combo++;
                    LiveData.Instance.BlockHitScore = new int[]
                    {
                        goodCutScoringElement.cutScoreBuffer.beforeCutScore,
                        goodCutScoringElement.cutScoreBuffer.afterCutScore,
                        goodCutScoringElement.cutScoreBuffer.centerDistanceCutScore
                    };
                    LiveData.Instance.Score += goodCutScoringElement.cutScore * goodCutScoringElement.multiplier;
                    LiveData.Instance.MaxScore += goodCutScoringElement.maxPossibleCutScore * goodCutScoringElement.multiplier;
                }
            }
        }
    }
}
