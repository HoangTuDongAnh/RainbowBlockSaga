using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Spawn
{
    public class SpawnStrategy
    {
        readonly PlacementService placement;

        public SpawnStrategy(PlacementService placement)
        {
            this.placement = placement;
        }

        public List<BlockShapeData> CreateBatch(BoardModel board, SpawnProfileData profile, int adventureLevel = 1)
        {
            var candidates = new List<BlockShapeData>();
            foreach (var shape in profile.Shapes)
                if (shape && shape.MinAdventureLevel <= adventureLevel && shape.SpawnWeight > 0f)
                    candidates.Add(shape);

            var result = new List<BlockShapeData>(profile.BatchSize);
            if (candidates.Count == 0)
                return result;

            for (int i = 0; i < profile.BatchSize; i++)
                result.Add(WeightedPick(candidates, profile.PreferDistinctShapes ? result : null));

            int requiredPlayable = profile.EnsureAtLeastOnePlayable
                ? Mathf.Clamp(Mathf.Max(1, profile.MinPlayablePerBatch), 1, profile.BatchSize)
                : 0;

            EnsurePlayableCount(board, candidates, result, requiredPlayable);
            return result;
        }

        void EnsurePlayableCount(
            BoardModel board,
            List<BlockShapeData> candidates,
            List<BlockShapeData> result,
            int requiredPlayable)
        {
            if (requiredPlayable <= 0)
                return;

            var playableCandidates = new List<BlockShapeData>();
            foreach (var shape in candidates)
                if (placement.HasAnyValidPlacement(board, shape))
                    playableCandidates.Add(shape);

            if (playableCandidates.Count == 0)
                return;

            int playableCount = CountPlayable(board, result);
            for (int i = result.Count - 1; i >= 0 && playableCount < requiredPlayable; i--)
            {
                if (placement.HasAnyValidPlacement(board, result[i]))
                    continue;

                result[i] = WeightedPick(
                    playableCandidates,
                    profileDistinctExclusion(result, i));

                playableCount = CountPlayable(board, result);
            }
        }

        // Keeps PreferDistinctShapes behavior when replacing an impossible pick.
        List<BlockShapeData> profileDistinctExclusion(List<BlockShapeData> current, int replaceIndex)
        {
            var excluded = new List<BlockShapeData>();
            for (int i = 0; i < current.Count; i++)
                if (i != replaceIndex && current[i] != null && !excluded.Contains(current[i]))
                    excluded.Add(current[i]);
            return excluded;
        }

        int CountPlayable(BoardModel board, List<BlockShapeData> shapes)
        {
            int count = 0;
            foreach (var shape in shapes)
                if (shape && placement.HasAnyValidPlacement(board, shape))
                    count++;
            return count;
        }

        BlockShapeData WeightedPick(List<BlockShapeData> source, List<BlockShapeData> excluded)
        {
            var pool = new List<BlockShapeData>();
            foreach (var item in source)
                if (excluded == null || !excluded.Contains(item))
                    pool.Add(item);

            if (pool.Count == 0)
                pool.AddRange(source);

            float total = 0f;
            foreach (var item in pool)
                total += item.SpawnWeight;

            float roll = Random.value * total;
            foreach (var item in pool)
            {
                roll -= item.SpawnWeight;
                if (roll <= 0f)
                    return item;
            }

            return pool[pool.Count - 1];
        }
    }
}
