using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Popups.Reward;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace RainbowBlockSaga.Presentation.Scripts.Editor.GUI
{
    public class CustomButtonEditor
    {
        public VisualElement CreateInspectorGUI(SerializedObject serializedObject)
        {
            var root = new VisualElement();

            root.Add(new VisualElement { name = "space", style = { height = 10 } });

            // Add the Interactable field
            root.Add(GetBindPropertyField(serializedObject, "m_Interactable", "Interactable"));

            // Add the Transition field
            var transition = GetPropertyField(serializedObject, "m_Transition", "Transition");
            transition.Bind(serializedObject);
            root.Add(transition);
            var onClickProperty = GetBindPropertyField(serializedObject, "m_OnClick", "On Click");
            var spriteProperty = GetPropertyField(serializedObject, "m_SpriteState", "Sprites");
            var colorProperty = GetPropertyField(serializedObject, "m_Colors", "Colors");
            var animations = GetPropertyField(serializedObject, "m_AnimationTriggers", "Animations");
            transition.RegisterValueChangeCallback(evt =>
            {
                Transition(serializedObject, root, spriteProperty, animations, colorProperty);
                root.Remove(onClickProperty);
                root.Add(onClickProperty);
            });
            Transition(serializedObject, root, spriteProperty, animations, colorProperty);

            root.Add(onClickProperty);

            colorProperty.Bind(serializedObject);
            spriteProperty.Bind(serializedObject);
            animations.Bind(serializedObject);
            return root;
        }

        private static void Transition(SerializedObject serializedObject, VisualElement root, PropertyField spriteProperty, PropertyField animations, PropertyField colorProperty)
        {
            var transitionProperty = serializedObject.FindProperty("m_Transition");
            // Add transition details based on the selected type
            if (transitionProperty.enumValueIndex == (int)Selectable.Transition.ColorTint)
            {
                if (root.Contains(spriteProperty))
                {
                    root.Remove(spriteProperty);
                }

                if (root.Contains(animations))
                {
                    root.Remove(animations);
                }

                root.Add(colorProperty);
            }
            else if (transitionProperty.enumValueIndex == (int)Selectable.Transition.SpriteSwap)
            {
                if (root.Contains(colorProperty))
                {
                    root.Remove(colorProperty);
                }

                if (root.Contains(animations))
                {
                    root.Remove(animations);
                }

                root.Add(spriteProperty);
            }
            else if (transitionProperty.enumValueIndex == (int)Selectable.Transition.Animation)
            {
                if (root.Contains(colorProperty))
                {
                    root.Remove(colorProperty);
                }

                if (root.Contains(spriteProperty))
                {
                    root.Remove(spriteProperty);
                }

                root.Add(animations);
            }
            else if (transitionProperty.enumValueIndex == (int)Selectable.Transition.None)
            {
                if (root.Contains(colorProperty))
                {
                    root.Remove(colorProperty);
                }

                if (root.Contains(spriteProperty))
                {
                    root.Remove(spriteProperty);
                }

                if (root.Contains(animations))
                {
                    root.Remove(animations);
                }
            }
        }

        private VisualElement GetBindPropertyField(SerializedObject serializedObject, string propertyName, string label)
        {
            var propertyField = GetPropertyField(serializedObject, propertyName, label);
            propertyField.Bind(serializedObject);
            return propertyField;
        }

        private PropertyField GetPropertyField(SerializedObject serializedObject, string propertyName, string label)
        {
            return new PropertyField(serializedObject.FindProperty(propertyName), label);
        }
    }

    [CustomEditor(typeof(RewardedButton))]
    internal class RewardedButtonEditor : UnityEditor.Editor
    {
        private CustomButtonEditor customButtonEditor;

        private void OnEnable()
        {
            customButtonEditor = new CustomButtonEditor();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Add a property field for placement
            var placementField = new PropertyField(serializedObject.FindProperty("placement"), "Placement");
            root.Add(placementField);
            root.Add(customButtonEditor.CreateInspectorGUI(serializedObject));
            return root;
        }
    }

    [CustomEditor(typeof(CustomButton))]
    internal class CustomButtonDrawer : UnityEditor.Editor
    {
        private CustomButtonEditor customButtonEditor;

        private void OnEnable()
        {
            customButtonEditor = new CustomButtonEditor();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Add a property field for placement
            root.Add(new PropertyField(serializedObject.FindProperty("overrideClickSound")));
            root.Add(new PropertyField(serializedObject.FindProperty("overrideAnimatorController")));
            root.Add(customButtonEditor.CreateInspectorGUI(serializedObject));
            return root;
        }
    }
}