using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using RainbowBlockSaga.Presentation.Scripts.Popups;
[InitializeOnLoad]
public static class RbsSpinFinish {
 static RbsSpinFinish(){EditorApplication.delayCall+=Finish;EditorApplication.update+=PreviewPaid;}
 static void Finish(){
 if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("Temp/spin-finish.flag"))return;
 File.Delete("Temp/spin-finish.flag");var path="Assets/_Game/Resources/Popups/LuckySpin.prefab";var root=PrefabUtility.LoadPrefabContents(path);
 var close=root.transform.Find("XButton");var glyph=new GameObject("CloseGlyph",typeof(RectTransform),typeof(Image));glyph.layer=5;glyph.transform.SetParent(close,false);((RectTransform)glyph.transform).sizeDelta=new Vector2(78,78);var im=glyph.GetComponent<Image>();im.sprite=close.GetComponent<Image>().sprite;im.raycastTarget=false;
 foreach(var image in root.GetComponentsInChildren<Image>(true))if(image.name.StartsWith("CandyCard"))image.pixelsPerUnitMultiplier=.4f;
 PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();File.WriteAllText("Temp/spin-finish-result.txt","SPIN_FINISH_OK");
 }
 static void PreviewPaid(){if(!EditorApplication.isPlaying||!File.Exists("Temp/spin-paid-preview.flag"))return;var popup=UnityEngine.Object.FindFirstObjectByType<LuckySpin>();if(!popup)return;File.Delete("Temp/spin-paid-preview.flag");popup.freeSpinButton.gameObject.SetActive(false);popup.buySpinButton.gameObject.SetActive(true);File.WriteAllText("Temp/spin-paid-result.txt","Paid button preview only; no coins spent or PlayerPrefs changed.");}
}
