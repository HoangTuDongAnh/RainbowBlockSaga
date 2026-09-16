using System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    [Serializable]
    public class ShapeRow
    {
        public bool[] cells = new bool[5];
    }

    [CreateAssetMenu(fileName = "Shape", menuName = "Rainbow Blocks Saga/Items/Shape", order = 1)]
    public class ShapeTemplate : ScriptableObject
    {
        public ShapeRow[] rows = new ShapeRow[5];
        public int scoreForSpawn;

        public float chanceForSpawn = 1;
        public int spawnFromLevel = 1;

        private void OnEnable()
        {
            if (rows == null || rows.Length != 5)
            {
                rows = new ShapeRow[5];
                for (var i = 0; i < 5; i++)
                {
                    rows[i] = new ShapeRow();
                }
            }
        }
    }
}