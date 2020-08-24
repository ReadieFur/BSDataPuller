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

namespace DataPuller
{
    class MapEvents
    {
        //Make use of SongDataCore plugin to get Beatmap infos
        private BeatSaver beatSaver = new BeatSaver(new HttpOptions() { ApplicationName = "BSDataPuller", Version = Assembly.GetExecutingAssembly().GetName().Version });
        private GameplayCoreSceneSetupData previousMap = null;
        private static ScoreController scoreController = null;
        private Beatmap previousBeatmap = null;
        private NoteCutInfo noteCutInfo = null;

        internal void Init()
        {
            TimeElapsedLogger.Interval = 1000;
            TimeElapsedLogger.Elapsed += TimeElapsed_Elapsed;

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

        #region Timer
        //This may only be temporary until I find a way to read the time elapsed from the game.
        private Timer TimeElapsedLogger = new Timer();
        private Stopwatch TimeElapsed = new Stopwatch();
        private TimeSpan startSongTime = new TimeSpan(0, 0, 0);

        private void TimeElapsed_Elapsed(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = (int)(startSongTime.Add(TimeElapsed.Elapsed).TotalMilliseconds / 1000);
            if (TimeElapsedLogger.Interval != 1000) { TimeElapsedLogger.Interval = 1000; }
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.LastSend).TotalMilliseconds) > 900) { LiveData.Send(); }
        }

        private void BSEvents_songPaused()
        {
            TimeElapsedLogger.Stop();
            TimeElapsed.Stop();
            TimeElapsedLogger.Interval = TimeElapsed.ElapsedMilliseconds % 1000;
            LiveData.LevelPaused = true;
            LiveData.Send();
        }

        private void BSEvents_songUnpaused()
        {
            TimeElapsedLogger.Start();
            TimeElapsed.Start();
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
            if (noteData.noteType != NoteType.Bomb)
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
                LiveData.Send();
            }
        }

        #region Scene Exits
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
            TimeElapsedLogger.Stop();
            TimeElapsed.Stop();
            LiveData.InLevel = false;
            LiveData.Send();
            scoreController = null;
            noteCutInfo = null;
        }
        #endregion

        private void ResetData()
        {
            TimeElapsedLogger.Stop();
            TimeElapsed.Stop();
            TimeElapsed.Reset();
            startSongTime = new TimeSpan(0, 0, 0);

            StaticData.Reset();
            LiveData.Reset();
        }

        private void BSEvents_gameSceneLoaded()
        {
            ResetData();

            LiveData.InLevel = true;
            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;

            AudioTimeSyncController audioController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            GameplayCoreSceneSetupData currentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;

            IBeatmapLevel levelData = currentMap.difficultyBeatmap.level;

            StaticData.SongName = levelData.songName;
            StaticData.SongSubName = levelData.songSubName;
            StaticData.SongAuthor = levelData.songAuthorName;
            StaticData.Mapper = levelData.levelAuthorName;
            StaticData.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            StaticData.Length = Convert.ToInt32(Math.Round(audioController.songLength));
            PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, currentMap.difficultyBeatmap.difficulty,
                currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            StaticData.PreviousRecord = playerLevelStats.highScore;

            if (previousMap == null || previousBeatmap == null || (levelData.levelID != previousMap.difficultyBeatmap.level.levelID))
            {
                Task.Run(async () =>
                {
                    if (previousBeatmap != null) { StaticData.PreviousBSR = previousBeatmap.Key; }
                    Beatmap bm = await beatSaver.Hash(levelData.levelID.Replace("custom_level_", ""));
                    if (bm != null)
                    {
                        StaticData.BSRKey = bm.Key;
                        StaticData.coverImage = BeatSaver.BaseURL + bm.CoverURL;
                        previousBeatmap = bm;
                    }
                    else { StaticData.BSRKey = null; StaticData.coverImage = null; previousBeatmap = null; }
                    StaticData.Send();
                });
            }
            else { StaticData.BSRKey = previousBeatmap.Key; StaticData.coverImage = BeatSaver.BaseURL + previousBeatmap.CoverURL; }

            StaticData.Difficulty = currentMap.difficultyBeatmap.difficultyRank;
            StaticData.NJS = currentMap.difficultyBeatmap.noteJumpMovementSpeed;

            StaticData.Modifiers.Add("instaFail", currentMap.gameplayModifiers.instaFail);
            StaticData.Modifiers.Add("batteryEnergy", currentMap.gameplayModifiers.batteryEnergy);
            StaticData.Modifiers.Add("disappearingArrows", currentMap.gameplayModifiers.disappearingArrows);
            StaticData.Modifiers.Add("ghostNotes", currentMap.gameplayModifiers.ghostNotes);
            StaticData.Modifiers.Add("fasterSong", currentMap.gameplayModifiers.songSpeedMul == 1.2f ? true : false);
            StaticData.Modifiers.Add("noFail", currentMap.gameplayModifiers.noFail);
            LiveData.PlayerHealth = StaticData.Modifiers["noFail"] ? 1 : 0.5;
            StaticData.Modifiers.Add("noObstacles", currentMap.gameplayModifiers.noObstacles);
            StaticData.Modifiers.Add("noBombs", currentMap.gameplayModifiers.noBombs);
            StaticData.Modifiers.Add("slowerSong", currentMap.gameplayModifiers.songSpeedMul == 0.85f ? true : false);
            StaticData.Modifiers.Add("noArrows", currentMap.gameplayModifiers.noArrows);
            if (currentMap.practiceSettings != null) //In pratice mode
            {
                StaticData.PraticeMode = true;
                startSongTime = new TimeSpan(0, 0, (int)Math.Round(currentMap.practiceSettings.startSongTime) - 1); //1s time desync
                StaticData.PraticeModeModifiers.Add("songSpeedMul", currentMap.practiceSettings.songSpeedMul);
            }

            previousMap = currentMap;

            TimeElapsed.Start();
            TimeElapsedLogger.Start();

            StaticData.Send();
            LiveData.Send();
        }

        private void ScoreController_scoreDidChangeEvent(int arg1, int arg2)
        {
            LiveData.Score = arg1;
            LiveData.Accuracy = arg1 / scoreController.immediateMaxPossibleRawScore * 100f;
            LiveData.Send();
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo nci, int arg3)
        {
            noteCutInfo = nci;

            if (noteCutInfo.allIsOK)
            {
                LiveData.Combo++;
                if (noteCutInfo == null) { noteCutInfo.swingRatingCounter.didFinishEvent += SwingRatingCounter_didFinishEvent; }
            }
            else
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
            }
            LiveData.Send();
        }

        private void SwingRatingCounter_didFinishEvent(SaberSwingRatingCounter SaberSwingRatingCounter)
        {
            ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore);
            int blockScoreWithoutModifier = beforeCutRawScore + afterCutRawScore + cutDistanceRawScore;
            LiveData.BlockHitScores.Add(blockScoreWithoutModifier);
            //LiveData.Send();
        }
    }
}