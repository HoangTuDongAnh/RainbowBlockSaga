using NUnit.Framework;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;

namespace RainbowBlockSaga.Tests.EditMode
{
    /// <summary>Kiểm tra PlacementService: đặt khối hợp lệ / không hợp lệ (REQ-011, REQ-012, REQ-017).</summary>
    public class PlacementServiceTests : TestBase
    {
        PlacementService service;

        [SetUp]
        public void SetUp() => service = new PlacementService();

        [Test]
        public void CanPlace_EmptyBoardInsideBounds_ReturnsTrue()
        {
            var board = NewBoard();
            Assert.IsTrue(service.CanPlace(board, Square2(), new BoardCoord(3, 3)));
        }

        [Test]
        public void Place_Valid_OccupiesAllShapeCells()
        {
            var board = NewBoard();
            var result = service.Place(board, Square2(), new BoardCoord(2, 2));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.Cells.Count);
            foreach (var cell in result.Cells) Assert.IsTrue(board.IsOccupied(cell), cell.ToString());
            Assert.AreEqual(4, CountOccupied(board));
        }

        [Test]
        public void GetPlacementCells_NormalizesShapeOffsets()
        {
            // Hình dạng khai báo lệch gốc (2,3),(3,3) phải được chuẩn hóa về (0,0),(1,0).
            var shape = NewShape((2, 3), (3, 3));
            var cells = service.GetPlacementCells(shape, new BoardCoord(1, 1));

            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(new BoardCoord(1, 1), cells[0]);
            Assert.AreEqual(new BoardCoord(2, 1), cells[1]);
        }

        [TestCase(0, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 7)]
        [TestCase(7, 7)]
        public void Place_AtBoardCorners_Succeeds(int x, int y)
        {
            var board = NewBoard();
            var result = service.Place(board, Single(), new BoardCoord(x, y));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(board.IsOccupied(new BoardCoord(x, y)));
        }

        [Test]
        public void CanPlace_OverlapsOccupiedCell_ReturnsFalse()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            // Khối 2x2 tại (3,3) phủ (4,4).
            Assert.IsFalse(service.CanPlace(board, Square2(), new BoardCoord(3, 3)));
        }

        [TestCase(6, 0, true)]   // vừa khít mép phải: ô (6,0),(7,0)
        [TestCase(7, 0, false)]  // ô (8,0) nằm ngoài bàn
        [TestCase(-1, 0, false)] // ô (-1,0) nằm ngoài bàn
        [TestCase(0, 7, true)]   // sát hàng dưới cùng
        [TestCase(0, 8, false)]  // cả khối nằm ngoài bàn
        public void CanPlace_HorizontalDomino_BoundaryPositions(int x, int y, bool expected)
        {
            var board = NewBoard();
            Assert.AreEqual(expected, service.CanPlace(board, DominoH(), new BoardCoord(x, y)));
        }

        [Test]
        public void CanPlace_VerticalDominoPartlyBelowBoard_ReturnsFalse()
        {
            var board = NewBoard();
            // Ô (0,7) trong bàn nhưng ô (0,8) ngoài bàn.
            Assert.IsFalse(service.CanPlace(board, DominoV(), new BoardCoord(0, 7)));
            Assert.IsTrue(service.CanPlace(board, DominoV(), new BoardCoord(0, 6)));
        }

        [Test]
        public void CanPlace_OnDisabledCell_ReturnsFalse()
        {
            var board = NewBoard(8, 8, new BoardCoord(2, 2));
            Assert.IsFalse(service.CanPlace(board, Single(), new BoardCoord(2, 2)));
            Assert.IsTrue(service.CanPlace(board, Single(), new BoardCoord(2, 3)));
        }

        [Test]
        public void Place_Invalid_LeavesBoardUnchanged()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(1, 1));

            // Khối 2x2 tại (0,0) phủ (1,1) nên không hợp lệ.
            var result = service.Place(board, Square2(), new BoardCoord(0, 0));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, CountOccupied(board));
            Assert.IsTrue(board.IsEmpty(new BoardCoord(0, 0)));
            Assert.IsTrue(board.IsEmpty(new BoardCoord(1, 0)));
            Assert.IsTrue(board.IsEmpty(new BoardCoord(0, 1)));
        }

        [Test]
        public void HasAnyValidPlacement_FullBoard_ReturnsFalse()
        {
            var board = NewBoard();
            FillAll(board);
            Assert.IsFalse(service.HasAnyValidPlacement(board, Single()));
        }

        [Test]
        public void HasAnyValidPlacement_OneHole_SingleTrueDominoFalse()
        {
            var board = NewBoard();
            FillAll(board);
            board.SetEmpty(new BoardCoord(3, 3));

            Assert.IsTrue(service.HasAnyValidPlacement(board, Single()));
            Assert.IsFalse(service.HasAnyValidPlacement(board, DominoH()));
        }

        [Test]
        public void GetValidPlacements_EmptyBoard_Single_Returns64()
        {
            var board = NewBoard();
            Assert.AreEqual(64, service.GetValidPlacements(board, Single()).Count);
        }
    }
}
