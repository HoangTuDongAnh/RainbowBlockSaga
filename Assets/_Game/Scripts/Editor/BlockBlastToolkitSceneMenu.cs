#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BlockBlastToolkitSceneMenu
{
    private const string ScenePath = "Assets/BlockPuzzleGameToolkit/Scenes/BlockBlastPuzzle.unity";

    [MenuItem("Rainbow Block Saga/Toolkit/Open Original BlockBlastPuzzle")]
    public static void OpenOriginalBlockBlastPuzzle()
    {
        EditorSceneManager.OpenScene(ScenePath);
    }
}
#endif
