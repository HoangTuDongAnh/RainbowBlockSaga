using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Spawn
{
    [CreateAssetMenu(fileName = "SpawnProfile", menuName = "Rainbow Block Saga/Spawn/Profile")]
    public class SpawnProfileData : BaseData
    {
        [Min(1)] public int BatchSize = 3;
        public bool EnsureAtLeastOnePlayable = true;
        [Min(0)] public int MinPlayablePerBatch = 1;
        public bool PreferDistinctShapes = true;
        public List<BlockShapeData> Shapes = new();
    }
}
