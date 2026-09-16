using RainbowBlockSaga.Presentation.Scripts.Settings;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Data
{
    public class Coins : ResourceObject
    {
        public override int DefaultValue => Resources.Load<GameSettings>("Settings/GameSettings").coins;

        public override bool Consume(int amount)
        {
            return base.Consume(amount);
        }
        public override void ResetResource()
        {
            base.Set(1000);
        }
    }
}