using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.FX;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.Pool;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using RainbowBlockSaga.Presentation.Contracts;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public partial class LevelManager : MonoBehaviour, IResolvePresentation, IGameEndPresentation
    {
        public int currentLevel;
        public LineExplosion lineExplosionPrefab;
        public ComboText comboTextPrefab;
        public Transform pool;
        public Transform fxPool;

        public int comboCounter;
        private int missCounter;
        private EndlessClearFeedback endlessFeedback;

        [SerializeField]
        private RectTransform gameCanvas;

        [SerializeField]
        private RectTransform shakeCanvas;

        [SerializeField]
        private GameObject scorePrefab;

        [SerializeField]
        private GameObject[] words;

        [SerializeField]
        private TutorialManager tutorialManager;

        [SerializeField]
        private GameObject timerPanel;

        public EGameMode gameMode;
        public Level _levelData;

        private Cell[] emptyCells;

        public UnityEvent<Level> OnLevelLoaded;
        public Action<int> OnScored;
        public Action OnLose;

        public static bool ExternalResolveEnabled { get; set; }
        public static bool ExternalEndlessLifecycleEnabled { get; set; }

        private FieldManager field;
        public CellDeckManager cellDeck;
        private ItemFactory itemFactory;
        private TargetManager targetManager;

        private ObjectPool<ComboText> comboTextPool;
        private ObjectPool<LineExplosion> lineExplosionPool;
        private ObjectPool<ScoreText> scoreTextPool;
        private ObjectPool<GameObject> wordsPool;
        private EndlessModeHandler endlessModeHandler;
        private TimedModeHandler timedModeHandler;
        public TimerManager timerManager;
        private int timerDuration;
        
        private Vector3 cachedFieldCenter;
        private bool isFieldCenterCached;

        private void OnEnable()
        {
            StateManager.instance.CurrentState = EScreenStates.Game;
            EventManager.GetEvent(EGameEvent.RestartLevel).Subscribe(RestartLevel);
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(CheckLines);
            EventManager.OnGameStateChanged += HandleGameStateChange;
            targetManager = FindObjectOfType<TargetManager>();
            itemFactory = FindObjectOfType<ItemFactory>();
            cellDeck = FindObjectOfType<CellDeckManager>();
            field = FindObjectOfType<FieldManager>();
            // TimerManager is serialized on the gameplay scene.
            timerManager = GetComponent<TimerManager>();
            if (timerManager != null)
            {
                timerManager.OnTimerExpired += OnTimerExpired;
            }

            comboTextPool = new ObjectPool<ComboText>(
                () => Instantiate(comboTextPrefab, fxPool),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            lineExplosionPool = new ObjectPool<LineExplosion>(
                () => Instantiate(lineExplosionPrefab, pool),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            scoreTextPool = new ObjectPool<ScoreText>(
                () => Instantiate(scorePrefab, fxPool).GetComponent<ScoreText>(),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            wordsPool = new ObjectPool<GameObject>(
                () => Instantiate(words[Random.Range(0, words.Length)], fxPool),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                Destroy
            );
            RestartLevel();
            RestoreRun();
        }

        private void RestartLevel()
        {
            CancelInvoke(nameof(StartGame));
            StopAllCoroutines();
            if (endlessFeedback) endlessFeedback.Clear();
            comboCounter = 0;
            missCounter = 0;
            field.ShowOutline(false);
            Load();
        }

        private void OnDisable()
        {
            runReady = false;
            CancelInvoke();
            StopAllCoroutines();
            if (endlessFeedback) endlessFeedback.Clear();
            EventManager.GetEvent(EGameEvent.RestartLevel).Unsubscribe(RestartLevel);
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(CheckLines);
            EventManager.OnGameStateChanged -= HandleGameStateChange;

            // Unsubscribe from timer events
            if (timerManager != null)
            {
                timerManager.OnTimerExpired -= OnTimerExpired;
            }
        }

        private void OnTimerExpired()
        {
            if (EventManager.GameStatus != EGameState.Playing)
                return;
            // Check if level is complete before triggering a loss
            if (targetManager != null && targetManager.IsLevelComplete())
            {
                // Level complete, trigger win
                SetWin();
            }
            else
            {
                // Level not complete, trigger loss
                SetLose();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveRun();
            PauseTimer(pauseStatus);
        }

        private void Load()
        {
            gameMode = GameDataManager.GetGameMode();
            if (GameManager.instance.IsTutorialMode())
            {
                _levelData = tutorialManager.GetLevelForPhase();
            }
            else
            {
                gameMode = GameDataManager.GetGameMode();
                _levelData = GameDataManager.GetLevel();
            }
            if(_levelData == null || _levelData.levelType == null)
            {
                Debug.LogError("Level data is null");
                return;
            }
            currentLevel = gameMode == EGameMode.Adventure ? _levelData.Number : 0;
            BeginRun();

            // Apply global time settings if timed mode is enabled
            if (_levelData.enableTimer)
            {
                timerDuration = _levelData.timerDuration;
                if(_levelData.timerDuration <= 0)
                    timerDuration = Mathf.Max(1, GameManager.instance.GameSettings.globalTimedModeSeconds);
            }

            FindObjectsOfType<MonoBehaviour>().OfType<IBeforeLevelLoadable>().ToList().ForEach(x => x.OnLevelLoaded(_levelData));
            LoadLevel(_levelData);
            FindObjectsOfType<MonoBehaviour>().OfType<ILevelLoadable>().ToList().ForEach(x => x.OnLevelLoaded(_levelData));
            Invoke(nameof(StartGame), 0.5f);
            if (GameManager.instance.IsTutorialMode())
            {
                tutorialManager.StartTutorial();
            }

            // Initialize timer if enabled for this level or if global timed mode is enabled
            if (_levelData.enableTimer && timerManager != null)
            {
                timerManager.InitializeTimer(timerDuration);
                if (timerPanel != null)
                {
                    timerPanel.SetActive(true);
                }
            }
            else if (timerManager != null)
            {
                timerManager.StopTimer();
                if (timerPanel != null)
                {
                    timerPanel.SetActive(false);
                }
            }
        }

        private void StartGame()
        {
            runReady = true;
            cellDeck.FillCellDecks();
            SaveRun();
            EventManager.GameStatus = EGameState.PrepareGame;
            endlessModeHandler = FindObjectOfType<EndlessModeHandler>();
            if (gameMode == EGameMode.Endless && endlessModeHandler != null) endlessModeHandler.UpdateScore(RunScore);
        }

        private void LoadLevel(Level levelData)
        {
            field.Generate(levelData);
            // Reset field center cache when loading new level
            isFieldCenterCached = false;
            EventManager.GetEvent<Level>(EGameEvent.LevelLoaded).Invoke(levelData);
            OnLevelLoaded?.Invoke(levelData);
        }

        private void  CheckLines(Shape obj)
        {
            if (ExternalResolveEnabled)
                return;

            int blockScore = obj.GetActiveItems().Count * GameManager.instance.GameSettings.ScorePerCell;

            var lines = field.GetFilledLines(false, false);
            if (lines.Count > 0)
            {
                missCounter = 0;
                comboCounter++;
                ShakeOnClear();
                StartCoroutine(AfterMoveProcessing(obj, lines, blockScore));
                if (comboCounter > 1)
                {
                    field.ShowOutline(true);
                }
            }
            else
            {
                AwardPoints(blockScore);
                missCounter++;
                if (missCounter >= (IsEndlessMode ? GameManager.instance.GameSettings.endlessScoring.ResetAfterMisses : GameManager.instance.GameSettings.ResetComboAfterMoves))
                {
                    field.ShowOutline(false);
                    missCounter = 0;
                    comboCounter = 0;
                }

                StartCoroutine(CheckLose());
            }
        }


        public bool IsEndlessMode =>
            gameMode == EGameMode.Endless;

        public void SetRuntimeResolveOwnership(bool enabled)
        {
            ExternalResolveEnabled = enabled;
        }

        public void SetRuntimeLifecycleOwnership(bool enabled)
        {
            ExternalEndlessLifecycleEnabled = enabled;
        }

        public void PresentResolve(
            UnityEngine.Object shapeHandle,
            IReadOnlyList<IReadOnlyList<UnityEngine.Object>> lineHandles,
            int scoreGain,
            int combo,
            ResolveScoreFeedback feedback,
            global::System.Action completed)
        {
            if (shapeHandle is not Shape shape)
            {
                completed?.Invoke();
                return;
            }

            var lines = new List<List<Cell>>();

            if (lineHandles != null)
            {
                foreach (var sourceLine in lineHandles)
                {
                    var line = new List<Cell>();

                    if (sourceLine != null)
                    {
                        foreach (var handle in sourceLine)
                        {
                            if (handle is Cell cell)
                                line.Add(cell);
                        }
                    }

                    if (line.Count > 0)
                        lines.Add(line);
                }
            }

            PresentExternalResolve(
                shape,
                lines,
                scoreGain,
                combo,
                completed, feedback);
        }

        public void PresentNoValidMoves()
        {
            PresentExternalLose();
        }

        public void PresentExternalResolve(
            Shape shape,
            List<List<Cell>> lines,
            int scoreGain,
            int externalComboCounter,
            Action completed, ResolveScoreFeedback feedback = default)
        {
            CommitTurn(shape, lines, scoreGain, externalComboCounter);
            AwardPoints(scoreGain);
            if (gameMode == EGameMode.Adventure && lines != null && lines.Count > 0)
                StartCoroutine(targetManager.AnimateTarget(lines));

            if (lines != null && lines.Count > 0)
            {
                ShakeOnClear();

                if (comboCounter > 1)
                    field.ShowOutline(true);
                else
                    field.ShowOutline(false);

                StartCoroutine(AfterExternalMoveProcessing(
                    shape,
                    lines,
                    scoreGain,
                    completed, feedback));
                return;
            }

            if (comboCounter == 0)
                field.ShowOutline(false);

            completed?.Invoke();

            if (EventManager.GameStatus == EGameState.Playing)
                StartCoroutine(CheckLose());
        }

        private IEnumerator AfterExternalMoveProcessing(
            Shape shape,
            List<List<Cell>> lines,
            int scoreGain,
            Action completed, ResolveScoreFeedback feedback = default)
        {
            Vector3 center = GetFieldCenter();
            Vector3 scorePosition = center + new Vector3(0, 0.75f, 0);
            Vector3 gratzPosition = center + new Vector3(0, 0.35f, 0);

            yield return new WaitForSeconds(0.1f);

            yield return StartCoroutine(DestroyLines(lines, shape, feedback.RainbowTriggered));

            if (feedback.IsEndless)
            {
                if (!endlessFeedback) endlessFeedback = gameObject.AddComponent<EndlessClearFeedback>();
                var font = scorePrefab.GetComponentInChildren<TMP_Text>(true)?.font;
                yield return endlessFeedback.Play(gameCanvas, center, font, feedback, GameManager.instance.GameSettings.endlessScoring);
                completed?.Invoke();
                if (EventManager.GameStatus == EGameState.Playing) yield return StartCoroutine(CheckLose());
                yield break;
            }
            if (comboCounter > 1)
            {
                ShowComboText(comboCounter);
                yield return new WaitForSeconds(0.5f);
            }

            if (scoreGain > 0)
            {
                var scoreText = scoreTextPool.Get();
                scoreText.transform.position = scorePosition;
                scoreText.ShowScore(scoreGain, scorePosition);
                DOVirtual.DelayedCall(
                    0.75f,
                    () => scoreTextPool.Release(scoreText));
            }

            if (Random.Range(0, 3) == 0)
            {
                var txt = wordsPool.Get();
                txt.transform.position = gratzPosition;

                var canvasCorners = new Vector3[4];
                gameCanvas.GetWorldCorners(canvasCorners);

                var txtPosition = txt.transform.position;
                txtPosition.x = Mathf.Clamp(
                    txtPosition.x,
                    canvasCorners[0].x,
                    canvasCorners[2].x);
                txtPosition.y = Mathf.Clamp(
                    txtPosition.y,
                    canvasCorners[0].y,
                    canvasCorners[2].y);
                txt.transform.position = txtPosition;

                DOVirtual.DelayedCall(
                    1.5f,
                    () => wordsPool.Release(txt));
            }

            completed?.Invoke();

            if (EventManager.GameStatus == EGameState.Playing)
                yield return StartCoroutine(CheckLose());
        }

        private Vector3 GetFieldCenter()
        {
            if (isFieldCenterCached)
                return cachedFieldCenter;

            Vector3 fieldCenter = Vector3.zero;
            int rowCount = field.cells.GetLength(0);
            int colCount = field.cells.GetLength(1);
            
            if (rowCount > 0 && colCount > 0)
            {
                Cell centerCell = field.cells[rowCount/2, colCount/2];
                if (centerCell != null)
                {
                    fieldCenter = centerCell.transform.position;
                }
            }
            
            cachedFieldCenter = fieldCenter;
            isFieldCenterCached = true;
            return fieldCenter;
        }

        private void ShakeOnClear()
        {
            var settings = GameManager.instance.GameSettings.endlessScoring;
            float strength = !IsEndlessMode ? 35f : comboCounter >= settings.StrongFeedbackCombo ? 20f
                : comboCounter >= settings.SmallFeedbackCombo ? 8f : 0f;
            if (strength > 0) shakeCanvas.DOShakePosition(.2f, strength, 30);
        }

        private void ShowComboText(int comboCount)
        {
            Vector3 center = GetFieldCenter();
            Vector3 comboPosition = center + new Vector3(0, 0.75f, 0); // Same height as score text
            var comboText = comboTextPool.Get();
            comboText.transform.position = comboPosition;
            comboText.Show(comboCount);
            DOVirtual.DelayedCall(0.75f, () => { comboTextPool.Release(comboText); }); // Adjusted to match faster animation
        }

        private IEnumerator AfterMoveProcessing(Shape shape, List<List<Cell>> lines, int blockScore)
        {
            int cellCount = lines.SelectMany(line => line).Distinct().Count();
            int lineScore = GameManager.instance.GameSettings.ScorePerCell * cellCount;
            if (IsEndlessMode) lineScore = Mathf.RoundToInt(lineScore * GameManager.instance.GameSettings.endlessScoring.LineMultiplier(lines.Count));
            var settings = GameManager.instance.GameSettings.endlessScoring;
            bool fullClear = cellCount > 0 && field.GetAllCells().Cast<Cell>().Count(c => !c.IsEmpty() && !c.IsDisabled()) == cellCount;
            bool rainbow = IsEndlessMode && (comboCounter == Mathf.Max(1, settings.RainbowCombo) || fullClear);
            var feedback = new ResolveScoreFeedback(IsEndlessMode, blockScore, lineScore,
                IsEndlessMode ? settings.Multiplier(comboCounter) : Mathf.Max(1,comboCounter),
                rainbow ? settings.RainbowBonus : 0, comboCounter, fullClear, rainbow);
            yield return AfterExternalMoveProcessing(shape, lines, feedback.Total, null, feedback);
        }
        private void AwardPoints(int gain)
        {
            if (gain <= 0) return;
            OnScored?.Invoke(gain);
            if (gameMode == EGameMode.Adventure)
                targetManager.UpdateScoreTarget(gain);
        }
        private IEnumerator CheckLose()
        {
            // Runtime owns the Endless lifecycle, but the visible board/tray are
            // the final presentation truth. Keep this as a safety net so a
            // runtime/presentation mismatch can never leave Endless stuck.
            if (gameMode == EGameMode.Endless &&
                ExternalEndlessLifecycleEnabled)
            {
                yield return new WaitForSeconds(0.5f);

                if (EventManager.GameStatus != EGameState.Playing)
                    yield break;

                if (!HasAnyPresentationMove())
                    PresentExternalLose();

                yield break;
            }

            if (gameMode != EGameMode.Endless &&
                targetManager != null &&
                targetManager.WillLevelBeComplete())
            {
                EventManager.GameStatus = EGameState.WinWaiting;
            }

            yield return new WaitForSeconds(0.5f);

            bool lose = !HasAnyPresentationMove();
            if (EventManager.GameStatus != EGameState.Playing && EventManager.GameStatus != EGameState.WinWaiting)
                yield break;
            if (EventManager.GameStatus != EGameState.Playing && EventManager.GameStatus != EGameState.WinWaiting)
                yield break;

            if (gameMode != EGameMode.Endless &&
                targetManager != null &&
                targetManager.WillLevelBeComplete())
            {
                yield return new WaitForSeconds(0.5f);
                SetWin();
                lose = false;
            }

            lose |= OutOfMoves;
            if (targetManager.IsLevelComplete() && gameMode == EGameMode.Adventure) lose = false;
            if (lose &&
                EventManager.GameStatus != EGameState.PreFailed &&
                EventManager.GameStatus != EGameState.Failed)
            {
                SetLose();
            }
        }

        private bool HasAnyPresentationMove()
        {
            if (field == null || cellDeck == null)
                return true;

            var availableShapes = cellDeck.GetShapes();

            // Empty tray means the next batch is still being prepared,
            // not an immediate game over.
            if (availableShapes == null || availableShapes.Length == 0)
                return true;

            foreach (var shape in availableShapes)
            {
                if (shape != null && field.CanPlaceShape(shape))
                    return true;
            }

            return false;
        }


        private void SetWin()
        {
            if (EventManager.GameStatus == EGameState.PreWin || EventManager.GameStatus == EGameState.Win)
                return;
            timerManager?.StopTimer();
            CompleteRun(true);
            var next = ArcadeLevelCatalog.Next(currentLevel);
            if (gameMode == EGameMode.Adventure && next != null)
                GameDataManager.UnlockLevel(next.Number);
            EventManager.GameStatus = EGameState.PreWin;
        }

        public void PresentExternalLose()
        {
            if (EventManager.GameStatus == EGameState.PreFailed ||
                EventManager.GameStatus == EGameState.Failed)
                return;

            SetLose();
        }

        private void SetLose()
        {
            if (EventManager.GameStatus == EGameState.PreFailed || EventManager.GameStatus == EGameState.Failed ||
                EventManager.GameStatus == EGameState.PreWin || EventManager.GameStatus == EGameState.Win)
                return;
            timerManager?.StopTimer();
            if (gameMode == EGameMode.Endless)
                GameState.Delete(EGameMode.Endless);
            else if (gameMode == EGameMode.Timed)
                GameState.Delete(EGameMode.Timed);
            OnLose?.Invoke();
            EventManager.GameStatus = EGameState.PreFailed;
        }

        public IEnumerator EndAnimations(Action action)
        {
            yield return StartCoroutine(FillEmptyCellsFailed());
            action?.Invoke();
        }

        private IEnumerator FillEmptyCellsFailed()
        {
            SoundBase.instance.PlaySound(SoundBase.instance.fillEmpty);
            var template = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
            emptyCells = field.GetEmptyCells();
            foreach (var cell in emptyCells)
            {
                cell.FillCellFailed(template);
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void ClearEmptyCells()
        {
            foreach (var cell in emptyCells)
            {
                cell.ClearCell();
            }
        }

        private IEnumerator DestroyLines(List<List<Cell>> lines, Shape shape, bool rainbow = false)
        {
            if (!rainbow) SoundBase.instance.PlayClearSound(comboCounter - 1);
            EventManager.GetEvent<Shape>(EGameEvent.LineDestroyed).Invoke(shape);

            // Mark cells as destroying immediately at the start
            foreach (var line in lines)
            {
                foreach (var cell in line)
                {
                    cell.SetDestroying(true);
                }
            }
            
            foreach (var line in lines)
            {
                if (line.Count == 0) continue;
                
                var lineExplosion = lineExplosionPool.Get();
                lineExplosion.Play(line, shape, RectTransformUtils.GetMinMaxAndSizeForCanvas(line, gameCanvas.GetComponent<Canvas>()), GetExplosionColor(shape));
                DOVirtual.DelayedCall(1.5f, () => { lineExplosionPool.Release(lineExplosion); });
                foreach (var cell in line)
                {
                    cell.DestroyCell();
                }
            }
            
            yield return null;
        }

        private Color GetExplosionColor(Shape shape)
        {
            var itemTemplateTopColor = shape.GetActiveItems()[0].itemTemplate.overlayColor;
            if (_levelData.levelType.singleColorMode)
            {
                itemTemplateTopColor = itemFactory.GetOneColor().overlayColor;
            }

            return itemTemplateTopColor;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Keyboard.current == null || EventManager.GameStatus != EGameState.Playing) return;
            if (Keyboard.current[GameManager.instance.debugSettings.Win].wasPressedThisFrame && gameMode == EGameMode.Adventure) SetWin();
            if (Keyboard.current[GameManager.instance.debugSettings.Lose].wasPressedThisFrame) SetLose();
#endif
        }
        public Level GetCurrentLevel()
        {
            return _levelData;
        }

        public EGameMode GetGameMode()
        {
            return gameMode;
        }

        public FieldManager GetFieldManager()
        {
            return field;
        }

        public void PauseTimer(bool pause)
        {
            if (timerManager != null)
            {
                timerManager.PauseTimer(pause);
            }
        }
    }
}
