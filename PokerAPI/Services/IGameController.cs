// using PokerAPI.Models;
// using PokerAPI.Models.DTOs;
// using System;
// using System.Collections.Generic;

// namespace PokerAPI.Services.Interfaces
// {
//     public interface IGameController
//     {
//         // ======================
//         // Game State
//         // ======================
//         string GetGameState();
//         bool CanStartRound();
//         int GetTotalPot();

        
//         IPlayer? GetPlayerByName(string name);

       
//         int GetTotalPlayers();

//         IEnumerable<PlayerPublicState> GetPlayersPublicState();

//         GamePhase Phase { get; }
//         int CurrentBet { get; }
//         int CurrentPlayerIndex { get; }
//         List<Card> CommunityCards { get; }
//         ShowdownResult? LastShowdown { get; }

//         // ======================
//         // Player Management
//         // ======================
//         void AddPlayer(IPlayer player);
//         void RemovePlayer(IPlayer player);
//         List<IPlayer> ActivePlayers();

//         // ======================
//         // Round Management
//         // ======================
//         void StartRound();
//         void NextPhase();

//         // ======================
//         // Player Turn Management
//         // ======================
//         IPlayer GetCurrentPlayer();
//         IPlayer GetNextActivePlayer();
//         bool IsBettingRoundOver();

//         // ======================
//         // Betting Actions
//         // ======================
//         bool HandleBet(IPlayer player, int amount);
//         bool HandleCall(IPlayer player);
//         bool HandleRaise(IPlayer player, int raiseAmount);
//         void HandleFold(IPlayer player);
//         void HandleCheck(IPlayer player);
//         bool HandleAllIn(string playerName);

//         // ======================
//         // Showdown
//         // ======================
//         Dictionary<IPlayer, HandRank> EvaluateHands();
//         List<IPlayer> DetermineWinners();
//         List<IPlayer> ResolveShowdown();
//         (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed();

//         // ======================
//         // Event
//         // ======================
//         event Action? RoundStarted;
//     }
// }
using PokerAPI.Models;
using PokerAPI.Models.DTOs;
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

        IEnumerable<PlayerPublicState> GetPlayersPublicState();

        GamePhase Phase { get; }
        int CurrentBet { get; }
        int CurrentPlayerIndex { get; }
        List<Card> CommunityCards { get; }
        ShowdownResult? LastShowdown { get; }

        // ======================
        // Player Management
        // ======================
        void AddPlayer(string name, int chips, int seatIndex);
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
        IPlayer GetCurrentPlayer();
        IPlayer GetNextActivePlayer();
        bool IsBettingRoundOver();

        // ======================
        // Betting Actions
        // ======================
        bool HandleBet(IPlayer player, int amount);
        bool HandleCall(IPlayer player);
        bool HandleRaise(IPlayer player, int raiseAmount);
        void HandleFold(IPlayer player);
        void HandleCheck(IPlayer player);
        bool HandleAllIn(string playerName);

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
