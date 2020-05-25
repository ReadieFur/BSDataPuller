using BeatSaverSharp;
using BS_Utils.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using IPA.Utilities;

namespace DataPuller
{
    //Try to convert this all to a MonoBehaviour (This will run on every frame) https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    class MapEvents
    {
        private BeatSaver beatSaver = new BeatSaver(new HttpOptions() { ApplicationName = "BSDataPuller", Version = Assembly.GetExecutingAssembly().GetName().Version });
        private GameplayCoreSceneSetupData previousMap = null;
        private static ScoreController scoreController = null;
        private Beatmap previousBeatmap = null;
        private Timer timer = new Timer();

        internal void Start()
        {
            timer.Interval = 1000;
            timer.Elapsed += (se, ev) => { LevelInfo.Timer++; };

            BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
            BSEvents.noteWasCut += BSEvents_noteWasCut;
            BSEvents.levelCleared += BSEvents_levelCleared;
            BSEvents.levelFailed += BSEvents_levelFailed;
            BSEvents.levelQuit += BSEvents_levelQuit;
            BSEvents.noteWasMissed += BSEvents_noteWasMissed;
            BSEvents.songPaused += () => { LevelInfo.LevelPaused = true; timer.Stop(); };
            BSEvents.songUnpaused += () => { LevelInfo.LevelPaused = false; timer.Start(); };
            BSEvents.energyDidChange += (health) => { LevelInfo.PlayerHealth = health; };
        }

        private void BSEvents_noteWasMissed(NoteData noteData, int arg2)
        {
            LevelInfo.Combo = 0;
            LevelInfo.FullCombo = false;
            LevelInfo.Misses++;
        }

        private void BSEvents_levelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            timer.Stop();
            LevelInfo.LevelQuit = true;
            LevelInfo.InLevel = false;
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            //Need to get new args from here
            timer.Stop();
            LevelInfo.LevelFailed = true;
            LevelInfo.InLevel = false;
        }

