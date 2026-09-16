using System.Collections;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Banner : Popup
    {
        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(1.0f);
            Close();
        }
    }
}