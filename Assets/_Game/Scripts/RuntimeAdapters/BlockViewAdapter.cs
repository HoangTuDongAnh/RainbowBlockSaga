using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Runtime;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.RuntimeAdapters
{
    /// <summary>
    /// Connects the current Shape presentation to BlockShapeData.
    /// </summary>
    public class BlockViewAdapter : MonoBehaviour
    {
        Shape shape;

        public Shape Shape => shape ? shape : shape = GetComponent<Shape>();
        public BlockShapeData Data { get; private set; }

        void Awake()
        {
            shape = GetComponent<Shape>();
            shape.OnShapeUpdated += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (shape != null)
                shape.OnShapeUpdated -= Refresh;
        }

        public void Refresh()
        {
            var catalog = GameSessionRuntime.Current?.ShapeCatalog;
            Data = ShapeDataAdapter.GetOrCreate(
                Shape.shapeTemplate,
                catalog);
        }
    }
}
