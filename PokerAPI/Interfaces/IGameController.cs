using PokerAPI.Models;
using PokerAPI.DTOs;
using PokerAPI.Services;
using System;
using System.Collections.Generic;

namespace PokerAPI.Services.Interfaces
{
    public interface IGameController
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
        void AddPlayer(string name, int chips, int seatIndex);

        // ----- NEW -----
        /// <summary>
        /// Register player tanpa seat (seatIndex = -1)
        /// </summary>
        void RegisterPlayer(string name, int chips);

        /// <summary>
        /// Update seat index untuk player yang sudah terdaftar
        /// </summary>
        void UpdatePlayerSeat(string playerName, int seatIndex);
        void RemovePlayer(IPlayer player);
        List<IPlayer> ActivePlayers();

        // ======================
        // Round Management
        // ======================
        void StartRound();
        void NextPhase();

        // ======================
        // Player Turn Management
        // ======================
        IPlayer? GetCurrentPlayer();
        IPlayer? GetNextActivePlayer();
        bool IsBettingRoundOver();

        // ======================
        // Betting Actions (now using ServiceResult)
        // ======================
        ServiceResult HandleBet(IPlayer player, int amount);
        ServiceResult HandleCall(IPlayer player);
        ServiceResult HandleRaise(IPlayer player, int raiseAmount);
        void HandleFold(IPlayer player);
        void HandleCheck(IPlayer player);
        ServiceResult HandleAllIn(string playerName);

        // ======================
        // Showdown
        // ======================
        Dictionary<IPlayer, HandRank> EvaluateHands();
        List<IPlayer> DetermineWinners();
        List<IPlayer> ResolveShowdown();
        (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed();

        // ======================
        // Events
        // ======================
        event Action? RoundStarted;

        // Fired when community cards change (flop/turn/river)
        event Action? CommunityCardsUpdated;

        // Fired when showdown completes
        event Action? ShowdownCompleted;

        // Per-player visible evaluation (their hand + community cards)
        object? EvaluateVisibleForPlayer(string playerName);

        // Full showdown details (all players' hands and ranks)
        object GetShowdownDetails();
    }
}
