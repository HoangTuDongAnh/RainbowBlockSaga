using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Utils.ImageCreator.Editor
{
    [InitializeOnLoad]
    public class ImageCreator : UnityEditor.Editor
    {
        static ImageCreator()
        {
            EditorApplication.hierarchyChanged += OnChanged;
        }

        private static void OnChanged()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var obj = Selection.activeGameObject;
            if (obj == null || obj.transform.parent == null)
            {
                return;
            }

            if ((obj.transform.parent.GetComponent<CanvasRenderer>() != null || obj.transform.parent.GetComponent<Canvas>() != null || obj.transform.parent.GetComponent<RectTransform>() != null) &&
                obj.GetComponent<SpriteRenderer>() != null)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Convert SpriteRenderer to Image");

                var spriteRenderer = obj.GetComponent<SpriteRenderer>();
                var sprite = spriteRenderer.sprite;
                var color = spriteRenderer.color;

                // Use Undo.AddComponent instead of obj.AddComponent
                var rectTransform = Undo.AddComponent<RectTransform>(obj);
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.localScale = Vector3.one;

                var image = Undo.AddComponent<Image>(obj);
                image.sprite = sprite;
                image.color = color;
                image.SetNativeSize();

                // Use Undo.DestroyObjectImmediate instead of Object.DestroyImmediate
                Undo.DestroyObjectImmediate(spriteRenderer);
            }
        }
    }
}