using RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class BackgroundChanger : MonoBehaviour, ILevelLoadable
    {
        public Sprite[] backgrounds;

        public void OnLevelLoaded(Level level)
        {
            var lastBackgroundIndex = GameManager.instance.GetLastBackgroundIndex();
            int newBackgroundIndex;
            do
            {
                newBackgroundIndex = Random.Range(0, backgrounds.Length);
            } while (newBackgroundIndex == lastBackgroundIndex);

            GameManager.instance.SetLastBackgroundIndex(newBackgroundIndex);
            GetComponent<Image>().sprite = backgrounds[newBackgroundIndex];
        }
    }
}