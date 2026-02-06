using Microsoft.AspNetCore.SignalR;
using PokerAPI.Models;

namespace PokerAPI.Hubs
{
    public class PokerHub : Hub
    {
        public async Task SendGameState(object gameState)
        {
            await Clients.All.SendAsync("ReceiveGameState", gameState);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        public async Task SendShowdownState(object showdownState)
        {
            await Clients.All.SendAsync("ShowdownStateUpdated", showdownState);
        }

    }
}
