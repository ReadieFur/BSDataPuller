using DataPuller.Client;
using HarmonyLib;

namespace DataPuller
{
    [HarmonyPatch(typeof(BeatmapObjectExecutionRatingsRecorder), "HandleScoringForNoteDidFinish")]
    internal class HandleScoringForNoteDidFinishPatch
    {
        static void Postfix(ScoringElement scoringElement)
        {
            //Plugin.Logger.Debug("in Postfix");

            if (scoringElement != null)
            {
                /*NoteData noteData = scoringElement.noteData;
                if (noteData.colorType == ColorType.None)
                {
                    LiveData.FullCombo = false;
                }*/

                GoodCutScoringElement goodCutScoringElement;
                if ((goodCutScoringElement = (scoringElement as GoodCutScoringElement)) != null)
                {
                    LiveData.Combo++;
                    LiveData.BlockHitScore = new int[] {goodCutScoringElement.cutScoreBuffer.beforeCutScore, goodCutScoringElement.cutScoreBuffer.afterCutScore, goodCutScoringElement.cutScoreBuffer.centerDistanceCutScore};
                    LiveData.Score += goodCutScoringElement.cutScore * goodCutScoringElement.multiplier;
                    LiveData.MaxScore += goodCutScoringElement.maxPossibleCutScore * goodCutScoringElement.multiplier;

                    Plugin.Logger.Info("Postfix: " + LiveData.Combo + " " + LiveData.BlockHitScore[0] + "," + LiveData.BlockHitScore[1] + "," + LiveData.BlockHitScore[2] + " " + LiveData.Score + " " + LiveData.MaxScore);
                }

                /*if (scoringElement is BadCutScoringElement)
                {
                    LiveData.Misses++;
                    LiveData.FullCombo = false;
                }

                if (scoringElement is MissScoringElement)
                {
                    LiveData.Misses++;
                    LiveData.FullCombo = false;
                }*/
            }
        }
    }
}
