using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.Domain.Models;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        IReadOnlyList<SeatStateDto> GetSeatsState();
        
        public IServiceScope? Scope { get; set; }
        

        IPlayer? GetPlayerByName(string name);
        int GetTotalPlayers();
        IEnumerable<PlayerPublicStateDto> GetPlayersPublicState();

        GamePhase Phase { get; }
        int CurrentBet { get; }
        int CurrentPlayerIndex { get; }
        List<ICard> CommunityCards { get; }
        ShowdownResult? LastShowdown { get; }

        int MinBuyIn { get; }
        int MaxBuyIn { get; }

        // ======================
        // Table Join / Leave / Seat
        // ======================
        Task<ServiceResult<TableStateDto>> JoinTableAsync(Guid tableId);
        Task<ServiceResult> SitDownAsync(Guid userId, string displayName, int seatIndex, int chips);
        Task<ServiceResult> StandUpAsync(Guid userId);
        Task<ServiceResult> LeaveTableAsync(Guid userId);

        List<IPlayer> ActivePlayers();

        // ======================
        // Round Management
        // ======================
        ServiceResult StartRound();
        ServiceResult NextPhase();

        // ======================
        // Player Turn Management
        // ======================
        IPlayer? GetCurrentPlayer();
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
        // Event (async-friendly)
        // ======================
        event Func<Task>? RoundStarted;
        event Func<Task>? CommunityCardsUpdated;
        event Func<Task>? ShowdownCompleted;

        // ======================
        // Per-player visible evaluation
        // ======================
        object? EvaluateVisibleForPlayer(string playerName);

        // ======================
        // Full showdown details
        // ======================
        object GetShowdownDetails();
    }
}
