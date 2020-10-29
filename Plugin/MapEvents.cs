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
        private BeatSaver _beatSaver = new BeatSaver(new HttpOptions() { ApplicationName = "BSDataPuller", Version = Assembly.GetExecutingAssembly().GetName().Version });
        private GameplayCoreSceneSetupData _previousMap = null;
        private static ScoreController _scoreController = null;
        private Beatmap _previousBeatmap = null;
        private NoteCutInfo _noteCutInfo = null;

        internal void Init()
        {
            _timeElapsedLogger.Interval = 1000;
            _timeElapsedLogger.Elapsed += TimeElapsed_Elapsed;

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
        private Timer _timeElapsedLogger = new Timer();
        private Stopwatch _timeElapsed = new Stopwatch();
        private TimeSpan _startSongTime = new TimeSpan(0, 0, 0);

        private void TimeElapsed_Elapsed(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = (int)(_startSongTime.Add(_timeElapsed.Elapsed).TotalMilliseconds / 1000);
            if (_timeElapsedLogger.Interval != 1000) { _timeElapsedLogger.Interval = 1000; }
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.LastSend).TotalMilliseconds) > 900) { LiveData.Send(); }
        }

        private void BSEvents_songPaused()
        {
            _timeElapsedLogger.Stop();
            _timeElapsed.Stop();
            _timeElapsedLogger.Interval = _timeElapsed.ElapsedMilliseconds % 1000;
            LiveData.LevelPaused = true;
            LiveData.Send();
        }

        private void BSEvents_songUnpaused()
        {
            _timeElapsedLogger.Start();
            _timeElapsed.Start();
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
            _timeElapsedLogger.Stop();
            _timeElapsed.Stop();
            LiveData.InLevel = false;
            LiveData.Send();
            _scoreController = null;
            _noteCutInfo = null;
        }
        #endregion

        private void ResetData()
        {
            _timeElapsedLogger.Stop();
            _timeElapsed.Stop();
            _timeElapsed.Reset();
            _startSongTime = new TimeSpan(0, 0, 0);

            StaticData.Reset();
            LiveData.Reset();
        }

        private void BSEvents_gameSceneLoaded()
        {
            ResetData();

            LiveData.InLevel = true;
            _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            _scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;

            AudioTimeSyncController audioController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            GameplayCoreSceneSetupData currentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;

            IBeatmapLevel levelData = currentMap.difficultyBeatmap.level;

            StaticData.SongName = levelData.songName;
            StaticData.SongSubName = levelData.songSubName;
            StaticData.SongAuthor = levelData.songAuthorName;
            StaticData.Mapper = levelData.levelAuthorName;
            StaticData.Bpm = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            StaticData.Length = Convert.ToInt32(Math.Round(audioController.songLength));
            PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, currentMap.difficultyBeatmap.difficulty,
                currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            StaticData.PreviousRecord = playerLevelStats.highScore;

            if (_previousMap == null || _previousBeatmap == null || (levelData.levelID != _previousMap.difficultyBeatmap.level.levelID))
            {
                Task.Run(async () =>
                {
                    if (_previousBeatmap != null) { StaticData.PreviousBsr = _previousBeatmap.Key; }
                    Beatmap bm = await _beatSaver.Hash(levelData.levelID.Replace("custom_level_", ""));
                    if (bm != null)
                    {
                        StaticData.BsrKey = bm.Key;
                        StaticData.CoverImage = BeatSaver.BaseURL + bm.CoverURL;
                        _previousBeatmap = bm;
                    }
                    else { StaticData.BsrKey = null; StaticData.CoverImage = null; _previousBeatmap = null; }
                    StaticData.Send();
                });
            }
            else { StaticData.BsrKey = _previousBeatmap.Key; StaticData.CoverImage = BeatSaver.BaseURL + _previousBeatmap.CoverURL; }

            StaticData.Difficulty = currentMap.difficultyBeatmap.difficultyRank;
            StaticData.Njs = currentMap.difficultyBeatmap.noteJumpMovementSpeed;

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
                _startSongTime = new TimeSpan(0, 0, (int)Math.Round(currentMap.practiceSettings.startSongTime) - 1); //1s time desync
                StaticData.PracticeModeModifiers.Add("songSpeedMul", currentMap.practiceSettings.songSpeedMul);
            }

            _previousMap = currentMap;

            _timeElapsed.Start();
            _timeElapsedLogger.Start();

            StaticData.Send();
            LiveData.Send();
        }

        private void ScoreController_scoreDidChangeEvent(int arg1, int arg2)
        {
            LiveData.Score = arg1;
            LiveData.Accuracy = arg1 / _scoreController.immediateMaxPossibleRawScore * 100f;
            LiveData.Send();
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo nci, int arg3)
        {
            _noteCutInfo = nci;

            if (_noteCutInfo.allIsOK)
            {
                LiveData.Combo++;
                if (_noteCutInfo == null) { _noteCutInfo.swingRatingCounter.didFinishEvent += SwingRatingCounter_didFinishEvent; }
            }
            else
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
            }
            LiveData.Send();
        }

        private void SwingRatingCounter_didFinishEvent(ISaberSwingRatingCounter SaberSwingRatingCounter)
        {
            ScoreModel.RawScoreWithoutMultiplier(_noteCutInfo, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore);
            int blockScoreWithoutModifier = beforeCutRawScore + afterCutRawScore + cutDistanceRawScore;
            LiveData.BlockHitScores.Add(blockScoreWithoutModifier);
            //LiveData.Send();
        }
    }
}