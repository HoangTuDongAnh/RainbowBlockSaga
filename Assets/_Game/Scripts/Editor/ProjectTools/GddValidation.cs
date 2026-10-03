using System;
using System.IO;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GddValidation
{
    static GddValidation() { EditorApplication.update += CheckRequest; }
    static void CheckRequest()
    {
        const string request = "Temp/GddImplementation/validate.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(request);
        try { Validate(); File.WriteAllText("Temp/GddImplementation/editor-result.txt", "PASS " + DateTime.Now); }
        catch(Exception ex) { File.WriteAllText("Temp/GddImplementation/editor-result.txt", ex.ToString()); Debug.LogException(ex); }
    }
    static void Check(bool value, string error) { if (!value) throw new Exception(error); }
    [MenuItem("Rainbow Blocks Saga/Validate GDD Features")]
    public static void Validate()
    {
        Debug.Log(RbsProjectChecks.Run());
        Check(RunRewards.EndlessCoins(999)==45 && RunRewards.EndlessCoins(1000)==70 && RunRewards.EndlessCoins(5000)==350 && RunRewards.EndlessCoins(5100)==510,"Reward bands");
        Check(RunRewards.AdventureCoins(1)==10 && RunRewards.AdventureCoins(5)==50 && RunRewards.AdventureCoins(10)==50,"Adventure milestones");
        foreach(var name in new[]{"Failed_Endless","PreWin_Bonus","PreWin_Score"})
        {
            var prefab=Resources.Load<GameObject>("Popups/"+name);
            Check(prefab!=null,"Missing result popup " + name);
            foreach(var component in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Check(component!=null,"Missing popup script " + name);
                var serialized=new SerializedObject(component);
                foreach(var key in new[]{"runSummaryText","rewardSummaryText"})
                {
                    var prop=serialized.FindProperty(key);
                    if(prop!=null) Check(prop.objectReferenceValue!=null,"Unassigned " + name + "." + key);
                }
            }
        }
        Debug.Log("GDD_VALIDATION_PASS: scoring, rewards, level data and result prefab references.");
    }
}
