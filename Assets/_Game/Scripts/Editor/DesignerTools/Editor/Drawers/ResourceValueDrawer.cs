using RainbowBlockSaga.Presentation.Scripts.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace RainbowBlockSaga.Presentation.Scripts.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ResourceValue))]
    public class ResourceValueDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.Add(new Label("Resource Value: " + ((ResourceObject)property.serializedObject.targetObject).LoadResource()));

            return root;
        }
    }
}