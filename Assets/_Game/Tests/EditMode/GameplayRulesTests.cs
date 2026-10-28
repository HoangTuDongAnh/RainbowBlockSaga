using NUnit.Framework;
using RainbowBlockSaga.Presentation.Contracts;
using RainbowBlockSaga.Presentation.Scripts.System;

namespace RainbowBlockSaga.Tests
{
    public sealed class GameplayRulesTests
    {
        [Test]
        public void EndlessCombo_UsesConfiguredMultipliersAndRainbowBonus()
        {
            var settings = new EndlessScoringSettings();
            Assert.That(settings.Multiplier(1), Is.EqualTo(1f));
            Assert.That(settings.Multiplier(3), Is.EqualTo(1.2f).Within(.001f));
            Assert.That(settings.Multiplier(5), Is.EqualTo(1.5f));
            Assert.That(settings.Multiplier(8), Is.EqualTo(1.5f));

            var turn = new ResolveScoreFeedback(true, 10, 80, settings.Multiplier(5),
                settings.RainbowBonus, 5, false, true);
            Assert.That(turn.Total, Is.EqualTo(230));
        }

        [TestCase(999, 45)]
        [TestCase(1000, 70)]
        [TestCase(5000, 350)]
        [TestCase(5100, 510)]
        public void EndlessReward_UsesScoreBands(int score, int coins)
        {
            Assert.That(RunRewards.EndlessCoins(score), Is.EqualTo(coins));
        }

        [TestCase(1, 10)]
        [TestCase(5, 50)]
        [TestCase(10, 50)]
        public void AdventureReward_UsesMilestones(int level, int coins)
        {
            Assert.That(RunRewards.AdventureCoins(level), Is.EqualTo(coins));
        }
    }
}
