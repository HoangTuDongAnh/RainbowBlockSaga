using BlockPuzzleGameToolkit.Scripts.Gameplay;
using RainbowBlockSaga.Gameplay.Block;
using UnityEngine;

namespace RainbowBlockSaga.Runtime
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
