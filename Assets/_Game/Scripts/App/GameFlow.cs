using System;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Modes.Adventure;
using RainbowBlockSaga.Modes.Endless;
using RainbowBlockSaga.Foundation;
using UnityEngine;

namespace RainbowBlockSaga.App
{
    public class GameFlow : Singleton<GameFlow>
    {
        [SerializeField] GameplayController gameplayController;
        public GameMode CurrentMode { get; private set; }
        public AdventureLevelData CurrentAdventureLevel { get; private set; }
        public event Action<GameMode> ModeChanged;
        public event Action<GameSessionResult> SessionCompleted;

        protected override void SetUp()
        {
            gameplayController.SessionEnded += OnSessionEnded;
        }

        public void StartAdventure(int levelNumber)
        {
            var level = AdventureSystem.Ins.GetLevel(levelNumber);
            CurrentMode = GameMode.Adventure;
            CurrentAdventureLevel = level;
            gameplayController.Initialize(AdventureSystem.Ins.CreateSessionConfig(level));
            ModeChanged?.Invoke(CurrentMode);
        }

        public void StartEndless()
        {
            CurrentMode = GameMode.Endless;
            CurrentAdventureLevel = null;
            gameplayController.Initialize(EndlessSystem.Ins.CreateSessionConfig());
            ModeChanged?.Invoke(CurrentMode);
        }

        public void ReturnHome()
        {
            if (gameplayController.Session != null && !gameplayController.Session.IsEnded)
                gameplayController.Session.Cancel();
            CurrentMode = GameMode.None;
            CurrentAdventureLevel = null;
            ModeChanged?.Invoke(CurrentMode);
        }

        void OnSessionEnded(GameSessionResult result)
        {
            if (CurrentMode == GameMode.Adventure) AdventureSystem.Ins.Complete(CurrentAdventureLevel, result);
            else if (CurrentMode == GameMode.Endless) EndlessSystem.Ins.Complete(result);
            SessionCompleted?.Invoke(result);
        }
    }
}
