#if UNITY_EDITOR
using RainbowBlockSaga.Modes.Adventure;
using UnityEditor;
using UnityEngine;

namespace RainbowBlockSaga.Editor
{
    [CustomEditor(typeof(AdventureLevelData))]
    public class AdventureLevelDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Adventure Level", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            var level = (AdventureLevelData)target;
            EditorGUILayout.Space(8);
            if (!level.Board || !level.SpawnProfile || !level.ScoreRule || !level.Objective)
                EditorGUILayout.HelpBox("Board, Spawn Profile, Score Rule and Objective are required for an Adventure level.", MessageType.Warning);
        }
    }
}
#endif
