using UnityEngine;

namespace RainbowBlockSaga.App
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] bool startAdventureInEditor;
        [SerializeField, Min(1)] int testAdventureLevel = 1;

        void Start()
        {
#if UNITY_EDITOR
            if (startAdventureInEditor) GameFlow.Ins.StartAdventure(testAdventureLevel);
#endif
        }
    }
}
