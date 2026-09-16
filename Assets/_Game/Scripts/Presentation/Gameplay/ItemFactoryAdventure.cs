using RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class ItemFactoryAdventure : ItemFactory, IBeforeLevelLoadable
    {
        public void OnLevelLoaded(Level level)
        {
            _oneColorMode = level.levelType.singleColorMode;
            if (_oneColorMode)
            {
               
                _oneColor = Random.Range(1, items.Length);
            }
            //Debug.Log("Hello" + items.Length);
        }
    }
}