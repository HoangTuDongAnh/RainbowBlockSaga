using System;
using System.Linq;
using System.Reflection;
using RainbowBlockSaga.Presentation.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using RainbowBlockSaga.Presentation.Scripts.Popups.Reward;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.RuntimeAdapters;
using RainbowBlockSaga.Runtime;
using UnityEditor;
using UnityEngine;

public static partial class GddPlayValidation
{
    static int expectedCoins, tutorialMoves, tutorialPhase;
    static double spinDeadline;
    static bool timerIntroAccepted;
    static Level[] liveLevels;
    static int liveLevelIndex;
    static object InvokePrivate(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
    static int Stock(EndlessSupportItem item) => PlayerPrefs.GetInt("RBS_EndlessSupport_"+item);
    static EndlessSupportPanel Panel => Find<EndlessSupportPanel>();
    static ResourceObject Coins => ResourceManager.instance.GetResource("Coins");
    static void ClickSupport(EndlessSupportItem item) => Panel.GetComponentsInChildren<EndlessSupportButton>().Single(b=>b.Item==item).Button.onClick.Invoke();
    static bool UseAt(EndlessSupportItem item,int row,int col) => (bool)InvokePrivate(Panel,"UseAt",item,row,col);
    static void SaveMatchesBoard()
    {
        var saved=RunSnapshot.Load(EGameMode.Endless);
        var field=Find<LevelManager>().GetFieldManager();
        Check(saved!=null,"Missing booster snapshot");
        for(int r=0;r<field.RowCount;r++) for(int c=0;c<field.ColumnCount;c++)
            Check(string.IsNullOrEmpty(saved.cells[r*field.ColumnCount+c].item)==field.cells[r,c].IsEmpty(),"Booster board was not saved at "+r+","+c);
    }
    static void RunInteractions()
    {
        switch(step)
        {
            case 11:
                SceneLoader.instance.GoMain();
                foreach(EndlessSupportItem supportItem in Enum.GetValues(typeof(EndlessSupportItem))) PlayerPrefs.SetInt("RBS_EndlessSupport_"+supportItem,0);
                RunSnapshot.Delete(EGameMode.Endless);SceneLoader.instance.StartGameSceneEndless();Wait(3);break;
            case 12:
                MenuManager.instance.CloseAllPopups();EventManager.GameStatus=EGameState.Playing;
                Check(Panel!=null,"Endless booster panel hidden");
                Coins.Set(0);ClickSupport(EndlessSupportItem.RainbowCell);
                Check(Stock(EndlessSupportItem.RainbowCell)==0&&Coins.GetValue()==0,"Insufficient funds purchase");
                Coins.Set(500);ClickSupport(EndlessSupportItem.RainbowCell);
                Check(Stock(EndlessSupportItem.RainbowCell)==1&&Coins.GetValue()==470,"Rainbow purchase must add one without using");
                var deck=Find<LevelManager>().cellDeck;
                var original=deck.cellDecks.Select(d=>d.shape).ToArray();
                ClickSupport(EndlessSupportItem.RainbowCell);
                Check(deck.HasSupportShape&&deck.GetShapes().Length==1&&Stock(EndlessSupportItem.RainbowCell)==1,"Support must wait for drag");
                Find<LevelManager>().SaveRun();
                Check(RunSnapshot.Load(EGameMode.Endless).tray.Select(t=>t.shape).SequenceEqual(original.Select(t=>t==null?null:t.shapeTemplate.name)),"Pending support replaced saved tray");
                ClickSupport(EndlessSupportItem.RainbowCell);
                Check(!deck.HasSupportShape&&deck.cellDecks.Select(d=>d.shape).SequenceEqual(original),"Cancel did not restore tray");
                ClickSupport(EndlessSupportItem.RainbowCell);
                var supportShape=deck.GetShapes().Single();
                var drag=supportShape.GetComponent<BlockInputController>();
                InvokePrivate(drag,"BeginDrag");InvokePrivate(drag,"HandleDrag",new Vector2(-1000,-1000));InvokePrivate(drag,"EndDrag");
                Check(Stock(EndlessSupportItem.RainbowCell)==1&&deck.HasSupportShape,"Invalid drop consumed booster");
                InvokePrivate(drag,"BeginDrag");InvokePrivate(drag,"HandleDrag",Vector2.zero);
                var rect=(RectTransform)supportShape.transform;
                var supportField=Find<LevelManager>().GetFieldManager();
                var delta=rect.parent.InverseTransformVector(supportField.cells[0,0].transform.position-supportShape.GetActiveItems()[0].transform.position);
                var desired=rect.anchoredPosition+new Vector2(delta.x,delta.y);
                var canvas=supportShape.GetComponentInParent<Canvas>();
                float scaleY=rect.rect.height/((RectTransform)canvas.transform).rect.height*2.5f;
                var local=new Vector3(desired.x,(desired.y-300-scaleY)*scaleY,0);
                var pointer=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,rect.parent.TransformPoint(local));
                InvokePrivate(drag,"HandleDrag",pointer);Physics2D.SyncTransforms();
                InvokePrivate(drag,"EndDrag");
                Check(deck.cellDecks.Select(d=>d.shape).SequenceEqual(original),"Support consumed original tray");
                Check(Stock(EndlessSupportItem.RainbowCell)==0,"Rainbow stock");Wait(2);break;
            case 13:
                Check(Find<LevelManager>().RunScore==10,"Rainbow placement score");SaveMatchesBoard();
                ClickSupport(EndlessSupportItem.Bomb3x3);
                Check(!UseAt(EndlessSupportItem.Bomb3x3,7,7)&&Stock(EndlessSupportItem.Bomb3x3)==1,"Empty bomb consumed stock");
                Check(UseAt(EndlessSupportItem.Bomb3x3,0,0),"Corner bomb failed");
                Check(Find<LevelManager>().GetFieldManager().cells[0,0].IsDestroying(),"Bomb cell must reject placement until destruction finishes");Wait();break;
            case 14:
                Check(Find<LevelManager>().RunScore==10&&Stock(EndlessSupportItem.Bomb3x3)==0,"Bomb changed score/stock");SaveMatchesBoard();
                var single=Resources.Load<ShapeTemplate>("Shapes/Single");
                Find<LevelManager>().cellDeck.FillCellDecksWithShapes(new[]{single,single,single},false);Find<LevelManager>().SaveRun();
                ClickSupport(EndlessSupportItem.ShuffleTray);ClickSupport(EndlessSupportItem.ShuffleTray);Wait();break;
            case 15:
                Check(Stock(EndlessSupportItem.ShuffleTray)==0,"Shuffle stock");
                var saved=RunSnapshot.Load(EGameMode.Endless);
                Check(saved.tray.Select(s=>s.shape).SequenceEqual(Find<LevelManager>().cellDeck.cellDecks.Select(d=>d.shape.shapeTemplate.name)),"Shuffled tray was not saved");
                expectedCoins=Coins.GetValue();EventManager.GameStatus=EGameState.Paused;ClickSupport(EndlessSupportItem.RainbowCell);
                Check(Coins.GetValue()==expectedCoins&&!GameSessionRuntime.Current.CanAcceptPlacement,"Pause allowed interaction");
                EventManager.GameStatus=EGameState.Playing;StartAdventure(false);testLevel.enableTimer=true;testLevel.timerDuration=2;testLevel.moveLimit=0;GameManager.instance.RestartLevel();timerIntroAccepted=false;Wait(3);break;
            case 16:
                if(!timerIntroAccepted)
                {
                    Check(Find<AdventureObjectivesPopup>()!=null&&EventManager.GameStatus==EGameState.PrepareGame,"Missing objective intro");
                    Check(Find<LevelManager>().timerManager.RemainingTime==2,"Timer ran behind objective popup");
                    var intro=Find<AdventureObjectivesPopup>();
                    ((RainbowBlockSaga.Presentation.Scripts.GUI.CustomButton)typeof(AdventureObjectivesPopup).GetField("playButton",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(intro)).onClick.Invoke();
                    timerIntroAccepted=true;next=EditorApplication.timeSinceStartup+5;return;
                }
                Check(Panel==null,"Adventure displayed boosters");
                Check(EventManager.GameStatus==EGameState.Failed||EventManager.GameStatus==EGameState.PreFailed,"Adventure timeout did not fail");
                SceneLoader.instance.GoMain();PlayerPrefs.DeleteKey("LastFreeSpinTime");Coins.Set(500);
                MenuManager.instance.ShowPopup<LuckySpin>();Wait();break;
            case 17:
                var spin=Find<LuckySpin>();Check(spin.freeSpinButton.gameObject.activeSelf,"Free spin button hidden");
                spin.freeSpinButton.onClick.Invoke();spin.freeSpinButton.onClick.Invoke();
                Check(Coins.GetValue()==500,"Free spin charged coins");spinDeadline=EditorApplication.timeSinceStartup+45;Wait(2);break;
            case 18:
                var reward=Find<RewardPopup>();
                if(reward==null){Check(EditorApplication.timeSinceStartup<spinDeadline,"Spin never ended");next=EditorApplication.timeSinceStartup+1;return;}
                expectedCoins=Coins.GetValue()+int.Parse(reward.countText.text);reward.Close();reward.Close();Wait(4);break;
            case 19:
                Check(Coins.GetValue()==expectedCoins,"Claim duplicated or missing");
                MenuManager.instance.ShowPopup<LuckySpin>();Wait();break;
            case 20:
                var paid=Find<LuckySpin>();Check(paid.buySpinButton.gameObject.activeSelf&&!paid.freeSpinButton.gameObject.activeSelf,"Daily spin gating");
                expectedCoins=Coins.GetValue()-GameManager.instance.luckySpinSettings.costToSpin;
                paid.buySpinButton.onClick.Invoke();paid.buySpinButton.onClick.Invoke();
                Check(Coins.GetValue()==expectedCoins,"Paid spin charged twice");spinDeadline=EditorApplication.timeSinceStartup+45;Wait();break;
            case 21:
                var paidReward=Find<RewardPopup>();
                if(paidReward==null){Check(EditorApplication.timeSinceStartup<spinDeadline,"Paid spin never ended");next=EditorApplication.timeSinceStartup+1;return;}
                expectedCoins+=int.Parse(paidReward.countText.text);paidReward.Close();Wait(4);break;
            case 22:
                Check(Coins.GetValue()==expectedCoins,"Paid spin reward");
                MenuManager.instance.CloseAllPopups();SceneLoader.instance.GoMain();GameManager.instance.SetTutorialMode(true);
                SceneLoader.instance.StartGameSceneEndless();tutorialMoves=0;tutorialPhase=0;Wait(3);break;
            case 23:
                if(!GameManager.instance.IsTutorialMode()){Wait(3);break;}
                Check(Panel==null,"Tutorial displayed boosters");
                var tutorial=Find<TutorialManager>();
                int phase=(int)typeof(TutorialManager).GetField("currentPhase",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(tutorial);
                Check(phase==tutorialPhase || tutorialMoves==3,"Tutorial advanced before three blocks");
                if(phase!=tutorialPhase){tutorialPhase=phase;tutorialMoves=0;}
                var level=Find<LevelManager>();var field=level.GetFieldManager();BoardRuntime.Current.SyncNow();
                var shape=level.cellDeck.GetShapes().First();var data=shape.GetComponent<BlockViewAdapter>().Data;
                var anchors=BoardRuntime.Current.Placement.GetValidPlacements(BoardRuntime.Current.Model,data);
                var anchor=anchors.First(a=>BoardRuntime.Current.Placement.GetPlacementCells(data,a).All(c=>field.cells[field.RowCount-1-c.Y,c.X].IsHighlighted()));
                var coords=BoardRuntime.Current.Placement.GetPlacementCells(data,anchor);
                var item=Resources.LoadAll<ItemTemplate>("Items").First(t=>t.customItemPrefab==null);
                foreach(var c in coords)field.cells[field.RowCount-1-c.Y,c.X].FillCell(item);
                BoardRuntime.Current.SyncNow();
                Check(ResolveScoreRuntime.Current.TryResolvePlacement(data,shape,coords),"Tutorial placement failed");
                EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(shape);tutorialMoves++;next=EditorApplication.timeSinceStartup+3;break;
            case 24:
                Check(!GameManager.instance.IsTutorialMode()&&Panel!=null&&Find<LevelManager>().RunScore==0,"Tutorial did not start a fresh Endless game");
                EventManager.GameStatus=EGameState.Paused;MenuManager.instance.ShowPopup("Popups/SettingsGame");Wait();break;
            case 25:
                Check(!GameSessionRuntime.Current.CanAcceptPlacement,"Settings allowed a placement");
                InvokePrivate(Find<Settings>(),"Retry");Wait(3);break;
            case 26:
                Check(Find<LevelManager>().RunScore==0&&!MenuManager.instance.IsAnyPopupOpened(),"Restart did not dismiss settings");
                EventManager.GameStatus=EGameState.Paused;MenuManager.instance.ShowPopup("Popups/SettingsGame");Wait();break;
            case 27:
                Find<Settings>().closeButton.Pressed();Wait();break;
            case 28:
                Check(EventManager.GameStatus==EGameState.Playing&&GameSessionRuntime.Current.CanAcceptPlacement,"Resume did not restore input");
                EventManager.GameStatus=EGameState.Paused;MenuManager.instance.ShowPopup("Popups/SettingsGame");Wait();break;
            case 29:
                InvokePrivate(Find<Settings>(),"BackToMain");Wait(3);break;
            case 30:
                Check(!MenuManager.instance.IsAnyPopupOpened(),"Home left an old popup visible");
                liveLevels=ArcadeLevelCatalog.LoadAll();liveLevelIndex=0;Wait();break;
            case 31:
                RunSnapshot.Delete(EGameMode.Adventure);SceneLoader.instance.StartGameScene(liveLevels[liveLevelIndex].Number);Wait(2);break;
            case 32:
                var actual=Find<LevelManager>();
                Check(actual.currentLevel==liveLevels[liveLevelIndex].Number&&Panel==null,"Incorrect Adventure scene mode");
                Check(Find<AdventureObjectivesPopup>()!=null&&EventManager.GameStatus==EGameState.PrepareGame,"Level intro missing: "+actual.currentLevel);
                Check(Find<TargetManager>().GetTargets().All(t=>t.amount>0&&t.targetScriptable!=null),"Invalid target: "+actual.currentLevel);
                var introButton=(RainbowBlockSaga.Presentation.Scripts.GUI.CustomButton)typeof(AdventureObjectivesPopup).GetField("playButton",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Find<AdventureObjectivesPopup>());
                introButton.Pressed();Wait(0.7);break;
            case 33:
                Check(GameSessionRuntime.Current.CanAcceptPlacement,"Level did not begin: "+liveLevels[liveLevelIndex].Number);
                var realLevel=Find<LevelManager>();var realField=realLevel.GetFieldManager();BoardRuntime.Current.SyncNow();
                var legal=realLevel.cellDeck.GetShapes().FirstOrDefault(sh=>BoardRuntime.Current.Placement.HasAnyValidPlacement(BoardRuntime.Current.Model,sh.GetComponent<BlockViewAdapter>().Data));
                Check(legal!=null,"Level starts without a legal move: "+realLevel.currentLevel);
                var legalData=legal.GetComponent<BlockViewAdapter>().Data;
                var legalAnchor=BoardRuntime.Current.Placement.GetValidPlacements(BoardRuntime.Current.Model,legalData)[0];
                var legalCoords=BoardRuntime.Current.Placement.GetPlacementCells(legalData,legalAnchor);
                var legalItems=legal.GetActiveItems();
                for(int i=0;i<legalCoords.Count;i++)
                {
                    var c=legalCoords[i];var cell=realField.cells[realField.RowCount-1-c.Y,c.X];cell.FillCell(legalItems[i].itemTemplate);
                    if(legalItems[i].bonusItemTemplate!=null)cell.SetBonus(legalItems[i].bonusItemTemplate);
                }
                BoardRuntime.Current.SyncNow();Check(ResolveScoreRuntime.Current.TryResolvePlacement(legalData,legal,legalCoords),"Level rejected legal move");
                EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(legal);Wait(1);break;
            case 34:
                Check(Find<LevelManager>().RunScore>0,"Level placement did not score");
                System.IO.File.AppendAllText(Dir+"levels-result.txt","PASS level "+liveLevels[liveLevelIndex].Number+": load / objectives / play / legal placement / score\n");
                liveLevelIndex++;
                if(liveLevelIndex<liveLevels.Length){step=31;next=EditorApplication.timeSinceStartup+0.3;break;}
                Finish(errors.Count==0?"PASS: score/resume, Adventure last move/time limit/objective gate, booster buy/invalid-drop/valid-drag/cancel/tray restore/save, pause, free/paid spin, single claim, tutorial/fresh run, Settings restart/resume/home, and all 20 Adventure levels load/start/place/score.":"FAIL runtime logs\n"+string.Join("\n",errors));break;
        }
    }
}
