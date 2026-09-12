using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Contracts
{
    public sealed class ShapeDescriptor
    {
        public UnityEngine.Object Handle;
        public string Name;
        public IReadOnlyList<Vector2Int> Cells;
        public float SpawnWeight;
        public int MinAdventureLevel;
        public int MinScore;
    }

    public interface IShapeCatalog
    {
        IReadOnlyList<ShapeDescriptor> GetEligibleShapes();

        bool TryGetDescriptor(
            UnityEngine.Object handle,
            out ShapeDescriptor descriptor);
    }
}
