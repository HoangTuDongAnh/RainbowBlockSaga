// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ItemFactory : MonoBehaviour, IShapeCatalog, IGameplaySessionPresentation
    {
        private static ClassicModeHandler classicModeHandlerCached;
        private static TimedModeHandler timeModeHandlerCached;
        private ShapeTemplate[] shapes;
        protected ItemTemplate[] items;
        private Level level;
        private Dictionary<BonusItemTemplate, int> predictedTargets;
        public bool _oneColorMode;
        protected int _oneColor;

        [SerializeField]
        private FieldManager field;

        [SerializeField]
        private CellDeckManager cellDeck;

        [SerializeField]
        private TargetManager targetManager;

        [SerializeField]
        private LevelManager levelManager;

        protected virtual void Awake()
        {
            // load shapes from resources
            shapes = Resources.LoadAll<ShapeTemplate>("Shapes");
            // load items from resources
            items = Resources.LoadAll<ItemTemplate>("Items");
        }

        public event global::System.Action RestartRequested;
        public event global::System.Action<GameplaySessionState> StateChanged;

        public GameplaySessionState CurrentState =>
            ToSessionState(EventManager.GameStatus);

        public int CurrentScore => GetClassicScore();

        public int ScorePerLine =>
            GameManager.instance.GameSettings.ScorePerLine;

        public int ResetComboAfterMoves =>
            GameManager.instance.GameSettings.ResetComboAfterMoves;

        private void OnEnable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel)
                .Subscribe(HandleRestartRequested);
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel)
                .Unsubscribe(HandleRestartRequested);
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        public void ResetCurrentScore()
        {
            var classic = FindObjectOfType<ClassicModeHandler>(true);
            if (classic != null)
            {
                classic.ResetScore();
                return;
            }

            var timed = FindObjectOfType<TimedModeHandler>(true);
            if (timed != null)
                timed.ResetScore();
        }

        private void HandleRestartRequested()
        {
            RestartRequested?.Invoke();
        }

        private void HandleGameStateChanged(EGameState state)
        {
            StateChanged?.Invoke(ToSessionState(state));
        }

        private static GameplaySessionState ToSessionState(EGameState state)
        {
            return state switch
            {
                EGameState.PrepareGame => GameplaySessionState.Prepare,
                EGameState.Tutorial => GameplaySessionState.Tutorial,
                EGameState.Pause => GameplaySessionState.Paused,
                EGameState.Paused => GameplaySessionState.Paused,
                EGameState.Playing => GameplaySessionState.Playing,
                EGameState.PreFailed => GameplaySessionState.PreFailed,
                EGameState.Failed => GameplaySessionState.Failed,
                EGameState.PreWin => GameplaySessionState.PreWin,
                EGameState.Win => GameplaySessionState.Win,
                EGameState.WinWaiting => GameplaySessionState.WinWaiting,
                EGameState.Loaded => GameplaySessionState.Loaded,
                _ => GameplaySessionState.Unknown
            };
        }

        private ShapeTemplate GetNonRepeatedShapeTemplate(HashSet<ShapeTemplate> usedShapeTemplates)
        {
            ShapeTemplate shapeTemplate = null;
            if (usedShapeTemplates == null)
            {
                return GetRandomShape();
            }

            do
            {
                shapeTemplate = GetRandomShape();
            } while (usedShapeTemplates.Contains(shapeTemplate));

            usedShapeTemplates.Add(shapeTemplate);
            return shapeTemplate;
        }

        public ShapeTemplate[] GetEligibleShapeTemplates()
        {
            return levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(shape => shape.spawnFromLevel <= levelManager.currentLevel).ToArray()
                : shapes.Where(shape => shape.scoreForSpawn <= GetClassicScore()).ToArray();
        }

        public IReadOnlyList<ShapeDescriptor> GetEligibleShapes()
        {
            var eligible = GetEligibleShapeTemplates();
            var result = new List<ShapeDescriptor>(eligible.Length);

            foreach (var template in eligible)
                result.Add(CreateDescriptor(template));

            return result;
        }

        public bool TryGetDescriptor(
            UnityEngine.Object handle,
            out ShapeDescriptor descriptor)
        {
            descriptor = null;

            if (handle is not ShapeTemplate template)
                return false;

            descriptor = CreateDescriptor(template);
            return true;
        }

        ShapeDescriptor CreateDescriptor(ShapeTemplate template)
        {
            var cells = new List<Vector2Int>();

            int minColumn = int.MaxValue;
            int maxRow = int.MinValue;

            for (int row = 0; row < template.rows.Length; row++)
            {
                var rowCells = template.rows[row].cells;

                for (int column = 0; column < rowCells.Length; column++)
                {
                    if (!rowCells[column])
                        continue;

                    minColumn = Mathf.Min(minColumn, column);
                    maxRow = Mathf.Max(maxRow, row);
                }
            }

            if (minColumn != int.MaxValue)
            {
                for (int row = 0; row < template.rows.Length; row++)
                {
                    var rowCells = template.rows[row].cells;

                    for (int column = 0; column < rowCells.Length; column++)
                    {
                        if (!rowCells[column])
                            continue;

                        cells.Add(new Vector2Int(
                            column - minColumn,
                            maxRow - row));
                    }
                }
            }

            return new ShapeDescriptor
            {
                Handle = template,
                Name = template.name,
                Cells = cells,
                SpawnWeight = Mathf.Max(0.0001f, template.chanceForSpawn),
                MinAdventureLevel = Mathf.Max(1, template.spawnFromLevel),
                MinScore = Mathf.Max(0, template.scoreForSpawn)
            };
        }

        private ShapeTemplate GetRandomShape()
        {
            ShapeTemplate shapeTemplate = null;
            var shapesToConsider = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(shape => shape.spawnFromLevel <= levelManager.currentLevel).ToArray()
                : shapes.Where(shape => shape.scoreForSpawn <= GetClassicScore()).ToArray();

            var totalWeight = shapesToConsider.Sum(shape => shape.chanceForSpawn);
            var randomWeight = Random.Range(0, totalWeight);

            foreach (var shape in shapesToConsider)
            {
                if (randomWeight < shape.chanceForSpawn)
                {
                    shapeTemplate = shape;
                    break;
                }

                randomWeight -= shape.chanceForSpawn;
            }

            return shapeTemplate;
        }

        private static int GetClassicScore()
        {
            if (classicModeHandlerCached == null)
                classicModeHandlerCached = FindObjectOfType<ClassicModeHandler>(true);

            var classicHandler = classicModeHandlerCached;
            if (classicHandler != null)
                return classicHandler.score;

            if (timeModeHandlerCached == null)
                timeModeHandlerCached = FindObjectOfType<TimedModeHandler>(true);

            var timedHandler = timeModeHandlerCached;
            if (timedHandler != null)
                return timedHandler.score;

            return 0;
        }

        public Shape CreateRandomShape(HashSet<ShapeTemplate> usedShapeTemplates, GameObject shapeObject)
        {
            var shape = shapeObject.GetComponent<Shape>();
            shape.transform.localScale = Vector3.one;
            shape.UpdateShape(GetNonRepeatedShapeTemplate(usedShapeTemplates));

            var currentTargets = targetManager.GetTargets();
            if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
            {
                GenerateBonus(shape, currentTargets);
            }

            shape.UpdateColor(GetColor());

            return shape;
        }

        public Shape CreateRandomShapeFits(GameObject shapeObject, HashSet<ShapeTemplate> usedShapes = null)
        {
            var shape = shapeObject.GetComponent<Shape>();
            
            var eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel && (usedShapes == null || !usedShapes.Contains(s))).ToArray()
                : shapes.Where(s => s.scoreForSpawn <= GetClassicScore() && (usedShapes == null || !usedShapes.Contains(s))).ToArray();
            
            // If no unused shapes are available, allow reusing shapes
            if (eligibleShapes.Length == 0)
            {
                eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                    ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel).ToArray()
                    : shapes.Where(s => s.scoreForSpawn <= GetClassicScore()).ToArray();
            }
            
            // Randomize shape order
            var shapes_random = eligibleShapes.OrderBy(x => Random.value).ToList();
            
            // Try each shape until we find one that fits
            foreach (var shapeTemplate in shapes_random)
            {
                shape.UpdateShape(shapeTemplate);
                shape.UpdateColor(GetColor());
                
                // Check if the shape fits
                if (field.CanPlaceShape(shape))
                {
                    // Add bonus generation like in CreateRandomShape
                    var currentTargets = targetManager.GetTargets();
                    if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
                    {
                        GenerateBonus(shape, currentTargets);
                    }
                    return shape;
                }
            }
            
            // No shape fits, return null
            PoolObject.Return(shapeObject);
            return null;
        }

        public ItemTemplate GetColor()
        {
            return !_oneColorMode ? items[Random.Range(1, items.Length)] : items[_oneColor];
        }

        public ItemTemplate GetOneColor()
        {
            return items[_oneColor];
        }

        private void GenerateBonus(Shape shapeObject, List<Target> targets)
        {
            var predictedTargets = new Dictionary<BonusItemTemplate, int>(targets.Count);
            foreach (var target in targets)
            {
                predictedTargets[target.targetScriptable.bonusItem] = target.amount;
            }

            // Count the amount of targets already on the field cells
            var fieldCells = field.GetAllCells();
            foreach (var cell in fieldCells)
            {
                if (cell.HasBonusItem())
                {
                    var bonusItem = cell.GetBonusItem();
                    if (predictedTargets.ContainsKey(bonusItem))
                    {
                        predictedTargets[bonusItem]--;
                        if (predictedTargets[bonusItem] <= 0)
                        {
                            predictedTargets.Remove(bonusItem);
                        }
                    }
                }
            }

            // get bonuses on deck
            var shapesOnDeck = cellDeck.GetShapes();
            foreach (var shape in shapesOnDeck)
            {
                foreach (var item in shape.GetActiveItems())
                {
                    if (item.HasBonusItem())
                    {
                        var bonusItem = item.bonusItemTemplate;
                        if (predictedTargets.ContainsKey(bonusItem))
                        {
                            predictedTargets[bonusItem]--;
                            if (predictedTargets[bonusItem] <= 0)
                            {
                                predictedTargets.Remove(bonusItem);
                            }
                        }
                    }
                }
            }

            var keys = predictedTargets.Keys.ToList();
            keys = keys.OrderBy(x => Random.value).ToList();

            foreach (var key in keys)
            {
                if (predictedTargets[key] > 0 && Random.Range(0, 3) == 0)
                {
                    shapeObject.SetBonus(key, predictedTargets[key]);
                    return;
                }
            }
        }
    }
}