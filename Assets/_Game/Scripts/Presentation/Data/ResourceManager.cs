using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Data
{
    public class ResourceManager : SingletonBehaviour<ResourceManager>
    {
        private ResourceObject[] resources;

        public ResourceObject[] Resources
        {
            get
            {
                if (resources == null || resources.Length == 0)
                {
                    Init();
                }

                return resources;
            }
            set => resources = value;
        }

        public override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            Resources = UnityEngine.Resources.LoadAll<ResourceObject>("Variables");
            foreach (var resource in Resources)
            {
                resource.LoadPrefs();
            }
        }

        public bool Consume(string resourceName, int amount)
        {
            var resource = GetResource(resourceName);
            if (resource == null)
            {
                Debug.LogError($"Resource {resourceName} not found");
                return false;
            }

            return resource.Consume(amount);
        }

        public ResourceObject GetResource(string resourceName)
        {
            foreach (var resource in Resources)
            {
                if (resource.name == resourceName)
                {
                    return resource;
                }
            }

            return null;
        }
    }
}