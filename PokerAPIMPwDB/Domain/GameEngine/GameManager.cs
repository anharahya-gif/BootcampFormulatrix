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

        // Ganti _db jadi scope factory
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<PokerHub> _hub;

        public GameManager(IServiceScopeFactory scopeFactory,
            IHubContext<PokerHub> hub)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
        }

        // ===========================
        // Game Management Helper
        // ===========================
        private IPokerGameEngine CreateGame(Guid tableId)
        {
            // Buat scope baru untuk scoped services
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var game = new PokerGameEngine(db, _hub)
            {
                CurrentTableId = tableId
            };

            // Load players async tapi dipanggil sync-safe dari GetOrAdd
            game.LoadPlayersFromTableAsync(tableId).Wait();
            return game;
        }

        public async Task<IPokerGameEngine> GetOrCreateGameAsync(Guid tableId)
        {
            return await Task.FromResult(_games.GetOrAdd(tableId, id => CreateGame(id)));
        }

        #region Game Management

        public bool RemoveGame(Guid tableId) => _games.TryRemove(tableId, out _);

        public bool GameExists(Guid tableId) => _games.ContainsKey(tableId);

        public ConcurrentDictionary<Guid, IPokerGameEngine> GetAllGames() => _games;

        #endregion

        #region Player Connection Helpers

        public void RegisterConnection(Guid playerId, string connectionId) => _playerConnections[playerId] = connectionId;

        public void RemoveConnection(Guid playerId) => _playerConnections.TryRemove(playerId, out _);

        public string? GetConnectionId(Guid playerId) =>
            _playerConnections.TryGetValue(playerId, out var id) ? id : null;

        #endregion

        #region Players in Tables

        public IEnumerable<IPlayer> GetPlayersInTable(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.ActivePlayers() : Array.Empty<IPlayer>();

        public IPlayer? GetPlayerInTable(Guid tableId, Guid playerId) =>
            _games.TryGetValue(tableId, out var game) ?
            game.ActivePlayers().FirstOrDefault(p => p.PlayerId == playerId) : null;

        public ServiceResult AddPlayerToTable(Guid tableId, string displayName, int chips, int seatIndex, Guid PlayerId)
        {
            var game = _games.GetOrAdd(tableId, id => CreateGame(id));
            return game.AddPlayer(displayName, chips, seatIndex, PlayerId);
        }

        public ServiceResult RemovePlayerFromTable(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            if (player == null) return ServiceResult.Fail("Player not found in table");

            return _games[tableId].RemovePlayer(player);
        }

        #endregion

        #region Player Actions via GameManager

        public ServiceResult HandleFold(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleFold(player) : ServiceResult.Fail("Player not found in table");
        }

        public ServiceResult HandleCheck(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleCheck(player) : ServiceResult.Fail("Player not found in table");
        }

        public ServiceResult<int> HandleBet(Guid tableId, Guid playerId, int amount)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleBet(player, amount) : ServiceResult<int>.Fail("Player not found in table");
        }

        public ServiceResult<int> HandleCall(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleCall(player) : ServiceResult<int>.Fail("Player not found in table");
        }

        public ServiceResult<int> HandleRaise(Guid tableId, Guid playerId, int raiseAmount)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleRaise(player, raiseAmount) : ServiceResult<int>.Fail("Player not found in table");
        }

        public ServiceResult HandleAllIn(Guid tableId, Guid playerId)
        {
            var player = GetPlayerInTable(tableId, playerId);
            return player != null ? _games[tableId].HandleAllIn(player.PlayerId.ToString()) : ServiceResult.Fail("Player not found in table");
        }

        #endregion

        #region Round Management via GameManager

        public ServiceResult StartRound(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.StartRound() : ServiceResult.Fail("Table not found");

        public ServiceResult NextPhase(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.NextPhase() : ServiceResult.Fail("Table not found");

        public string GetGameState(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.GetGameState() : "Unknown";

        public int GetCurrentBet(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.CurrentBet : 0;

        #endregion

        #region Showdown via GameManager

        public List<IPlayer> ResolveShowdown(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.ResolveShowdown() : new List<IPlayer>();

        public object GetShowdownDetails(Guid tableId) =>
            _games.TryGetValue(tableId, out var game) ? game.GetShowdownDetails() : new { };

        #endregion
    }
}
