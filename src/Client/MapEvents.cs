﻿using BeatSaverSharp;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;
using System.IO;
using SongDataCore.BeatStar;
using System.Collections.Generic;
using Zenject;
using HarmonyLib;
using TMPro;
using IPA.Utilities;

namespace DataPuller.Client
{
    class MapEvents : IInitializable, IDisposable
    {
        //I think I need to fix my refrences as VS does not notice when I update them.
        private static BeatSaver beatSaver = new BeatSaver(new HttpOptions("BSDataPuller", Assembly.GetExecutingAssembly().GetName().Version));
        internal static MapData.JsonData previousStaticData = new MapData.JsonData();
        private Timer timer = new Timer { Interval = 250 };
        private Dictionary<int, NoteCutInfo> noteCutInfo = new Dictionary<int, NoteCutInfo>();
        private int NoteCount = 0;

        //Required objects - Made [InjectOptional] and checked at Initialize()
        [InjectOptional] private BeatmapObjectManager beatmapObjectManager;
        [InjectOptional] private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
        [InjectOptional] private AudioTimeSyncController audioTimeSyncController;
        [InjectOptional] private RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        [InjectOptional] private GameEnergyCounter gameEnergyCounter;

        //Optional objects for different gamemodes - checked by each gamemode
        [InjectOptional] private ScoreController scoreController;
        [InjectOptional] private MultiplayerController multiplayerController;
        [InjectOptional] private ScoreUIController scoreUIController;
        [InjectOptional] private PauseController pauseController;
        [InjectOptional] private StandardLevelGameplayManager standardLevelGameplayManager;

        public MapEvents(){} //Injects made above now

        public void Initialize()
        {
            MapData.Reset();
            LiveData.Reset();

            if (MainRequiredObjectsExist())
            {
                timer.Elapsed += TimerElapsedEvent;

                beatmapObjectManager.noteWasCutEvent += NoteWasCutEvent;
                beatmapObjectManager.noteWasMissedEvent += NoteWasMissedEvent;

                gameEnergyCounter.gameEnergyDidChangeEvent += EnergyDidChangeEvent;

                if (scoreController is ScoreController && multiplayerController is MultiplayerController) //Multiplayer
                {
                    Plugin.Logger.Info("In multiplayer");
                    scoreController.scoreDidChangeEvent += ScoreDidChangeEvent;
                    scoreController.immediateMaxPossibleScoreDidChangeEvent += ImmediateMaxPossibleScoreDidChangeEvent;

                    multiplayerController.stateChangedEvent += MultiplayerController_stateChangedEvent;
                    
                    MapData.IsMultiplayer = true;
                }
                else if (IsReplay() && relativeScoreAndImmediateRankCounter is RelativeScoreAndImmediateRankCounter && scoreUIController is ScoreUIController) //Replay
                {
                    Plugin.Logger.Info("In replay");
                    relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += RelativeScoreOrImmediateRankDidChangeEvent;

                    SetupMapDataAndMisc();
                }
                else if (scoreController is ScoreController && pauseController is PauseController && standardLevelGameplayManager is StandardLevelGameplayManager) //Singleplayer
                {
                    Plugin.Logger.Info("In singleplayer");
                    //In replay mode the scorecontroller does not work so 'RelativeScoreOrImmediateRankDidChangeEvent' will read from the UI
                    scoreController.scoreDidChangeEvent += ScoreDidChangeEvent;
                    scoreController.immediateMaxPossibleScoreDidChangeEvent += ImmediateMaxPossibleScoreDidChangeEvent;

                    pauseController.didPauseEvent += LevelPausedEvent;
                    pauseController.didResumeEvent += LevelUnpausedEvent;
                    pauseController.didReturnToMenuEvent += LevelQuitEvent;

                    standardLevelGameplayManager.levelFailedEvent += LevelFailedEvent;
                    standardLevelGameplayManager.levelFinishedEvent += LevelFinishedEvent;

                    SetupMapDataAndMisc();
                }
                else
                {
                    Plugin.Logger.Info("No gamemode detected");
                    EarlyDispose("Could not find the required objects for any of the valid gamemodes");
                }
            }
        }

