using PokerAPI.Models;
using PokerAPI.DTOs;
using PokerAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PokerAPI.Services
{
    public class GameService : IGameService, IDisposable
    {
        #region Game State / Core Properties
        private bool _hasRoundStarted = false;
        private const int MaxPlayers = 8;
        private string? _currentPlayerName;
        public IDeck Deck { get; private set; } = new Deck();
        public IPot Pot { get; private set; } = new Pot();
        public Dictionary<IPlayer, PlayerStatus> PlayerMap { get; private set; } = new Dictionary<IPlayer, PlayerStatus>();
        public List<ICard> CommunityCards { get; private set; } = new List<ICard>();
        public int CurrentPlayerIndex { get; private set; } = 0;
        public int CurrentBet { get; private set; } = 0;
        public GamePhase Phase { get; private set; } = GamePhase.PreFlop;
        public ShowdownResult? LastShowdown { get; private set; }
        #endregion

        #region Events
        public event Action? RoundStarted;
        public event Action? CommunityCardsUpdated;
        public event Action? ShowdownCompleted;
        #endregion

        #region Constructor
        public GameService()
        {
            RoundStarted += OnRoundStarted;
            CommunityCardsUpdated += OnCommunityCardsUpdated;
            ShowdownCompleted += OnShowdownCompleted;
        }
        #endregion

        #region Event Handlers
        private void OnRoundStarted()
        {
            Console.WriteLine("[Event] RoundStarted triggered");
        }

        private void OnCommunityCardsUpdated()
        {
            Console.WriteLine("[Event] CommunityCardsUpdated triggered. CommunityCards: " +
                string.Join(", ", CommunityCards.Select(c => $"{c.Rank} of {c.Suit}")));
        }

        private void OnShowdownCompleted()
        {
            Console.WriteLine("[Event] ShowdownCompleted triggered");
            if (LastShowdown != null)
            {
                Console.WriteLine("Winners: " + string.Join(", ", LastShowdown.Winners.Select(p => p.Name)));
                Console.WriteLine("Winning Rank: " + LastShowdown.HandRank);
            }
        }
        #endregion

        #region Dispose / Event CleanUp
        public void UnsubscribeEvents()
        {
            RoundStarted -= OnRoundStarted;
            CommunityCardsUpdated -= OnCommunityCardsUpdated;
            ShowdownCompleted -= OnShowdownCompleted;
        }

        public void Dispose()
        {
            UnsubscribeEvents();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public State Accessors
        public IEnumerable<PlayerPublicStateDto> GetPlayersPublicState()
        {
            List<PlayerPublicStateDto> states = PlayerMap.Select(kv =>
            {
                IPlayer player = kv.Key;
                PlayerStatus status = kv.Value;

                return new PlayerPublicStateDto
                {
                    SeatIndex = player.SeatIndex,
                    Name = player.Name,
                    ChipStack = player.ChipStack,
                    State = status.State.ToString(),
                    CurrentBet = status.CurrentBet,
                    IsFolded = status.State == PlayerState.Folded,
                    Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                    PossibleHandRank = (status.Hand.Any() || CommunityCards.Any())
                        ? EvaluateHand(status.Hand.Concat(CommunityCards).ToList()).ToString()
                        : string.Empty
                };
            }).ToList();
            return states;
        }

        public ServiceResult<object> EvaluateVisibleForPlayer(string playerName)
        {
            KeyValuePair<IPlayer, PlayerStatus> kv = PlayerMap.FirstOrDefault(k => k.Key.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            if (kv.Key == null)
            {
                ServiceResult<object> resultFail = ServiceResult<object>.Failure("Player not found");
                return resultFail;
            }

            IPlayer player = kv.Key;
            PlayerStatus status = kv.Value;

            List<ICard> combined = status.Hand.Concat(CommunityCards).ToList();
            HandRank rank = EvaluateHand(combined);

            object data = new
            {
                Player = player.Name,
                SeatIndex = player.SeatIndex,
                Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                CommunityCards = CommunityCards.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                Rank = rank.ToString()
            };

            ServiceResult<object> result = ServiceResult<object>.Success(data, "Evaluation successful");
            return result;
        }

        public ServiceResult<object> GetShowdownDetails()
        {
            List<IPlayer> winnersList = LastShowdown?.Winners ?? DetermineWinners();
            List<string> winnerNames = winnersList.Select(p => p.Name).ToList();
            HandRank winningRank = LastShowdown?.HandRank ?? (winnersList.Any() ? EvaluateHands().Values.Max() : HandRank.HighCard);

            int potShare = winnersList.Count > 0 ? Pot.TotalChips / winnersList.Count : 0;

            var allPlayers = PlayerMap.Select(kv =>
            {
                IPlayer player = kv.Key;
                PlayerStatus status = kv.Value;
                List<ICard> combined = status.Hand.Concat(CommunityCards).ToList();
                HandRank rank = EvaluateHand(combined);
                bool isWinner = winnerNames.Contains(player.Name);

                return new
                {
                    name = player.Name,
                    seatIndex = player.SeatIndex,
                    hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                    handRank = rank.ToString(),
                    chipStack = player.ChipStack,
                    isFolded = status.State == PlayerState.Folded,
                    isWinner = isWinner,
                    winnings = isWinner ? potShare : 0
                };
            }).ToList();

            object data = new
            {
                winners = winnerNames,
                players = allPlayers,
                communityCards = CommunityCards.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                handRank = winningRank.ToString(),
                pot = Pot.TotalChips,
                message = LastShowdown?.Message ?? (winnerNames.Any()
                    ? $"{string.Join(", ", winnerNames)} wins with {winningRank}"
                    : "No winner")
            };

            ServiceResult<object> result = ServiceResult<object>.Success(data, "Showdown details retrieved");
            return result;
        }

        public int GetTotalPot() => Pot.TotalChips;
        public IPlayer? GetPlayerByName(string name) => PlayerMap.Keys.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public int GetTotalPlayers() => PlayerMap.Count;
        
        public string GetGameState()
        {
            if (PlayerMap.Count < 2) return "WaitingForPlayers";
            if (Phase == GamePhase.Showdown) return "Completed";
            return _hasRoundStarted ? "InProgress" : "WaitingForStartRound";
        }

        public bool CanStartRound()
        {
            bool hasEnoughPlayers = PlayerMap.Keys.Count(p => p.SeatIndex >= 0) >= 2;
            if (!hasEnoughPlayers) return false;
            if (!_hasRoundStarted) return true;
            return Phase == GamePhase.Showdown;
        }

        public ServiceResult AddPlayer(string name, int chips, int seatIndex)
        {
            if (PlayerMap.Count >= MaxPlayers)
            {
                ServiceResult resultFail = ServiceResult.Failure($"Table is full (max {MaxPlayers} players)");
                return resultFail;
            }

            if (PlayerMap.Keys.Any(p => p.Name == name))
            {
                ServiceResult resultFail = ServiceResult.Failure("Player already exists");
                return resultFail;
            }

            if (seatIndex < 0 || seatIndex >= MaxPlayers)
            {
                ServiceResult resultFail = ServiceResult.Failure("Seat index invalid");
                return resultFail;
            }

            List<int> occupiedSeats = PlayerMap.Keys.Where(p => p.SeatIndex >= 0).Select(p => p.SeatIndex).ToList();
            if (occupiedSeats.Contains(seatIndex))
            {
                ServiceResult resultFail = ServiceResult.Failure("Seat already occupied");
                return resultFail;
            }

            IPlayer player = new Player(name, chips) { SeatIndex = seatIndex };
            PlayerMap[player] = new PlayerStatus();

            ServiceResult result = ServiceResult.Success("Player added successfully");
            return result;
        }
        #endregion

        #region Player Management and Register
        public ServiceResult RegisterPlayer(string name, int chips)
        {
            if (PlayerMap.Count >= MaxPlayers)
            {
                ServiceResult resultFail = ServiceResult.Failure($"Maximum {MaxPlayers} players sudah terdaftar");
                return resultFail;
            }

            if (GetPlayerByName(name) != null)
            {
                ServiceResult resultFail = ServiceResult.Failure("PlayerName sudah terdaftar");
                return resultFail;
            }

            IPlayer player = new Player(name, chips) { SeatIndex = -1 };
            PlayerMap[player] = new PlayerStatus();

            ServiceResult result = ServiceResult.Success("Player registered successfully");
            return result;
        }

        public ServiceResult UpdatePlayerSeat(string playerName, int seatIndex)
        {
            IPlayer? player = GetPlayerByName(playerName);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player tidak ditemukan");
                return resultFail;
            }

            if (seatIndex < 0 || seatIndex >= MaxPlayers)
            {
                ServiceResult resultFail = ServiceResult.Failure("Seat index invalid");
                return resultFail;
            }

            List<int> occupiedSeats = PlayerMap.Keys.Where(p => p.SeatIndex >= 0).Select(p => p.SeatIndex).ToList();
            if (occupiedSeats.Contains(seatIndex))
            {
                ServiceResult resultFail = ServiceResult.Failure("Seat sudah terisi");
                return resultFail;
            }

            player.SeatIndex = seatIndex;

            ServiceResult result = ServiceResult.Success($"Seat updated to {seatIndex} for {playerName}");
            return result;
        }

        public ServiceResult RemovePlayer(IPlayer player)
        {
            if (!PlayerMap.ContainsKey(player))
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found in this game");
                return resultFail;
            }

            bool wasCurrent = GetCurrentPlayer() == player;
            PlayerMap.Remove(player);

            if (PlayerMap.Count == 0)
            {
                CurrentPlayerIndex = 0;
                _hasRoundStarted = false;
                Phase = GamePhase.PreFlop;
            }
            else if (wasCurrent)
            {
                GetNextActivePlayer();
            }

            ServiceResult result = ServiceResult.Success("Player removed successfully");
            return result;
        }

        public List<IPlayer> ActivePlayers()
        {
            return PlayerMap.Where(kv => (kv.Value.State == PlayerState.Active || kv.Value.State == PlayerState.AllIn)
                                         && kv.Key.SeatIndex >= 0)
                            .Select(kv => kv.Key).ToList();
        }
        #endregion

        #region Round Management
        public ServiceResult StartRound()
        {
            if (!CanStartRound())
            {
                ServiceResult resultFail = ServiceResult.Failure("Cannot start round. Ensure at least 2 players are seated.");
                return resultFail;
            }

            if (_hasRoundStarted && Phase != GamePhase.Showdown)
            {
                ServiceResult resultFail = ServiceResult.Failure("Round already in progress.");
                return resultFail;
            }

            _hasRoundStarted = true;
            Deck = new Deck();
            Deck.Shuffle();
            Pot.Reset();
            CurrentBet = 0;
            Phase = GamePhase.PreFlop;
            CommunityCards.Clear();

            foreach (PlayerStatus status in PlayerMap.Values)
                status.ResetStatus();

            
            List<IPlayer> seatedPlayers = PlayerMap.Keys.Where(p => p.SeatIndex >= 0).OrderBy(p => p.SeatIndex).ToList();
            IPlayer? firstPlayer = seatedPlayers.FirstOrDefault(p => PlayerMap[p].State == PlayerState.Active);
            _currentPlayerName = firstPlayer?.Name;
            CurrentPlayerIndex = firstPlayer != null ? seatedPlayers.IndexOf(firstPlayer) : 0;

            DealHoleCards();
            RoundStarted?.Invoke();

            ServiceResult result = ServiceResult.Success("Round started successfully");
            return result;
        }

        private void DealHoleCards()
        {
            foreach (IPlayer player in PlayerMap.Keys)
            {
                if (player.SeatIndex < 0) continue;

                PlayerMap[player].Hand.Clear();
                PlayerMap[player].Hand.Add(Deck.Draw());
                PlayerMap[player].Hand.Add(Deck.Draw());
            }
        }

        private void DealFlop()
        {
            if (Deck.RemainingCards() >= 3)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCards.Add(Deck.Draw());
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        private void DealTurn()
        {
            if (Deck.RemainingCards() >= 1)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        private void DealRiver()
        {
            if (Deck.RemainingCards() >= 1)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        public ServiceResult NextPhase()
        {
            if (!_hasRoundStarted)
            {
                ServiceResult resultFail = ServiceResult.Failure("No round in progress");
                return resultFail;
            }

            switch (Phase)
            {
                case GamePhase.PreFlop:
                    DealFlop();
                    StartBettingRound();
                    Phase = GamePhase.Flop;
                    break;
                case GamePhase.Flop:
                    DealTurn();
                    StartBettingRound();
                    Phase = GamePhase.Turn;
                    break;
                case GamePhase.Turn:
                    DealRiver();
                    StartBettingRound();
                    Phase = GamePhase.River;
                    break;
                case GamePhase.River:
                    Phase = GamePhase.Showdown;
                    break;
                case GamePhase.Showdown:
                    ServiceResult resultFail = ServiceResult.Failure("Game is already at showdown");
                    return resultFail;
            }

            ServiceResult result = ServiceResult.Success($"Advanced to {Phase}");
            return result;
        }

        private void StartBettingRound()
        {
            foreach (PlayerStatus status in PlayerMap.Values)
            {
                status.CurrentBet = 0;
                status.HasActed = false;
            }
            CurrentBet = 0;

            List<IPlayer> players = PlayerMap.Keys.ToList();
            IPlayer? firstActive = players.FirstOrDefault(p => p.SeatIndex >= 0 && PlayerMap[p].State == PlayerState.Active);
            CurrentPlayerIndex = firstActive != null ? players.IndexOf(firstActive) : players.FindIndex(p => p.SeatIndex >= 0);
            if (CurrentPlayerIndex < 0) CurrentPlayerIndex = 0;
        }
        #endregion

        #region Player Turn Management
        public IPlayer? GetCurrentPlayer()
        {
            if (string.IsNullOrEmpty(_currentPlayerName)) return null;
            return PlayerMap.Keys.FirstOrDefault(p => p.Name.Equals(_currentPlayerName, StringComparison.OrdinalIgnoreCase));
        }

        public IPlayer? GetNextActivePlayer()
        {
            List<IPlayer> seatedActive = PlayerMap.Keys
                .Where(p => p.SeatIndex >= 0 && PlayerMap[p].State == PlayerState.Active)
                .OrderBy(p => p.SeatIndex)
                .ToList();

            if (!seatedActive.Any())
            {
                _currentPlayerName = null;
                return null;
            }

            IPlayer? current = GetCurrentPlayer();
            int currentIndex = seatedActive.IndexOf(current!);
            
            
            int nextIndex = (currentIndex + 1) % seatedActive.Count;
            IPlayer nextPlayer = seatedActive[nextIndex];
            
            _currentPlayerName = nextPlayer.Name;
            
            
            CurrentPlayerIndex = PlayerMap.Keys.ToList().IndexOf(nextPlayer);
            
            return nextPlayer;
        }

        public bool IsBettingRoundOver()
        {
            List<IPlayer> playersInPot = PlayerMap.Keys
                .Where(p => p.SeatIndex >= 0 && PlayerMap[p].State != PlayerState.Folded)
                .ToList();

            if (playersInPot.Count <= 1) return true;

 
            
            bool everyoneActed = playersInPot.All(p => PlayerMap[p].HasActed);
            bool betsMatched = playersInPot.All(p => 
                PlayerMap[p].State == PlayerState.AllIn || 
                PlayerMap[p].CurrentBet == CurrentBet
            );

            return everyoneActed && betsMatched;
        }
        #endregion

        #region Betting Actions
        private ServiceResult ValidateTurn(IPlayer player)
        {
            if (!_hasRoundStarted) return ServiceResult.Failure("Round has not started");
            if (Phase == GamePhase.Showdown) return ServiceResult.Failure("Game is in showdown phase");
            
            IPlayer? currentPlayer = GetCurrentPlayer();
            if (currentPlayer == null || !currentPlayer.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase))
                return ServiceResult.Failure("It is not your turn");

            return ServiceResult.Success("Turn valid");
        }

        public ServiceResult HandleBet(IPlayer player, int amount)
        {
            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            if (amount <= 0) return ServiceResult.Failure("Bet amount must be greater than 0");
            if (CurrentBet > 0) return ServiceResult.Failure("Use Call or Raise when there is an existing bet");

            PlayerStatus status = PlayerMap[player];
            if (player.ChipStack < amount) return ServiceResult.Failure("Insufficient chips");

            player.ChipStack -= amount;
            status.CurrentBet += amount;
            status.HasActed = true;
            Pot.AddChips(amount);
            
            if (status.CurrentBet > CurrentBet)
            {
                CurrentBet = status.CurrentBet;
                ResetHasActedExcept(player);
            }

            TryAutoAdvance();
            return ServiceResult.Success($"Bet of {amount} placed by {player.Name}");
        }

        public ServiceResult HandleCall(IPlayer player)
        {
            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            PlayerStatus status = PlayerMap[player];
            int toCall = CurrentBet - status.CurrentBet;

            if (toCall < 0) return ServiceResult.Failure("Invalid call: Current bet is lower than your contribution");

            if (player.ChipStack <= toCall)
            {
                int allInAmount = player.ChipStack;
                status.CurrentBet += allInAmount;
                player.ChipStack = 0;
                status.State = PlayerState.AllIn;
                status.HasActed = true;
                Pot.AddChips(allInAmount);
            }
            else
            {
                player.ChipStack -= toCall;
                status.CurrentBet += toCall;
                status.HasActed = true;
                Pot.AddChips(toCall);
            }

            TryAutoAdvance();
            return ServiceResult.Success($"{player.Name} called successfuly");
        }

        public ServiceResult HandleRaise(IPlayer player, int raiseAmount)
        {
            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            if (raiseAmount <= 0) return ServiceResult.Failure("Raise amount must be positive");

            PlayerStatus status = PlayerMap[player];
            int toCall = CurrentBet - status.CurrentBet;
            int totalRequirement = toCall + raiseAmount;

            if (player.ChipStack < totalRequirement)
                return ServiceResult.Failure("Insufficient chips to raise");

            player.ChipStack -= totalRequirement;
            status.CurrentBet += totalRequirement;
            status.HasActed = true;
            Pot.AddChips(totalRequirement);
            
            if (status.CurrentBet > CurrentBet)
            {
                CurrentBet = status.CurrentBet;
                ResetHasActedExcept(player);
            }

            TryAutoAdvance();
            return ServiceResult.Success($"{player.Name} raised by {raiseAmount}");
        }

        public ServiceResult HandleFold(IPlayer player)
        {
            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            PlayerStatus status = PlayerMap[player];
            status.State = PlayerState.Folded;
            status.HasActed = true;
            status.Hand.Clear();

            TryAutoAdvance();

            return ServiceResult.Success($"{player.Name} folded");
        }

        public ServiceResult HandleCheck(IPlayer player)
        {
            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            PlayerStatus status = PlayerMap[player];
            if (CurrentBet > status.CurrentBet)
                return ServiceResult.Failure("Cannot check when there is an active bet. You must Call, Raise, or Fold.");

            status.HasActed = true;
            TryAutoAdvance();

            return ServiceResult.Success($"{player.Name} checked");
        }

        public ServiceResult HandleAllIn(string playerName)
        {
            IPlayer? player = GetPlayerByName(playerName);
            if (player == null) return ServiceResult.Failure("Player not found");

            ServiceResult turnValidation = ValidateTurn(player);
            if (!turnValidation.IsSuccess) return turnValidation;

            PlayerStatus status = PlayerMap[player];
            int amount = player.ChipStack;
            if (amount <= 0) return ServiceResult.Failure("No chips left for All-In");

            player.ChipStack = 0;
            status.CurrentBet += amount;
            status.State = PlayerState.AllIn;
            status.HasActed = true;
            Pot.AddChips(amount);

            if (status.CurrentBet > CurrentBet)
            {
                CurrentBet = status.CurrentBet;
                ResetHasActedExcept(player);
            }

            TryAutoAdvance();
            return ServiceResult.Success($"{player.Name} is All-In with {amount} chips");
        }
        #endregion

        #region Showdown
        public Dictionary<IPlayer, HandRank> EvaluateHands()
        {
            Dictionary<IPlayer, HandRank> result = new Dictionary<IPlayer, HandRank>();
            foreach (KeyValuePair<IPlayer, PlayerStatus> kv in PlayerMap)
            {
                IPlayer player = kv.Key;
                PlayerStatus status = kv.Value;
                if (status.State == PlayerState.Folded || player.SeatIndex < 0) continue;

                List<ICard> combinedCards = status.Hand.Concat(CommunityCards).ToList();
                result[player] = EvaluateHand(combinedCards);
            }
            return result;
        }

        public List<IPlayer> DetermineWinners()
        {
            Dictionary<IPlayer, HandRank> hands = EvaluateHands();
            if (!hands.Any()) return new List<IPlayer>();

            HandRank maxRank = hands.Values.Max();
            List<IPlayer> winners = hands.Where(kv => kv.Value == maxRank).Select(kv => kv.Key).ToList();
            return winners;
        }

        public List<IPlayer> ResolveShowdown()
        {
            List<IPlayer> winners = DetermineWinners();
            if (!winners.Any()) return winners;

            int share = Pot.TotalChips / winners.Count;
            foreach (IPlayer winner in winners)
                winner.ChipStack += share;

            Pot.Reset();
            return winners;
        }

        public (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed()
        {
            if (Phase != GamePhase.Showdown)
            {
                
                if (PlayerMap.Values.Count(s => s.State == PlayerState.Active || s.State == PlayerState.AllIn) <= 1)
                {
                    Phase = GamePhase.Showdown;
                }
                else
                {
                    return (new List<IPlayer>(), HandRank.HighCard);
                }
            }

            Dictionary<IPlayer, HandRank> handResults = EvaluateHands();
            if (!handResults.Any())
                return (new List<IPlayer>(), HandRank.HighCard);

            HandRank bestRank = handResults.Values.Max();
            List<IPlayer> winners = handResults
                .Where(kv => kv.Value == bestRank)
                .Select(kv => kv.Key)
                .ToList();

            int share = Pot.TotalChips / winners.Count;
            foreach (IPlayer winner in winners)
                winner.ChipStack += share;

            LastShowdown = new ShowdownResult(winners, bestRank);
            ShowdownCompleted?.Invoke();

            Pot.Reset();
            CleanupAfterRound();

            _hasRoundStarted = false;
            Phase = GamePhase.PreFlop;

            return (winners, bestRank);
        }

        private void CleanupAfterRound()
        {
            CommunityCards.Clear();

            foreach (PlayerStatus status in PlayerMap.Values)
            {
                status.Hand.Clear();
                status.CurrentBet = 0;
                status.HasActed = false;

                if (status.State == PlayerState.Folded || status.State == PlayerState.AllIn)
                    status.State = PlayerState.Active;
            }

            CurrentBet = 0;
            CurrentPlayerIndex = 0;
        }

        private void ResetHasActedExcept(IPlayer activePlayer)
        {
            foreach (var kv in PlayerMap)
            {
                if (kv.Key.Name.Equals(activePlayer.Name, StringComparison.OrdinalIgnoreCase)) continue;
                
                if (kv.Value.State == PlayerState.Active)
                {
                    kv.Value.HasActed = false;
                }
            }
        }
        #endregion

        #region Internal Helper
        private bool NoMoreActionsPossible()
        {
            List<PlayerStatus> alive = PlayerMap.Values
                .Where(s => s.State != PlayerState.Folded)
                .ToList();

            
            if (alive.Count <= 1) return true;

            
            if (alive.All(s => s.State == PlayerState.AllIn))
                return true;

            
            List<PlayerStatus> active = alive.Where(s => s.State == PlayerState.Active).ToList();
            if (active.Count <= 1)
            {
                return IsBettingRoundOver();
            }

            return false;
        }

        private void TryAutoAdvance()
        {
            List<IPlayer> seatedActive = PlayerMap.Keys
                .Where(p => p.SeatIndex >= 0 && PlayerMap[p].State == PlayerState.Active)
                .ToList();

            if (seatedActive.Count <= 1 && PlayerMap.Values.Count(s => s.State != PlayerState.Folded) <= 1)
            {
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            if (NoMoreActionsPossible())
            {
                DealRemainingCommunityCards();
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            if (IsBettingRoundOver())
            {
                NextPhase();
                if (Phase == GamePhase.Showdown)
                    ResolveShowdownDetailed();
            }
            else
            {
               
                GetNextActivePlayer();
            }
        }

        private void DealRemainingCommunityCards()
        {
            if (Phase == GamePhase.PreFlop) { DealFlop(); DealTurn(); DealRiver(); }
            else if (Phase == GamePhase.Flop) { DealTurn(); DealRiver(); }
            else if (Phase == GamePhase.Turn) { DealRiver(); }
        }
        #endregion

        #region Internal Hand Evaluator
        private static HandRank EvaluateHand(List<ICard> cards)
        {
            if (cards.Count == 0) return HandRank.HighCard;

            var rankGroups = cards
                .GroupBy(c => c.Rank)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => (int)g.Key)
                .ToList();

            var suitGroups = cards
                .GroupBy(c => c.Suit)
                .ToList();

            foreach (var suitGroup in suitGroups)
            {
                Rank? straightHigh = GetStraightHighCard(suitGroup.Select(c => c.Rank).ToList());
                if (straightHigh != null && suitGroup.Count() >= 5) return HandRank.StraightFlush;
            }

            if (rankGroups[0].Count() == 4) return HandRank.FourOfAKind;
            if (rankGroups[0].Count() == 3 && rankGroups.Any(g => g.Count() >= 2 && g != rankGroups[0])) return HandRank.FullHouse;
            if (suitGroups.Any(g => g.Count() >= 5)) return HandRank.Flush;
            if (GetStraightHighCard(cards.Select(c => c.Rank).ToList()) != null) return HandRank.Straight;
            if (rankGroups[0].Count() == 3) return HandRank.ThreeOfAKind;
            if (rankGroups.Count(g => g.Count() == 2) >= 2) return HandRank.TwoPair;
            if (rankGroups[0].Count() == 2) return HandRank.Pair;

            return HandRank.HighCard;
        }

        private static Rank? GetStraightHighCard(List<Rank> ranks)
        {
            List<int> distinct = ranks.Select(r => (int)r).Distinct().OrderBy(r => r).ToList();

            if (distinct.Contains(14) && distinct.Take(4).Intersect(new[] { 2, 3, 4, 5 }).Count() == 4)
                return Rank.Five;

            for (int i = distinct.Count - 1; i >= 4; i--)
            {
                bool straight = true;
                for (int j = 0; j < 4; j++)
                {
                    if (distinct[i - j] != distinct[i - j - 1] + 1)
                    {
                        straight = false;
                        break;
                    }
                }
                if (straight) return (Rank)distinct[i];
            }

            return null;
        }
        #endregion

        #region Reset GameState
        public ServiceResult ResetGame()
        {
            _hasRoundStarted = false;
            PlayerMap.Clear();
            CommunityCards.Clear();
            Pot.Reset();
            Phase = GamePhase.PreFlop;
            CurrentBet = 0;
            CurrentPlayerIndex = 0;
            LastShowdown = null;

            ServiceResult result = ServiceResult.Success("Game reset successful");
            return result;
        }
        #endregion
    }
}
