using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
[InitializeOnLoad]
public static class RbsSpinEdit {
 static RbsSpinEdit(){EditorApplication.delayCall+=Run;}
 static void Run(){
 if(EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists("Temp/spin-inspect.flag"))return;
 File.Delete("Temp/spin-inspect.flag");
 var root=PrefabUtility.LoadPrefabContents("Assets/_Game/Resources/Popups/LuckySpin.prefab");
 var sb=new StringBuilder();
 foreach(var t in root.GetComponentsInChildren<RectTransform>(true)){
 var im=t.GetComponent<Image>();var tx=t.GetComponent<TMP_Text>();
 sb.AppendLine(AnimationUtility.CalculateTransformPath(t,root.transform)+" active="+t.gameObject.activeSelf+" pos="+t.anchoredPosition+" size="+t.sizeDelta+" rot="+t.localEulerAngles+" sprite="+(im&&im.sprite?AssetDatabase.GetAssetPath(im.sprite):"")+" color="+(im?im.color.ToString():"")+" text="+(tx?tx.text:"")+" id="+GlobalObjectId.GetGlobalObjectIdSlow(t));
 }
 File.WriteAllText("Temp/spin-hierarchy.txt",sb.ToString());PrefabUtility.UnloadPrefabContents(root);
 }
}
