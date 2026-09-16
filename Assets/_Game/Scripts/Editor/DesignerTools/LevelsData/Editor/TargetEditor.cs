using UnityEditor;
using UnityEngine.UIElements;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData.Editor
{
    [CanEditMultipleObjects]
    // [CustomEditor(typeof(Target), true)]
    public class TargetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // var target = (Target)serializedObject.targetObject;
            //
            var root = new VisualElement();
            // root.style.flexDirection = FlexDirection.Column;
            //
            // var amountField = new IntegerField("Amount")
            // {
            //     value = target.amount
            // };
            // amountField.RegisterValueChangedCallback(evt =>
            // {
            //     target.amount = evt.newValue;
            //     MarkDirtyAndSave(target);
            // });
            //
            // root.Add(amountField);


            return root;
        }

        private void MarkDirtyAndSave(TargetScriptable targetScriptable)
        {
            EditorUtility.SetDirty(targetScriptable);
            AssetDatabase.SaveAssets();
        }
    }
}