using System;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.Enums;

using RainbowBlockSaga.Presentation.Scripts.LevelsData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public static class GameDataManager
    {
        public static int LevelNum;

        private static Level _level;
        public static bool isTestPlay = false;

        public static void ClearPlayerProgress()
        {
            PlayerPrefs.DeleteKey("Level");
            PlayerPrefs.Save();
        }

        public static void ClearALlData()
        {
            #if UNITY_EDITOR
            // clear variables ResourceObject from Resources/Variables
            var resourceObjects = Resources.LoadAll<ResourceObject>("Variables");
            foreach (var resourceObject in resourceObjects)
            {
                resourceObject.Set(0);
            }

            AssetDatabase.SaveAssets();

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            #endif
        }

        public static void UnlockLevel(int currentLevel)
        {
            if (isTestPlay) return;
            int savedLevel = PlayerPrefs.GetInt("Level", 1);
            if (savedLevel < currentLevel)
            {
                LevelNum = currentLevel;
                PlayerPrefs.SetInt("Level", currentLevel);
                PlayerPrefs.Save();
            }
        }

        public static int GetLevelNum()
        {
            return PlayerPrefs.GetInt("Level", 1);
        }

        public static Level GetLevel()
        {
            if (_level != null && isTestPlay)
            {
                return _level;
            }

            if (_level != null)
            {
                return _level;
            }

            _level = GetGameMode() switch
            {
                EGameMode.Endless => Resources.Load<Level>("Misc/EndlessLevel"),
                EGameMode.Timed => Resources.Load<Level>("Misc/TimeLevel"),
                _ => ArcadeLevelCatalog.Find(GetLevelNum())
            };
            return _level;
        }




        public static void SetLevel(Level level)
        {
            _level = level;
        }
public static EGameMode GetGameMode()
        {
            return (EGameMode)PlayerPrefs.GetInt("GameMode");
        }

        public static void SetGameMode(EGameMode gameMode)
        {
            if (GetGameMode() != gameMode)
                _level = null;
            PlayerPrefs.SetInt("GameMode", (int)gameMode);
            PlayerPrefs.Save();
        }

        public static void SetAllLevelsCompleted()
        {
            var levels = Resources.LoadAll<Level>("Levels").Length;
            PlayerPrefs.SetInt("Level", levels);
            PlayerPrefs.Save();
        }

        internal static bool HasMoreLevels()
        {
            var playedLevel = GetLevel();
            return playedLevel != null && ArcadeLevelCatalog.Next(playedLevel.Number) != null;
        }

        public static void SetLevelNum(int stateCurrentLevel)
        {
            LevelNum = stateCurrentLevel;
        }
    }
}
