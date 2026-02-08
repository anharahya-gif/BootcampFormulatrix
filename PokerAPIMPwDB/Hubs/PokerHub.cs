using Microsoft.AspNetCore.SignalR;
using PokerAPIMPwDB.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.Hubs
{
    public class PokerHub : Hub
    {
        private readonly GameManager _gameManager;

        // ⚡ GameManager tetap singleton, hub inject langsung
        public PokerHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        // ===========================
        // Player join table
        // ===========================
        public async Task JoinTable(Guid tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tableId.ToString());
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            await Clients.Caller.SendAsync("InitialState", game.GetPlayersPublicState());
        }

        // ===========================
        // Player actions
        // ===========================
        public async Task PlaceBet(Guid tableId, string playerName, int amount)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            var result = game.HandleBet(player, amount);

            await Clients.Group(tableId.ToString()).SendAsync("BetPlaced", new
            {
                Player = playerName,
                Amount = amount,
                CurrentBet = result.Value,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        public async Task Call(Guid tableId, string playerName)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            if (player == null) return;

            var result = game.HandleCall(player);
            await Clients.Group(tableId.ToString()).SendAsync("PlayerCalled", new
            {
                Player = playerName,
                CurrentBet = result.Value,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        public async Task Raise(Guid tableId, string playerName, int raiseAmount)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            if (player == null) return;

            var result = game.HandleRaise(player, raiseAmount);
            await Clients.Group(tableId.ToString()).SendAsync("PlayerRaised", new
            {
                Player = playerName,
                CurrentBet = result.Value,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        public async Task Fold(Guid tableId, string playerName)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            if (player == null) return;

            var result = game.HandleFold(player);
            await Clients.Group(tableId.ToString()).SendAsync("PlayerFolded", new
            {
                Player = playerName,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        public async Task Check(Guid tableId, string playerName)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            if (player == null) return;

            var result = game.HandleCheck(player);
            await Clients.Group(tableId.ToString()).SendAsync("PlayerChecked", new
            {
                Player = playerName,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        public async Task AllIn(Guid tableId, string playerName)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.GetPlayerByName(playerName);
            if (player == null) return;

            var result = game.HandleAllIn(playerName);
            await Clients.Group(tableId.ToString()).SendAsync("PlayerAllIn", new
            {
                Player = playerName,
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        // ===========================
        // Round management
        // ===========================
        public async Task StartRound(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var result = game.StartRound();

            await Clients.Group(tableId.ToString()).SendAsync("RoundStarted", new
            {
                Success = result.IsSuccess,
                Message = result.Message,
                Players = game.GetPlayersPublicState()
            });
        }

        public async Task NextPhase(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var result = game.NextPhase();

            await Clients.Group(tableId.ToString()).SendAsync("PhaseAdvanced", new
            {
                Phase = game.Phase.ToString(),
                Success = result.IsSuccess,
                Message = result.Message
            });
        }

        // ===========================
        // Showdown / Hand evaluation
        // ===========================
        public async Task ResolveShowdown(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var winners = game.ResolveShowdown();

            await Clients.Group(tableId.ToString()).SendAsync("ShowdownCompleted", new
            {
                Winners = winners.Select(p => p.DisplayName),
                CommunityCards = game.CommunityCards
            });
        }

        public async Task ResolveShowdownDetailed(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var (winners, rank) = game.ResolveShowdownDetailed();

            await Clients.Group(tableId.ToString()).SendAsync("ShowdownDetailed", new
            {
                Winners = winners.Select(p => p.DisplayName),
                BestHandRank = rank.ToString(),
                CommunityCards = game.CommunityCards
            });
        }

        public async Task EvaluateHands(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var hands = game.EvaluateHands();

            await Clients.Group(tableId.ToString()).SendAsync("HandsEvaluated", new
            {
                Players = hands.Select(h => new
                {
                    Player = h.Key.DisplayName,
                    HandRank = h.Value.ToString()
                })
            });
        }

        public async Task EvaluateVisibleForPlayer(Guid tableId, string playerName)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var data = game.EvaluateVisibleForPlayer(playerName);

            await Clients.Caller.SendAsync("VisibleCards", data);
        }
    }
}
