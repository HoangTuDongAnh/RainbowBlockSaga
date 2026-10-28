using System.Linq;
using NUnit.Framework;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Tests
{
    public sealed class ContentAndPopupTests
    {
        [Test]
        public void AdventureLevels_HaveOrderedIdsPlayableBoardsAndObjectives()
        {
            var levels = ArcadeLevelCatalog.LoadAll();
            Assert.That(levels.Length, Is.GreaterThan(0));
            Assert.That(levels.Select(level => level.Number).Distinct().Count(), Is.EqualTo(levels.Length));
            foreach (var level in levels)
            {
                Assert.That(level.rows, Is.GreaterThan(0), level.name);
                Assert.That(level.columns, Is.GreaterThan(0), level.name);
                Assert.That(level.levelRows, Has.Length.EqualTo(level.rows), level.name);
                Assert.That(level.levelRows.All(row => row.cells.Length == level.columns), Is.True, level.name);
                Assert.That(level.levelType != null && level.levelType.stateHandler != null, Is.True, level.name);
                Assert.That(level.targetInstance.Any(target => target.targetScriptable != null && target.amount > 0),
                    Is.True, level.name);
            }
            Assert.That(ArcadeLevelCatalog.Next(levels[levels.Length - 1].Number), Is.Null);
        }

        [Test]
        public void RuntimePopups_HaveModalBackdrop()
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/Resources/Popups" });
            var count = 0;
            foreach (var guid in paths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var popup = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Popup>();
                if (popup == null) continue;
                count++;
                Assert.That(popup.fade, Is.True, path);
                Assert.That(popup.GetComponent<CanvasGroup>(), Is.Not.Null, path);
            }
            Assert.That(count, Is.GreaterThanOrEqualTo(15));
        }

        [Test]
        public void AdventureObjectivePopup_HasAssignedButtonsAndSafeAreaFit()
        {
            var popup = Resources.Load<AdventureObjectivesPopup>("Popups/AdventureObjectivesPopup");
            Assert.That(popup, Is.Not.Null);
            Assert.That(popup.GetComponent<PopupContentFit>(), Is.Not.Null);
            var serialized = new SerializedObject(popup);
            foreach (var field in new[] { "title", "limits", "playButton", "mapButton" })
                Assert.That(serialized.FindProperty(field).objectReferenceValue, Is.Not.Null, field);
            Assert.That(serialized.FindProperty("objectives").arraySize, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void PopupCanvas_BackdropBlocksInputWithoutNestedCanvas()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/Prefabs/GUI/CanvasMenu.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Canvas>(true), Has.Length.EqualTo(1));
            var backdrop = prefab.transform.Find("Fade").GetComponent<Image>();
            Assert.That(backdrop.raycastTarget, Is.True);
            Assert.That(backdrop.transform.GetSiblingIndex(), Is.EqualTo(0));
        }
    }
}
