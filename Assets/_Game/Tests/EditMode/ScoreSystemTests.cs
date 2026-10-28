using System.Collections.Generic;
using NUnit.Framework;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;

namespace RainbowBlockSaga.Tests.EditMode
{
    /// <summary>
    /// Kiểm tra ScoreSystem theo công thức trong GDD §7 (REQ-018..REQ-026).
    /// Cấu hình khớp cách game tạo ScoreRule lúc chạy (Runtime/GameSessionRuntime.RefreshScoreRule):
    /// 10 điểm/ô, UseComboStreak = true, ResetComboAfterMisses = 3 (Adventure), Endless dùng EndlessScoringSettings mặc định.
    /// </summary>
    public class ScoreSystemTests : TestBase
    {
        PlacementService placement;
        BoardResolver resolver;

        [SetUp]
        public void SetUp()
        {
            placement = new PlacementService();
            resolver = new BoardResolver();
        }

        ScoreRuleData Rule(bool endless, bool comboStreak = true)
        {
            var rule = Track(ScriptableObject.CreateInstance<ScoreRuleData>());
            rule.PlacementScorePerCell = 10;
            rule.ClearScorePerCell = 10;
            rule.UseComboStreak = comboStreak;
            rule.IsEndless = endless;
            rule.Endless = new EndlessScoringSettings();
            rule.ResetComboAfterMisses = 3;
            return rule;
        }

        // Đặt khối rồi resolve bàn chơi, trả về điểm của lượt.
        int Play(ScoreSystem score, BoardModel board, BlockShapeData shape, int x, int y)
        {
            var placed = placement.Place(board, shape, new BoardCoord(x, y));
            var resolved = resolver.Resolve(board);
            return score.Apply(placed, resolved);
        }

        // Khối 1 ô lấp ô cuối (7,0) của hàng 0 - xóa đúng một hàng 8 ô.
        int ClearRow0WithSingle(ScoreSystem score, BoardModel board)
        {
            FillRow(board, 0, 0, 6);
            return Play(score, board, Single(), 7, 0);
        }

        // ---------- Điểm đặt khối ----------

        [Test]
        public void Placement_NoClear_Endless_Gain40_For4CellBlock()
        {
            var board = NewBoard();
            var score = new ScoreSystem(Rule(true));

            int gain = Play(score, board, Square2(), 2, 2);

            Assert.AreEqual(40, gain);
            Assert.AreEqual(40, score.Score);
            Assert.AreEqual(0, score.Combo);
        }

        [Test]
        public void Placement_NoClear_Adventure_Gain40_For4CellBlock()
        {
            var board = NewBoard();
            var score = new ScoreSystem(Rule(false));

            Assert.AreEqual(40, Play(score, board, Square2(), 2, 2));
            Assert.AreEqual(0, score.Combo);
        }

        [Test]
        public void Apply_InvalidPlacement_AddsNoScore()
        {
            var score = new ScoreSystem(Rule(true));
            var failed = new PlacementResult(false, new List<BoardCoord>());
            var resolve = new BoardResolveResult(0, new HashSet<BoardCoord>());

            Assert.AreEqual(0, score.Apply(failed, resolve));
            Assert.AreEqual(0, score.Score);
        }

        // ---------- Adventure (chuỗi combo kiểu cổ điển) ----------

