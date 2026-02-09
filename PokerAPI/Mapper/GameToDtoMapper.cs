using PokerAPI.DTOs;
using PokerAPI.Models;
using System.Linq;
using System.Collections.Generic;
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Mapping
{
    public static class GameToDtoMapper
    {
        public static ShowdownPlayerDto MapPlayerToShowdownDto(Player player, PlayerStatus status, List<ICard> communityCards, List<string> winnerNames, int potShare, Func<List<ICard>, HandRank> evaluateHand)
        {
            var combined = status.Hand.Concat(communityCards).ToList(); // status.Hand juga List<ICard>
            var rank = evaluateHand(combined);
            bool isWinner = winnerNames.Contains(player.Name);

            return new ShowdownPlayerDto
            {
                Name = player.Name,
                SeatIndex = player.SeatIndex,
                Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                HandRank = rank.ToString(),
                ChipStack = player.ChipStack,
                IsFolded = status.State == PlayerState.Folded,
                IsWinner = isWinner,
                Winnings = isWinner ? potShare : 0
            };
        }

        public static ShowdownResultDto MapShowdownToDto(List<Player> winnersList, Dictionary<Player, PlayerStatus> playerStatuses, List<ICard> communityCards, HandRank winningRank, int pot, string message, Func<List<ICard>, HandRank> evaluateHand)
        {
            var winnerNames = winnersList.Select(p => p.Name).ToList();
            int potShare = winnersList.Count > 0 ? pot / winnersList.Count : 0;

            var allPlayers = playerStatuses.Select(kv =>
                MapPlayerToShowdownDto(kv.Key, kv.Value, communityCards, winnerNames, potShare, evaluateHand)
            ).ToList();

            return new ShowdownResultDto
            {
                Winners = winnerNames,
                Players = allPlayers,
                CommunityCards = communityCards.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                HandRank = winningRank.ToString(),
                Pot = pot,
                Message = message
            };
        }

    }
}
