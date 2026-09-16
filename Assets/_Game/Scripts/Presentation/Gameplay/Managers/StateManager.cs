using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class StateManager : SingletonBehaviour<StateManager>
    {
        [SerializeField]
        private GameObject[] mainMenus;

        [SerializeField]
        private GameObject[] maps;

        [SerializeField]
        private GameObject[] games;

        private EScreenStates _currentState;

        public EScreenStates CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                SetActiveState(mainMenus, _currentState == EScreenStates.MainMenu);
                SetActiveState(maps, _currentState == EScreenStates.Map);
                SetActiveState(games, _currentState == EScreenStates.Game);
            }
        }

        private void SetActiveState(GameObject[] gameObjects, bool isActive)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.activeSelf != isActive)
                {
                    gameObject.SetActive(isActive);
                }
            }
        }
    }

    public enum EScreenStates
    {
        MainMenu,
        Map,
        Game
    }
}