        private bool MainRequiredObjectsExist()
        {
            if (!(scoreController is ScoreController)) { Plugin.Logger.Error("ScoreController not found"); return false; }
            if (!(beatmapObjectManager is BeatmapObjectManager)) { Plugin.Logger.Error("BeatmapObjectManager not found"); return false; }
            if (!(gameplayCoreSceneSetupData is GameplayCoreSceneSetupData)) { Plugin.Logger.Error("GameplayCoreSceneSetupData not found"); return false; }
            if (!(audioTimeSyncController is AudioTimeSyncController)) { Plugin.Logger.Error("AudioTimeSyncController not found"); return false; }
            if (!(gameEnergyCounter is GameEnergyCounter)) { Plugin.Logger.Error("GameEnergyCounter not found"); return false; }
            return true;
        }

        private bool IsReplay()
        {
            Type ReplayPlayer = AccessTools.TypeByName("ScoreSaber.ReplayPlayer"); //Get the ReplayPlayer type (class)
            PropertyInfo playbackEnabled = ReplayPlayer?.GetProperty("playbackEnabled", BindingFlags.Public | BindingFlags.Instance); //Find the desired property in that class?
            UnityEngine.Object _replayPlayer = Resources.FindObjectsOfTypeAll(ReplayPlayer).FirstOrDefault(); //Find the existing class (if any)
            if (ReplayPlayer != null && playbackEnabled != null && _replayPlayer != null)
            { return (bool)playbackEnabled.GetValue(_replayPlayer); }
            else { return false; }
        }

        //This should be logged as an error as there is currently no reason as to why the script should stop early, unless required objects are not found.
        private void EarlyDispose(string reason)
        {
            Plugin.Logger.Error("MapEvents quit early. Reason: " + reason);
            Dispose();
        }

        public void Dispose()
        {
            #region Unsubscribe from events
            timer.Elapsed -= TimerElapsedEvent;

            beatmapObjectManager.noteWasCutEvent -= NoteWasCutEvent;
            beatmapObjectManager.noteWasMissedEvent -= NoteWasMissedEvent;

            gameEnergyCounter.gameEnergyDidChangeEvent -= EnergyDidChangeEvent;

            if (scoreController is ScoreController && multiplayerController is MultiplayerController) //In a multiplayer lobby
            {
                scoreController.scoreDidChangeEvent -= ScoreDidChangeEvent;
                scoreController.immediateMaxPossibleScoreDidChangeEvent -= ImmediateMaxPossibleScoreDidChangeEvent;

                multiplayerController.stateChangedEvent -= MultiplayerController_stateChangedEvent;
            }
            else if (IsReplay() && relativeScoreAndImmediateRankCounter is RelativeScoreAndImmediateRankCounter)
            {
                relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= RelativeScoreOrImmediateRankDidChangeEvent;
            }
            else if (scoreController is ScoreController && pauseController is PauseController && standardLevelGameplayManager is StandardLevelGameplayManager) //Singleplayer
            {
                scoreController.scoreDidChangeEvent -= ScoreDidChangeEvent; //In replay mode this does not fire so 'RelativeScoreOrImmediateRankDidChangeEvent' will read from the UI
                scoreController.immediateMaxPossibleScoreDidChangeEvent -= ImmediateMaxPossibleScoreDidChangeEvent;

                pauseController.didPauseEvent -= LevelPausedEvent;
                pauseController.didResumeEvent -= LevelUnpausedEvent;
                pauseController.didReturnToMenuEvent -= LevelQuitEvent;

                standardLevelGameplayManager.levelFailedEvent -= LevelFailedEvent;
                standardLevelGameplayManager.levelFinishedEvent -= LevelFinishedEvent;
            }
            #endregion

            timer.Stop();
            MapData.InLevel = false;
            MapData.Send();
        }

        public void SetupMapDataAndMisc()
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            IBeatmapLevel levelData = gameplayCoreSceneSetupData.difficultyBeatmap.level;
            bool isCustomLevel = true;
            string mapHash = string.Empty;
            try { mapHash = levelData.levelID.Split('_')[2]; } catch { isCustomLevel = false; }
            isCustomLevel = isCustomLevel && mapHash.Length == 40 ? true : false;

            var difficultyData = SongCore.Collections.RetrieveDifficultyData(gameplayCoreSceneSetupData.difficultyBeatmap);

            MapData.Hash = isCustomLevel ? mapHash : null;
            MapData.SongName = levelData.songName;
            MapData.SongSubName = levelData.songSubName;
            MapData.SongAuthor = levelData.songAuthorName;
            MapData.Mapper = levelData.levelAuthorName;
            MapData.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            MapData.Length = Convert.ToInt32(Math.Round(audioTimeSyncController.songLength));
            PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, gameplayCoreSceneSetupData.difficultyBeatmap.difficulty,
                gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            MapData.PreviousRecord = playerLevelStats.highScore;
            MapData.MapType = gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            MapData.Difficulty = gameplayCoreSceneSetupData.difficultyBeatmap.difficulty.ToString("g");
            MapData.NJS = gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed;
            MapData.CustomDifficultyLabel = difficultyData?._difficultyLabel ?? null;
            

