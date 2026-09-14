// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Popups.Reward;
using RainbowBlockSaga.Presentation.Scripts.Settings;
using RainbowBlockSaga.Presentation.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class LuckySpin : PopupWithCurrencyLabel
    {
        public float velocity;
        public float stoptime;

        [SerializeField]
        private GameObject spin;

        [SerializeField]
        private List<Image> lights = new();

        public CustomButton freeSpinButton;
        public CustomButton buySpinButton;
        public CustomButton rewardedAdButton;
        public TextMeshProUGUI costToSpinText;
        public RewardSettingSpin[] spinRewards;
        public List<RewardVisual> rewards = new();
        private SpinSettings spinSettings;
        private Rigidbody2D rb;
        private bool isSpinning;
        private int previousRotationMarker;
        private const string LastFreeSpinTimeKey = "LastFreeSpinTime";

        [SerializeField]
        private float minVelocityMultiplier = 0.5f;

        [SerializeField]
        private float maxVelocityMultiplier = 2.5f;

        [SerializeField]
        private float additionalRandomFactor = 0.2f;

        private void OnEnable()
        {
            rb = spin.GetComponent<Rigidbody2D>();
            freeSpinButton.onClick.AddListener(FreeSpin);
            buySpinButton.onClick.AddListener(BuySpin);

            UpdateButtonVisibility();

            spinSettings = GameManager.instance.luckySpinSettings;
            DefineRewards(spinSettings.rewards);
            StartCoroutine(SwitchLightsAlpha());
            costToSpinText.text = spinSettings.costToSpin.ToString();
        }

        private void UpdateButtonVisibility()
        {
            var canFreeSpin = CanUseFreeSpinToday();
            freeSpinButton.gameObject.SetActive(canFreeSpin);
            buySpinButton.gameObject.SetActive(!canFreeSpin);
            rewardedAdButton?.gameObject.SetActive(false);
        }

        private void SetButtonsVisibility(bool visible)
        {
            freeSpinButton.gameObject.SetActive(visible);
            buySpinButton.gameObject.SetActive(visible);
            rewardedAdButton?.gameObject.SetActive(false);
        }

        private bool CanUseFreeSpinToday()
        {
            if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
            {
                return true;
            }

            var lastFreeSpinTimeStr = PlayerPrefs.GetString(LastFreeSpinTimeKey);
            var lastFreeSpinTime = DateTime.Parse(lastFreeSpinTimeStr);
            return DateTime.Now.Date > lastFreeSpinTime.Date;
        }

        private void FreeSpin()
        {
            if (isSpinning || !CanUseFreeSpinToday()) return;
            PlayerPrefs.SetString(LastFreeSpinTimeKey, DateTime.Now.ToString("o"));
            Spin();
        }

        private IEnumerator SwitchLightsAlpha()
        {
            const float maxSpeed = 100;

            while (true)
            {
                var speedRatio = Mathf.Abs(rb.angularVelocity) / maxSpeed; // Ratio of the current speed to the maximum speed
                speedRatio = Mathf.Min(speedRatio, .9f);
                var delay = 1f - speedRatio; // Higher speed -> smaller delay
                yield return new WaitForSeconds(delay);

                foreach (var light in lights)
                {
                    light.color = new Color(light.color.r, light.color.g, light.color.b, light.color.a == 0 ? 1 : 0);
                }
            }
        }

        public void DefineRewards(RewardSettingSpin[] spinRewards)
        {
            this.spinRewards = spinRewards;
            foreach (var reward in spinRewards)
            {
                var obj = Instantiate(reward.rewardVisualPrefab, spin.transform);
                //rotate to 360/number of rewards
                obj.transform.RotateAround(spin.transform.position, Vector3.forward, 360f / spinRewards.Length * obj.transform.GetSiblingIndex());
                obj.SetCount(reward.count);
                rewards.Add(obj);
            }
        }

        private void BuySpin()
        {
            if (isSpinning) return;
            if (ResourceManager.instance.Consume("Coins", spinSettings.costToSpin))
            {
                ShowCoinsSpendFX(buySpinButton.transform.position);
                Spin();
            }
        }

        public void Spin()
        {
            if (isSpinning) return;
            isSpinning = true;
            StartCoroutine(StartSpin());
        }

        private IEnumerator StartSpin()
        {
            // buttons interaction
            closeButton.interactable = false;
            freeSpinButton.interactable = false;
            buySpinButton.interactable = false;
            if (rewardedAdButton != null) rewardedAdButton.interactable = false;

            var randomVelocity = CalculateRandomVelocity();

            float timeElapsed = 0;
            isSpinning = true;
            previousRotationMarker = Mathf.FloorToInt(spin.transform.eulerAngles.z / 25);

            while (timeElapsed < stoptime)
            {
                var appliedTorque = Mathf.Lerp(0, randomVelocity, timeElapsed / stoptime);
                rb.AddTorque(appliedTorque);
                timeElapsed += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            rb.angularDamping *= 100;
            yield return new WaitWhile(() => Mathf.Abs(rb.angularVelocity) > .1f);
            rb.angularVelocity = 0;
            isSpinning = false;
            CheckReward(GetWinReward());
        }

        private float CalculateRandomVelocity()
        {
            var baseMultiplier = Random.Range(minVelocityMultiplier, maxVelocityMultiplier);
            var additionalRandomness = Random.Range(-additionalRandomFactor, additionalRandomFactor);
            return velocity * (baseMultiplier + additionalRandomness);
        }

        private void Update()
        {
            if (isSpinning)
            {
                CheckPlaySound();
            }
        }

        private void CheckPlaySound()
        {
            var currentZRotation = spin.transform.eulerAngles.z;
            var currentTenDegreeMarker = Mathf.FloorToInt(currentZRotation / 25);

            if (currentTenDegreeMarker != previousRotationMarker)
            {
                SoundBase.instance.PlaySound(SoundBase.instance.luckySpin);
                previousRotationMarker = currentTenDegreeMarker;
            }
        }

        private int GetWinReward()
        {
            var highestYIndex = 0; // Start with first item's index
            var highestY = rewards[0].transform.position.y; // and its 'y' position

            for (var i = 1; i < rewards.Count; i++)
            {
                // If current item's 'y' position is higher
                if (rewards[i].transform.position.y > highestY)
                {
                    highestY = rewards[i].transform.position.y;
                    highestYIndex = i;
                }
            }

            return highestYIndex;
        }

        public void CheckReward(int rewardIndex)
        {
            Close();

            var rewardSettingSpin = spinRewards[rewardIndex];
            Popup popup = MenuManager.instance.ShowPopup(rewardSettingSpin.rewardPopupPrefab);
            var rewardPopup = popup as RewardPopup;
            if (rewardPopup != null)
            {
                rewardPopup.SetReward(rewardSettingSpin);
            }
        }
    }
}
