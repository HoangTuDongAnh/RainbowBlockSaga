using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Utils
{
    public static class TryAddComponent
    {
        public static T AddComponentIfNotExists<T>(this GameObject gameObject) where T : Component
        {
            return AddComponentIfNotExists<T>(gameObject.transform);
        }

        public static T AddComponentIfNotExists<T>(this Transform transform) where T : Component
        {
            if (!transform.TryGetComponent<T>(out var component))
            {
                component = transform.gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}