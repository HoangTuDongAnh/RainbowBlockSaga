using System.Collections;
using RainbowBlockSaga.Presentation.Contracts;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay.FX
{
    // Owns only the visual sequence. Scoring is committed by the game session.
    public sealed class EndlessClearFeedback : MonoBehaviour
    {
        RectTransform root;
        Image flash;
        Image[] particles;
        TextMeshProUGUI scoreLabel, comboLabel;
        static readonly Color[] Colors = { new(1,.3f,.55f), new(1,.65f,.25f), new(1,.9f,.35f), new(.4f,1,.6f), new(.3f,.8f,1), new(.7f,.4f,1) };

        public IEnumerator Play(RectTransform parent, Vector3 center, TMP_FontAsset font, Sprite sprite,
            ResolveScoreFeedback turn, EndlessScoringSettings settings)
        {
            Build(parent, font, sprite);
            root.gameObject.SetActive(true);
            root.position = center;
            root.SetAsLastSibling();
            bool rainbow = turn.RainbowTriggered;
            scoreLabel.fontSize = rainbow ? 90 : 70;
            bool strong = turn.Combo >= settings.StrongFeedbackCombo;
            float duration = rainbow ? 1.15f : strong ? .75f : .5f;
            int count = rainbow ? particles.Length : strong ? 18 : turn.Combo >= settings.SmallFeedbackCombo ? 10 : 0;
            scoreLabel.text = rainbow
                ? $"<color=#FFF2AB>RAINBOW!</color>\n+{turn.RainbowBonus} BONUS"
                : $"+{turn.Total}";
            comboLabel.text = rainbow
                ? (turn.FullClear ? "FULL CLEAR  ·  " : "") + $"COMBO {turn.Combo}  ×{turn.Multiplier:0.0}\nTOTAL +{turn.Total}"
                : $"COMBO {turn.Combo}  ×{turn.Multiplier:0.0}";
            scoreLabel.color = Color.white;
            comboLabel.color = Color.white;
            scoreLabel.gameObject.SetActive(false);
            comboLabel.gameObject.SetActive(false);
            for (int i=0; i<particles.Length; i++) particles[i].gameObject.SetActive(i<count);
            flash.color = Color.clear;
            bool sounded = false;
            // Timed order: particles, soft flash, sound, score, combo.
            for (float t=0; t<duration; t+=Time.deltaTime)
            {
                for(int i=0; i<count; i++)
                {
                    float angle = (i * 137.5f) * Mathf.Deg2Rad;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    particles[i].rectTransform.anchoredPosition = direction * (35 + t*(270+i%5*35)) + Vector2.down*(100*t*t);
                    particles[i].rectTransform.localRotation = Quaternion.Euler(0,0,i*37+t*160);
                    var color = Colors[i%Colors.Length]; color.a = Mathf.Clamp01(1-t/duration);
                    particles[i].color = color;
                }
                float flashAlpha = rainbow && t>=.10f ? settings.FlashOpacity * Mathf.Clamp01(1-(t-.10f)/.27f) : 0;
                flash.color = new Color(1,.92f,1,flashAlpha);
                if (t>=.17f && !sounded)
                {
                    sounded = true;
                    var sound = SoundBase.instance;
                    if (rainbow && sound != null)
                    {
                        var clip = settings.RainbowSound;
                        if (!clip && sound.combo != null && sound.combo.Length>0) clip=sound.combo[sound.combo.Length-1];
                        sound.PlaySound(clip ? clip : sound.coins);
                    }
                }
                float scoreStart = rainbow ? .25f : 0;
                if (t>=scoreStart)
                {
                    scoreLabel.gameObject.SetActive(true);
                    scoreLabel.rectTransform.localScale = Vector3.one * (1 + .12f*Mathf.Sin(Mathf.Clamp01((t-scoreStart)/.25f)*Mathf.PI));
                }
                if (t>=(rainbow ? .48f : .12f)) comboLabel.gameObject.SetActive(true);
                float alpha = Mathf.Clamp01((duration-t)/.25f);
                scoreLabel.alpha = comboLabel.alpha = alpha;
                yield return null;
            }
            Clear();
        }

        void Build(RectTransform parent, TMP_FontAsset font, Sprite sprite)
        {
            if(root) return;
            var go = new GameObject("EndlessClearFeedback",typeof(RectTransform));
            root = (RectTransform)go.transform; root.SetParent(parent,false); root.sizeDelta=new Vector2(800,450);
            flash = Image("RainbowFlash",new Vector2(850,850));
            particles = new Image[30];
            for(int i=0;i<particles.Length;i++) { particles[i]=Image("SugarSpark",new Vector2(i%3==0?10:18, i%3==0?30:18)); particles[i].sprite=sprite; }
            scoreLabel = Label("Score",font,70,new Vector2(0,85));
            comboLabel = Label("Combo",font,44,new Vector2(0,-110));
        }
        Image Image(string name, Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(root,false);
            var image=go.GetComponent<Image>();image.raycastTarget=false;image.rectTransform.sizeDelta=size;return image;
        }
        TextMeshProUGUI Label(string name,TMP_FontAsset font,float size,Vector2 position)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);
            var label=go.GetComponent<TextMeshProUGUI>();label.font=font;label.fontSize=size;label.fontStyle=FontStyles.Bold;
            label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.rectTransform.sizeDelta=new Vector2(850,210);
            label.rectTransform.anchoredPosition=position;return label;
        }
        public void Clear(){if(root)root.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        void OnDestroy(){if(root)Destroy(root.gameObject);}
    }
}
