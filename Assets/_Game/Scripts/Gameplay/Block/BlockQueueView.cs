using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Block
{
    public class BlockQueueView : MonoBehaviour
    {
        [SerializeField] RectTransform container;
        [SerializeField] RectTransform dragRoot;
        [SerializeField] BlockView blockPrefab;

        readonly List<BlockView> views = new();
        GameplayController gameplay;
        BlockQueue boundQueue;

        public void Initialize(GameplayController gameplayController)
        {
            if (boundQueue != null) boundQueue.Changed -= Rebuild;
            gameplay = gameplayController;
            boundQueue = gameplay.Session.Queue;
            boundQueue.Changed += Rebuild;
            Rebuild();
        }

        void Rebuild()
        {
            foreach (var view in views)
                if (view) Destroy(view.gameObject);
            views.Clear();

            foreach (var shape in gameplay.Session.Queue.Shapes)
            {
                var view = Instantiate(blockPrefab, container);
                view.Initialize(shape);
                view.GetComponent<BlockDragHandler>().Initialize(gameplay, dragRoot);
                views.Add(view);
            }
        }

        void OnDestroy()
        {
            if (boundQueue != null) boundQueue.Changed -= Rebuild;
        }
    }
}
