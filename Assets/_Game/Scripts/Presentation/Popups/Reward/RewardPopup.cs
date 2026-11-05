using RainbowBlockSaga.Presentation.Scripts.Data;
using System.Collections;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using UnityEngine.UI;
using RainbowBlockSaga.Presentation.Scripts.Settings;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups.Reward
{
    public class RewardPopup : PopupWithCurrencyLabel
    {
        public Transform iconPos;
        private int _count;
        private ResourceObject _resource;
        private RewardSettingSpin rewardVisual;
        private bool claiming;

        public TextMeshProUGUI countText;

        public void SetReward(RewardSettingSpin rewardVisual)
        {
            this.rewardVisual = rewardVisual;
            _count = rewardVisual.count;
            countText.text = _count.ToString();
            _resource = rewardVisual.resource;
        }

        public override void Close()
        {
            if (claiming || rewardVisual == null || _resource == null) return;
            claiming = true;
            StopInteration();

            _resource.Add(_count);
            var menu = FindAnyObjectByType<MainMenu>();
            if (menu == null || menu.HomeCoin == null)
            {
                base.Close();
                return;
            }
            StartCoroutine(FlyCoin(menu.HomeCoin));
        }

        private IEnumerator FlyCoin(RectTransform destination)
        {
            var coin = iconPos.GetComponentInChildren<Image>().rectTransform;
            var root = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var hud = destination.GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var hudCamera = hud.renderMode == RenderMode.ScreenSpaceOverlay ? null : hud.worldCamera;
            coin.SetParent(root, true);
            coin.SetAsLastSibling();
            Vector3 start = coin.position;
            Vector3 scale = coin.localScale;
            for (float elapsed = 0; elapsed < .55f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / .55f);
                var screen = RectTransformUtility.WorldToScreenPoint(hudCamera, destination.TransformPoint(destination.rect.center));
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(root, screen, camera, out var end))
                    coin.position = Vector3.Lerp(start, end, t);
                coin.localScale = scale * Mathf.Lerp(1f, .28f, t);
                yield return null;
            }
            var finalScreen = RectTransformUtility.WorldToScreenPoint(hudCamera, destination.TransformPoint(destination.rect.center));
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(root, finalScreen, camera, out var finalPosition))
                coin.position = finalPosition;
            coin.localScale = scale * .28f;
            if (_resource.sound != null) SoundBase.instance.PlaySound(_resource.sound);
            base.Close();
        }
    }
}
