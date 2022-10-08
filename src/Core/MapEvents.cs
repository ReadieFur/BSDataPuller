using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using BeatSaverSharp;
using DataPuller.Client;
using HarmonyLib;
using IPA.Utilities;
using SongDetailsCache;
using SongDetailsCache.Structs;
using TMPro;
using UnityEngine;
using Zenject;

#nullable enable
namespace DataPuller.Core
{
    internal class MapEvents : IInitializable, IDisposable
    {
        //I think I need to fix my refrences as VS does not notice when I update them.
        private static BeatSaver beatSaver = new(Plugin.PLUGIN_NAME, Assembly.GetExecutingAssembly().GetName().Version);
        private static SongDetails? songDetailsCache = null;
        internal static MapData.JsonData previousStaticData = new();
        private Timer timer = new() { Interval = 250 };
        private int noteCount = 0;

        //Required objects - Made [InjectOptional] and checked at Initialize()
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
        [InjectOptional] private BeatmapObjectManager beatmapObjectManager;
        [InjectOptional] private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
        [InjectOptional] private AudioTimeSyncController audioTimeSyncController;
        [InjectOptional] private RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        [InjectOptional] private GameEnergyCounter gameEnergyCounter;

        //Optional objects for different gamemodes - checked by each gamemode.
        [InjectOptional] private ScoreController? scoreController;
        [InjectOptional] private MultiplayerController? multiplayerController;
        [InjectOptional] private ScoreUIController? scoreUIController;
        [InjectOptional] private PauseController? pauseController;
        [InjectOptional] private StandardLevelGameplayManager? standardLevelGameplayManager;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MapEvents() {} //Injects made above now
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Initialize()
        {
            MapData.Reset();
            LiveData.Reset();

            if (DoRequiredObjectsExist(out List<string> missingObjects))
            {
                if (scoreController is ScoreController && multiplayerController is MultiplayerController) //Multiplayer
                {
                    Plugin.Logger.Info("In multiplayer.");

                    MapData.Reset();
                    LiveData.Reset();
                    MapData.Send();
                    LiveData.Send();

                    multiplayerController.stateChangedEvent += MultiplayerController_stateChangedEvent;
                    scoreController.scoreDidChangeEvent += ScoreDidChangeEvent;

                    MapData.IsMultiplayer = true;
                }
                else if (IsLegacyReplay() && relativeScoreAndImmediateRankCounter is RelativeScoreAndImmediateRankCounter && scoreUIController is ScoreUIController) //Legacy Replay
                {
                    Plugin.Logger.Info("In legacy replay.");

                    LevelLoaded();

                    relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += RelativeScoreOrImmediateRankDidChangeEvent;
                }
                else if (scoreController is ScoreController && pauseController is PauseController && standardLevelGameplayManager is StandardLevelGameplayManager) //Singleplayer or New Replay.
                {
                    Plugin.Logger.Info("In singleplayer.");

                    LevelLoaded();

                    //In replay mode the scorecontroller does not work so 'RelativeScoreOrImmediateRankDidChangeEvent' will read from the UI
                    scoreController.scoreDidChangeEvent += ScoreDidChangeEvent;

                    pauseController.didPauseEvent += LevelPausedEvent;
                    pauseController.didResumeEvent += LevelUnpausedEvent;
                    pauseController.didReturnToMenuEvent += LevelQuitEvent;

                    standardLevelGameplayManager.levelFailedEvent += LevelFailedEvent;
                    standardLevelGameplayManager.levelFinishedEvent += LevelFinishedEvent;
                }
                else
                {
                    Plugin.Logger.Info("No gamemode detected.");
                    EarlyDispose("Could not find the required objects for any of the valid gamemodes.");
                }
            }
            else Plugin.Logger.Error($"Required objects not found. Missing: {string.Join(", ", missingObjects)}");
        }

