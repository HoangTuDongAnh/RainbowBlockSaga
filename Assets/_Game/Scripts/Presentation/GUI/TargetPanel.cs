using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetPanel : TargetPanelBase
    {
        public GameObject targetPrefab;
        private readonly Dictionary<TargetScriptable, TargetBonusGUIElement> _list = new();

        protected override void OnEnableInternal()
        {
            _list.Clear();
            var levelManager = FindObjectOfType<LevelManager>(true);
            if (levelManager != null)
            {
                OnLevelLoaded(levelManager.GetCurrentLevel());
                RegisterTargets();
            }
        }

        private void OnLevelLoaded(Level obj)
        {
            if (obj == null) return;
            
            foreach (var target in obj.targetInstance)
            {
                if (target.amount == 0) continue;

                var targetElement = Instantiate(targetPrefab, transform);
                var targetBonusGUIElement = targetElement.GetComponent<TargetBonusGUIElement>();
                targetBonusGUIElement.FillElement(target.targetScriptable.bonusItem, target.amount);
                _list.Add(target.targetScriptable, targetBonusGUIElement);
            }
        }
        private void RegisterTargets()
        {
            foreach (var target in _list)
            {
                targetManager?.RegisterTargetGuiElement(target.Key, target.Value);
                
            }
            Debug.Log("Target GUI: "+ targetManager?.GetTargetGuiElements().Count);
        }
    }
} 