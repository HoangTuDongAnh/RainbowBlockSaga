using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Board
{
    [CreateAssetMenu(fileName = "BoardViewProfile", menuName = "Rainbow Block Saga/Board/View Profile")]
    public class BoardViewProfile : ScriptableObject
    {
        [Min(1f)] public float MinCellSize = 24f;
        [Min(1f)] public float MaxCellSize = 120f;
        [Min(0f)] public float Spacing = 4f;
        public Vector2 Padding = new(8f, 8f);
    }
}
