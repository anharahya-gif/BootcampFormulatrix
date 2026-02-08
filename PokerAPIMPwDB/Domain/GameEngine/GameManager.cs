using PokerAPIMPwDB.Domain.GameEngine;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokerAPIMPwDB.Common.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using PokerAPIMPwDB.Hubs;

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
        public async Task<ServiceResult> PlayerJoinTableAsync(Guid tableId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.JoinTableAsync(tableId);
        }

        public async Task<ServiceResult> SitPlayerAsync(Guid tableId, Guid userId, string displayName, int seatIndex, int chips)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.SitDownAsync(userId, displayName, seatIndex, chips);
        }

        public async Task<ServiceResult> StandPlayerAsync(Guid tableId, Guid userId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.StandUpAsync(userId);
        }

        public async Task<ServiceResult> PlayerLeaveTableAsync(Guid tableId, Guid userId)
        {
            var game = await GetOrCreateGameAsync(tableId);
            return await game.LeaveTableAsync(userId);
        }

        // ===========================
        // Player Actions (Turn / Betting)
        // ===========================
        public ServiceResult HandleFold(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleFold(player) : ServiceResult.Fail("Player not found");
        }

        public ServiceResult HandleCheck(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleCheck(player) : ServiceResult.Fail("Player not found");
        }

        public ServiceResult<int> HandleBet(Guid tableId, Guid playerId, int amount)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleBet(player, amount) : ServiceResult<int>.Fail("Player not found");
        }

        public ServiceResult<int> HandleCall(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleCall(player) : ServiceResult<int>.Fail("Player not found");
        }

        public ServiceResult<int> HandleRaise(Guid tableId, Guid playerId, int raiseAmount)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleRaise(player, raiseAmount) : ServiceResult<int>.Fail("Player not found");
        }

        public ServiceResult HandleAllIn(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleAllIn(player.DisplayName) : ServiceResult.Fail("Player not found");
        }

        // ===========================
        // Round Management
        // ===========================
        public ServiceResult StartRound(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.StartRound() : ServiceResult.Fail("Table not found");

        public ServiceResult NextPhase(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.NextPhase() : ServiceResult.Fail("Table not found");

        public string GetGameState(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.GetGameState() : "Unknown";

        public int GetCurrentBet(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.CurrentBet : 0;

        // ===========================
        // Showdown
        // ===========================
        public List<IPlayer> ResolveShowdown(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.ResolveShowdown() : new List<IPlayer>();

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

    }
}
