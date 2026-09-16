using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.GUI.Labels;
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

            LabelAnim.AnimateForResource(_resource, iconPos.position, "+" + _count, _resource.sound, () =>
            {
                rewardVisual.resource.Add(rewardVisual.count);
                base.Close();
            });
        }
    }
}
