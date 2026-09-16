using System;
using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Map
{
    [Serializable]
    public struct MapTypeBinding
    {
        public GameObject mapContainer;
        public EMapType mapType;
    }
    
    [ExecuteInEditMode]
    public class MapTypeManager : SingletonBehaviour<MapTypeManager>
    {
        [SerializeField]
        private List<MapTypeBinding> mapBindings = new List<MapTypeBinding>();
        
        private void OnEnable()
        {
            ApplyMapType();
        }
        
        public void ApplyMapType()
        {
            EMapType currentMapType = GameManager.instance.GameSettings.mapType;
            
            // Set active state for all map containers
            foreach (var binding in mapBindings)
            {
                if (binding.mapContainer != null)
                {
                    binding.mapContainer.SetActive(binding.mapType == currentMapType);
                }
            }
        }
        
        public void SwitchMapType(EMapType newMapType)
        {
            // Update the GameSettings mapType value
            GameManager.instance.GameSettings.mapType = newMapType;
            
            // Apply the new map type
            ApplyMapType();
        }
    }
} 