using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RainbowBlockSaga.EditorTools
{
    public static class ContinueGamePopupPrefabBuilder
    {
        const string SourcePath =
            "Assets/_Game/Resources/Popups/Quit.prefab";

        const string DestinationPath =
            "Assets/_Game/Resources/Popups/ContinueGamePopup.prefab";

        [MenuItem("Rainbow Blocks Saga/Designer/Create Continue Game Popup")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DestinationPath))
            {
                Selection.activeObject =
                    AssetDatabase.LoadAssetAtPath<GameObject>(DestinationPath);
                Debug.Log("ContinueGamePopup already exists.");
                return;
            }

            if (!AssetDatabase.CopyAsset(SourcePath, DestinationPath))
            {
                Debug.LogError(
                    "Could not copy Quit.prefab. Source: " + SourcePath);
                return;
            }

            AssetDatabase.Refresh();

            var root = PrefabUtility.LoadPrefabContents(DestinationPath);
            var quit = root.GetComponent<Quit>();

            if (quit == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                AssetDatabase.DeleteAsset(DestinationPath);
                Debug.LogError("Quit component was not found on copied prefab.");
                return;
            }

            var newGameButton = quit.yes;
            var continueButton = quit.closeButton;

            string json = EditorJsonUtility.ToJson(quit);
            Object.DestroyImmediate(quit, true);

            var popup = root.AddComponent<ContinueGamePopup>();
            EditorJsonUtility.FromJsonOverwrite(json, popup);

            var serialized = new SerializedObject(popup);
            serialized.FindProperty("newGameButton").objectReferenceValue =
                newGameButton;
            serialized.FindProperty("continueButton").objectReferenceValue =
                continueButton;

            // These two buttons are now explicit choices, not a generic close button.
            serialized.FindProperty("closeButton").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            root.name = "ContinueGamePopup";
            newGameButton.gameObject.name = "NewGameButton";
            continueButton.gameObject.name = "ContinueButton";

            SetButtonText(newGameButton, "NEW GAME");
            SetButtonText(continueButton, "CONTINUE");

            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string value = label.text.ToUpperInvariant();
                if (value.Contains("QUIT") || value.Contains("EXIT"))
                {
                    label.text = "CONTINUE GAME?";
                    break;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, DestinationPath);
            PrefabUtility.UnloadPrefabContents(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject =
                AssetDatabase.LoadAssetAtPath<GameObject>(DestinationPath);

            Debug.Log(
                "Created ContinueGamePopup from Quit.prefab. " +
                "You can now adjust visuals/text without changing popup structure.");
        }

        static void SetButtonText(CustomButton button, string value)
        {
            if (button == null)
                return;

            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
                label.text = value;
        }
    }
}