        /// <param name="missingObjects">Empty when returning true</param>
        /// <returns>True if the object was found, otherwise false.</returns>
        private bool DoRequiredObjectsExist(out List<string> missingObjects)
        {
            missingObjects = new();

            if (!(beatmapObjectManager is BeatmapObjectManager)) missingObjects.Add("BeatmapObjectManager not found");
            if (!(gameplayCoreSceneSetupData is GameplayCoreSceneSetupData)) missingObjects.Add("GameplayCoreSceneSetupData not found");
            if (!(audioTimeSyncController is AudioTimeSyncController)) missingObjects.Add("AudioTimeSyncController not found");
            if (!(relativeScoreAndImmediateRankCounter is RelativeScoreAndImmediateRankCounter)) missingObjects.Add("RelativeScoreAndImmediateRankCounter not found");
            if (!(gameEnergyCounter is GameEnergyCounter)) missingObjects.Add("GameEnergyCounter not found");

            return missingObjects.Count == 0;
        }

        private bool IsLegacyReplay()
        {
            //Try ang get the legacy ScoreSaber replay class.
            Type legacyReplayPlayer = AccessTools.TypeByName("ScoreSaber.LegacyReplayPlayer");
            if (legacyReplayPlayer == null) return false;

            //Check if replay mode is active.
            PropertyInfo? playbackEnabled = legacyReplayPlayer.GetProperty("playbackEnabled", BindingFlags.Public | BindingFlags.Instance);

            //Check if an instance of the legacy replay player exists.
            UnityEngine.Object replayPlayer = Resources.FindObjectsOfTypeAll(legacyReplayPlayer).FirstOrDefault();

            //If all of the above objects aren't null, return the value of playbackEnabled, otherwise return false.
            if (legacyReplayPlayer != null && playbackEnabled != null && replayPlayer != null) return (bool)playbackEnabled.GetValue(replayPlayer);
            return false;
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

            beatmapObjectManager.noteWasMissedEvent -= NoteWasMissedEvent;

            gameEnergyCounter.gameEnergyDidChangeEvent -= EnergyDidChangeEvent;

            if (scoreController is ScoreController && multiplayerController is MultiplayerController) //In a multiplayer lobby
            {
                scoreController.scoreDidChangeEvent -= ScoreDidChangeEvent;

                multiplayerController.stateChangedEvent -= MultiplayerController_stateChangedEvent;
            }
            else if (IsLegacyReplay() && relativeScoreAndImmediateRankCounter is RelativeScoreAndImmediateRankCounter) //In a legacy replay.
            {
                relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= RelativeScoreOrImmediateRankDidChangeEvent;
            }
            else if (scoreController is ScoreController && pauseController is PauseController && standardLevelGameplayManager is StandardLevelGameplayManager) //Singleplayer/New replay.
            {
                scoreController.scoreDidChangeEvent -= ScoreDidChangeEvent; //In replay mode this does not fire so 'RelativeScoreOrImmediateRankDidChangeEvent' will read from the UI

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

        public void LevelLoaded()
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            IBeatmapLevel levelData = gameplayCoreSceneSetupData.difficultyBeatmap.level;
            bool isCustomLevel = true;
            string? mapHash = null;
            try { mapHash = levelData.levelID.Split('_')[2]; }
            catch { isCustomLevel = false; }
            isCustomLevel = isCustomLevel && mapHash != null && mapHash.Length == 40 ? true : false;

            SongCore.Data.ExtraSongData.DifficultyData? difficultyData = SongCore.Collections.RetrieveDifficultyData(gameplayCoreSceneSetupData.difficultyBeatmap);

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

            if (isCustomLevel)
            {
                void SetSongDetails()
                {
                    if (songDetailsCache!.songs.FindByHash(mapHash, out Song song))
                    {
                        MapCharacteristic mapType;
                        switch (gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName)
                        {
                            case "Degree360":
                                mapType = MapCharacteristic.ThreeSixtyDegree;
                                break;
                            case "Degree90":
                                mapType = MapCharacteristic.NinetyDegree;
                                break;
                            default:
                                if (!Enum.TryParse(
                                    gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName,
                                    out mapType
                                )) { return; }
                                break;
                        }

                        if (song.GetDifficulty(
                            out SongDifficulty difficulty,
                            (MapDifficulty)gameplayCoreSceneSetupData.difficultyBeatmap.difficulty,
                            mapType
                        ))
                        {
                            MapData.PP = difficulty.approximatePpValue;
                            MapData.Star = difficulty.stars;
                            MapData.Send();
                        }
                    }
                }

                if (songDetailsCache == null)
                {
                    SongDetails.Init().ContinueWith((task) =>
                    {
                        if (task.Result == null) { return; }
                        songDetailsCache = task.Result;
                        SetSongDetails();
                    });
                }
                else { SetSongDetails(); }

                if (mapHash != null)
                {
                    beatSaver.BeatmapByHash(mapHash).ContinueWith((task) =>
                    {
                        if (task.Result != null)
                        {
                            MapData.BSRKey = task.Result.ID;
                            BeatSaverSharp.Models.BeatmapVersion? mapDetails = null;
                            try { mapDetails = task.Result.Versions.First(map => map.Hash.ToLower() == mapHash.ToLower()); } catch (Exception ex) { Plugin.Logger.Error(ex); }
                            MapData.coverImage = mapDetails?.CoverURL ?? null;
                        }
                        else
                        {
                            MapData.BSRKey = null;
                            MapData.coverImage = null;
                        }
                        MapData.Send();
                    });
                }
            }

            if (MapData.Hash != previousStaticData.Hash) MapData.PreviousBSR = previousStaticData.BSRKey;

            MapData.Modifiers.Add("noFailOn0Energy", gameplayCoreSceneSetupData.gameplayModifiers.noFailOn0Energy);
            MapData.Modifiers.Add("oneLife", gameplayCoreSceneSetupData.gameplayModifiers.instaFail);
            MapData.Modifiers.Add("fourLives", gameplayCoreSceneSetupData.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery);
            MapData.Modifiers.Add("noBombs", gameplayCoreSceneSetupData.gameplayModifiers.noBombs);
            MapData.Modifiers.Add("noWalls", gameplayCoreSceneSetupData.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles);
            MapData.Modifiers.Add("noArrows", gameplayCoreSceneSetupData.gameplayModifiers.noArrows);
            MapData.Modifiers.Add("ghostNotes", gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes);
            MapData.Modifiers.Add("disappearingArrows", gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows);
            MapData.Modifiers.Add("smallNotes", gameplayCoreSceneSetupData.gameplayModifiers.smallCubes);
            MapData.Modifiers.Add("proMode", gameplayCoreSceneSetupData.gameplayModifiers.proMode);
            MapData.Modifiers.Add("strictAngles", gameplayCoreSceneSetupData.gameplayModifiers.strictAngles);
            MapData.Modifiers.Add("zenMode", gameplayCoreSceneSetupData.gameplayModifiers.zenMode);
            MapData.Modifiers.Add("slowerSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 0.85f ? true : false);
            MapData.Modifiers.Add("fasterSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 1.2f ? true : false);
            MapData.Modifiers.Add("superFastSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 1.5f ? true : false);

            foreach (KeyValuePair<string, bool> keyValue in MapData.Modifiers)
                if (MapData.Modifiers[keyValue.Key] && Enum.IsDefined(typeof(EModifiers), keyValue.Key))
                    MapData.ModifiersMultiplier += (int)(EModifiers)Enum.Parse(typeof(EModifiers), keyValue.Key) / 100f;

            MapData.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null ? true : false;
            MapData.PracticeModeModifiers.Add("songSpeedMul", MapData.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.songSpeedMul : 1);
            MapData.PracticeModeModifiers.Add("startInAdvanceAndClearNotes", MapData.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.startInAdvanceAndClearNotes ? 1 : 0 : 0);
            MapData.PracticeModeModifiers.Add("startSongTime", MapData.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.startSongTime : 0);

            timer.Elapsed += TimerElapsedEvent;
            beatmapObjectManager.noteWasCutEvent += NoteWasCutEvent;
            beatmapObjectManager.noteWasMissedEvent += NoteWasMissedEvent;
            gameEnergyCounter.gameEnergyDidChangeEvent += EnergyDidChangeEvent;

            MapData.InLevel = true;
            timer.Start();

            MapData.Send();
            LiveData.Send();
        }

        private void TimerElapsedEvent(object se, ElapsedEventArgs ev)
        {
            LiveData.TimeElapsed = (int)Math.Round(audioTimeSyncController.songTime);
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.LastSend).TotalMilliseconds) > 950 / MapData.PracticeModeModifiers["songSpeedMul"])
            { LiveData.Send(ELiveDataEventTriggers.TimerElapsed); }
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
                LevelLoaded();
            }
            else if (multiplayerState == MultiplayerController.State.Finished)
            {
                LevelFinishedEvent();
            }
        }

