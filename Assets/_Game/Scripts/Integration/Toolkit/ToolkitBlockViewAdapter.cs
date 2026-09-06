using BlockPuzzleGameToolkit.Scripts.Gameplay;
using RainbowBlockSaga.Gameplay.Block;
using UnityEngine;

namespace RainbowBlockSaga.Integration.Toolkit
{
    /// <summary>
    /// Keeps the original toolkit Shape hierarchy/visuals while exposing the new BlockShapeData model.
    /// </summary>
    public class ToolkitBlockViewAdapter : MonoBehaviour
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
            Data = ToolkitShapeDataAdapter.GetOrCreate(LegacyShape.shapeTemplate);
        }
    }
}
