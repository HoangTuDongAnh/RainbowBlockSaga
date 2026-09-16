using System;
using System.Threading.Tasks;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Data
{
    public abstract class ResourceObject : ScriptableObject
    {
        public ResourceValue ResourceValue;

        //name of the resource
        private string ResourceName => name;

        public abstract int DefaultValue { get; }

        //value of the resource
        private int Resource;

        public AudioClip sound;

        //delegate for resource update
        public delegate void ResourceUpdate(int count);

        //event for resource update
        public event ResourceUpdate OnResourceUpdate;

        //runs when the object is created
        private void OnEnable()
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await LoadPrefs();
            });
        }

        //loads prefs from player prefs and assigns to resource variable
        public Task LoadPrefs()
        {
            Resource = LoadResource();
            return Task.CompletedTask;
        }

        public int LoadResource()
        {
            return PlayerPrefs.GetInt(ResourceName, DefaultValue);
        }

        //adds amount to resource and saves to player prefs
        public void Add(int amount)
        {
            Resource += amount;
            PlayerPrefs.SetInt(ResourceName, Resource);
            OnResourceChanged();
        }

        //sets resource to amount and saves to player prefs
        public void Set(int amount)
        {
            Resource = amount;
            PlayerPrefs.SetInt(ResourceName, Resource);
            PlayerPrefs.Save();
            OnResourceChanged();
        }

        //consumes amount from resource and saves to player prefs if there is enough
        public virtual bool Consume(int amount)
        {
            if (IsEnough(amount))
            {
                Resource -= amount;
                PlayerPrefs.SetInt(ResourceName, Resource);
                PlayerPrefs.Save();
                OnResourceChanged();
                return true;
            }

            return false;
        }

        //callback for ui elements
        private void OnResourceChanged()
        {
            OnResourceUpdate?.Invoke(Resource);
        }

        //get the resource
        public int GetValue()
        {
            return Resource;
        }

        //check if there is enough of the resource
        public bool IsEnough(int targetAmount)
        {
            if (GetValue() < targetAmount)
            {
                Debug.Log("Not enough " + ResourceName);
            }

            return GetValue() >= targetAmount;
        }

        public abstract void ResetResource();
    }

    [Serializable]
    public class ResourceValue
    {
    }
}