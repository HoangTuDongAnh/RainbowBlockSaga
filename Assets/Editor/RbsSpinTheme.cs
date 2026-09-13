using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using RainbowBlockSaga.Presentation.Scripts.Popups;
[InitializeOnLoad]
public static class RbsSpinTheme {
 const string Path="Assets/_Game/Resources/Popups/LuckySpin.prefab";
 static RbsSpinTheme(){EditorApplication.delayCall+=Run;}
 static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
 static Sprite S(string n){return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Sprites/"+n+".png");}
 static void Place(Transform t,Vector2 pos,Vector2 size){var rt=(RectTransform)t;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;}
 static RectTransform Rect(Transform p,string name,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.layer=5;var rt=(RectTransform)go.transform;rt.SetParent(p,false);Place(rt,pos,size);return rt;}
 static Image Img(Transform p,string name,Vector2 pos,Vector2 size,Sprite s,string hex,bool sliced=false){var rt=Rect(p,name,pos,size);var im=rt.gameObject.AddComponent<Image>();im.sprite=s;im.color=C(hex);im.raycastTarget=false;im.type=sliced?Image.Type.Sliced:Image.Type.Simple;return im;}
 static TextMeshProUGUI Text(Transform p,string name,string value,Vector2 pos,Vector2 size,TMP_FontAsset font,float fs,string color){var rt=Rect(p,name,pos,size);var tx=rt.gameObject.AddComponent<TextMeshProUGUI>();tx.font=font;tx.text=value;tx.fontSize=fs;tx.color=C(color);tx.alignment=TextAlignmentOptions.Center;tx.raycastTarget=false;return tx;}
 static void Run(){
 if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("Temp/spin-build.flag"))return;
 File.Delete("Temp/spin-build.flag");GameObject root=null;
 try{
 root=PrefabUtility.LoadPrefabContents(Path);var content=root.transform.Find("Content");var luck=root.GetComponent<LuckySpin>();
 var so=new SerializedObject(luck);var spin=so.FindProperty("spin").objectReferenceValue as GameObject;var physics=spin.GetComponent<Rigidbody2D>();float damping=physics.angularDamping;int spinChildren=spin.transform.childCount;
 var title=content.Find("LuckySpin").GetComponent<TextMeshProUGUI>();var font=title.font;var round=S("general-ui/buttons2");var circle=S("general-ui/play-back-label");var candy=S("general-ui/logo/rainbow-candy-icon");
 Img(content,"CandyCardShadow",new Vector2(0,50),new Vector2(1270,2120),round,"3C1B60",true).transform.SetAsFirstSibling();
 Img(content,"CandyCardBorder",new Vector2(0,75),new Vector2(1250,2120),round,"E879B8",true).transform.SetSiblingIndex(1);
 Img(content,"CandyCardCream",new Vector2(0,83),new Vector2(1218,2070),round,"FFF1FA",true).transform.SetSiblingIndex(2);
 Img(content,"CandyCardInset",new Vector2(0,80),new Vector2(1168,2000),round,"F0DFFA",true).transform.SetSiblingIndex(3);
 content.Find("rays").gameObject.SetActive(false);var reference=content.Find("lucky-1-ref");if(reference)UnityEngine.Object.DestroyImmediate(reference.gameObject);
 title.text="LUCKY SPIN";title.color=C("713C9C");title.enableAutoSizing=false;title.fontSize=100;title.fontSharedMaterial=font.material;Place(title.transform,new Vector2(0,895),new Vector2(1060,160));
 Text(content,"SweetRewards","SWEET REWARDS",new Vector2(0,1020),new Vector2(760,80),font,40,"BB568D").characterSpacing=8;
 Img(content,"HeaderCandyLeft",new Vector2(-455,970),new Vector2(165,88),candy,"FFFFFF").transform.localEulerAngles=new Vector3(0,0,22);
 Img(content,"HeaderCandyRight",new Vector2(455,970),new Vector2(165,88),candy,"FFFFFF").transform.localEulerAngles=new Vector3(0,0,-18);
 string[] colors={"FFB9D8","FFD48D","FFF0A6","A9EAD3","B1E3FF","D4B8F6"};var sprites=spin.transform.Find("Sprites");var wedges=sprites.GetComponentsInChildren<Image>().Where(x=>x.name.StartsWith("lucky-spin")).ToArray();if(wedges.Length!=6)throw new Exception("Expected six reward sectors");for(int i=0;i<6;i++)wedges[i].color=C(colors[i]);
 sprites.Find("play-back-label").GetComponent<Image>().color=C("8B468E");sprites.Find("play-back-label (1)").GetComponent<Image>().color=C("FFAED1");sprites.Find("play-back-label (2)").GetComponent<Image>().color=C("FFF7E9");sprites.Find("play-back-label (3)").GetComponent<Image>().color=C("FFF7E9");
 for(int i=0;i<30;i++){float a=i*12*Mathf.Deg2Rad;var im=Img(sprites,"Sugar-"+i,new Vector2(Mathf.Sin(a)*431,Mathf.Cos(a)*431),new Vector2(i%3==0?25:19,10),round,i%3==0?"FFFFFF":colors[i%6],true);im.transform.localEulerAngles=new Vector3(0,0,-i*12+(i%2==0?25:-20));}
 foreach(var im in content.Find("Arrow").GetComponentsInChildren<Image>())im.enabled=false;
 Img(content,"CandyHubBorder",new Vector2(0,171),new Vector2(180,180),circle,"A256A6");Img(content,"CandyHubCream",new Vector2(0,177),new Vector2(153,153),circle,"FFF7EF");Img(content,"CandyHub",new Vector2(0,180),new Vector2(220,115),candy,"FFFFFF");
 var pointer=Img(content,"TopPointer",new Vector2(0,678),new Vector2(110,135),S("general-ui/lucky-spin"),"AD4E92");pointer.transform.localEulerAngles=new Vector3(0,0,270);
 int k=0;foreach(Transform lamp in content.Find("Lights")){lamp.GetComponent<Image>().color=C(colors[k++%6]);var glow=lamp.GetComponentsInChildren<Image>().Last();float alpha=glow.color.a;glow.color=new Color(1,.98f,.9f,alpha);}
 var buttons=content.Find("Buttons");foreach(var group in buttons.GetComponents<LayoutGroup>())group.enabled=false;Place(buttons,new Vector2(0,-565),new Vector2(700,220));
 foreach(Transform button in buttons){Place(button,Vector2.zero,new Vector2(650,205));foreach(var im in button.GetComponentsInChildren<Image>(true)){if(im.name=="main-color")im.color=C("F65CAD");else if(im.name=="shadow-layer")im.color=C("CD3C8F");else if(im.name=="border")im.color=C("8C387F");else if(im.name=="gloss")im.color=C("FFF5E8");}}
 var ft=luck.freeSpinButton.GetComponentInChildren<TextMeshProUGUI>(true);ft.text="FREE SPIN";ft.fontSize=64;ft.enableAutoSizing=false;Place(ft.transform,new Vector2(0,8),new Vector2(560,110));
 var buy=luck.buySpinButton.transform;Place(buy.Find("button_1"),Vector2.zero,new Vector2(650,205));var coin=buy.Find("CoinsLabelButton");Place(coin,new Vector2(-170,0),new Vector2(100,100));Place(luck.costToSpinText.transform,new Vector2(105,0),new Vector2(115,90));luck.costToSpinText.fontSize=60;
 var bt=coin.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();bt.text="SPIN";bt.fontSize=60;Place(bt.transform,new Vector2(285,0),new Vector2(220,90));
 Text(content,"SpinHint","Give the rainbow a whirl!",new Vector2(0,-427),new Vector2(1000,80),font,41,"98689E");Text(content,"BalanceCaption","YOUR COINS",new Vector2(0,-750),new Vector2(600,70),font,34,"98689E").characterSpacing=5;
 Place(content.Find("CoinsLabel"),new Vector2(-132,-857),new Vector2(203,222));foreach(var tx in content.Find("CoinsLabel").GetComponentsInChildren<TMP_Text>())tx.color=C("713C9C");
 var close=root.transform.Find("XButton");close.SetAsLastSibling();Img(close,"CandyCloseBack",Vector2.zero,new Vector2(145,145),circle,"B66AC0").transform.SetAsFirstSibling();close.GetComponent<Image>().color=C("FFFFFF");
 if(spin.transform.childCount!=spinChildren||physics.angularDamping!=damping||luck.spinRewards.Length!=0)throw new Exception("Spin mechanics changed");foreach(var im in root.GetComponentsInChildren<Image>(true))if(im.enabled&&im.sprite==null)throw new Exception("Missing sprite "+im.name);
 PrefabUtility.SaveAsPrefabAsset(root,Path);AssetDatabase.SaveAssets();File.WriteAllText("Temp/spin-build-result.txt","SPIN_THEME_OK: six sectors, reward indexing and physics unchanged, buttons bound.");
 }catch(Exception ex){Debug.LogException(ex);File.WriteAllText("Temp/spin-build-result.txt",ex.ToString());}finally{if(root)PrefabUtility.UnloadPrefabContents(root);}
 }
}
