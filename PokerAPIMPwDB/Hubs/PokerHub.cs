using Microsoft.AspNetCore.SignalR;
using PokerAPIMPwDB.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using PokerAPIMPwDB.Domain.Interfaces;
using PokerAPIMPwDB.Common.Results;

namespace PokerAPIMPwDB.Hubs
{
    public class PokerHub : Hub
    {
        private readonly GameManager _gameManager;

        public PokerHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        // ===========================
        // Helpers
        // ===========================
        private Guid GetPlayerId()
        {
            var claim = Context.User?.Claims.FirstOrDefault(c =>
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier ||
                c.Type.EndsWith("/nameidentifier"));

            if (claim == null)
                throw new HubException("Unauthorized");

            return Guid.Parse(claim.Value);
        }

        private string GetPlayerName()
        {
            var claim = Context.User?.Claims.FirstOrDefault(c =>
                c.Type == System.Security.Claims.ClaimTypes.Name ||
                c.Type.EndsWith("/name"));

            return claim?.Value ?? "Player";
        }

        // ===========================
        // Join Table (spectator first)
        // ===========================
        public async Task JoinTable(Guid tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tableId.ToString());
            var result = await _gameManager.PlayerJoinTableAsync(tableId);

            var game = await _gameManager.GetOrCreateGameAsync(tableId);

            await Clients.Caller.SendAsync("InitialState", new
            {
                TableId = tableId,
                Phase = game.Phase.ToString(),
                Seats = game.GetSeatsState(),
                CommunityCards = game.CommunityCards
            });
        }

        // ===========================
        // Sit Down
        // ===========================
        public async Task SitDown(Guid tableId, int seatIndex, int chips)
        {
            var playerId = GetPlayerId();
            var displayName = GetPlayerName();

            var result = await _gameManager.SitPlayerAsync(tableId, playerId, displayName, seatIndex, chips);

            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            await Clients.Group(tableId.ToString()).SendAsync("SeatsUpdated", game.GetSeatsState());
        }

        // ===========================
        // Stand Up
        // ===========================
        public async Task StandUp(Guid tableId)
        {
            var playerId = GetPlayerId();
            var result = await _gameManager.StandPlayerAsync(tableId, playerId);

            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            await Clients.Group(tableId.ToString()).SendAsync("SeatsUpdated", game.GetSeatsState());
        }

        // ===========================
        // Leave Table
        // ===========================
        public async Task LeaveTable(Guid tableId)
        {
            var playerId = GetPlayerId();
            var result = await _gameManager.PlayerLeaveTableAsync(tableId, playerId);

            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tableId.ToString());
        }

        // ===========================
        // Player Actions
        // ===========================
        private async Task HandlePlayerAction(Guid tableId, Func<IPlayer, ServiceResult> action)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == GetPlayerId());
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not seated at this table");
                return;
            }

            var result = action(player);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            await Clients.Group(tableId.ToString()).SendAsync("SeatsUpdated", game.GetSeatsState());
        }

        private async Task HandlePlayerAction<T>(Guid tableId, Func<IPlayer, ServiceResult<T>> action)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var player = game.ActivePlayers().FirstOrDefault(p => p.PlayerId == GetPlayerId());
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not seated at this table");
                return;
            }

            var result = action(player);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            await Clients.Group(tableId.ToString()).SendAsync("SeatsUpdated", game.GetSeatsState());
        }

        public Task PlaceBet(Guid tableId, int amount) => HandlePlayerAction<int>(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleBet(p, amount));
        public Task Call(Guid tableId) => HandlePlayerAction<int>(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleCall(p));
        public Task Raise(Guid tableId, int raiseAmount) => HandlePlayerAction<int>(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleRaise(p, raiseAmount));
        public Task Fold(Guid tableId) => HandlePlayerAction(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleFold(p));
        public Task Check(Guid tableId) => HandlePlayerAction(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleCheck(p));
        public Task AllIn(Guid tableId) => HandlePlayerAction(tableId, p => _gameManager.GetOrCreateGameAsync(tableId).Result.HandleAllIn(p.DisplayName));

        // ===========================
        // Round Management
        // ===========================
        public async Task StartRound(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var result = game.StartRound();

            await Clients.Group(tableId.ToString()).SendAsync("RoundStarted", new
            {
                Phase = game.Phase.ToString(),
                Seats = game.GetSeatsState(),
                CommunityCards = game.CommunityCards
            });
        }

        public async Task NextPhase(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var result = game.NextPhase();

            await Clients.Group(tableId.ToString()).SendAsync("PhaseAdvanced", new
            {
                Phase = game.Phase.ToString(),
                Seats = game.GetSeatsState(),
                CommunityCards = game.CommunityCards
            });
        }

        // ===========================
        // Showdown
        // ===========================
        public async Task ResolveShowdown(Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);
            var winners = game.ResolveShowdown();

            await Clients.Group(tableId.ToString()).SendAsync("ShowdownCompleted", new
            {
                Winners = winners.Select(w => w.DisplayName),
                Seats = game.GetSeatsState(),
                CommunityCards = game.CommunityCards
            });
        }
    }
}
