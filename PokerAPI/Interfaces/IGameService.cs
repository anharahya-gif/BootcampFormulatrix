using PokerAPI.Models;
using PokerAPI.DTOs;
using PokerAPI.Services;
using System;
using System.Collections.Generic;

namespace PokerAPI.Services.Interfaces
{
    public interface IGameService
    {

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


        ServiceResult AddPlayer(string name, int chips, int seatIndex);
        ServiceResult RegisterPlayer(string name, int chips);
        ServiceResult UpdatePlayerSeat(string playerName, int seatIndex);
        ServiceResult RemovePlayer(IPlayer player);
        List<IPlayer> ActivePlayers();


        ServiceResult StartRound();
        ServiceResult NextPhase();


        IPlayer? GetCurrentPlayer();
        IPlayer? GetNextActivePlayer();
        bool IsBettingRoundOver();

        ServiceResult HandleBet(IPlayer player, int amount);
        ServiceResult HandleCall(IPlayer player);
        ServiceResult HandleRaise(IPlayer player, int raiseAmount);
        ServiceResult HandleFold(IPlayer player);
        ServiceResult HandleCheck(IPlayer player);
        ServiceResult HandleAllIn(string playerName);

        Dictionary<IPlayer, HandRank> EvaluateHands();
        List<IPlayer> DetermineWinners();
        List<IPlayer> ResolveShowdown();
        (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed();


        event Action? RoundStarted;
        event Action? CommunityCardsUpdated;
        event Action? ShowdownCompleted;

        ServiceResult<object> EvaluateVisibleForPlayer(string playerName);
        ServiceResult<object> GetShowdownDetails();

        ServiceResult ResetGame();
    }
}