        [Test]
        public void Adventure_Combo1_ClearOneRow_Gain90()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4)); // tránh full clear
            var score = new ScoreSystem(Rule(false));

            Assert.AreEqual(90, ClearRow0WithSingle(score, board)); // 10 + 8x10x1
            Assert.AreEqual(1, score.Combo);
        }

        [Test]
        public void Adventure_Combo2_RowAndColumnIntersect_Gain310()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(false));

            // Lượt 1: xóa hàng 7 -> combo 1.
            FillRow(board, 7, 0, 6);
            Assert.AreEqual(90, Play(score, board, Single(), 7, 7));

            // Lượt 2: hàng 0 + cột 0 giao nhau (15 ô) ở combo 2: 10 + 15x10x2.
            FillRow(board, 0, 1, 7);
            FillColumn(board, 0, 1, 7);
            Assert.AreEqual(310, Play(score, board, Single(), 0, 0));
            Assert.AreEqual(2, score.Combo);
        }

        [Test]
        public void Adventure_ComboResetsAfterThreeConsecutiveMisses()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(false));
            ClearRow0WithSingle(score, board);
            Assert.AreEqual(1, score.Combo);

            Play(score, board, Single(), 1, 5);
            Assert.AreEqual(1, score.Combo, "Sau 1 lượt không xóa");
            Play(score, board, Single(), 2, 5);
            Assert.AreEqual(1, score.Combo, "Sau 2 lượt không xóa");
            Play(score, board, Single(), 3, 5);
            Assert.AreEqual(0, score.Combo, "Sau 3 lượt không xóa");
        }

        [Test]
        public void Adventure_FullClear_DoesNotGiveRainbowBonus()
        {
            var board = NewBoard(); // chỉ có hàng 0, xóa xong bàn trống
            var score = new ScoreSystem(Rule(false));

            int gain = ClearRow0WithSingle(score, board);

            Assert.AreEqual(90, gain);
            Assert.IsFalse(score.LastTurn.RainbowTriggered);
        }

        [Test]
        public void ClassicRule_TwoLines_UsesAdditionalLineMultiplier()
        {
            // Nhánh dự phòng của ScoreSystem (UseComboStreak = false), game không dùng ở runtime.
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(false, comboStreak: false));
            FillRow(board, 0, 0, 6);
            FillRow(board, 1, 0, 6);

            int gain = Play(score, board, DominoV(), 7, 0); // 20 + 16x10x1.5

            Assert.AreEqual(260, gain);
        }

        // ---------- Endless ----------

        [Test]
        public void Endless_ConsecutiveRowClears_GainSequence_90_98_106_114_230_130()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(true));
            int[] expected = { 90, 98, 106, 114, 230, 130 };
            int total = 0;

            for (int turn = 0; turn < expected.Length; turn++)
            {
                int gain = ClearRow0WithSingle(score, board);
                total += gain;
                Assert.AreEqual(expected[turn], gain, "Lượt " + (turn + 1));
                Assert.AreEqual(turn + 1, score.Combo, "Combo lượt " + (turn + 1));
            }

            Assert.AreEqual(total, score.Score);
            Assert.AreEqual(6, score.HighestCombo);
        }

        [Test]
        public void Endless_TwoLinesOneTurn_Uses1p5LineMultiplier_Gain260()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(true));
            FillRow(board, 0, 0, 6);
            FillRow(board, 1, 0, 6);

            Assert.AreEqual(260, Play(score, board, DominoV(), 7, 0)); // 20 + round(160x1.5)x1.0
        }

        [Test]
        public void Endless_RowAndColumnIntersect_Gain235()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(true));
            FillRow(board, 0, 1, 7);
            FillColumn(board, 0, 1, 7);

            Assert.AreEqual(235, Play(score, board, Single(), 0, 0)); // 10 + round(15x10x1.5)
        }

        [Test]
        public void Endless_ComboResetsAfterOneMiss()
        {
            var board = NewBoard();
            board.SetOccupied(new BoardCoord(4, 4));
            var score = new ScoreSystem(Rule(true));
            ClearRow0WithSingle(score, board);
            Assert.AreEqual(1, score.Combo);

            Assert.AreEqual(10, Play(score, board, Single(), 2, 6));
            Assert.AreEqual(0, score.Combo);
        }

        [Test]
        public void Endless_FullClearAtCombo1_GivesRainbowBonus_Gain190()
        {
            var board = NewBoard(); // chỉ có hàng 0; xóa xong bàn trống
            var score = new ScoreSystem(Rule(true));

            int gain = ClearRow0WithSingle(score, board);

            Assert.AreEqual(190, gain); // 10 + 80 + 100
            Assert.IsTrue(score.LastTurn.FullClear);
            Assert.IsTrue(score.LastTurn.RainbowTriggered);
        }

        [Test]
        public void Endless_Combo5AndFullClearSameTurn_BonusAwardedOnce_Gain230()
        {
            var board = NewBoard();
            var score = new ScoreSystem(Rule(true));
            score.Restore(0, 4, 0, 4); // đang ở combo 4

            int gain = ClearRow0WithSingle(score, board); // combo 5 + full clear

            Assert.AreEqual(230, gain); // 10 + round(80x1.5) + 100 (một lần)
            Assert.AreEqual(5, score.Combo);
        }

        // ---------- Đường tính điểm thứ hai: LevelManager.AfterMoveProcessing ----------
        // LevelManager là MonoBehaviour phụ thuộc scene nên không chạy được trong EditMode.
        // Test này sao chép công thức từ LevelManager.cs (CheckLines + AfterMoveProcessing)
        // và so sánh kết quả với ScoreSystem trên cùng một bộ dữ liệu. Nó kiểm tra "công thức
        // tương đương", KHÔNG kiểm tra trực tiếp mã của LevelManager. Đường LevelManager thật
        // được kiểm bằng Test Case thủ công trên Android.

        static int LegacyTotal(bool endless, int blockCells, int clearedCells, int lines,
            int combo, bool fullClear, EndlessScoringSettings settings)
        {
            const int scorePerCell = 10;
            int blockScore = blockCells * scorePerCell;
            if (lines == 0) return blockScore; // CheckLines: AwardPoints(blockScore)

            int lineScore = scorePerCell * clearedCells;
            if (endless) lineScore = Mathf.RoundToInt(lineScore * settings.LineMultiplier(lines));
            bool rainbow = endless && (combo == Mathf.Max(1, settings.RainbowCombo) || fullClear);
            var feedback = new ResolveScoreFeedback(endless, blockScore, lineScore,
                endless ? settings.Multiplier(combo) : Mathf.Max(1, combo),
                rainbow ? settings.RainbowBonus : 0, combo, fullClear, rainbow);
            return feedback.Total;
        }

        //        endless, block, cleared, lines, combo, fullClear
        [TestCase(true, 1, 8, 1, 1, false)]
        [TestCase(true, 1, 8, 1, 2, false)]
        [TestCase(true, 1, 8, 1, 5, false)]
        [TestCase(true, 1, 8, 1, 6, false)]
        [TestCase(true, 4, 16, 2, 1, false)]
        [TestCase(true, 1, 15, 2, 3, false)]
        [TestCase(true, 2, 24, 3, 2, false)]
        [TestCase(true, 1, 40, 5, 4, false)]
        [TestCase(true, 1, 8, 1, 1, true)]
        [TestCase(true, 1, 8, 1, 5, true)]
        [TestCase(false, 1, 8, 1, 1, false)]
        [TestCase(false, 1, 15, 2, 2, false)]
        [TestCase(false, 4, 24, 3, 3, false)]
        [TestCase(false, 1, 8, 1, 1, true)]
        [TestCase(true, 4, 0, 0, 0, false)]
        [TestCase(false, 4, 0, 0, 0, false)]
        public void ScoreSystem_MatchesLegacyLevelManagerFormula(
            bool endless, int blockCells, int clearedCells, int lines, int combo, bool fullClear)
        {
            var rule = Rule(endless);
            var score = new ScoreSystem(rule);
            if (lines > 0) score.Restore(0, combo - 1, 0, combo - 1); // Apply sẽ tăng lên đúng 'combo'

            var blockCoords = new List<BoardCoord>();
            for (int i = 0; i < blockCells; i++) blockCoords.Add(new BoardCoord(i, 0));
            var cleared = new HashSet<BoardCoord>();
            for (int i = 0; i < clearedCells; i++) cleared.Add(new BoardCoord(i, 1));

            var placed = new PlacementResult(true, blockCoords);
            var resolved = new BoardResolveResult(lines, cleared, new List<int>(), new List<int>(), fullClear);

            int fromScoreSystem = score.Apply(placed, resolved);
            int fromLegacy = LegacyTotal(endless, blockCells, clearedCells, lines, combo, fullClear, rule.Endless);

            Assert.AreEqual(fromLegacy, fromScoreSystem);
        }
    }
}
