using System.Collections.Generic;
using UnoGame.Model;

namespace UnoGame.View
{
    public interface IGameView
    {
        void ShowGameStart(List<Player> players);
        void ShowPlayerTurn(Player player);
        void ShowPlayerHand(Player player);

        void ShowGameStatus(
            int deckCount,
            int discardCount,
            Dictionary<Player, int> playerCardCounts,
            Card? lastPlayedCard,
            Card topCard,
            CardColor currentColor,
            Player? lastPlayer,
            Player currentPlayer,
            Player nextPlayer,
            Direction direction
        );

        void ShowCardPlayed(Player player, Card card);
        void ShowCardDrawn(Player player, Card card);
        void ShowUnoCalled(Player player);
        void ShowUnoPenalty(Player player);
        void ShowDirectionChanged(Direction direction);
        void ShowGameOver(Player winner);
        void ShowPenalty(Player player, string reason, int drawCount);

    }

}
