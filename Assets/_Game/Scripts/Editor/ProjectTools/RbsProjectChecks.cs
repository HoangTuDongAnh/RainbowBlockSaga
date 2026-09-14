using System;
using System.Linq;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RbsProjectChecks
{
    [MenuItem("Rainbow Blocks Saga/Validate Project")]
    public static void Validate() { Debug.Log(Run()); }
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static List<BoardCoord> Cells(int n) => Enumerable.Range(0,n).Select(i => new BoardCoord(i,0)).ToList();
    public static string Run()
    {
        var rule=ScriptableObject.CreateInstance<ScoreRuleData>();
        var level=ScriptableObject.CreateInstance<Level>();
        var boardData=ScriptableObject.CreateInstance<BoardData>();
        try {
            boardData.Width=boardData.Height=8;
            var board=new BoardModel(boardData);
            for(int i=0;i<8;i++){board.SetOccupied(new BoardCoord(i,3));board.SetOccupied(new BoardCoord(2,i));}
            var cross=new BoardResolver().Resolve(board);
            Check(cross.ClearedLines==2&&cross.ClearedCells.Count==15&&!board.GetOccupiedCoords().Any(),"Cross resolution must clear exactly 15 unique cells");
            rule.PlacementScorePerCell=10;rule.ClearScorePerCell=10;rule.UseComboStreak=true;rule.ResetComboAfterMisses=3;
            var score=new ScoreSystem(rule);
            Check(score.Apply(new PlacementResult(true,Cells(4)),new BoardResolveResult(0,Cells(0)))==40,"Placement must award 40 for four cells");
            Check(score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(1,Cells(8)))==90,"First eight-cell clear must award 10 + 80");
            Check(score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(2,Cells(15)))==310,"Cross clear must count 15 unique cells with x2 combo");
            score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(0,Cells(0)));
            score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(1,Cells(8)));
            Check(score.Misses==0,"A successful clear must reset consecutive misses");
            for(int i=0;i<3;i++)score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(0,Cells(0)));
            Check(score.Combo==0,"Combo must reset after three misses");
            Check(score.Apply(new PlacementResult(true,Cells(1)),new BoardResolveResult(1,Cells(8)))==90,"Combo must restart at x1");
            var item=Resources.LoadAll<ItemTemplate>("Items").First();
            level.Resize(3,4);level.SetItem(1,2,item);level.SetBonus(1,2,true);level.DisableCellToggle(0,0);
            level.Resize(5,6);Check(level.GetItem(1,2)==item&&level.GetBonus(1,2)&&level.IsDisabled(0,0),"Resize must preserve layout and flags");
            level.levelRows[1].disabled=null;level.InitializeIfNeeded();Check(level.GetItem(1,2)==item&&level.levelRows[1].disabled.Length==6,"Repair must preserve cells");
            var state=new ClassicGameState{score=400,levelRows=level.levelRows};
            var json=GameState.Serialize(state);
            json=global::System.Text.RegularExpressions.Regex.Replace(json,"\"instanceID\":-?[0-9]+","\"instanceID\":0");
            var restored=GameState.Deserialize(json,EGameMode.Classic);
            Check(restored.score==400&&restored.levelRows[1].cells[2]==item,"Save must resolve sprites across instance ID changes");
            Check(restored.levelRows[0].disabled[0],"Save must retain disabled empty cells");
            var levels=ArcadeLevelCatalog.LoadAll();Check(levels.Length>0,"Arcade has no levels");
            Check(levels.Select(l=>l.Number).Distinct().Count()==levels.Length,"Duplicate Arcade level IDs");
            foreach(var l in levels){
                Check(l.levelType!=null&&l.levelType.stateHandler!=null,"Missing level type/handler: "+l.name);
                Check(l.levelRows.Length==l.rows&&l.levelRows.All(r=>r.cells.Length==l.columns),"Invalid grid: "+l.name);
                Check(l.targetInstance.Any(t=>t.targetScriptable!=null&&t.amount>0),"No positive Arcade target: "+l.name);
                Check(l.timerDuration>=0,"Negative timer: "+l.name);
            }
            Check(ArcadeLevelCatalog.Next(levels.Last().Number)==null,"Final level must have no next level");
            Check(ArcadeLevelCatalog.Next(levels.First().Number)==levels.Skip(1).FirstOrDefault(),"Replay progression must use played level");
            Check(Resources.LoadAll<ItemTemplate>("Items").GroupBy(i=>i.name).All(g=>g.Count()==1),"Item names must be unique for saved games");
            return "RBS_CHECKS_OK: scoring, unique-cell combo, resize/repair, persistent save IDs, and "+levels.Length+" Arcade levels.";
        }finally{Object.DestroyImmediate(rule);Object.DestroyImmediate(level);Object.DestroyImmediate(boardData);}
    }
}
