using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;

namespace RainbowBlockSaga.Runtime
{
    public static class ShapeDataAdapter
    {
        static readonly Dictionary<UnityEngine.Object, BlockShapeData> Cache = new();
        static readonly Dictionary<BlockShapeData, UnityEngine.Object> ReverseCache = new();

        public static BlockShapeData GetOrCreate(ShapeDescriptor descriptor)
        {
            if (descriptor == null || descriptor.Handle == null)
                return null;

            if (Cache.TryGetValue(descriptor.Handle, out var data) && data)
                return data;

            data = ScriptableObject.CreateInstance<BlockShapeData>();
            data.name = "Runtime_" + descriptor.Name;
            data.Cells = BuildCells(descriptor);
            data.SpawnWeight = Mathf.Max(0.0001f, descriptor.SpawnWeight);
            data.MinAdventureLevel = Mathf.Max(1, descriptor.MinAdventureLevel);

            Cache[descriptor.Handle] = data;
            ReverseCache[data] = descriptor.Handle;
            return data;
        }

        public static BlockShapeData GetOrCreate(
            UnityEngine.Object handle,
            IShapeCatalog catalog)
        {
            if (handle == null ||
                catalog == null ||
                !catalog.TryGetDescriptor(handle, out var descriptor))
            {
                return null;
            }

            return GetOrCreate(descriptor);
        }

        public static UnityEngine.Object GetPresentationHandle(BlockShapeData data)
        {
            if (data != null &&
                ReverseCache.TryGetValue(data, out var handle))
            {
                return handle;
            }

            return null;
        }

        static List<BoardCoord> BuildCells(ShapeDescriptor descriptor)
        {
            var result = new List<BoardCoord>();

            if (descriptor.Cells == null)
                return result;

            foreach (var cell in descriptor.Cells)
                result.Add(new BoardCoord(cell.x, cell.y));

            return result;
        }
    }
}
