#if UNITY_EDITOR
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using UnityEditor;
using UnityEngine;

namespace RainbowBlockSaga.Editor
{
    [CustomEditor(typeof(BlockShapeData))]
    public class BlockShapeDataEditor : UnityEditor.Editor
    {
        int gridSize = 5;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var data = (BlockShapeData)target;
            gridSize = EditorGUILayout.IntSlider("Editor Grid", gridSize, 3, 8);
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Shape Editor", EditorStyles.boldLabel);

            for (int y = gridSize - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < gridSize; x++)
                {
                    var coord = new BoardCoord(x, y);
                    bool active = data.Cells.Contains(coord);
                    bool next = GUILayout.Toggle(active, GUIContent.none, "Button", GUILayout.Width(30), GUILayout.Height(30));
                    if (next == active) continue;
                    Undo.RecordObject(data, "Edit Block Shape");
                    if (next) data.Cells.Add(coord); else data.Cells.Remove(coord);
                    EditorUtility.SetDirty(data);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Normalize Coordinates"))
            {
                Undo.RecordObject(data, "Normalize Block Shape");
                data.Cells = new System.Collections.Generic.List<BoardCoord>(data.GetNormalizedCells());
                EditorUtility.SetDirty(data);
            }
        }
    }
}
#endif
