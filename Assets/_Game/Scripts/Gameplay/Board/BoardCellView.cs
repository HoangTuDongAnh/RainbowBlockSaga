using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Gameplay.Board
{
    public class BoardCellView : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Image content;
        [SerializeField] Image preview;

        public BoardCoord Coord { get; private set; }

        public void Initialize(BoardCoord coord)
        {
            Coord = coord;
            SetOccupied(false);
            SetPreview(false, true);
        }

        public void SetOccupied(bool occupied)
        {
            content.gameObject.SetActive(occupied);
        }

        public void SetPlayable(bool playable)
        {
            background.gameObject.SetActive(playable);
            content.gameObject.SetActive(false);
            preview.gameObject.SetActive(false);
        }

        public void SetPreview(bool show, bool valid)
        {
            preview.gameObject.SetActive(show);
            if (show) preview.color = valid ? Color.white : new Color(1f, .25f, .25f, .8f);
        }
    }
}
