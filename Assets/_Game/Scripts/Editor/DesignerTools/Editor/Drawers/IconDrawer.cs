using RainbowBlockSaga.Presentation.Scripts.Attributes;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEditor;
using UnityEngine.UIElements;

namespace RainbowBlockSaga.Presentation.Scripts.Editor.Drawers
{
    // Custom attribute

    // Drawer for the custom attribute
    [CustomPropertyDrawer(typeof(IconPreviewAttribute))]
    public class IconDrawer : PropertyDrawer
    {
        private Label m_Icon;
        private ScriptableData m_IconScriptable;
        private SerializedProperty m_property;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_property = property;
            m_Icon = new Label();
            m_Icon.style.width = 200;
            m_Icon.style.height = 200;

            // get parent of the property
            m_IconScriptable = property.serializedObject.targetObject as ScriptableData;
            if (m_IconScriptable != null)
            {
                m_IconScriptable.OnChange += UpdatePreview;
            }

            UpdatePreview();
            return m_Icon;
        }

        private void UpdatePreview()
        {
            EditorApplication.delayCall += () => 
            { 
                var itemTemplate = m_IconScriptable as ItemTemplate;
                if (itemTemplate != null && itemTemplate.HasCustomPrefab())
                {
                    m_Icon.style.backgroundImage = EditorUtils.GetPrefabPreview(itemTemplate.customItemPrefab.gameObject);
                }
                else
                {
                    m_Icon.style.backgroundImage = EditorUtils.GetCanvasPreviewVisualElement(m_IconScriptable.prefab, obj => obj.FillIcon(m_IconScriptable));
                }
            };
        }
    }
}