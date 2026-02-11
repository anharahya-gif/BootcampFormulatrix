using PokerAPI.Services.Interfaces;
using PokerAPI.Models;
using System.Collections.Generic;

public static class TestHelper
{
    public static List<IPlayer> CreatePlayers(int count, int startChips)
    {
        var players = new List<IPlayer>();
        for (int i = 0; i < count; i++)
        {
            players.Add(new Player($"Player{i}", startChips) { SeatIndex = i });
        }
        return players;
    }
}
