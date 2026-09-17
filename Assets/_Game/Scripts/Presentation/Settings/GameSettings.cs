using RainbowBlockSaga.Presentation.Scripts.Enums;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Settings
{

    public class GameSettings : SettingsBase
    {
        [Header("On start")]
        public int coins;

        [Header("Optional Features")]
        public bool enableLuckySpin = true;
        public bool enablePreFailedPopup = true;

        [Header("Timed mode")]
        public bool enableTimedMode = false;
        public int globalTimedModeSeconds = 60; // Default time value for timed mode in seconds
        public int continueTimerBonus = 30;

        [Header("Gameplay")]
            [Min(0), Tooltip("Points per placed cell and per cleared cell, before the clear combo multiplier.")]
        public int ScorePerCell = 10;
        [Header("Endless scoring and Rainbow feedback")]
        public RainbowBlockSaga.Presentation.Contracts.EndlessScoringSettings endlessScoring = new();
        public bool enablePool;
        public int ResetComboAfterMoves = 3;

        public int continuePrice = 15;
        public int failedTimerStart = 5;

        [Header("Map settings")]
        public EMapType mapType = EMapType.Tiled;
        public int maxLevelsInRow = 8;
        public int maxRows = 8;

        [Header("Booter")]
        public int shuffleItem;
        public int Bomb;
    }
}
