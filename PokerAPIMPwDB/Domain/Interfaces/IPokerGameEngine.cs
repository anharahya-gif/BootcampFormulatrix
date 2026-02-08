using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.GameEngine;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.Domain.Models;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface IPokerGameEngine
    {
        // ======================
        // Game State
        // ======================
        string GetGameState();
        bool CanStartRound();
        int GetTotalPot();

        IPlayer? GetPlayerByName(string name);
        int GetTotalPlayers();
        IEnumerable<PlayerPublicStateDto> GetPlayersPublicState();

        GamePhase Phase { get; }
        int CurrentBet { get; }
        int CurrentPlayerIndex { get; }
        List<ICard> CommunityCards { get; }
        ShowdownResult? LastShowdown { get; }

        // ======================
        // Player Management
        // ======================
        ServiceResult AddPlayer(string name, int chips, int seatIndex,Guid PlayerId);
        ServiceResult RemovePlayer(IPlayer player);
        List<IPlayer> ActivePlayers();

        // ======================
        // Round Management
        // ======================
        ServiceResult StartRound();
        ServiceResult NextPhase();

        // ======================
        // Player Turn Management
        // ======================
        IPlayer GetCurrentPlayer();
        IPlayer GetNextActivePlayer();
        bool IsBettingRoundOver();

        // ======================
        // Betting Actions
        // ======================
        ServiceResult<int> HandleBet(IPlayer player, int amount);
        ServiceResult<int> HandleCall(IPlayer player);
        ServiceResult<int> HandleRaise(IPlayer player, int raiseAmount);
        ServiceResult HandleFold(IPlayer player);
        ServiceResult HandleCheck(IPlayer player);
        ServiceResult HandleAllIn(string playerName);

        // ======================
        // Showdown
        // ======================
        Dictionary<IPlayer, HandRank> EvaluateHands();
        List<IPlayer> DetermineWinners();
        List<IPlayer> ResolveShowdown();
        (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed();

        // ======================
        // Event
        // ======================
        event Action? RoundStarted;
        event Action? CommunityCardsUpdated;
        event Action? ShowdownCompleted;

        // Per-player visible evaluation (their hand + community cards)
        object? EvaluateVisibleForPlayer(string playerName);

        // Full showdown details (all players' hands and ranks)
        object GetShowdownDetails();
    }
}
