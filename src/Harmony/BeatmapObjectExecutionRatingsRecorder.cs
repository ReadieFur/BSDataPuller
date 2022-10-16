using DataPuller.Data;
using HarmonyLib;

#nullable enable
namespace DataPuller.Harmony
{
    [HarmonyPatch(typeof(global::BeatmapObjectExecutionRatingsRecorder), nameof(global::BeatmapObjectExecutionRatingsRecorder.HandleScoringForNoteDidFinish))]
    internal class BeatmapObjectExecutionRatingsRecorder
    {
        [HarmonyPostfix]
        public static void HandleScoringForNoteDidFinish_PostFix(ScoringElement scoringElement)
        {
            if (scoringElement != null)
            {
                if (scoringElement is GoodCutScoringElement goodCutScoringElement)
                {
                    LiveData.Instance.BlockHitScore = new()
                    {
                        PreSwing = goodCutScoringElement.cutScoreBuffer.beforeCutScore,
                        PostSwing = goodCutScoringElement.cutScoreBuffer.afterCutScore,
                        CenterSwing = goodCutScoringElement.cutScoreBuffer.centerDistanceCutScore
                    };
                    LiveData.Instance.Score += goodCutScoringElement.cutScore * goodCutScoringElement.multiplier;
                    LiveData.Instance.MaxScore += goodCutScoringElement.maxPossibleCutScore * goodCutScoringElement.multiplier;
                    LiveData.Instance.MaxScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.Instance.MaxScore, MapData.Instance.ModifiersMultiplier);
                }
            }
        }
    }
}