        private void EnergyDidChangeEvent(float health)
        {
            health *= 100;
            if (MapData.Modifiers["noFailOn0Energy"] && health <= 0)
            {
                MapData.LevelFailed = true;
                MapData.ModifiersMultiplier += -50 / 100f;
                MapData.Send();
            }
            if (health < LiveData.PlayerHealth) { LiveData.Combo = 0; }
            LiveData.PlayerHealth = health;
            LiveData.Send(ELiveDataEventTriggers.EnergyChange);
        }

        private void NoteWasMissedEvent(NoteController noteController)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                LiveData.Combo = 0;
                LiveData.FullCombo = false;
                LiveData.Misses++;
                LiveData.ColorType = noteController.noteData.colorType;
                LiveData.Send(ELiveDataEventTriggers.NoteMissed);
            }
        }

        private void LevelQuitEvent() { MapData.LevelQuit = true; }

        private void LevelFailedEvent() { MapData.LevelFailed = true; }

        private void LevelFinishedEvent() { MapData.LevelFinished = true; }

        private void RelativeScoreOrImmediateRankDidChangeEvent() //For replay mode
        {
            TextMeshProUGUI textMeshProUGUI = scoreUIController!.GetField<TextMeshProUGUI, ScoreUIController>("_scoreText");
            LiveData.Score = int.Parse(textMeshProUGUI.text.Replace(" ", ""));
            LiveData.ScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.Score, MapData.ModifiersMultiplier);
            LiveData.MaxScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.MaxScore, MapData.ModifiersMultiplier);
            SetRankAndAccuracy();
        }

        private void ScoreDidChangeEvent(int score, int scoreWithMultipliers)
        {
            LiveData.Score = score;
            LiveData.ScoreWithMultipliers = scoreWithMultipliers;
            SetRankAndAccuracy();
        }

        private void SetRankAndAccuracy()
        {
            LiveData.Accuracy = relativeScoreAndImmediateRankCounter.relativeScore * 100;
            LiveData.Rank = relativeScoreAndImmediateRankCounter.immediateRank.ToString();
            LiveData.Send(ELiveDataEventTriggers.ScoreChange);
        }

        private void NoteWasCutEvent(NoteController arg1, in NoteCutInfo _noteCutInfo)
        {
            if (_noteCutInfo.allIsOK)
            {
                LiveData.ColorType = arg1.noteData.colorType;
                LiveData.Combo++;
                noteCount++;
                //_noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(new SwingRatingCounterDidFinishController(_noteCutInfo));
            }

            //LiveData.Send(); //Sent by SetRankAndAccuracy()
        }

        private static string? GetBase64CoverImage(CustomPreviewBeatmapLevel level) //Thanks UnskilledFreak
        {
            if (level == null) return null;

            string? coverPath = Path.Combine(level.customLevelPath, level.standardLevelInfoSaveData.coverImageFilename);
            if (string.IsNullOrEmpty(coverPath)) return null;

            string prefix = coverPath.Substring(0, coverPath.Length - 3) == "png" ? "png" : "jpeg";
            
            string base64;
            try { base64 = Convert.ToBase64String(File.ReadAllBytes(coverPath)); }
            catch { return null; }

            return string.Concat("data:image/", prefix, ";base64,", base64);
        }
    }
}