            SongDataCoreCurrent sdc = new SongDataCoreCurrent { available = isCustomLevel ? SongDataCore.Plugin.Songs.IsDataAvailable() : false };
            if (sdc.available)
            {
                BeatStarSong map;
                SongDataCore.Plugin.Songs.Data.Songs.TryGetValue(mapHash, out map);
                sdc.map = map;
                if (sdc.map != null)
                {
                    Dictionary<string, BeatStarSongDifficultyStats> diffs = sdc.map.characteristics[(BeatStarCharacteristics)Enum.Parse(typeof(BeatStarCharacteristics),
                        gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName)];
                    sdc.stats = diffs[MapData.Difficulty == "ExpertPlus" ? "Expert+" : MapData.Difficulty];
                    MapData.PP = sdc.stats.pp;
                    MapData.Star = sdc.stats.star;
                }
                else { sdc.available = false; }
            }

            if (sdc.available)
            {
                MapData.BSRKey = sdc.map.key;
                if (levelData is CustomPreviewBeatmapLevel customLevel) { MapData.coverImage = GetBase64CoverImage(customLevel); }
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
                        MapData.BSRKey = bm.Key;
                        MapData.coverImage = BeatSaver.BaseURL + bm.CoverURL;
                    }
                    else { MapData.BSRKey = null; }
                    MapData.Send();
                });
            }

            if (MapData.Hash != previousStaticData.Hash) { MapData.PreviousBSR = previousStaticData.BSRKey; }

            MapData.Modifiers.Add("instaFail", gameplayCoreSceneSetupData.gameplayModifiers.instaFail);
            MapData.Modifiers.Add("batteryEnergy", gameplayCoreSceneSetupData.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery);
            MapData.Modifiers.Add("disappearingArrows", gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows);
            MapData.Modifiers.Add("ghostNotes", gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes);
            MapData.Modifiers.Add("fasterSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 1.2f ? true : false);
            MapData.Modifiers.Add("noFail", gameplayCoreSceneSetupData.gameplayModifiers.noFailOn0Energy);
            MapData.Modifiers.Add("noObstacles", gameplayCoreSceneSetupData.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles);
            MapData.Modifiers.Add("noBombs", gameplayCoreSceneSetupData.gameplayModifiers.noBombs);
            MapData.Modifiers.Add("slowerSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 0.85f ? true : false);
            MapData.Modifiers.Add("noArrows", gameplayCoreSceneSetupData.gameplayModifiers.noArrows);
            foreach (KeyValuePair<string, bool> keyValue in MapData.Modifiers)
            {
                if (MapData.Modifiers[keyValue.Key])
                {
                    MapData.ModifiersMultiplier += (int)(Modifiers)Enum.Parse(typeof(Modifiers), keyValue.Key) / 100f;
                }
            }
            MapData.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null ? true : false;
            MapData.PracticeModeModifiers.Add("songSpeedMul", MapData.PracticeMode ? gameplayCoreSceneSetupData.practiceSettings.songSpeedMul : 1);

            LiveData.PlayerHealth = MapData.Modifiers["noFail"] ? 100 : 50; //Not static data but it relies on static data to be set the first time
            MapData.InLevel = true;
            timer.Start();

            MapData.Send();
            LiveData.Send();
        }

        private void TimerElapsedEvent(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = (int) Math.Round(audioTimeSyncController.songTime);
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.LastSend).TotalMilliseconds) > 950 / MapData.PracticeModeModifiers["songSpeedMul"]) { LiveData.Send(); }
        }

        private void LevelPausedEvent()
        {
            timer.Stop();
            MapData.LevelPaused = true;
            MapData.Send();
        }

        private void LevelUnpausedEvent()
        {
            timer.Start();
            MapData.LevelPaused = false;
            MapData.Send();
        }

        private void MultiplayerController_stateChangedEvent(MultiplayerController.State multiplayerState)
        {
            if (multiplayerState == MultiplayerController.State.Gameplay)
            {
                SetupMapDataAndMisc();
            }
            else if (multiplayerState == MultiplayerController.State.Finished)
            {
                LevelFinishedEvent();
            }
        }

        private void EnergyDidChangeEvent(float health)
        {
            health *= 100;
            if (health < LiveData.PlayerHealth) { LiveData.Combo = 0; } //I could impliment a check to see if NF is enabled and active but I will leave that out for now as I do not want to change the dat sent right now.
            LiveData.PlayerHealth = health;
            LiveData.Send();
        }

        private void NoteWasMissedEvent(NoteController noteController)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
                LiveData.Send();
            }
        }

        private void LevelQuitEvent() { MapData.LevelQuit = true; }

        private void LevelFailedEvent() { MapData.LevelFailed = true; }

        private void LevelFinishedEvent() { MapData.LevelFinished = true; }

        private void RelativeScoreOrImmediateRankDidChangeEvent() //For replay mode
        {
            TextMeshProUGUI textMeshProUGUI = scoreUIController.GetField<TextMeshProUGUI, ScoreUIController>("_scoreText");
            LiveData.Score = int.Parse(textMeshProUGUI.text.Replace(" ", ""));
            LiveData.ScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.Score, MapData.ModifiersMultiplier);
            LiveData.MaxScore = ScoreModel.MaxRawScoreForNumberOfNotes(NoteCount);
            LiveData.MaxScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.MaxScore, MapData.ModifiersMultiplier);
            SetRankAndAccuracy();
        }

        private void ScoreDidChangeEvent(int score, int scoreWithMultipliers)
        {
            LiveData.Score = score;
            LiveData.ScoreWithMultipliers = scoreWithMultipliers;
            SetRankAndAccuracy();
        }

        private void ImmediateMaxPossibleScoreDidChangeEvent(int maxScore, int maxScoreWithMultipliers)
        {
            LiveData.MaxScore = maxScore;
            LiveData.MaxScoreWithMultipliers = maxScoreWithMultipliers;
            SetRankAndAccuracy();
        }

        private void SetRankAndAccuracy()
        {
            LiveData.Accuracy = relativeScoreAndImmediateRankCounter.relativeScore * 100;
            LiveData.Rank = relativeScoreAndImmediateRankCounter.immediateRank.ToString();
            LiveData.Send();
        }

        private void NoteWasCutEvent(NoteController arg1, NoteCutInfo _noteCutInfo)
        {
            if (_noteCutInfo.allIsOK)
            {
                noteCutInfo.Add(_noteCutInfo.swingRatingCounter.GetHashCode(), _noteCutInfo);
                LiveData.Combo++;
                NoteCount++;
                _noteCutInfo.swingRatingCounter.didFinishEvent += SwingRatingCounter_didFinishEvent;
            }
            else
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
            }

            //LiveData.Send(); //Sent by SetRankAndAccuracy()
        }

        private void SwingRatingCounter_didFinishEvent(ISaberSwingRatingCounter saberSwingRatingCounter)
        {
            int hashCode = saberSwingRatingCounter.GetHashCode();
            NoteCutInfo _noteCutInfo = noteCutInfo[hashCode];
            noteCutInfo.Remove(hashCode);
            _noteCutInfo.swingRatingCounter.didFinishEvent -= SwingRatingCounter_didFinishEvent;
            ScoreModel.RawScoreWithoutMultiplier(_noteCutInfo, out var _beforeCutRawScore, out var _afterCutRawScore, out var _cutDistanceRawScore);

            LiveData.BlockHitScore = new int[] { _beforeCutRawScore, _afterCutRawScore, _cutDistanceRawScore };

            //LiveData.Send(); //Sent by SetRankAndAccuracy()
        }

        private static string GetBase64CoverImage(CustomPreviewBeatmapLevel level) //Thanks UnskilledFreak
        {
            if (level == null) { return null; }

            var coverPath = Path.Combine(level.customLevelPath, level.standardLevelInfoSaveData.coverImageFilename);

            if (coverPath == string.Empty) { return null; }

            var prefix = coverPath.Substring(0, coverPath.Length - 3) == "png" ? "png" : "jpeg";

            var coverData = File.ReadAllBytes(coverPath);
            var base64String = Convert.ToBase64String(coverData);

            return string.Concat("data:image/", prefix, ";base64,", base64String);
        }

        private class SongDataCoreCurrent
        {
            public bool available { get; set; }
            public BeatStarSong map { get; set; }
            public BeatStarSongDifficultyStats stats { get; set; }
        }

        enum Modifiers
        {
            instaFail = 0,
            batteryEnergy = 0,
            disappearingArrows = 7,
            ghostNotes = 11,
            fasterSong = 8,
            noFail = -50,
            noObstacles = -05,
            noBombs = -10,
            slowerSong = -30,
            noArrows = 0,
        }
    }
}