using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
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

            if (profile.UseBoardLookAhead)
            {
                var planned = PlanBatch(board, candidates, profile);
                if (planned != null) return planned;
            }

            for (int i = 0; i < profile.BatchSize; i++)
                result.Add(WeightedPick(candidates, profile.PreferDistinctShapes ? result : null));

            int requiredPlayable = profile.EnsureAtLeastOnePlayable
                ? Mathf.Clamp(Mathf.Max(1, profile.MinPlayablePerBatch), 1, profile.BatchSize)
                : 0;

            EnsurePlayableCount(board, candidates, result, requiredPlayable);
            return result;
        }

        // Bounded stochastic rollouts: each candidate tray has a witnessed sequence
        // of placements, including line clears. The real board is never modified.
        List<BlockShapeData> PlanBatch(BoardModel board, List<BlockShapeData> candidates, SpawnProfileData profile)
        {
            var resolver = new BoardResolver();
            List<BlockShapeData> best = null;
            float bestScore = float.NegativeInfinity;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                var simulated = CopyBoard(board);
                var batch = new List<BlockShapeData>();
                float score = 0;
                for (int slot = 0; slot < profile.BatchSize; slot++)
                {
                    var playable = candidates.FindAll(s => placement.HasAnyValidPlacement(simulated, s));
                    if (playable.Count == 0) break;
                    var shape = WeightedPick(playable, profile.PreferDistinctShapes ? batch : null);
                    var anchors = placement.GetValidPlacements(simulated, shape);
                    BoardModel next = null;
                    float nextScore = float.NegativeInfinity;
                    int offset = Random.Range(0, anchors.Count);
                    int samples = Mathf.Min(12, anchors.Count);
                    for (int sample = 0; sample < samples; sample++)
                    {
                        var candidate = CopyBoard(simulated);
                        var anchor = anchors[(offset + sample * anchors.Count / samples) % anchors.Count];
                        placement.Place(candidate, shape, anchor);
                        var cleared = resolver.Resolve(candidate);
                        float value = cleared.ClearedCells.Count * 2f - CountIsolatedHoles(candidate) * 3f + Random.value;
                        if (value > nextScore) { nextScore = value; next = candidate; }
                    }
                    simulated = next;
                    batch.Add(shape);
                    score += nextScore;
                }
                // Do not claim a complete path when the bounded search found none.
                if (batch.Count == profile.BatchSize && score > bestScore)
                {
                    best = batch;
                    bestScore = score;
                }
            }
            return best;
        }

        static BoardModel CopyBoard(BoardModel source)
        {
            var copy = new BoardModel(source.Data);
            foreach (var cell in source.GetOccupiedCoords()) copy.SetOccupied(cell);
            return copy;
        }

        static int CountIsolatedHoles(BoardModel board)
        {
            int holes = 0;
            foreach (var cell in board.GetPlayableCoords())
                if (board.IsEmpty(cell) &&
                    !board.IsEmpty(new BoardCoord(cell.X - 1, cell.Y)) &&
                    !board.IsEmpty(new BoardCoord(cell.X + 1, cell.Y)) &&
                    !board.IsEmpty(new BoardCoord(cell.X, cell.Y - 1)) &&
                    !board.IsEmpty(new BoardCoord(cell.X, cell.Y + 1))) holes++;
            return holes;
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
