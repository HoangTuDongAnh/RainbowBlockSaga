using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData.Editor
{
    public class LevelSwitcher : VisualElement
    {
        public LevelSwitcher(SerializedObject serialized, Level level, LevelEditor editor)
        {
            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.Add(new Button(() => { editor.Save(); LevelPlayPreview.Play(level); }) { text = "Play level" });
            actions.Add(new Button(editor.Save) { text = "Save" });
            Add(actions);
            var navigation = new VisualElement();
            navigation.style.flexDirection = FlexDirection.Row;
            navigation.Add(new Button(() => Select(ArcadeLevelCatalog.LoadAll().LastOrDefault(l => l.Number < level.Number))) { text = "<<" });
            var number = new IntegerField { value = level.Number };
            number.style.width = 70;
            number.RegisterCallback<KeyDownEvent>(e => { if (e.keyCode == KeyCode.Return) Select(ArcadeLevelCatalog.Find(number.value)); });
            navigation.Add(number);
            navigation.Add(new Button(() => Select(ArcadeLevelCatalog.Next(level.Number))) { text = ">>" });
            navigation.Add(new Button(() => Create(level)) { text = "New" });
            var delete = new Button(() => DeleteLast(level)) { text = "Delete last" };
            delete.SetEnabled(!EditorApplication.isPlaying && ArcadeLevelCatalog.LoadAll().Length > 1 && ArcadeLevelCatalog.LoadAll().LastOrDefault() == level);
            navigation.Add(delete);
            Add(navigation);
        }
        static void Select(Level level) { if (level != null) Selection.activeObject = level; }
        static void Create(Level source)
        {
            if (EditorApplication.isPlaying || source.levelType == null) return;
            var levels = ArcadeLevelCatalog.LoadAll();
            var id = levels.Length == 0 ? 1 : levels.Last().Number + 1;
            var level = ScriptableObject.CreateInstance<Level>();
            level.name = "Level_" + id;
            level.Resize(source.rows, source.columns);
            level.levelType = source.levelType;
            level.UpdateTargets();
            AssetDatabase.CreateAsset(level, "Assets/_Game/Resources/Levels/" + level.name + ".asset");
            AssetDatabase.SaveAssets();
            Select(level);
        }
        static void DeleteLast(Level level)
        {
            var levels = ArcadeLevelCatalog.LoadAll();
            if (EditorApplication.isPlaying || levels.Length <= 1 || levels.Last() != level) return;
            if (!EditorUtility.DisplayDialog("Delete level", "Delete " + level.name + "? Its asset and meta will be removed.", "Delete", "Cancel")) return;
            var path = AssetDatabase.GetAssetPath(level);
            Select(levels[levels.Length - 2]);
            AssetDatabase.DeleteAsset(path);
        }
    }

    // SessionState keeps the selected asset across domain reloads.
    [InitializeOnLoad]
    internal static class LevelPlayPreview
    {
        const string Key = "RBS.LevelPreview";
        static LevelPlayPreview() { EditorApplication.playModeStateChanged += OnPlayMode; }
        internal static void Play(Level level)
        {
            if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
            if (level == null || level.levelType == null || level.levelType.stateHandler == null)
            { Debug.LogError("Set Level Type and its State Handler before playing."); return; }
            if (StateManager.instance == null)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene("Assets/_Game/Scenes/Gameplay.unity");
            }
            SessionState.SetString(Key, AssetDatabase.GetAssetPath(level));
            SessionState.SetInt(Key + ".mode", PlayerPrefs.GetInt("GameMode", 0));
            SessionState.SetBool(Key + ".hadMode", PlayerPrefs.HasKey("GameMode"));
            Prepare();
            EditorApplication.isPlaying = true;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Prepare()
        {
            var path = SessionState.GetString(Key, "");
            if (string.IsNullOrEmpty(path)) return;
            var level = AssetDatabase.LoadAssetAtPath<Level>(path);
            if (level == null) return;
            GameDataManager.SetGameMode(level.levelType.elevelType == ELevelType.Classic ? EGameMode.Classic : EGameMode.Adventure);
            GameDataManager.SetLevel(level);
            GameDataManager.isTestPlay = true;
        }
        static void OnPlayMode(PlayModeStateChange state)
        {
            if (string.IsNullOrEmpty(SessionState.GetString(Key, ""))) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Prepare();
                GameManager.instance.SetTutorialMode(false);
                StateManager.instance.CurrentState = EScreenStates.Game;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Key + ".hadMode", false))
                    PlayerPrefs.SetInt("GameMode", SessionState.GetInt(Key + ".mode", 0));
                else PlayerPrefs.DeleteKey("GameMode");
                PlayerPrefs.Save();
                GameDataManager.SetLevel(null);
                GameDataManager.isTestPlay = false;
                SessionState.EraseString(Key);
            }
        }
    }
}
