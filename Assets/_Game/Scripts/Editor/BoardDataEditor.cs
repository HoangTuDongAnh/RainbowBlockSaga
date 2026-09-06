#if UNITY_EDITOR
using RainbowBlockSaga.Gameplay.Board;
using UnityEditor;
using UnityEngine;

namespace RainbowBlockSaga.Editor
{
    [CustomEditor(typeof(BoardData))]
    public class BoardDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var data = (BoardData)target;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Playable Mask", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("If PlayableCells is empty, the whole rectangular board is playable. Use Enable Mask to author irregular boards.", MessageType.Info);

            bool masked = data.PlayableCells.Count > 0;
            if (GUILayout.Button(masked ? "Disable Mask (Full Board)" : "Enable Mask (Full Board)"))
            {
                Undo.RecordObject(data, "Toggle Board Mask");
                data.PlayableCells.Clear();
                if (!masked)
                    for (int y = 0; y < data.Height; y++)
                        for (int x = 0; x < data.Width; x++)
                            data.PlayableCells.Add(new BoardCoord(x, y));
                EditorUtility.SetDirty(data);
            }

            if (data.PlayableCells.Count == 0) return;
            for (int y = data.Height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < data.Width; x++)
                {
                    var coord = new BoardCoord(x, y);
                    bool active = data.PlayableCells.Contains(coord);
                    bool next = GUILayout.Toggle(active, GUIContent.none, "Button", GUILayout.Width(28), GUILayout.Height(28));
                    if (next == active) continue;
                    Undo.RecordObject(data, "Edit Board Cell");
                    if (next) data.PlayableCells.Add(coord); else data.PlayableCells.Remove(coord);
                    EditorUtility.SetDirty(data);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif
