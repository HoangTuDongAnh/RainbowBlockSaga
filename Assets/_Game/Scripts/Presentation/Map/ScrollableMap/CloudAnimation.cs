using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    public class CloudAnimation : MonoBehaviour
    {
        public float speed = 2f;
        public float leftThreshold = -5f;
        public float rightThreshold = 5f;

        private bool moveRight = true;

        void Update()
        {
            if (moveRight)
            {
                transform.Translate(Vector3.right * (speed * Time.deltaTime));
            }
            else
            {
                transform.Translate(Vector3.left * (speed * Time.deltaTime));
            }

            if (transform.position.x > rightThreshold && moveRight)
            {
                transform.position = new Vector3(rightThreshold, transform.position.y, transform.position.z);
                moveRight = false;
            }
            else if (transform.position.x < leftThreshold && !moveRight)
            {
                transform.position = new Vector3(leftThreshold, transform.position.y, transform.position.z);
                moveRight = true;
            }
        }
    }
}