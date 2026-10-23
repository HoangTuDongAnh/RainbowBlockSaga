using System;
using System.Linq;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEngine;

public static class TrayPlannerChecks
{
    static BoardModel Copy(BoardModel b) { var c=new BoardModel(b.Data);foreach(var p in b.GetOccupiedCoords())c.SetOccupied(p);return c; }
    static bool CanFinish(BoardModel board,List<BlockShapeData> batch,int index)
    {
        if(index==batch.Count)return true;
        var placement=new PlacementService();
        foreach(var anchor in placement.GetValidPlacements(board,batch[index]))
        {
            var copy=Copy(board);placement.Place(copy,batch[index],anchor);new BoardResolver().Resolve(copy);
            if(CanFinish(copy,batch,index+1))return true;
        }
        return false;
    }
    public static string Run()
    {
        var state=UnityEngine.Random.state;
        var data=ScriptableObject.CreateInstance<BoardData>();
        var profile=ScriptableObject.CreateInstance<SpawnProfileData>();profile.UseBoardLookAhead=true;
        var single=ScriptableObject.CreateInstance<BlockShapeData>();
        var domino=ScriptableObject.CreateInstance<BlockShapeData>();domino.Cells.Add(new BoardCoord(1,0));
        var square=ScriptableObject.CreateInstance<BlockShapeData>();square.Cells.AddRange(new[]{new BoardCoord(1,0),new BoardCoord(0,1),new BoardCoord(1,1)});
        profile.Shapes.AddRange(new[]{single,domino,square});
        try
        {
            var watch=System.Diagnostics.Stopwatch.StartNew();
            int tested=0;
            foreach(int size in new[]{6,8,10})
            for(int seed=0;seed<12;seed++)
            {
                data.Width=data.Height=size;var board=new BoardModel(data);UnityEngine.Random.InitState(seed);
                foreach(var p in board.GetPlayableCoords().ToArray())if(UnityEngine.Random.value<.45f)board.SetOccupied(p);
                var before=board.GetOccupiedCoords().ToArray();
                var batch=new SpawnStrategy(new PlacementService()).CreateBatch(board,profile);
                if(!before.SequenceEqual(board.GetOccupiedCoords()))throw new Exception("Planner mutated real board");
                if(batch.Count!=3||!CanFinish(board,batch,0))throw new Exception("No legal sequence for generated tray");
                tested++;
            }
            return "PASS: "+tested+" seeded boards (6/8/10), full tray paths and immutable source; "+watch.ElapsedMilliseconds+" ms total in Editor.";
        }
        finally { UnityEngine.Random.state=state;UnityEngine.Object.DestroyImmediate(data);UnityEngine.Object.DestroyImmediate(profile);UnityEngine.Object.DestroyImmediate(single);UnityEngine.Object.DestroyImmediate(domino);UnityEngine.Object.DestroyImmediate(square); }
    }
}
