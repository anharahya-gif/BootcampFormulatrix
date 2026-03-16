using UnoGame.Cards;
using UnoGame.Players;

namespace UnoGame.Core
{
    public interface IGameContext
    {
         Player CurrentPlayer { get; }
        void SkipNextPlayer();
        void ReverseDirection();
        void ForceDraw(Player player, int count);
        void SetCurrentColor(CardColor color);
    }

    public interface IGameEvents
    {
        void OnCardPlayed(Player player, Card card);
        void OnUnoCalled(Player player);
        void OnUnoPenalty(Player player);
        void OnDirectionChanged(Direction direction);
        void OnGameEnded(Player winner);
    }
}
