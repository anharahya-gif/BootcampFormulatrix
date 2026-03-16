using PokerAPIMPwDB.Domain.GameEngine;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using PokerAPIMPwDB.Hubs;
using PokerAPIMPwDB.DTO.Table;

namespace PokerAPIMPwDB.Services
{
    public class GameManager
    {
        // Setiap table punya instance PokerGameEngine sendiri
        private readonly ConcurrentDictionary<Guid, IPokerGameEngine> _games = new();

        // PlayerId -> ConnectionId (SignalR)
        private readonly ConcurrentDictionary<Guid, string> _playerConnections = new();

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<PokerHub> _hub;



        public GameManager(IServiceScopeFactory scopeFactory, IHubContext<PokerHub> hub)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
        }

        // ===========================
        // Game Creation / Retrieval
        // ===========================
        private async Task<IPokerGameEngine> CreateGameAsync(Guid tableId)
        {
            var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var game = new PokerGameEngine(db, _hub)
            {
                CurrentTableId = tableId,
                Scope = scope // simpan scope di game agar tidak dispose
            };

            await game.LoadPlayersFromTableAsync(tableId);
            return game;
        }

        public async Task<IPokerGameEngine> GetOrCreateGameAsync(Guid tableId)
        {
            if (_games.TryGetValue(tableId, out var existingGame))
                return existingGame;

            var game = await CreateGameAsync(tableId);
            _games[tableId] = game;
            return game;
        }

        // ===========================
        // Player Connection Helpers
        // ===========================
        public void RegisterConnection(Guid playerId, string connectionId) => _playerConnections[playerId] = connectionId;
        public void RemoveConnection(Guid playerId) => _playerConnections.TryRemove(playerId, out _);
        public string? GetConnectionId(Guid playerId) =>
            _playerConnections.TryGetValue(playerId, out var id) ? id : null;

        // ===========================
        // Table / Player Actions
        // ===========================
        public async Task<ServiceResult<TableStateDto>> PlayerJoinTableAsync(Guid tableId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.JoinTableAsync(tableId);
        }

        public async Task<ServiceResult> SitPlayerAsync(Guid tableId, Guid userId, string displayName, int seatIndex, int chips)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var result = await game.SitDownAsync(userId, displayName, seatIndex, chips);
            if (result.IsSuccess) await BroadcastStateAsync(tableId, game);
            return result;
        }

        public async Task<ServiceResult> StandPlayerAsync(Guid tableId, Guid userId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var result = await game.StandUpAsync(userId);
            if (result.IsSuccess) await BroadcastStateAsync(tableId, game);
            return result;
        }

        public async Task<ServiceResult> PlayerLeaveTableAsync(Guid tableId, Guid userId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.LeaveTableAsync(userId);
        }

        // ===========================
        // Player Actions (Turn / Betting)
        // ===========================
        public async Task<ServiceResult> HandleFold(Guid tableId, Guid playerId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleFold Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult.Fail("Player not found");
            }
            return await game.HandleFold(player);
        }

        public async Task<ServiceResult> HandleCheck(Guid tableId, Guid playerId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleCheck Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult.Fail("Player not found");
            }
            return await game.HandleCheck(player);
        }

        public async Task<ServiceResult<int>> HandleBet(Guid tableId, Guid playerId, int amount)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleBet Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult<int>.Fail("Player not found");
            }
            return await game.HandleBet(player, amount);
        }

        public async Task<ServiceResult<int>> HandleCall(Guid tableId, Guid playerId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleCall Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult<int>.Fail("Player not found");
            }
            return await game.HandleCall(player);
        }

        public async Task<ServiceResult<int>> HandleRaise(Guid tableId, Guid playerId, int raiseAmount)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleRaise Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult<int>.Fail("Player not found");
            }
            return await game.HandleRaise(player, raiseAmount);
        }

        public async Task<ServiceResult> HandleAllIn(Guid tableId, Guid playerId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
            {
                var allPlayers = string.Join(", ", game.ActivePlayers().Select(p => $"{p.DisplayName}({p.PlayerId})"));
                Console.WriteLine($"[GM_LOG] HandleAllIn Failed: Player {playerId} not found. Active players: {allPlayers}");
                return ServiceResult.Fail("Player not found");
            }
            return await game.HandleAllIn(player.DisplayName);
        }

        // ===========================
        // Round Management
        // ===========================
        public async Task<ServiceResult> StartRound(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? await game.StartRound() : ServiceResult.Fail("Table not found");

        public async Task<ServiceResult> NextPhase(Guid tableId)
        {
             if (_games.TryGetValue(tableId, out var game))
             {
                 var result = game.NextPhase();
                 if (result.IsSuccess) await BroadcastStateAsync(tableId, game);
                 return result;
             }
             return ServiceResult.Fail("Table not found");
        }

        public string GetGameState(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.GetGameState() : "Unknown";

        public int GetCurrentBet(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.CurrentBet : 0;

        // ===========================
        // Showdown
        // ===========================
        public async Task<List<IPlayer>> ResolveShowdown(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? await game.ResolveShowdown() : new List<IPlayer>();

        public object GetShowdownDetails(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.GetShowdownDetails() : new { };

        // ===========================
        // Helper: player lookup
        // ===========================
        private IPlayer? GetPlayerInTable(Guid tableId, Guid playerId) =>
            _games.TryGetValue(tableId, out var game) ?
            game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId) : null;

        public IEnumerable<IPlayer> GetPlayersInTable(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.ActivePlayers() : Array.Empty<IPlayer>();

        public bool RemoveGame(Guid tableId)
        {
            if (_games.TryRemove(tableId, out var game))
            {
                game.Scope?.Dispose(); // dispose scope saat game benar-benar selesai
                return true;
            }
            return false;
        }

        private async Task BroadcastStateAsync(Guid tableId, IPokerGameEngine game)
        {
            await _hub.Clients.Group(tableId.ToString()).SendAsync("ReceiveGameState", new
            {
                TableId = tableId,
                Phase = game.Phase.ToString(),
                Seats = game.GetSeatsState(),
                Players = game.GetPlayersPublicState(),
                CurrentPlayer = game.Phase != GamePhase.WaitingForPlayer ? game.GetCurrentPlayer()?.DisplayName : null,
                CommunityCards = game.CommunityCards,
                MinBuyIn = game.MinBuyIn,
                MaxBuyIn = game.MaxBuyIn
            });
        }

    }
}
