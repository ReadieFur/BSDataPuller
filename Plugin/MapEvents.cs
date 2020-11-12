using BeatSaverSharp;
using BS_Utils.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;
using DataPuller.GameData;
using System.Diagnostics;
using System.IO;
using IPA.Utilities;

namespace DataPuller
{
    class MapEvents
    {
        //Make use of SongDataCore plugin to get Beatmap infos
        private BeatSaver beatSaver;
        private GameplayCoreSceneSetupData previousMap;
        private static ScoreController scoreController;
        private Beatmap previousBeatmap;
        private NoteCutInfo noteCutInfo;
        private Timer timer;
        private AudioTimeSyncController audioTimeSyncController;

        internal void Init()
        {
            timer = new Timer
            {
                Interval = 250
            };

            timer.Elapsed += TimeElapsed_Elapsed;

            beatSaver = new BeatSaver(new HttpOptions
            {
                ApplicationName = "BSDataPuller",
                Version = Assembly.GetExecutingAssembly().GetName().Version
            });

            BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
            BSEvents.noteWasCut += BSEvents_noteWasCut;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelQuit += BSEvents_levelQuit;
            BSEvents.noteWasMissed += BSEvents_noteWasMissed;
            BSEvents.songPaused += BSEvents_songPaused;
            BSEvents.songUnpaused += BSEvents_songUnpaused;
            BSEvents.energyDidChange += BSEvents_energyDidChange;
        }

        private void TimeElapsed_Elapsed(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = Convert.ToInt32(Math.Ceiling(audioTimeSyncController.songTime) + 1);
            LiveData.Send();
        }

        private void BSEvents_songPaused()
        {
            LiveData.LevelPaused = true;
            LiveData.Send();
        }

        private void BSEvents_songUnpaused()
        {
            LiveData.LevelPaused = false;
            LiveData.Send();
        }

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
            LiveData.Send();
            scoreController = null;
            noteCutInfo = null;
        }

        private void ResetData()
        {
            StaticData.Reset();
            LiveData.Reset();
        }

        private void BSEvents_gameSceneLoaded()
        {
            ResetData();

            LiveData.InLevel = true;
            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault();
            scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
            audioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            var playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().LastOrDefault().playerData;
            var currentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;
            var levelData = currentMap.difficultyBeatmap.level;

            StaticData.SongName = levelData.songName;
            StaticData.SongSubName = levelData.songSubName;
            StaticData.SongAuthor = levelData.songAuthorName;
            StaticData.Mapper = levelData.levelAuthorName;
            StaticData.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            StaticData.Length = Convert.ToInt32(Math.Round(audioTimeSyncController.songLength));
            StaticData.TimeScale = audioTimeSyncController.timeScale;
            var playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, currentMap.difficultyBeatmap.difficulty, currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            StaticData.PreviousRecord = playerLevelStats.highScore;
            StaticData.coverImage = null;

            SetCustomDifficultyLevel(currentMap.difficultyBeatmap);

            if (levelData is CustomPreviewBeatmapLevel customLevel)
            {
                StaticData.coverImage = GetBase64CoverImage(customLevel);
            }

            if (previousMap == null || previousBeatmap == null || levelData.levelID != previousMap.difficultyBeatmap.level.levelID)
            {
                Task.Run(async () =>
                {
                    if (previousBeatmap != null)
                    {
                        StaticData.PreviousBSR = previousBeatmap.Key;
                    }

                    var bm = await beatSaver.Hash(levelData.levelID.Replace("custom_level_", ""));
                    if (bm != null)
                    {
                        StaticData.BSRKey = bm.Key;
                        previousBeatmap = bm;

                        if (StaticData.coverImage == null && bm.CoverURL != "")
                        {
                            StaticData.coverImage = BeatSaver.BaseURL + bm.CoverURL;
                        }
                    }
                    else
                    {
                        StaticData.BSRKey = null;
                        previousBeatmap = null;
                    }

                    StaticData.Send();
                });
            }
            else
            {
                StaticData.BSRKey = previousBeatmap.Key;
            }

            StaticData.Difficulty = currentMap.difficultyBeatmap.difficultyRank;
            StaticData.NJS = currentMap.difficultyBeatmap.noteJumpMovementSpeed;

            StaticData.Modifiers.Add("instaFail", currentMap.gameplayModifiers.instaFail);
            StaticData.Modifiers.Add("batteryEnergy", currentMap.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery);
            StaticData.Modifiers.Add("disappearingArrows", currentMap.gameplayModifiers.disappearingArrows);
            StaticData.Modifiers.Add("ghostNotes", currentMap.gameplayModifiers.ghostNotes);
            StaticData.Modifiers.Add("fasterSong", currentMap.gameplayModifiers.songSpeedMul == 1.2f);
            StaticData.Modifiers.Add("noFail", currentMap.gameplayModifiers.noFail);
            LiveData.PlayerHealth = StaticData.Modifiers["noFail"] ? 100 : 50;
            StaticData.Modifiers.Add("noObstacles", currentMap.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles);
            StaticData.Modifiers.Add("noBombs", currentMap.gameplayModifiers.noBombs);
            StaticData.Modifiers.Add("slowerSong", currentMap.gameplayModifiers.songSpeedMul == 0.85f);
            StaticData.Modifiers.Add("noArrows", currentMap.gameplayModifiers.noArrows);

            if (currentMap.practiceSettings != null) //In Practice mode
            {
                StaticData.PracticeMode = true;
                StaticData.PracticeModeModifiers.Add("songSpeedMul", currentMap.practiceSettings.songSpeedMul);
            }

            previousMap = currentMap;
            StaticData.Send();
            LiveData.Send();
        }

        private void ScoreController_scoreDidChangeEvent(int arg1, int arg2)
        {
            LiveData.Score = arg1;
            LiveData.Accuracy = arg1 / (float) scoreController.immediateMaxPossibleRawScore * 100;
            LiveData.Send();
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo nci, int arg3)
        {
            noteCutInfo = nci;

            if (noteCutInfo.allIsOK)
            {
                LiveData.Combo++;
                if (noteCutInfo == null)
                {
                    noteCutInfo.swingRatingCounter.didFinishEvent += SwingRatingCounter_didFinishEvent;
                }
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
            //LiveData.Send();
        }

        private void SetCustomDifficultyLevel(IDifficultyBeatmap difficultyBeatmap)
        {
            var difficultyData = SongCore.Collections.RetrieveDifficultyData(difficultyBeatmap);
            StaticData.CustomDifficultyLabel = difficultyData?._difficultyLabel ?? "";
        }

        private static string GetBase64CoverImage(CustomPreviewBeatmapLevel level)
        {
            if (level == null)
            {
                return null;
            }

            var coverPath = Path.Combine(level.customLevelPath, level.standardLevelInfoSaveData.coverImageFilename);

            if (coverPath == string.Empty)
            {
                return null;
            }

            var prefix = coverPath.Substring(0, coverPath.Length - 3) == "png" ? "png" : "jpeg";

            var coverData = File.ReadAllBytes(coverPath);
            var base64String = Convert.ToBase64String(coverData);

            return string.Concat("data:image/", prefix, ";base64,", base64String);
        }
    }
}