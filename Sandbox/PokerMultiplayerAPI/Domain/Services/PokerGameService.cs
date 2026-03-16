using PokerMultiplayerAPI.Domain.Entities;
using PokerMultiplayerAPI.Domain.Enums;
using PokerMultiplayerAPI.Domain.Interfaces;
using PokerMultiplayerAPI.Shared.DTOs;
using System.Collections.Concurrent;

namespace PokerMultiplayerAPI.Domain.Services;

public class PokerGameService : IGameService
{
    private readonly ITableRepository _tableRepository;
    private readonly IGameNotifier _notifier;

    // Lock per table untuk concurrency
    private static readonly ConcurrentDictionary<Guid, object> TableLocks = new();
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> TurnTimers = new();
    private const int TurnTimeoutSeconds = 30;

    public PokerGameService(ITableRepository tableRepository, IGameNotifier notifier)
    {
        _tableRepository = tableRepository;
        _notifier = notifier;
    }

    // ===========================
    // Table / Game Management
    // ===========================
    private void ResetTableForNewRound(Table table)
    {
        table.GameState.Phase = GamePhase.PreFlop;
        table.Deck.Reset();
        table.GameState.CommunityCards.Clear();
        table.GameState.CurrentPot = 0;
        table.GameState.CurrentMaxBet = 0;
        table.GameState.PlayersActed.Clear();

        table.GameState.DealerIndex = (table.GameState.DealerIndex + 1) % table.Seats.Count;

        foreach (var p in table.Seats)
        {
            p.ResetForNewRound();
            p.HoleCards.Add(table.Deck.Draw());
            p.HoleCards.Add(table.Deck.Draw());
        }

        table.GameState.CurrentTurnPlayerId = table.Seats[(table.GameState.DealerIndex + 1) % table.Seats.Count].Id;
    }

    public async Task<Table> JoinTableAsync(Guid tableId, Guid playerId, string playerName, decimal chips)
    {
        var tableLock = TableLocks.GetOrAdd(tableId, _ => new object());
        lock (tableLock)
        {
            var table = _tableRepository.GetTable(tableId) ?? throw new Exception("Table not found");

            if (table.Seats.Count >= table.MaxSeats) throw new Exception("Table full");

            var existingPlayer = table.Seats.FirstOrDefault(p => p.Id == playerId);
            if (existingPlayer != null)
            {
                existingPlayer.IsActive = true;
                existingPlayer.Name = playerName;
            }
            else
            {
                table.Seats.Add(new Player
                {
                    Id = playerId,
                    Name = playerName,
                    Chips = chips,
                    IsActive = true
                });
            }

            _tableRepository.UpdateTable(table);
            _ = BroadcastState(tableId);

            return table;
        }
    }

