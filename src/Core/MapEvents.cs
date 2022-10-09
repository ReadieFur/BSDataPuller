using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using BeatSaverSharp;
using DataPuller.Data;
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
        private static readonly BeatSaver beatSaver = new(Plugin.PLUGIN_NAME, Assembly.GetExecutingAssembly().GetName().Version);
        private static SongDetails? songDetailsCache = null;
        private readonly Timer timer = new() { Interval = 250 };
        private int noteCount = 0;
        private string? previousHash = null;
        private string? previousBSRKey = null;

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
            previousHash = MapData.Instance.Hash;
            previousBSRKey = MapData.Instance.BSRKey;
            MapData.Instance.Reset();
            LiveData.Instance.Reset();

            if (DoRequiredObjectsExist(out List<string> missingObjects))
            {
                if (scoreController is not null && multiplayerController is not null) //Multiplayer
                {
                    Plugin.Logger.Info("In multiplayer.");

                    MapData.Instance.Send();
                    LiveData.Instance.Send();

                    multiplayerController.stateChangedEvent += MultiplayerController_stateChangedEvent;
                    scoreController.scoreDidChangeEvent += ScoreDidChangeEvent;

                    MapData.Instance.IsMultiplayer = true;
                }
                else if (IsLegacyReplay() && relativeScoreAndImmediateRankCounter is not null && scoreUIController is not null) //Legacy Replay
                {
                    Plugin.Logger.Info("In legacy replay.");

                    LevelLoaded();

                    relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += RelativeScoreOrImmediateRankDidChangeEvent;
                }
                else if (scoreController is not null && pauseController is not null && standardLevelGameplayManager is not null) //Singleplayer or New Replay.
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

            if (beatmapObjectManager is null) missingObjects.Add("BeatmapObjectManager not found");
            if (gameplayCoreSceneSetupData is null) missingObjects.Add("GameplayCoreSceneSetupData not found");
            if (audioTimeSyncController is null) missingObjects.Add("AudioTimeSyncController not found");
            if (relativeScoreAndImmediateRankCounter is null) missingObjects.Add("RelativeScoreAndImmediateRankCounter not found");
            if (gameEnergyCounter is null) missingObjects.Add("GameEnergyCounter not found");

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

            if (scoreController is not null && multiplayerController is not null) //In a multiplayer lobby
            {
                scoreController.scoreDidChangeEvent -= ScoreDidChangeEvent;

                multiplayerController.stateChangedEvent -= MultiplayerController_stateChangedEvent;
            }
            else if (IsLegacyReplay() && relativeScoreAndImmediateRankCounter is not null) //In a legacy replay.
            {
                relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= RelativeScoreOrImmediateRankDidChangeEvent;
            }
            else if (scoreController is not null && pauseController is not null && standardLevelGameplayManager is not null) //Singleplayer/New replay.
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
            MapData.Instance.InLevel = false;
            MapData.Instance.Send();
        }

        public void LevelLoaded()
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            IBeatmapLevel levelData = gameplayCoreSceneSetupData.difficultyBeatmap.level;
            bool isCustomLevel = true;
            string? mapHash = null;
            try { mapHash = levelData.levelID.Split('_')[2]; }
            catch { isCustomLevel = false; }
            isCustomLevel = isCustomLevel && mapHash != null && mapHash.Length == 40;

            SongCore.Data.ExtraSongData.DifficultyData? difficultyData = SongCore.Collections.RetrieveDifficultyData(gameplayCoreSceneSetupData.difficultyBeatmap);

            MapData.Instance.Hash = isCustomLevel ? mapHash : null;
            MapData.Instance.SongName = levelData.songName;
            MapData.Instance.SongSubName = levelData.songSubName;
            MapData.Instance.SongAuthor = levelData.songAuthorName;
            MapData.Instance.Mapper = levelData.levelAuthorName;
            MapData.Instance.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            MapData.Instance.Length = Convert.ToInt32(Math.Round(audioTimeSyncController.songLength));
            PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, gameplayCoreSceneSetupData.difficultyBeatmap.difficulty,
                gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            MapData.Instance.PreviousRecord = playerLevelStats.highScore;
            MapData.Instance.MapType = gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            MapData.Instance.Difficulty = gameplayCoreSceneSetupData.difficultyBeatmap.difficulty.ToString("g");
            MapData.Instance.NJS = gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed;
            MapData.Instance.CustomDifficultyLabel = difficultyData?._difficultyLabel ?? null;

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
                            MapData.Instance.PP = difficulty.approximatePpValue;
                            MapData.Instance.Star = difficulty.stars;
                            MapData.Instance.Send();
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
                else SetSongDetails();

                if (mapHash != null)
                {
                    beatSaver.BeatmapByHash(mapHash).ContinueWith((task) =>
                    {
                        if (task.Result != null)
                        {
                            MapData.Instance.BSRKey = task.Result.ID;
                            BeatSaverSharp.Models.BeatmapVersion? mapDetails = null;
                            try { mapDetails = task.Result.Versions.First(map => map.Hash.ToLower() == mapHash.ToLower()); } catch (Exception ex) { Plugin.Logger.Error(ex); }
                            MapData.Instance.CoverImage = mapDetails?.CoverURL ?? null;
                        }
                        else
                        {
                            MapData.Instance.BSRKey = null;
                            MapData.Instance.CoverImage = null;
                        }
                        MapData.Instance.Send();
                    });
                }
            }

            if (MapData.Instance.Hash != previousHash) MapData.Instance.PreviousBSR = previousBSRKey;

            MapData.Instance.Modifiers.Add("noFailOn0Energy", gameplayCoreSceneSetupData.gameplayModifiers.noFailOn0Energy);
            MapData.Instance.Modifiers.Add("oneLife", gameplayCoreSceneSetupData.gameplayModifiers.instaFail);
            MapData.Instance.Modifiers.Add("fourLives", gameplayCoreSceneSetupData.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery);
            MapData.Instance.Modifiers.Add("noBombs", gameplayCoreSceneSetupData.gameplayModifiers.noBombs);
            MapData.Instance.Modifiers.Add("noWalls", gameplayCoreSceneSetupData.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles);
            MapData.Instance.Modifiers.Add("noArrows", gameplayCoreSceneSetupData.gameplayModifiers.noArrows);
            MapData.Instance.Modifiers.Add("ghostNotes", gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes);
            MapData.Instance.Modifiers.Add("disappearingArrows", gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows);
            MapData.Instance.Modifiers.Add("smallNotes", gameplayCoreSceneSetupData.gameplayModifiers.smallCubes);
            MapData.Instance.Modifiers.Add("proMode", gameplayCoreSceneSetupData.gameplayModifiers.proMode);
            MapData.Instance.Modifiers.Add("strictAngles", gameplayCoreSceneSetupData.gameplayModifiers.strictAngles);
            MapData.Instance.Modifiers.Add("zenMode", gameplayCoreSceneSetupData.gameplayModifiers.zenMode);
            MapData.Instance.Modifiers.Add("slowerSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 0.85f);
            MapData.Instance.Modifiers.Add("fasterSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 1.2f);
            MapData.Instance.Modifiers.Add("superFastSong", gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul == 1.5f);

            foreach (KeyValuePair<string, bool> keyValue in MapData.Instance.Modifiers)
                if (MapData.Instance.Modifiers[keyValue.Key] && Enum.IsDefined(typeof(EModifiers), keyValue.Key))
                    MapData.Instance.ModifiersMultiplier += (int)(EModifiers)Enum.Parse(typeof(EModifiers), keyValue.Key) / 100f;

            MapData.Instance.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null;
            MapData.Instance.PracticeModeModifiers.Add("songSpeedMul", MapData.Instance.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.songSpeedMul : 1);
            MapData.Instance.PracticeModeModifiers.Add("startInAdvanceAndClearNotes", MapData.Instance.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.startInAdvanceAndClearNotes ? 1 : 0 : 0);
            MapData.Instance.PracticeModeModifiers.Add("startSongTime", MapData.Instance.PracticeMode ? gameplayCoreSceneSetupData!.practiceSettings!.startSongTime : 0);

            timer.Elapsed += TimerElapsedEvent;
            beatmapObjectManager.noteWasCutEvent += NoteWasCutEvent;
            beatmapObjectManager.noteWasMissedEvent += NoteWasMissedEvent;
            gameEnergyCounter.gameEnergyDidChangeEvent += EnergyDidChangeEvent;

            MapData.Instance.InLevel = true;
            timer.Start();

            MapData.Instance.Send();
            LiveData.Instance.Send();
        }

        private void TimerElapsedEvent(object sender, ElapsedEventArgs ev)
        {
            LiveData.Instance.TimeElapsed = (int)Math.Round(audioTimeSyncController.songTime);
            if (Math.Truncate(DateTime.Now.Subtract(LiveData.Instance.lastSendTime).TotalMilliseconds) > 950 / MapData.Instance.PracticeModeModifiers["songSpeedMul"])
                LiveData.Instance.Send(ELiveDataEventTriggers.TimerElapsed);
        }

        private void LevelPausedEvent()
        {
            timer.Stop();
            MapData.Instance.LevelPaused = true;
            MapData.Instance.Send();
        }

        private void LevelUnpausedEvent()
        {
            timer.Start();
            MapData.Instance.LevelPaused = false;
            MapData.Instance.Send();
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
            if (MapData.Instance.Modifiers["noFailOn0Energy"] && health <= 0)
            {
                MapData.Instance.LevelFailed = true;
                MapData.Instance.ModifiersMultiplier += -50 / 100f;
                MapData.Instance.Send();
            }
            if (health < LiveData.Instance.PlayerHealth) { LiveData.Instance.Combo = 0; }
            LiveData.Instance.PlayerHealth = health;
            LiveData.Instance.Send(ELiveDataEventTriggers.EnergyChange);
        }

        private void NoteWasMissedEvent(NoteController noteController)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                LiveData.Instance.Combo = 0;
                LiveData.Instance.FullCombo = false;
                LiveData.Instance.Misses++;
                LiveData.Instance.ColorType = noteController.noteData.colorType;
                LiveData.Instance.Send(ELiveDataEventTriggers.NoteMissed);
            }
        }

        private void LevelQuitEvent() { MapData.Instance.LevelQuit = true; }

        private void LevelFailedEvent() { MapData.Instance.LevelFailed = true; }

        private void LevelFinishedEvent() { MapData.Instance.LevelFinished = true; }

        private void RelativeScoreOrImmediateRankDidChangeEvent() //For replay mode
        {
            TextMeshProUGUI textMeshProUGUI = scoreUIController!.GetField<TextMeshProUGUI, ScoreUIController>("_scoreText");
            LiveData.Instance.Score = int.Parse(textMeshProUGUI.text.Replace(" ", ""));
            LiveData.Instance.ScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.Instance.Score, MapData.Instance.ModifiersMultiplier);
            LiveData.Instance.MaxScoreWithMultipliers = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(LiveData.Instance.MaxScore, MapData.Instance.ModifiersMultiplier);
            SetRankAndAccuracy();
        }

        private void ScoreDidChangeEvent(int score, int scoreWithMultipliers)
        {
            LiveData.Instance.Score = score;
            LiveData.Instance.ScoreWithMultipliers = scoreWithMultipliers;
            SetRankAndAccuracy();
        }

        private void SetRankAndAccuracy()
        {
            LiveData.Instance.Accuracy = relativeScoreAndImmediateRankCounter.relativeScore * 100;
            LiveData.Instance.Rank = relativeScoreAndImmediateRankCounter.immediateRank.ToString();
            LiveData.Instance.Send(ELiveDataEventTriggers.ScoreChange);
        }

        private void NoteWasCutEvent(NoteController arg1, in NoteCutInfo _noteCutInfo)
        {
            if (_noteCutInfo.allIsOK)
            {
                LiveData.Instance.ColorType = arg1.noteData.colorType;
                LiveData.Instance.Combo++;
                noteCount++;
                //_noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(new SwingRatingCounterDidFinishController(_noteCutInfo));
            }

            //LiveData.Instance.Send(); //Sent by SetRankAndAccuracy()
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