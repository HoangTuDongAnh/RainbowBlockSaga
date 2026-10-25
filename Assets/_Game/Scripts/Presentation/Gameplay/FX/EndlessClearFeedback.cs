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
        TextMeshProUGUI messageLabel;
        static readonly Color[] Colors = { new(1,.3f,.55f), new(1,.65f,.25f), new(1,.9f,.35f), new(.4f,1,.6f), new(.3f,.8f,1), new(.7f,.4f,1) };
        static int sequenceIndex;
        int variation;

        public IEnumerator Play(RectTransform parent, Vector3 center, TMP_FontAsset font,
            ResolveScoreFeedback turn, EndlessScoringSettings settings)
        {
            Build(parent, font);
            root.gameObject.SetActive(true);
            root.position = center;
            root.SetAsLastSibling();
            bool rainbow = turn.RainbowTriggered;
            bool strong = turn.Combo >= settings.StrongFeedbackCombo;
            variation = sequenceIndex++ % 3;
            int count = rainbow ? particles.Length : strong ? 20 : turn.Combo >= settings.SmallFeedbackCombo ? 14 : 8;
            messageLabel.gameObject.SetActive(false);
            for (int i=0; i<particles.Length; i++) particles[i].gameObject.SetActive(i<count);
            flash.color = Color.clear;

            // One message at a time: the player sees the achievement, its bonus,
            // the combo, and finally the total gained by the turn.
            if (rainbow)
            {
                yield return AnimateIntro(count, settings.FlashOpacity, .27f);
                SoundBase.instance.PlaySound(settings.RainbowSound);
                yield return ShowMessage("RAINBOW!", Colors[(variation + 2) % Colors.Length], 90, .55f);
                yield return ShowMessage($"+{turn.RainbowBonus} BONUS", Color.white, 76, .48f);
                yield return ShowMessage($"COMBO {turn.Combo}  ×{turn.Multiplier:0.0}", Colors[(variation + 4) % Colors.Length], 66, .48f);
                yield return ShowMessage($"TOTAL +{turn.Total}", new Color(1f, .94f, .48f), 76, .72f, turn.Total);
            }
            else
            {
                yield return AnimateIntro(count, strong ? .18f : .08f, .15f);
                yield return ShowMessage($"+{turn.Total}", Colors[(variation + turn.Combo) % Colors.Length], 76, .62f, turn.Total);
                if (turn.Combo > 1)
                    yield return ShowMessage($"COMBO {turn.Combo}  ×{turn.Multiplier:0.0}", Colors[(variation + 3) % Colors.Length], 66, .54f);
            }

            Clear();
        }

        IEnumerator AnimateIntro(int count, float opacity, float duration)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = Mathf.Clamp01(t / duration);
                for (int i = 0; i < count; i++)
                {
                    float angle = (i * 137.5f + variation * 42f) * Mathf.Deg2Rad;
                    float distance = variation switch
                    {
                        0 => 30 + progress * (155 + i % 5 * 15),
                        1 => 18 + progress * (125 + i % 4 * 18),
                        _ => 50 + Mathf.Sin(progress * Mathf.PI * .75f) * (130 + i % 3 * 25)
                    };
                    if (variation == 1) angle += progress * 1.6f;
                    particles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    particles[i].rectTransform.localRotation = Quaternion.Euler(0, 0, i * 37 + progress * (variation == 1 ? 270 : 110));
                    var color = Colors[(i + variation) % Colors.Length];
                    color.a = Mathf.Sin(progress * Mathf.PI);
                    particles[i].color = color;
                }
                flash.color = new Color(1, .92f, 1, opacity * Mathf.Sin(progress * Mathf.PI));
                yield return null;
            }
            flash.color = Color.clear;
            for (int i = 0; i < count; i++) particles[i].gameObject.SetActive(false);
        }

        IEnumerator ShowMessage(string value, Color color, float size, float duration, int rollScore = -1)
        {
            messageLabel.text = value;
            messageLabel.fontSize = size;
            messageLabel.color = color;
            messageLabel.gameObject.SetActive(true);
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = Mathf.Clamp01(t / duration);
                if (rollScore >= 0)
                {
                    int visibleScore = Mathf.RoundToInt(Mathf.Lerp(0, rollScore, 1 - Mathf.Pow(1 - Mathf.Min(1, progress * 2.3f), 3)));
                    messageLabel.text = value.StartsWith("TOTAL") ? $"TOTAL +{visibleScore}" : $"+{visibleScore}";
                }
                messageLabel.alpha = Mathf.Min(1, progress * 6, (1 - progress) * 5);
                float scale = 1 + .26f * Mathf.Sin(Mathf.Min(1, progress * 3) * Mathf.PI);
                messageLabel.rectTransform.localScale = Vector3.one * scale;
                float drift = variation == 0 ? 24 : variation == 1 ? -18 : 0;
                messageLabel.rectTransform.anchoredPosition = new Vector2(
                    variation == 2 ? Mathf.Sin(progress * 7f) * 10f * (1 - progress) : drift * (1 - progress),
                    (variation == 1 ? -30f : 30f) * (1 - progress));
                messageLabel.rectTransform.localRotation = Quaternion.Euler(0, 0,
                    variation == 2 ? 0 : (variation == 0 ? -5f : 5f) * (1 - progress));
                yield return null;
            }
            messageLabel.text = value;
            messageLabel.rectTransform.localRotation = Quaternion.identity;
            messageLabel.rectTransform.anchoredPosition = Vector2.zero;
            messageLabel.gameObject.SetActive(false);
        }

        void Build(RectTransform parent, TMP_FontAsset font)
        {
            if(root) return;
            var go = new GameObject("EndlessClearFeedback",typeof(RectTransform));
            root = (RectTransform)go.transform; root.SetParent(parent,false); root.sizeDelta=new Vector2(800,450);
            flash = Image("RainbowFlash",new Vector2(850,850));
            particles = new Image[30];
            for(int i=0;i<particles.Length;i++) particles[i]=Image("SugarSpark",new Vector2(i%3==0?10:18, i%3==0?30:18));
            messageLabel = Label("Message",font,76,Vector2.zero);
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
            label.enableAutoSizing = true; label.fontSizeMin = 44; label.fontSizeMax = 90;
            label.outlineWidth = .24f;
            label.outlineColor = new Color(.18f, .03f, .3f, .95f);
            label.rectTransform.anchoredPosition=position;return label;
        }
        public void Clear(){if(root)root.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        void OnDestroy(){if(root)Destroy(root.gameObject);}
    }
}