    public async Task LeaveTableAsync(Guid tableId, Guid playerId)
    {
        var tableLock = TableLocks.GetOrAdd(tableId, _ => new object());
        lock (tableLock)
        {
            var table = _tableRepository.GetTable(tableId);
            if (table == null) return;

            var player = table.Seats.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                table.Seats.Remove(player);
                _tableRepository.UpdateTable(table);
            }
        }
        await BroadcastState(tableId);
    }

    public async Task StartGameAsync(Guid tableId)
    {
        var tableLock = TableLocks.GetOrAdd(tableId, _ => new object());
        lock (tableLock)
        {
            var table = _tableRepository.GetTable(tableId) ?? throw new Exception("Table not found");
            if (table.Seats.Count < 2) throw new Exception("Not enough players");

            ResetTableForNewRound(table);
            _tableRepository.UpdateTable(table);
        }

        await BroadcastState(tableId);
        StartTurnTimer(tableId);
    }

    // ===========================
    // Player Actions
    // ===========================
    public async Task<bool> PlayerActionAsync(Guid tableId, Guid playerId, TurnAction action, decimal amount)
    {
        var tableLock = TableLocks.GetOrAdd(tableId, _ => new object());
        Table table;

        lock (tableLock)
        {
            table = _tableRepository.GetTable(tableId) ?? throw new Exception("Table not found");

            if (table.GameState.CurrentTurnPlayerId != playerId) throw new Exception("Not your turn");

            var player = table.Seats.First(p => p.Id == playerId);

            switch (action)
            {
                case TurnAction.Fold:
                    player.HasFolded = true;
                    table.GameState.PlayersActed.Add(playerId);
                    break;
                case TurnAction.Call:
                    decimal callAmount = table.GameState.CurrentMaxBet - player.CurrentBet;
                    if (player.Chips < callAmount) throw new Exception("Not enough chips");
                    player.Chips -= callAmount;
                    player.CurrentBet += callAmount;
                    table.GameState.CurrentPot += callAmount;
                    table.GameState.PlayersActed.Add(playerId);
                    break;
                case TurnAction.Raise:
                    if (player.Chips < amount) throw new Exception("Not enough chips");
                    if (amount <= table.GameState.CurrentMaxBet) throw new Exception("Raise must be greater than current bet");

                    decimal diff = amount - player.CurrentBet;
                    player.Chips -= diff;
                    player.CurrentBet = amount;
                    table.GameState.CurrentPot += diff;
                    table.GameState.CurrentMaxBet = amount;

                    table.GameState.PlayersActed.Clear();
                    table.GameState.PlayersActed.Add(playerId);
                    break;
                case TurnAction.Check:
                    if (player.CurrentBet < table.GameState.CurrentMaxBet) throw new Exception("Cannot check, must call");
                    table.GameState.PlayersActed.Add(playerId);
                    break;
            }

            NextTurn(table);
            _tableRepository.UpdateTable(table);
        }

        await _notifier.NotifyPlayerAction(table.Id, table.Seats.First(p => p.Id == playerId).Name, action.ToString(), amount);
        await BroadcastState(table.Id);

        StartTurnTimer(tableId);
        return true;
    }

    // ===========================
    // Turn & Auto-Advance
    // ===========================
    private void NextTurn(Table table)
    {
        TryAutoAdvance(table);
        if (table.GameState.Phase == GamePhase.Showdown) return;

        int currentIndex = table.Seats.FindIndex(p => p.Id == table.GameState.CurrentTurnPlayerId);
        int count = table.Seats.Count;
        int nextIndex = (currentIndex + 1) % count;
        int attempts = 0;

        while ((table.Seats[nextIndex].HasFolded || table.Seats[nextIndex].IsAllIn) && attempts < count)
        {
            nextIndex = (nextIndex + 1) % count;
            attempts++;
        }

        table.GameState.CurrentTurnPlayerId = table.Seats[nextIndex].Id;
    }

    private void TryAutoAdvance(Table table)
    {
        var activePlayers = table.Seats.Where(p => !p.HasFolded).ToList();

        if (activePlayers.Count == 1)
        {
            table.GameState.Phase = GamePhase.Showdown;
            AwardPot(table, new List<Player> { activePlayers.First() });
            return;
        }

        var notAllIn = activePlayers.Where(p => !p.IsAllIn).ToList();
        if (notAllIn.Count == 0 || (notAllIn.Count == 1 && activePlayers.All(p => p.IsAllIn || p == notAllIn[0]) && IsRoundComplete(table)))
        {
            DealRemainingCommunityCards(table);
            table.GameState.Phase = GamePhase.Showdown;
            ResolveShowdown(table);
            return;
        }

        if (IsRoundComplete(table))
        {
            AdvancePhase(table);
            if (table.GameState.Phase == GamePhase.Showdown)
                ResolveShowdown(table);
        }
    }

    private bool IsRoundComplete(Table table)
    {
        var activePlayers = table.Seats.Where(p => !p.HasFolded && !p.IsAllIn).ToList();
        if (activePlayers.Count == 0) return true;

        bool allMatched = activePlayers.All(p => p.CurrentBet == table.GameState.CurrentMaxBet);
        bool allActed = activePlayers.All(p => table.GameState.PlayersActed.Contains(p.Id));

        return allMatched && allActed;
    }

    private void AdvancePhase(Table table)
    {
        foreach (var p in table.Seats) p.CurrentBet = 0;
        table.GameState.CurrentMaxBet = 0;
        table.GameState.PlayersActed.Clear();

        switch (table.GameState.Phase)
        {
            case GamePhase.PreFlop:
                table.GameState.Phase = GamePhase.Flop;
                table.GameState.CommunityCards.AddRange(new[] { table.Deck.Draw(), table.Deck.Draw(), table.Deck.Draw() });
                break;
            case GamePhase.Flop:
                table.GameState.Phase = GamePhase.Turn;
                table.GameState.CommunityCards.Add(table.Deck.Draw());
                break;
            case GamePhase.Turn:
                table.GameState.Phase = GamePhase.River;
                table.GameState.CommunityCards.Add(table.Deck.Draw());
                break;
            case GamePhase.River:
                table.GameState.Phase = GamePhase.Showdown;
                break;
        }

        if (table.GameState.Phase != GamePhase.Showdown)
        {
            int nextIndex = (table.GameState.DealerIndex + 1) % table.Seats.Count;
            int attempts = 0;
            while ((table.Seats[nextIndex].HasFolded || table.Seats[nextIndex].IsAllIn) && attempts < table.Seats.Count)
            {
                nextIndex = (nextIndex + 1) % table.Seats.Count;
                attempts++;
            }
            table.GameState.CurrentTurnPlayerId = table.Seats[nextIndex].Id;
        }
    }

    private void DealRemainingCommunityCards(Table table)
    {
        while (table.GameState.CommunityCards.Count < 5)
            table.GameState.CommunityCards.Add(table.Deck.Draw());
    }

    // ===========================
    // Showdown / Pot
    // ===========================
    private void ResolveShowdown(Table table)
    {
        var activePlayers = table.Seats.Where(p => !p.HasFolded).ToList();
        if (!activePlayers.Any()) return;

        var bestRank = HandRank.HighCard;
        var playerRanks = new Dictionary<Guid, HandRank>();

        foreach (var p in activePlayers)
        {
            var allCards = p.HoleCards.Concat(table.GameState.CommunityCards).ToList();
            var rank = EvaluateHand(allCards);
            playerRanks[p.Id] = rank;
            if (rank > bestRank) bestRank = rank;
        }

        var winners = activePlayers.Where(p => playerRanks[p.Id] == bestRank).ToList();
        AwardPot(table, winners);
    }

    private void AwardPot(Table table, List<Player> winners)
    {
        if (!winners.Any()) return;

        decimal share = table.GameState.CurrentPot / winners.Count;
        foreach (var w in winners) w.Chips += share;

        string names = string.Join(", ", winners.Select(w => w.Name));
        _notifier.NotifyHandResult(table.Id, $"Winner(s): {names} with Pot {table.GameState.CurrentPot}");

        table.GameState.CurrentPot = 0;

        // Auto-start new round after 5s
        Task.Delay(5000).ContinueWith(_ =>
        {
            var tableLock = TableLocks.GetOrAdd(table.Id, _ => new object());
            lock (tableLock)
            {
                ResetTableForNewRound(table);
                _tableRepository.UpdateTable(table);
            }
            _ = BroadcastState(table.Id);
        });
    }

    // ===========================
    // Turn Timer
    // ===========================
    private void StartTurnTimer(Guid tableId)
    {
        if (TurnTimers.TryGetValue(tableId, out var cts)) cts.Cancel();

        var newCts = new CancellationTokenSource();
        TurnTimers[tableId] = newCts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TurnTimeoutSeconds * 1000, newCts.Token);
                var tableLock = TableLocks.GetOrAdd(tableId, _ => new object());
                lock (tableLock)
                {
                    var table = _tableRepository.GetTable(tableId);
                    if (table == null) return;

                    var currentPlayer = table.Seats.FirstOrDefault(p => p.Id == table.GameState.CurrentTurnPlayerId);
                    if (currentPlayer != null && !currentPlayer.HasFolded)
                    {
                        currentPlayer.HasFolded = true;
                        table.GameState.PlayersActed.Add(currentPlayer.Id);
                        NextTurn(table);
                        _tableRepository.UpdateTable(table);
                    }
                }
                await BroadcastState(tableId);
            }
            catch (TaskCanceledException) { }
        });
    }

    // ===========================
    // Broadcast
    // ===========================
    private async Task BroadcastState(Guid tableId)
    {
        var table = _tableRepository.GetTable(tableId);
        if (table == null) return;

        var dto = new TableStateDto
        {
            TableId = table.Id,
            TableName = table.Name,
            Phase = table.GameState.Phase.ToString(),
            Pot = table.GameState.CurrentPot,
            CurrentMaxBet = table.GameState.CurrentMaxBet,
            CommunityCards = table.GameState.CommunityCards.Select(CardDto.FromEntity).ToList(),
            Players = table.Seats.Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                Chips = p.Chips,
                CurrentBet = p.CurrentBet,
                HasFolded = p.HasFolded,
                IsAllIn = p.IsAllIn,
                IsMyTurn = p.Id == table.GameState.CurrentTurnPlayerId,
                HoleCards = new List<CardDto>()
            }).ToList()
        };

        foreach (var p in table.Seats)
        {
           var personalDto = new TableStateDto
{
    TableId = dto.TableId,
    TableName = dto.TableName,
    Phase = dto.Phase,
    Pot = dto.Pot,
    CurrentMaxBet = dto.CurrentMaxBet,
    CommunityCards = dto.CommunityCards,
    Players = dto.Players.Select(pl =>
        pl.Id == p.Id
            ? new PlayerDto
            {
                Id = pl.Id,
                Name = pl.Name,
                Chips = pl.Chips,
                CurrentBet = pl.CurrentBet,
                HasFolded = pl.HasFolded,
                IsAllIn = pl.IsAllIn,
                IsMyTurn = pl.IsMyTurn,
                HoleCards = p.HoleCards.Select(CardDto.FromEntity).ToList()
            }
            : new PlayerDto
            {
                Id = pl.Id,
                Name = pl.Name,
                Chips = pl.Chips,
                CurrentBet = pl.CurrentBet,
                HasFolded = pl.HasFolded,
                IsAllIn = pl.IsAllIn,
                IsMyTurn = pl.IsMyTurn,
                HoleCards = new List<CardDto>()
            }
    ).ToList()
};

            await _notifier.NotifyGameStateChangedForPlayer(table.Id, p.Id, personalDto);
        }
    }

    // ===========================
    // Hand Evaluation (Sederhana)
    // ===========================
    private static HandRank EvaluateHand(List<Card> cards)
    {
        if (cards.Count < 5) return HandRank.HighCard;

        var rankGroups = cards.GroupBy(c => c.Rank)
                              .OrderByDescending(g => g.Count())
                              .ThenByDescending(g => g.Key)
                              .ToList();
        var suitGroups = cards.GroupBy(c => c.Suit).ToList();

        bool isFlush = suitGroups.Any(g => g.Count() >= 5);
        bool isStraight = GetStraightHighCard(cards.Select(c => c.Rank).ToList()) != null;

        if (isFlush && isStraight) return HandRank.StraightFlush;
        if (rankGroups[0].Count() == 4) return HandRank.FourOfAKind;
        if (rankGroups[0].Count() == 3 && rankGroups.Count > 1 && rankGroups[1].Count() >= 2) return HandRank.FullHouse;
        if (isFlush) return HandRank.Flush;
        if (isStraight) return HandRank.Straight;
        if (rankGroups[0].Count() == 3) return HandRank.ThreeOfAKind;
        if (rankGroups.Count(g => g.Count() == 2) >= 2) return HandRank.TwoPair;
        if (rankGroups[0].Count() == 2) return HandRank.Pair;

        return HandRank.HighCard;
    }

    private static Rank? GetStraightHighCard(List<Rank> ranks)
    {
        var distinct = ranks.Select(r => (int)r).Distinct().OrderBy(r => r).ToList();

        if (distinct.Contains(14) && distinct.Take(4).SequenceEqual(new[] { 2, 3, 4, 5 })) return Rank.Five;

        for (int i = 0; i <= distinct.Count - 5; i++)
        {
            bool straight = true;
            for (int j = 0; j < 4; j++)
            {
                if (distinct[i + j + 1] != distinct[i + j] + 1) { straight = false; break; }
            }
            if (straight) return (Rank)distinct[i + 4];
        }
        return null;
    }
}
