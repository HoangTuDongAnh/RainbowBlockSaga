using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusItemTemplateCounter", menuName = "Rainbow Blocks Saga/Items/BonusItemTemplateCounter", order = 2)]
public class BonusItemTemplateCounter : BonusItemTemplate
{
    [Header("Counter Bonus Settings")]
    [Range(1, 10)] public int minHits = 1;
    [Range(1, 10)] public int maxHits = 5;
}
