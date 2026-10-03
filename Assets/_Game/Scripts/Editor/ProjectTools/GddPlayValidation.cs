using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.RuntimeAdapters;
using RainbowBlockSaga.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class GddPlayValidation
{
    const string Dir="Temp/GddImplementation/";
    const string Active="RBS_GDD_PLAY_TEST";
    static double next;
    static int step;
    static bool busy;
    static Level testLevel;
    static List<string> errors=new();
    [Serializable] class Backup { public Entry[] entries; }
    [Serializable] class Entry { public string key,value;public int number;public bool exists,isString; }
    static GddPlayValidation()
    {
        EditorApplication.update+=Tick;
        EditorApplication.playModeStateChanged+=StateChanged;
        Application.logMessageReceived+=OnLog;
    }
    static void OnLog(string message,string trace,LogType type)
    {
        if(SessionState.GetBool(Active,false)&&EditorApplication.isPlaying&&(type==LogType.Exception||type==LogType.Error))
            errors.Add(message+"\n"+trace);
    }
    static void StateChanged(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Active,false))return;
        if(state==PlayModeStateChange.EnteredPlayMode){Application.runInBackground=true;step=0;next=EditorApplication.timeSinceStartup+4;}
        if(state==PlayModeStateChange.EnteredEditMode)
        {
            RestorePrefs(); SessionState.SetBool(Active,false);
        }
    }
    static void RestorePrefs()
    {
        if(!File.Exists(Dir+"prefs-backup.json"))return;
        foreach(var e in JsonUtility.FromJson<Backup>(File.ReadAllText(Dir+"prefs-backup.json")).entries)
        {
            if(!e.exists)PlayerPrefs.DeleteKey(e.key);
            else if(e.isString)PlayerPrefs.SetString(e.key,e.value);
            else PlayerPrefs.SetInt(e.key,e.number);
        }
        PlayerPrefs.Save();
        foreach(var resource in Resources.LoadAll<ResourceObject>("Variables"))resource.LoadPrefs();
    }
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||busy)return;
        if(!SessionState.GetBool(Active,false))
        {
            if(!File.Exists(Dir+"play.request")||EditorApplication.isPlayingOrWillChangePlaymode)return;
            if(EditorSceneManager.GetActiveScene().path!="Assets/_Game/Scenes/Gameplay.unity"||EditorSceneManager.GetActiveScene().isDirty)
            { File.WriteAllText(Dir+"play-result.txt","BLOCKED: Gameplay scene must be open and saved before automated Play tests.");File.Delete(Dir+"play.request");return; }
            File.Delete(Dir+"play.request");
            string[] ints={"tutorial","GameMode","Level","Coins","Score","RewardStreak","RBS_RewardLevel_2","RBS_RewardRun_gdd-validation","RBS_EndlessSupport_RainbowCell","RBS_EndlessSupport_Bomb3x3","RBS_EndlessSupport_ShuffleTray"};
            string[] strings={"RBS_Run_Endless","RBS_Run_Adventure","GameState_Endless","GameState_Timed","LastPlayedMode","DailyBonusDay","LastFreeSpinTime"};
            var entries=ints.Select(k=>new Entry{key=k,exists=PlayerPrefs.HasKey(k),number=PlayerPrefs.GetInt(k)}).Concat(strings.Select(k=>new Entry{key=k,exists=PlayerPrefs.HasKey(k),isString=true,value=PlayerPrefs.GetString(k)})).ToArray();
            File.WriteAllText(Dir+"prefs-backup.json",JsonUtility.ToJson(new Backup{entries=entries}));
            PlayerPrefs.SetInt("tutorial",1);PlayerPrefs.DeleteKey("RBS_Run_Endless");PlayerPrefs.DeleteKey("RBS_Run_Adventure");PlayerPrefs.DeleteKey("RBS_RewardLevel_2");PlayerPrefs.DeleteKey("RBS_RewardRun_gdd-validation");PlayerPrefs.Save();
            SessionState.SetBool(Active,true);EditorApplication.isPlaying=true;return;
        }
        if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        busy=true;
        try { RunStep(); }
        catch(Exception ex){Finish("FAIL step "+step+": "+ex+"\n"+string.Join("\n",errors));}
        finally{busy=false;}
    }
    static T Find<T>() where T:Object => Object.FindFirstObjectByType<T>();
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Wait(double seconds=2){step++;next=EditorApplication.timeSinceStartup+seconds;}
    static void PlaceSingle(bool clear)
    {
        var level=Find<LevelManager>();var field=level.GetFieldManager();
        var single=Resources.Load<ShapeTemplate>("Shapes/Single");
        level.cellDeck.FillCellDecksWithShapes(new[]{single,single,single},false);
        var shape=level.cellDeck.cellDecks[0].shape;
        var item=Resources.LoadAll<ItemTemplate>("Items").First(x=>x.customItemPrefab==null);
        if(clear)for(int c=0;c<field.ColumnCount-1;c++)field.cells[0,c].FillCell(item);
        int col=clear?field.ColumnCount-1:0;
        field.cells[0,col].FillCell(item);
        BoardRuntime.Current.SyncNow();
        var data=shape.GetComponent<BlockViewAdapter>().Data;
        Check(ResolveScoreRuntime.Current.TryResolvePlacement(data,shape,new[]{new BoardCoord(col,field.RowCount-1)}),"Placement rejected");
        EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(shape);
    }
    static void StartAdventure(bool win)
    {
        MenuManager.instance.CloseAllPopups();
        testLevel=Object.Instantiate(Resources.Load<Level>("Misc/EndlessLevel"));testLevel.name="Level_2";
        testLevel.levelType=Resources.Load<LevelTypeScriptable>("LevelTypes/BonusItem");
        testLevel.moveLimit=1;testLevel.enableTimer=false;
        testLevel.targetInstance=new List<Target>{new Target(Resources.Load<ScoreTargetScriptable>("Targets/ScoreTarget")){amount=win?10:1000}};
        if(win)testLevel.targetInstance.Add(new Target(Resources.Load<BonusItemTargetScriptable>("Targets/Bonus 3")){amount=1});
        GameDataManager.SetGameMode(EGameMode.Adventure);GameDataManager.SetLevel(testLevel);GameManager.instance.RestartLevel();
    }
    static void RunStep()
    {
        File.AppendAllText(Dir+"play-progress.txt","step "+step+"\n");
        switch(step)
        {
            case 0: MenuManager.instance.CloseAllPopups();GameManager.instance.SetTutorialMode(false);SceneLoader.instance.StartGameSceneEndless();Wait(3);break;
            case 1: MenuManager.instance.CloseAllPopups();EventManager.GameStatus=EGameState.Playing;PlaceSingle(false);Wait();break;
            case 2:
                Check(Find<LevelManager>().RunScore==10,"Endless placement score");
                Find<LevelManager>().SaveRun();var saved=RunSnapshot.Load(EGameMode.Endless);
                Check(saved!=null&&saved.score==10&&saved.tray.Count(x=>!string.IsNullOrEmpty(x.shape))==2,"Full tray/score snapshot: "+PlayerPrefs.GetString(RunSnapshot.Key(EGameMode.Endless))+" state="+EventManager.GameStatus+" test="+GameDataManager.isTestPlay+" tutorial="+GameManager.instance.IsTutorialMode()+" flags="+string.Join(",",typeof(LevelManager).GetFields(System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Where(f=>f.FieldType==typeof(bool)).Select(f=>f.Name+"="+f.GetValue(Find<LevelManager>()))));
                saved.combo=4;saved.highestCombo=8;saved.Save();SceneLoader.instance.GoMain();
                // GoMain saves the live run; set the fixture after leaving it.
                saved.Save();Wait();break;
            case 3: SceneLoader.instance.StartGameSceneEndless();Wait(3);break;
            case 4:
                MenuManager.instance.CloseAllPopups();EventManager.GameStatus=EGameState.Playing;
                Check(Find<LevelManager>().RunScore==10&&Find<LevelManager>().HighestCombo==8,"Run stats restore");
                Check(GameSessionRuntime.Current.GetOrCreateSession().Score.Combo==4,"Runtime combo restore");PlaceSingle(true);Wait(3);break;
            case 5:
                Check(Find<LevelManager>().RunScore==240&&Find<LevelManager>().HighestCombo==8,"Restored fifth combo/full-clear score");
                StartAdventure(true);Wait(3);break;
            case 6:
                MenuManager.instance.CloseAllPopups();EventManager.GameStatus=EGameState.Playing;
                var manager=Find<LevelManager>();var gem=manager.GetCurrentLevel().targetInstance.First(t=>t.targetScriptable.bonusItem!=null).targetScriptable.bonusItem;
                manager.GetFieldManager().cells[0,0].SetBonus(gem);PlaceSingle(true);Wait(4);break;
            case 7:
                Check(PlayerPrefs.HasKey("RBS_RewardLevel_2"),"Last move mixed objective did not win/reward");
                SceneLoader.instance.StartGameSceneEndless();Wait(3);break;
            case 8: StartAdventure(false);Wait(3);break;
            case 9: MenuManager.instance.CloseAllPopups();EventManager.GameStatus=EGameState.Playing;PlaceSingle(false);Wait(4);break;
            case 10:
                Check(Find<LevelManager>().OutOfMoves&&!GameSessionRuntime.Current.CanAcceptPlacement,"Move limit not enforced");
                Check(EventManager.GameStatus==EGameState.Failed||EventManager.GameStatus==EGameState.PreFailed,"Out of moves did not fail");
                int before=ResourceManager.instance.GetResource("Coins").GetValue();
                RunRewards.Grant(EGameMode.Endless,"gdd-validation",0,1000);RunRewards.Grant(EGameMode.Endless,"gdd-validation",0,1000);
                Check(ResourceManager.instance.GetResource("Coins").GetValue()==before+70,"Reward duplicated");
                Finish(errors.Count==0?"PASS: Play Mode placement, full resume, restored combo, last-move mixed win, move-limit failure and reward idempotence.":"FAIL: runtime logs\n"+string.Join("\n",errors));break;
        }
    }
    static void Finish(string message)
    {
        File.WriteAllText(Dir+"play-result.txt",message);
        if(testLevel!=null)Object.Destroy(testLevel);
        EditorApplication.isPlaying=false;
    }
}
