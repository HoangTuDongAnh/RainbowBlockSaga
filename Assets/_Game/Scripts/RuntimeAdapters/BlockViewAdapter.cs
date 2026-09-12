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
        Shape legacyShape;

        public Shape LegacyShape => legacyShape ? legacyShape : legacyShape = GetComponent<Shape>();
        public BlockShapeData Data { get; private set; }

        void Awake()
        {
            legacyShape = GetComponent<Shape>();
            legacyShape.OnShapeUpdated += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (legacyShape != null)
                legacyShape.OnShapeUpdated -= Refresh;
        }

        public void Refresh()
        {
            var catalog = GameSessionRuntime.Current?.ShapeCatalog;
            Data = ShapeDataAdapter.GetOrCreate(
                LegacyShape.shapeTemplate,
                catalog);
        }
    }
}
