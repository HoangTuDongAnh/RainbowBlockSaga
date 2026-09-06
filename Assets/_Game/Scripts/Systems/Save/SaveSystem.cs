using RainbowBlockSaga.Foundation;
using UnityEngine;

namespace RainbowBlockSaga.Systems.Save
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        const string SaveKey = "RBS_PLAYER_SAVE";
        public PlayerSaveData Data { get; private set; }

        protected override void SetUp()
        {
            Load();
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            Data = string.IsNullOrEmpty(json) ? new PlayerSaveData() : JsonUtility.FromJson<PlayerSaveData>(json);
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void ResetSave()
        {
            Data = new PlayerSaveData();
            Save();
        }
    }
}
