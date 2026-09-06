namespace RainbowBlockSaga.Gameplay.Board
{
    public class BoardController
    {
        public BoardModel Model { get; }
        public BoardView View { get; }

        public BoardController(BoardData data, BoardView view)
        {
            Model = new BoardModel(data);
            View = view;
            View.Initialize(Model);
        }

        public void Refresh() => View.Refresh();
    }
}
