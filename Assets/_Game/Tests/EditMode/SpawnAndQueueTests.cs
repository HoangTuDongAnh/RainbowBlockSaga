using System.Collections.Generic;
using NUnit.Framework;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEngine;

namespace RainbowBlockSaga.Tests.EditMode
{
    /// <summary>Kiểm tra SpawnStrategy và BlockQueue (REQ-009, REQ-029..REQ-031).</summary>
    public class SpawnAndQueueTests : TestBase
    {
        const int Iterations = 100;
        PlacementService placement;
        SpawnStrategy strategy;

        [SetUp]
        public void SetUp()
        {
            placement = new PlacementService();
            strategy = new SpawnStrategy(placement);
        }

        SpawnProfileData Profile(params BlockShapeData[] shapes)
        {
            var profile = Track(ScriptableObject.CreateInstance<SpawnProfileData>());
            profile.BatchSize = 3;
            profile.EnsureAtLeastOnePlayable = true;
            profile.MinPlayablePerBatch = 2;
            profile.PreferDistinctShapes = true;
            profile.Shapes = new List<BlockShapeData>(shapes);
            return profile;
        }

        [Test]
        public void CreateBatch_ReturnsBatchSizeShapes()
        {
            var board = NewBoard();
            var profile = Profile(Single(), DominoH(), Square2());

            for (int i = 0; i < Iterations; i++)
                Assert.AreEqual(3, strategy.CreateBatch(board, profile).Count);
        }

        [Test]
        public void CreateBatch_ExcludesZeroWeightAndShapesAboveLevel()
        {
            var board = NewBoard();
            var allowed = Single();
            var zeroWeight = DominoH();
            zeroWeight.SpawnWeight = 0f;
            var lockedUntilLevel5 = Square2();
            lockedUntilLevel5.MinAdventureLevel = 5;
            var profile = Profile(allowed, zeroWeight, lockedUntilLevel5);

            for (int i = 0; i < Iterations; i++)
                foreach (var shape in strategy.CreateBatch(board, profile, 1))
                    Assert.AreSame(allowed, shape);
        }

        [Test]
        public void CreateBatch_IncludesShapeOnceLevelRequirementMet()
        {
            var board = NewBoard();
            var basic = Single();
            var unlocked = Square2();
            unlocked.MinAdventureLevel = 5;
            var profile = Profile(basic, unlocked);

            for (int i = 0; i < Iterations; i++)
                CollectionAssert.Contains(strategy.CreateBatch(board, profile, 5), unlocked);
        }

        [Test]
        public void CreateBatch_AtLeastTwoPlayableShapes_WhenBoardAlmostFull()
        {
            var board = NewBoard();
            FillAll(board);
            board.SetEmpty(new BoardCoord(0, 0));
            board.SetEmpty(new BoardCoord(7, 7)); // chỉ còn 2 ô trống rời nhau
            var profile = Profile(Single(), Square2());

            for (int i = 0; i < Iterations; i++)
            {
                int playable = 0;
                foreach (var shape in strategy.CreateBatch(board, profile))
                    if (placement.HasAnyValidPlacement(board, shape)) playable++;
                Assert.GreaterOrEqual(playable, 2);
            }
        }

        [Test]
        public void CreateBatch_PreferDistinctShapes_ThreeDifferentShapesWhenAvailable()
        {
            var board = NewBoard();
            var profile = Profile(Single(), DominoH(), Square2());

            for (int i = 0; i < Iterations; i++)
            {
                var batch = strategy.CreateBatch(board, profile);
                CollectionAssert.AllItemsAreUnique(batch);
            }
        }

        [Test]
        public void BlockQueue_SetBatchOfThree_ConsumeUntilEmpty()
        {
            var queue = new BlockQueue();
            var a = Single();
            var b = DominoH();
            var c = Square2();
            queue.SetBatch(new[] { a, b, c });
            Assert.AreEqual(3, queue.Shapes.Count);
            Assert.IsFalse(queue.IsEmpty);

            Assert.IsTrue(queue.Consume(b)); // đặt theo thứ tự tùy ý
            Assert.AreEqual(2, queue.Shapes.Count);
            Assert.IsTrue(queue.Consume(c));
            Assert.IsFalse(queue.IsEmpty);
            Assert.IsTrue(queue.Consume(a));
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void BlockQueue_ConsumeShapeNotInQueue_ReturnsFalse()
        {
            var queue = new BlockQueue();
            queue.SetBatch(new[] { Single() });

            Assert.IsFalse(queue.Consume(DominoH()));
            Assert.AreEqual(1, queue.Shapes.Count);
        }
    }
}
