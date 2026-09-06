using System;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Board
{
    [Serializable]
    public struct BoardCoord : IEquatable<BoardCoord>
    {
        public int X;
        public int Y;

        public BoardCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static BoardCoord operator +(BoardCoord a, BoardCoord b) => new(a.X + b.X, a.Y + b.Y);
        public bool Equals(BoardCoord other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is BoardCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}
