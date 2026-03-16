using System.Collections.Generic;
using System.Linq;
using PokerAPI.DTOs;
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Mapper
{
    public static class GameMapper
    {
        public static GameStateDto MapToGameStateDto(IGameService game)
        {
            GameStateDto dto = new GameStateDto
            {
                GameState = game.GetGameState(),
                Phase = game.Phase.ToString(),
                CurrentPlayer = game.GetCurrentPlayer()?.Name,
                CurrentBet = game.CurrentBet,
                Pot = game.GetTotalPot(),
                CommunityCards = game.CommunityCards
                    .Select(c => $"{c.Rank} of {c.Suit}")
                    .ToList(),
                Players = game.GetPlayersPublicState().ToList(),
                Showdown = game.LastShowdown == null ? null : new ShowdownDto
                {
                    Winners = game.LastShowdown.Winners.Select(p => p.Name).ToList(),
                    HandRank = game.LastShowdown.HandRank.ToString(),
                    Message = game.LastShowdown.Message
                }
            };

            return dto;
        }
    }
}
