using System.Collections;
using NUnit.Framework;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using UnityEngine;
using UnityEngine.TestTools;

namespace RainbowBlockSaga.Tests
{
    public sealed class RuntimeContentTests
    {
        [UnityTest]
        public IEnumerator AdventureLevels_AreAvailableInPlayMode()
        {
            yield return null;
            var levels = ArcadeLevelCatalog.LoadAll();
            Assert.That(levels.Length, Is.GreaterThan(0));
            Assert.That(ArcadeLevelCatalog.Find(levels[0].Number), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator EssentialPopups_AreAvailableInPlayMode()
        {
            yield return null;
            foreach (var name in new[] { "AdventureObjectivesPopup", "LuckySpin", "RewardPopupCoins", "Failed_Endless" })
                Assert.That(Resources.Load<Popup>("Popups/" + name), Is.Not.Null, name);
        }
    }
}
