#if DEBUG
using IPA.Utilities;
using System;
using TMPro;
using Zenject;
using System.Linq;
using UnityEngine;
using System.Reflection;
//using ScoreSaber;
using HarmonyLib;

namespace DataPuller
{
    #region Notes
    /*LiveData.MaxScore = ScoreModel.MaxRawScoreForNumberOfNotes(scoreData.NoteCount);
    LiveData.MaxScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(_beforeCutRawScore + _afterCutRawScore + _cutDistanceRawScore, 1);

    int multiplier = 1;
    if (LiveData.Combo >= 8) { multiplier = 8; }
    else if (LiveData.Combo >= 4) { multiplier = 4; }
    else if (LiveData.Combo >= 2) { multiplier = 2; }
    score2 += (_beforeCutRawScore + _afterCutRawScore + _cutDistanceRawScore) * multiplier;
    Plugin.Logger.Info(score2.ToString());

    scoreData.blockScoreData.Add(new int[] { _beforeCutRawScore, _afterCutRawScore, _cutDistanceRawScore });

    private class ScoreData
    {
        public NoteCutInfo noteCutInfo = null;
        public int NoteCount = 0;
        public float Multiplier = 1;
        public float Modifiers = 1;
        public List<int[]> blockScoreData = new List<int[]>(); //Dont need this
    }*/

    /*var ico = currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.icon.texture;
    byte[] bytes = ico.GetRawTextureData();
    string enc = Convert.ToBase64String(bytes);
    Logger.log.Info(enc);*/

    /*StaticData.ColLeft = difficultyData?._colorLeft ?? null;
    StaticData.ColRight = difficultyData?._colorRight ?? null;
    StaticData.EnvLeft = difficultyData?._envColorLeft ?? null;
    StaticData.EnvRight = difficultyData?._envColorRight ?? null;
    StaticData.EnvLeft2= difficultyData?._envColorLeftBoost ?? null;
    StaticData.EnvRight2 = difficultyData?._envColorRightBoost ?? null;
    StaticData.ObstacleColor = difficultyData?._obstacleColor ?? null;*/
    #endregion

    class TestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TestClass>().AsSingle();
        }
    }

    class TestClass : IInitializable, IDisposable
    {
        [InjectOptional] RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        [InjectOptional] ScoreUIController scoreUIController;
        ScoreController scoreController;
        //ReplayPlayer rpv;

        public TestClass([InjectOptional] ScoreController _scoreController)
        {
            //relativeScoreAndImmediateRankCounter = _relativeScoreAndImmediateRankCounter;
            //scoreUIController = _scoreUIController;
            if (!(_scoreController is ScoreController)) { Plugin.Logger.Error("ScoreController not found"); }
            else { scoreController = _scoreController; }
        }

        public void Initialize()
        {

        }

        string ScoreName;
        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            /*if (!(rpv is ReplayPlayer))
            {
                rpv = Resources.FindObjectsOfTypeAll<ReplayPlayer>().FirstOrDefault();
            }

            if (rpv is ReplayPlayer)
            {
                Plugin.Logger.Info(rpv.playbackEnabled.ToString());
                if (ScoreName == null)
                {
                    FieldInfo[] fields = rpv.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                    int count = 0;
                    foreach (var o in fields)
                    {
                        if (o.FieldType == typeof(int))
                        {
                            if (count == 1)
                            {
                                Plugin.Logger.Info($"{o.Name} - {o.FieldType}: {o.GetValue(rpv)}");
                                ScoreName = o.Name;
                                break;
                            }
                            count++;
                        }
                    }
                }

                Plugin.Logger.Info("====");
                Plugin.Logger.Info(rpv.GetField<int, ReplayPlayer>(ScoreName).ToString());
            }*/

            TextMeshProUGUI textMeshProUGUI = scoreUIController.GetField<TextMeshProUGUI, ScoreUIController>("_scoreText");
            Plugin.Logger.Info(int.Parse(textMeshProUGUI.text.Replace(" ", "")).ToString());
            Plugin.Logger.Info((relativeScoreAndImmediateRankCounter.relativeScore * 100).ToString());
            Plugin.Logger.Info(relativeScoreAndImmediateRankCounter.immediateRank.ToString());
        }

        public void Dispose()
        {
            relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
        }
    }
}
#endif
