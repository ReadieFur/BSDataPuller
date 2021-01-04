using BeatSaverSharp;
using BS_Utils.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;
using DataPuller.GameData;
using System.IO;
using SongDataCore.BeatStar;
using System.Collections.Generic;

namespace DataPuller
{
    class MapEvents
    {
        private BeatSaver beatSaver = new BeatSaver(new HttpOptions() { ApplicationName = "BSDataPuller", Version = Assembly.GetExecutingAssembly().GetName().Version });
        internal static StaticData.JsonData previous = new StaticData.JsonData();
        private static ScoreController scoreController = null;
        private NoteCutInfo noteCutInfo = null;
        private AudioTimeSyncController audioTimeSyncController = null;

        internal void Init()
        {
            BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
            BSEvents.noteWasCut += BSEvents_noteWasCut;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelQuit += BSEvents_levelQuit;
            BSEvents.noteWasMissed += BSEvents_noteWasMissed;
            BSEvents.songPaused += BSEvents_songPaused;
            BSEvents.songUnpaused += BSEvents_songUnpaused;
            BSEvents.energyDidChange += BSEvents_energyDidChange;
            timer.Elapsed += TimeElapsed_Elapsed;
        }

        #region Timer
        private Timer timer = new Timer { Interval = 250 };

        private void TimeElapsed_Elapsed(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = (int) Math.Round(audioTimeSyncController.songTime);
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.LastSend).TotalMilliseconds) > 950 / StaticData.PracticeModeModifiers["songSpeedMul"]) { LiveData.Send(); }
        }

        private void BSEvents_songPaused()
        {
            timer.Stop();
            LiveData.LevelPaused = true;
            LiveData.Send();
        }

        private void BSEvents_songUnpaused()
        {
            timer.Start();
            LiveData.LevelPaused = false;
            LiveData.Send();
        }
        #endregion

        private void BSEvents_energyDidChange(float health)
        {
            LiveData.PlayerHealth = health * 100;
            LiveData.Send();
        }

        private void BSEvents_noteWasMissed(NoteData noteData, int arg2)
        {
            if (noteData.colorType != ColorType.None)
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
                LiveData.Send();
            }
        }

        #region Scene exits
        private void BSEvents_levelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            LiveData.LevelQuit = true;
            SceneExit();
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            //Need to get new args from here
            LiveData.LevelFailed = true;
            SceneExit();
        }

        private void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            //Need to get new args from here
            LiveData.LevelFinished = true;
            SceneExit();
        }

        private void SceneExit()
        {
            timer.Stop();
            LiveData.InLevel = false;
            scoreController = null;
            noteCutInfo = null;
            LiveData.Send();
        }

        private void ResetData()
        {
            timer.Stop();
            StaticData.Reset();
            LiveData.Reset();
        }
        #endregion

        private void BSEvents_gameSceneLoaded()
        {
            try
            {
                ResetData();

                LiveData.InLevel = true;
                scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault(x => x.isActiveAndEnabled);
                scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
                scoreController.immediateMaxPossibleScoreDidChangeEvent += ScoreController_immediateMaxPossibleScoreDidChangeEvent;

                audioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault(x => x.isActiveAndEnabled);
                PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
                GameplayCoreSceneSetupData currentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;

                IBeatmapLevel levelData = currentMap.difficultyBeatmap.level;
                //string mapHash = levelData.levelID.Replace("custom_level_", "");
                bool isCustomLevel = true;
                string mapHash = string.Empty;
                try { mapHash = levelData.levelID.Split('_')[2]; } catch { isCustomLevel = false; }
                isCustomLevel = isCustomLevel && mapHash.Length == 40 ? true : false;

                var difficultyData = SongCore.Collections.RetrieveDifficultyData(currentMap.difficultyBeatmap);

                /*var ico = currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.icon.texture;
                byte[] bytes = ico.GetRawTextureData();
                string enc = Convert.ToBase64String(bytes);
                Logger.log.Info(enc);*/

                StaticData.Hash = isCustomLevel ? mapHash : null;
                StaticData.SongName = levelData.songName;
                StaticData.SongSubName = levelData.songSubName;
                StaticData.SongAuthor = levelData.songAuthorName;
                StaticData.Mapper = levelData.levelAuthorName;
                StaticData.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
                StaticData.Length = Convert.ToInt32(Math.Round(audioTimeSyncController.songLength));
                PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, currentMap.difficultyBeatmap.difficulty,
                    currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
                StaticData.PreviousRecord = playerLevelStats.highScore;
                StaticData.MapType = currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
                StaticData.Difficulty = currentMap.difficultyBeatmap.difficulty.ToString("g");
                StaticData.NJS = currentMap.difficultyBeatmap.noteJumpMovementSpeed;
                StaticData.CustomDifficultyLabel = difficultyData?._difficultyLabel ?? null;
                /*Use a different method
                StaticData.ColLeft = difficultyData?._colorLeft ?? null;
                StaticData.ColRight = difficultyData?._colorRight ?? null;
                StaticData.EnvLeft = difficultyData?._envColorLeft ?? null;
                StaticData.EnvRight = difficultyData?._envColorRight ?? null;
                StaticData.EnvLeft2= difficultyData?._envColorLeftBoost ?? null;
                StaticData.EnvRight2 = difficultyData?._envColorRightBoost ?? null;
                StaticData.ObstacleColor = difficultyData?._obstacleColor ?? null;*/

                songDataCoreCurrent sdc = new songDataCoreCurrent { available = isCustomLevel ? SongDataCore.Plugin.Songs.IsDataAvailable() : false };
                if (sdc.available)
                {
                    //sdc.map = SongDataCore.Plugin.Songs.Data.Songs[mapHash];
                    BeatStarSong map;
                    SongDataCore.Plugin.Songs.Data.Songs.TryGetValue(mapHash, out map);
                    sdc.map = map;
                    if (sdc.map != null)
                    {
                        Dictionary<string, BeatStarSongDifficultyStats> diffs = sdc.map.characteristics[(BeatStarCharacteristics)Enum.Parse(typeof(BeatStarCharacteristics),
                            currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName)];
                        sdc.stats = diffs[StaticData.Difficulty == "ExpertPlus" ? "Expert+" : StaticData.Difficulty];
                        StaticData.PP = sdc.stats.pp;
                        StaticData.Star = sdc.stats.star;
                    }
                    else { sdc.available = false; }
                }

                if (sdc.available)
                {
                    StaticData.BSRKey = sdc.map.key;
                    if (levelData is CustomPreviewBeatmapLevel customLevel) { StaticData.coverImage = GetBase64CoverImage(customLevel); }
                    else { getBeatsaverMap(); }
                }
                else { getBeatsaverMap(); }

                void getBeatsaverMap()
                {
                    Task.Run(async () =>
                    {
                        Beatmap bm = await beatSaver.Hash(mapHash);
                        if (bm != null)
                        {
                            StaticData.BSRKey = bm.Key;
                            StaticData.coverImage = BeatSaver.BaseURL + bm.CoverURL;
                        }
                        else { StaticData.BSRKey = null; }
                        StaticData.Send();
                    });
                }

                if (StaticData.Hash != previous.Hash) { StaticData.PreviousBSR = previous.BSRKey; }

                StaticData.Modifiers.Add("instaFail", currentMap.gameplayModifiers.instaFail);
                StaticData.Modifiers.Add("batteryEnergy", currentMap.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery);
                StaticData.Modifiers.Add("disappearingArrows", currentMap.gameplayModifiers.disappearingArrows);
                StaticData.Modifiers.Add("ghostNotes", currentMap.gameplayModifiers.ghostNotes);
                StaticData.Modifiers.Add("fasterSong", currentMap.gameplayModifiers.songSpeedMul == 1.2f ? true : false);
                StaticData.Modifiers.Add("noFail", currentMap.gameplayModifiers.noFail);
                LiveData.PlayerHealth = StaticData.Modifiers["noFail"] ? 100 : 50;
                StaticData.Modifiers.Add("noObstacles", currentMap.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles);
                StaticData.Modifiers.Add("noBombs", currentMap.gameplayModifiers.noBombs);
                StaticData.Modifiers.Add("slowerSong", currentMap.gameplayModifiers.songSpeedMul == 0.85f ? true : false);
                StaticData.Modifiers.Add("noArrows", currentMap.gameplayModifiers.noArrows);
                StaticData.PracticeMode = currentMap.practiceSettings != null ? true : false;
                StaticData.PracticeModeModifiers.Add("songSpeedMul", StaticData.PracticeMode ? currentMap.practiceSettings.songSpeedMul : 1);

                timer.Start();

                StaticData.Send();
                LiveData.Send();

            }
            catch (Exception ex) { Logger.log.Error(ex); }
        }

        private void ScoreController_scoreDidChangeEvent(int beforeMultipliers, int afterMultipliers)
        {
            LiveData.Score = beforeMultipliers;
            LiveData.ScoreWithMultipliers = afterMultipliers;
            RecalculateRankAndAccuracy();
        }

        private void ScoreController_immediateMaxPossibleScoreDidChangeEvent(int rawScore, int modifiedScore)
        {
            LiveData.MaxScore = rawScore;
            LiveData.MaxScoreWithMultipliers = modifiedScore;
            RecalculateRankAndAccuracy();
        }

        private void RecalculateRankAndAccuracy()
        {
            if (LiveData.MaxScore != 0)
            {
                RankModel.Rank rank = RankModel.GetRankForScore(LiveData.Score, LiveData.ScoreWithMultipliers, LiveData.MaxScore, LiveData.MaxScoreWithMultipliers);
                LiveData.Rank = RankModel.GetRankName(rank);
                LiveData.Accuracy = LiveData.Score / (float) LiveData.MaxScore * 100;
                LiveData.Send();
            }
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo nci, int arg3)
        {
            noteCutInfo = nci;

            if (noteCutInfo.allIsOK)
            {
                LiveData.Combo++;
                noteCutInfo.swingRatingCounter.didFinishEvent += SwingRatingCounter_didFinishEvent;
            }
            else
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
            }

            LiveData.Send();
        }

        private void SwingRatingCounter_didFinishEvent(ISaberSwingRatingCounter saberSwingRatingCounter)
        {
            ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out var beforeCutRawScore, out var afterCutRawScore, out var cutDistanceRawScore);
            var blockScoreWithoutModifier = beforeCutRawScore + afterCutRawScore + cutDistanceRawScore;
            LiveData.BlockHitScores.Add(blockScoreWithoutModifier);
            noteCutInfo.swingRatingCounter.didFinishEvent -= SwingRatingCounter_didFinishEvent;
            //LiveData.Send(); //BSEvents_noteWasCut will manage this
        }

        private static string GetBase64CoverImage(CustomPreviewBeatmapLevel level) //Thanks UnskilledFreak, very nice
        {
            if (level == null) { return null; }

            var coverPath = Path.Combine(level.customLevelPath, level.standardLevelInfoSaveData.coverImageFilename);

            if (coverPath == string.Empty) { return null; }

            var prefix = coverPath.Substring(0, coverPath.Length - 3) == "png" ? "png" : "jpeg";

            var coverData = File.ReadAllBytes(coverPath);
            var base64String = Convert.ToBase64String(coverData);

            return string.Concat("data:image/", prefix, ";base64,", base64String);
        }

        private class songDataCoreCurrent
        {
            public bool available { get; set; }
            public BeatStarSong map { get; set; }
            public BeatStarSongDifficultyStats stats { get; set; }
        }
    }
}