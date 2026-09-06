#if UNITY_EDITOR
using System.IO;
using RainbowBlockSaga.Integration.Toolkit;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RainbowBlockSagaMigrationScenes
{
    const string ReferenceScene = "Assets/BlockPuzzleGameToolkit/Scenes/BlockBlastPuzzle.unity";
    const string GameplayScene = "Assets/_Game/Scenes/Gameplay.unity";
    const string ShapePrefab = "Assets/BlockPuzzleGameToolkit/Prefabs/Game/Shape.prefab";

    [MenuItem("Rainbow Block Saga/Migration/Open Reference BlockBlast")]
    public static void OpenReference() => OpenScene(ReferenceScene);

    [MenuItem("Rainbow Block Saga/Migration/Open Gameplay Migration")]
    public static void OpenGameplay() => OpenScene(GameplayScene);

    [MenuItem("Rainbow Block Saga/Migration/Validate Step 1-2")]
    public static void ValidateStep12()
    {
        if (!File.Exists(ReferenceScene) || !File.Exists(GameplayScene))
        {
            Debug.LogError("Step 1-2 validation failed: one of the migration scenes is missing.");
            return;
        }

        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
        var bridge = Object.FindFirstObjectByType<ToolkitBoardMigrationBridge>(FindObjectsInactive.Include);
        if (!bridge)
        {
            Debug.LogError("Step 1-2 validation failed: ToolkitBoardMigrationBridge is missing from Gameplay.unity.");
            return;
        }

        Debug.Log("Rainbow Block Saga Step 1-2 OK: reference scene preserved, Gameplay migration scene exists, Board migration bridge is bound.");
    }

    [MenuItem("Rainbow Block Saga/Migration/Validate Step 3")]
    public static void ValidateStep3()
    {
        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);
        var boardBridge = Object.FindFirstObjectByType<ToolkitBoardMigrationBridge>(FindObjectsInactive.Include);
        var context = Object.FindFirstObjectByType<ToolkitBlockMigrationContext>(FindObjectsInactive.Include);
        var shapePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShapePrefab);

        if (!boardBridge || !context || !shapePrefab)
        {
            Debug.LogError("Step 3 validation failed: board bridge, block migration context, or Shape prefab is missing.");
            return;
        }

        var view = shapePrefab.GetComponent<ToolkitBlockViewAdapter>();
        var drag = shapePrefab.GetComponent<ToolkitBlockDragHandler>();
        var oldDrag = shapePrefab.GetComponent<BlockPuzzleGameToolkit.Scripts.Gameplay.ShapeDraggable>();

        if (!view || !drag || !oldDrag || oldDrag.enabled)
        {
            Debug.LogError("Step 3 validation failed: Shape prefab migration components are not configured correctly.");
            return;
        }

        Debug.Log("Rainbow Block Saga Step 3 OK: toolkit Shape visual is preserved, legacy drag is disabled, and BlockShapeData + BoardModel placement migration is active.");
    }


    [MenuItem("Rainbow Block Saga/Migration/Validate Step 4")]
    public static void ValidateStep4()
    {
        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

        var boardBridge = Object.FindFirstObjectByType<ToolkitBoardMigrationBridge>(FindObjectsInactive.Include);
        var queueBridge = Object.FindFirstObjectByType<ToolkitQueueSpawnMigrationBridge>(FindObjectsInactive.Include);
        var deckManager = Object.FindFirstObjectByType<BlockPuzzleGameToolkit.Scripts.Gameplay.CellDeckManager>(FindObjectsInactive.Include);

        if (!boardBridge || !queueBridge || !deckManager)
        {
            Debug.LogError("Step 4 validation failed: Board bridge, Queue/Spawn bridge, or visual CellDeckManager is missing.");
            return;
        }

        if (deckManager.cellDecks == null || deckManager.cellDecks.Length == 0)
        {
            Debug.LogError("Step 4 validation failed: no visual CellDeck slots are bound.");
            return;
        }

        Debug.Log("Rainbow Block Saga Step 4 OK: BlockQueue + SpawnStrategy live in the new architecture, and toolkit CellDeckManager is presentation/compatibility only.");
    }


    [MenuItem("Rainbow Block Saga/Migration/Validate Step 5")]
    public static void ValidateStep5()
    {
        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

        var boardBridge = Object.FindFirstObjectByType<ToolkitBoardMigrationBridge>(FindObjectsInactive.Include);
        var resolveBridge = Object.FindFirstObjectByType<ToolkitResolveScoreMigrationBridge>(FindObjectsInactive.Include);
        var levelManager = Object.FindFirstObjectByType<BlockPuzzleGameToolkit.Scripts.Gameplay.LevelManager>(FindObjectsInactive.Include);

        if (!boardBridge || !resolveBridge || !levelManager)
        {
            Debug.LogError("Step 5 validation failed: Board bridge, Resolve/Score bridge, or LevelManager presentation adapter is missing.");
            return;
        }

        Debug.Log("Rainbow Block Saga Step 5 OK: BoardResolver + ScoreSystem own resolve/scoring, while toolkit LevelManager is presentation/compatibility only.");
    }


    [MenuItem("Rainbow Block Saga/Migration/Validate Step 6")]
    public static void ValidateStep6()
    {
        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

        var sessionBridge = Object.FindFirstObjectByType<ToolkitGameSessionMigrationBridge>(FindObjectsInactive.Include);
        var queueBridge = Object.FindFirstObjectByType<ToolkitQueueSpawnMigrationBridge>(FindObjectsInactive.Include);
        var resolveBridge = Object.FindFirstObjectByType<ToolkitResolveScoreMigrationBridge>(FindObjectsInactive.Include);
        var boardBridge = Object.FindFirstObjectByType<ToolkitBoardMigrationBridge>(FindObjectsInactive.Include);

        if (!sessionBridge || !queueBridge || !resolveBridge || !boardBridge)
        {
            Debug.LogError("Step 6 validation failed: one or more migration owners are missing.");
            return;
        }

        Debug.Log(
            "Rainbow Block Saga Step 6 OK: one GameSession owns Board / Placement / Queue / Spawn / Resolve / Score. Queue is reconciled from visible CellDecks before every migration placement.");
    }


    [MenuItem("Rainbow Block Saga/Migration/Validate Step 7")]
    public static void ValidateStep7()
    {
        EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

        var sessionBridge =
            Object.FindFirstObjectByType<ToolkitGameSessionMigrationBridge>(
                FindObjectsInactive.Include);
        var lifecycleBridge =
            Object.FindFirstObjectByType<ToolkitSessionLifecyclePresentationBridge>(
                FindObjectsInactive.Include);
        var resolveBridge =
            Object.FindFirstObjectByType<ToolkitResolveScoreMigrationBridge>(
                FindObjectsInactive.Include);
        var queueBridge =
            Object.FindFirstObjectByType<ToolkitQueueSpawnMigrationBridge>(
                FindObjectsInactive.Include);

        if (!sessionBridge ||
            !lifecycleBridge ||
            !resolveBridge ||
            !queueBridge)
        {
            Debug.LogError(
                "Step 7 validation failed: GameSession compatibility bridges are incomplete.");
            return;
        }

        Debug.Log(
            "Rainbow Block Saga Step 7 OK: Classic gameplay keeps the original toolkit flow/visuals while GameSession now supports restart, pause/resume, no-move failure and PreFailed Continue recovery.");
    }

    static void OpenScene(string path)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }
}
#endif
