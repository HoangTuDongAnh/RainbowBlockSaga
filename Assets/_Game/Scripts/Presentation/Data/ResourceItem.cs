using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Data
{
    [CreateAssetMenu(fileName = "Resource", menuName = "Rainbow Blocks Saga/Data/ResourceItem", order = 1)]
    public class ResourceItem : ResourceObject
    {
        public int defaultValue;
        public override int DefaultValue => defaultValue;

        public override void ResetResource()
        {
        }
    }
}