        private void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2)
        {
            //Need to get new args from here
            timer.Stop();
            LevelInfo.LevelFinished = true;
            LevelInfo.InLevel = false;
        }

        private void ResetLevelInfo()
        {
            timer.Stop();

            //Level Info
            LevelInfo.LevelPaused = default;
            LevelInfo.LevelFinished = default;
            LevelInfo.LevelFailed = default;
            LevelInfo.LevelQuit = default;

            //Map Info
            LevelInfo.SongName = default;
            LevelInfo.SongSubName = default;
            LevelInfo.SongAuthor = default;
            LevelInfo.Mapper = default;
            LevelInfo.BSRKey = default;
            LevelInfo.BPM = default;
            LevelInfo.coverImage = default;
            LevelInfo.Length = default;
            LevelInfo.PreviousRecord = default;

            //Difficult Info
            LevelInfo.Difficulty = default;
            LevelInfo.NJS = default;

            //Score Info
            LevelInfo.FullCombo = true;
            LevelInfo.Score = default;
            LevelInfo.Combo = default;
            LevelInfo.Misses = default;
            LevelInfo.Accuracy = 100;
            LevelInfo.BlockHitScores = new List<int>();
            LevelInfo.PlayerHealth = 0.5;

            //Modifiers/Pratice Mode
            LevelInfo.Modifiers = new Dictionary<string, bool>();
            LevelInfo.PraticeMode = default;
            LevelInfo.PraticeModeModifiers = new Dictionary<string, float>();

            //Misc
            LevelInfo.Timer = 0;
            //LevelInfo.PreviousBSR = default;
        }

        private void BSEvents_gameSceneLoaded()
        {
            ResetLevelInfo();
            LevelInfo.InLevel = true;
            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            scoreController.scoreDidChangeEvent += (int1, int2) =>
            {
                LevelInfo.Score = int1;
                LevelInfo.Accuracy = int1 / scoreController.immediateMaxPossibleRawScore * 100f;
            }; //This will be duplicated every level - data effect none, after time could cause a pefromance hit

            AudioTimeSyncController audioController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            GameplayCoreSceneSetupData currentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;

            IBeatmapLevel levelData = currentMap.difficultyBeatmap.level;

            LevelInfo.SongName = levelData.songName;
            LevelInfo.SongSubName = levelData.songSubName;
            LevelInfo.SongAuthor = levelData.songAuthorName;
            LevelInfo.Mapper = levelData.levelAuthorName;
            LevelInfo.BPM = Convert.ToInt32(Math.Round(levelData.beatsPerMinute));
            LevelInfo.Length = Convert.ToInt32(Math.Round(audioController.songLength));
            PlayerLevelStatsData playerLevelStats = playerData.GetPlayerLevelStatsData(levelData.levelID, currentMap.difficultyBeatmap.difficulty,
                currentMap.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            LevelInfo.PreviousRecord = playerLevelStats.highScore;

            if (previousMap == null || previousBeatmap == null || (levelData.levelID != previousMap.difficultyBeatmap.level.levelID))
            {
                Task.Run(async () =>
                {
                    if (previousBeatmap != null) { LevelInfo.PreviousBSR = previousBeatmap.Key; }
                    Beatmap bm = await beatSaver.Hash(levelData.levelID.Replace("custom_level_", ""));
                    if (bm != null)
                    {
                        LevelInfo.BSRKey = bm.Key;
                        LevelInfo.coverImage = BeatSaver.BaseURL + bm.CoverURL;
                        previousBeatmap = bm;
                    }
                    else { LevelInfo.BSRKey = null; LevelInfo.coverImage = null; previousBeatmap = null; }
                });
            }
            else { LevelInfo.BSRKey = previousBeatmap.Key; LevelInfo.coverImage = BeatSaver.BaseURL + previousBeatmap.CoverURL; }

            LevelInfo.Difficulty = currentMap.difficultyBeatmap.difficultyRank;
            LevelInfo.NJS = currentMap.difficultyBeatmap.noteJumpMovementSpeed;

            LevelInfo.Modifiers.Add("instaFail", currentMap.gameplayModifiers.instaFail);
            LevelInfo.Modifiers.Add("batteryEnergy", currentMap.gameplayModifiers.batteryEnergy);
            LevelInfo.Modifiers.Add("disappearingArrows", currentMap.gameplayModifiers.disappearingArrows);
            LevelInfo.Modifiers.Add("ghostNotes", currentMap.gameplayModifiers.ghostNotes);
            //LevelInfo.Modifiers.Add("failOnSaberClash", currentMap.gameplayModifiers.failOnSaberClash);
            LevelInfo.Modifiers.Add("fasterSong", currentMap.gameplayModifiers.songSpeedMul == 1.2f ? true : false);
            LevelInfo.Modifiers.Add("noFail", currentMap.gameplayModifiers.noFail); LevelInfo.PlayerHealth = LevelInfo.Modifiers["noFail"] ? 1 : 0.5;
            LevelInfo.Modifiers.Add("noObstacles", currentMap.gameplayModifiers.noObstacles);
            LevelInfo.Modifiers.Add("noBombs", currentMap.gameplayModifiers.noBombs);
            LevelInfo.Modifiers.Add("slowerSong", currentMap.gameplayModifiers.songSpeedMul == 0.85f ? true : false);
            LevelInfo.Modifiers.Add("noArrows", currentMap.gameplayModifiers.noArrows);
            if (currentMap.practiceSettings != null) //In pratice mode
            {
                LevelInfo.PraticeMode = true;
                LevelInfo.PraticeModeModifiers.Add("startSongTime", (float)Math.Round(currentMap.practiceSettings.startSongTime));
                LevelInfo.PraticeModeModifiers.Add("songSpeedMul", currentMap.practiceSettings.songSpeedMul);
            }

            previousMap = currentMap;
            timer.Start();
            new GameObject("InLevel").AddComponent<InLevel>();
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo noteCutInfo, int arg3)
        {
            if (noteCutInfo.allIsOK)
            {
                LevelInfo.Combo++;
                noteCutInfo.swingRatingCounter.didFinishEvent += (saberSwingRatingCounter) =>
                {
                    ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore);
                    int blockScoreWithoutModifier = beforeCutRawScore + afterCutRawScore + cutDistanceRawScore;
                    LevelInfo.BlockHitScores.Add(blockScoreWithoutModifier);
                };
            }
            else
            {
                LevelInfo.Combo = 0;
                LevelInfo.FullCombo = false;
                LevelInfo.Misses++;
            }
        }

        class InLevel : MonoBehaviour
        {
            public void Update()
            {
                LevelInfo.eventJsonUpdated();
            }
        }
    }
}
