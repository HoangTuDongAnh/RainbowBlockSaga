using System;
using System.Collections.Generic;
using NUnit.Framework;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RainbowBlockSaga.Tests.EditMode
{
    /// <summary>
    /// Lớp cơ sở cho các Unity Test: tạo Board/Shape bằng ScriptableObject.CreateInstance
    /// và tự hủy sau mỗi test. Không chỉnh sửa mã nguồn game.
    /// </summary>
    public abstract class TestBase
    {
        readonly List<Object> created = new();

        [TearDown]
        public void DestroyCreatedObjects()
        {
            foreach (var obj in created)
                if (obj) Object.DestroyImmediate(obj);
            created.Clear();
        }

        protected T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        /// <summary>Tạo bàn chơi width x height. Các ô trong 'disabled' bị khóa.</summary>
        protected BoardModel NewBoard(int width = 8, int height = 8, params BoardCoord[] disabled)
        {
            var data = Track(ScriptableObject.CreateInstance<BoardData>());
            data.Width = width;
            data.Height = height;
            if (disabled.Length > 0)
            {
                data.PlayableCells = new List<BoardCoord>();
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var coord = new BoardCoord(x, y);
                        if (Array.IndexOf(disabled, coord) < 0) data.PlayableCells.Add(coord);
                    }
            }
            return new BoardModel(data);
        }

        protected BlockShapeData NewShape(params (int x, int y)[] cells)
        {
            var shape = Track(ScriptableObject.CreateInstance<BlockShapeData>());
            shape.Cells = new List<BoardCoord>();
            foreach (var cell in cells) shape.Cells.Add(new BoardCoord(cell.x, cell.y));
            return shape;
        }

        protected BlockShapeData Single() => NewShape((0, 0));
        protected BlockShapeData DominoH() => NewShape((0, 0), (1, 0));
        protected BlockShapeData DominoV() => NewShape((0, 0), (0, 1));
        protected BlockShapeData Square2() => NewShape((0, 0), (1, 0), (0, 1), (1, 1));

        protected static void FillRow(BoardModel board, int y, int fromX, int toX)
        {
            for (int x = fromX; x <= toX; x++) board.SetOccupied(new BoardCoord(x, y));
        }

        protected static void FillColumn(BoardModel board, int x, int fromY, int toY)
        {
            for (int y = fromY; y <= toY; y++) board.SetOccupied(new BoardCoord(x, y));
        }

        protected static void FillAll(BoardModel board)
        {
            foreach (var coord in new List<BoardCoord>(board.GetPlayableCoords()))
                board.SetOccupied(coord);
        }

        protected static int CountOccupied(BoardModel board)
        {
            int count = 0;
            foreach (var _ in board.GetOccupiedCoords()) count++;
            return count;
        }
    }
}
