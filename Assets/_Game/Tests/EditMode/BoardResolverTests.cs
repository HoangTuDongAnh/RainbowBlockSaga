using NUnit.Framework;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Resolve;

namespace RainbowBlockSaga.Tests.EditMode
{
    /// <summary>Kiểm tra LineDetector/BoardResolver: phát hiện và xóa hàng/cột (REQ-008, 013..016).</summary>
    public class BoardResolverTests : TestBase
    {
        BoardResolver resolver;

        [SetUp]
        public void SetUp() => resolver = new BoardResolver();

        [Test]
        public void Board_DefaultData_Is8x8With64PlayableCells()
        {
            var board = NewBoard();
            Assert.AreEqual(8, board.Width);
            Assert.AreEqual(8, board.Height);
            int count = 0;
            foreach (var _ in board.GetPlayableCoords()) count++;
            Assert.AreEqual(64, count);
        }

        [Test]
        public void Resolve_FullRow_ClearsRowAndKeepsOtherCells()
        {
            var board = NewBoard();
            FillRow(board, 0, 0, 7);
            board.SetOccupied(new BoardCoord(3, 3));

            var result = resolver.Resolve(board);

            Assert.AreEqual(1, result.ClearedLines);
            Assert.AreEqual(8, result.ClearedCells.Count);
            CollectionAssert.AreEqual(new[] { 0 }, result.Rows);
            for (int x = 0; x < 8; x++) Assert.IsTrue(board.IsEmpty(new BoardCoord(x, 0)));
            Assert.IsTrue(board.IsOccupied(new BoardCoord(3, 3)), "Ô ngoài dòng bị xóa phải giữ nguyên");
            Assert.IsFalse(result.IsFullClear);
        }

        [Test]
        public void Resolve_FullColumn_ClearsColumn()
        {
            var board = NewBoard();
            FillColumn(board, 5, 0, 7);

            var result = resolver.Resolve(board);

            Assert.AreEqual(1, result.ClearedLines);
            Assert.AreEqual(8, result.ClearedCells.Count);
            CollectionAssert.AreEqual(new[] { 5 }, result.Columns);
        }

        [Test]
        public void Resolve_RowMissingOneCell_IsNotCleared()
        {
            var board = NewBoard();
            FillRow(board, 0, 0, 6);

            var result = resolver.Resolve(board);

            Assert.AreEqual(0, result.ClearedLines);
            Assert.AreEqual(0, result.ClearedCells.Count);
            Assert.AreEqual(7, CountOccupied(board));
            Assert.IsFalse(result.IsFullClear);
        }

        [Test]
        public void Resolve_TwoFullRows_ClearsBothInOneTurn()
        {
            var board = NewBoard();
            FillRow(board, 0, 0, 7);
            FillRow(board, 1, 0, 7);
            board.SetOccupied(new BoardCoord(4, 4));

            var result = resolver.Resolve(board);

            Assert.AreEqual(2, result.ClearedLines);
            Assert.AreEqual(16, result.ClearedCells.Count);
            Assert.AreEqual(1, CountOccupied(board));
        }

        [Test]
        public void Resolve_RowAndColumnIntersect_Counts15UniqueCells()
        {
            var board = NewBoard();
            FillRow(board, 0, 0, 7);
            FillColumn(board, 0, 0, 7);
            board.SetOccupied(new BoardCoord(4, 4));

            var result = resolver.Resolve(board);

            Assert.AreEqual(2, result.ClearedLines);
            Assert.AreEqual(15, result.ClearedCells.Count, "Ô giao điểm chỉ tính một lần");
            Assert.AreEqual(1, CountOccupied(board));
        }

        [Test]
        public void Resolve_DisabledCellInRow_IgnoredWhenNotRequireCompleteLines()
        {
            var board = NewBoard(8, 8, new BoardCoord(0, 0));
            FillRow(board, 0, 1, 7); // 7 ô chơi được trong hàng 0

            var result = resolver.Resolve(board);

            Assert.AreEqual(1, result.ClearedLines);
            Assert.AreEqual(7, result.ClearedCells.Count);
            CollectionAssert.AreEqual(new[] { 0 }, result.Rows);
        }

        [Test]
        public void Resolve_DisabledCellInRow_BlocksClearWhenRequireCompleteLines()
        {
            var board = NewBoard(8, 8, new BoardCoord(0, 0));
            board.Data.RequireCompleteLines = true;
            FillRow(board, 0, 1, 7);

            var result = resolver.Resolve(board);

            Assert.AreEqual(0, result.ClearedLines);
        }

        [Test]
        public void Resolve_BoardEmptyAfterClear_IsFullClear()
        {
            var board = NewBoard();
            FillRow(board, 0, 0, 7);

            var result = resolver.Resolve(board);

            Assert.IsTrue(result.IsFullClear);
        }

        [Test]
        public void Resolve_EmptyBoard_ClearsNothing()
        {
            var board = NewBoard();

            var result = resolver.Resolve(board);

            Assert.AreEqual(0, result.ClearedLines);
            Assert.IsFalse(result.IsFullClear);
        }
    }
}
