using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.System;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Win : Popup
    {
        public CustomButton nextLevelButton;

        protected override void Awake()
        {
            base.Awake();
            nextLevelButton.onClick.AddListener(() =>
            {
                StopInteration();

                if (GameDataManager.HasMoreLevels())
                {
                    GameManager.instance.NextLevel();
                }
                else
                {
                    GameManager.instance.MainMenu();
                }
                Close();
            });
            closeButton.onClick.AddListener(() => GameManager.instance.OpenMap());
        }
    }
}