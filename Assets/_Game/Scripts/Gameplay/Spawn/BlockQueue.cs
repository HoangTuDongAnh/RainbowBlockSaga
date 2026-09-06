using System;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;

namespace RainbowBlockSaga.Gameplay.Spawn
{
    public class BlockQueue
    {
        readonly List<BlockShapeData> shapes = new();
        public IReadOnlyList<BlockShapeData> Shapes => shapes;
        public event Action Changed;

        public void SetBatch(IEnumerable<BlockShapeData> batch)
        {
            shapes.Clear();
            shapes.AddRange(batch);
            Changed?.Invoke();
        }

        public bool Consume(BlockShapeData shape)
        {
            bool removed = shapes.Remove(shape);
            if (removed) Changed?.Invoke();
            return removed;
        }

        public void Add(BlockShapeData shape)
        {
            shapes.Add(shape);
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (shapes.Count == 0)
                return;

            shapes.Clear();
            Changed?.Invoke();
        }

        public bool IsEmpty => shapes.Count == 0;
    }
}
