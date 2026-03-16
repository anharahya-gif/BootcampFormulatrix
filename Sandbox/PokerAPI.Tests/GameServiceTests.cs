using NUnit.Framework;
using Moq;
using PokerAPI.Services;
using PokerAPI.Models;
using Serilog;
using System.Linq;
using PokerAPI.Services.Interfaces;

[TestFixture]
public class GameServiceTests
{
    private GameService _gameService;
    private Mock<ILogger> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
        _gameService = new GameService(_loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _gameService.Dispose();
    }

    [Test]
    public void StartRound_WhenTwoPlayersOrMore_ShouldInitializeRound()
    {
        // Arrange: 2 pemain seated
        var add1 = _gameService.AddPlayer("Anhar", 1000, 0);
        var add2 = _gameService.AddPlayer("Ahya", 1000, 1);
        Assert.IsTrue(add1.IsSuccess);
        Assert.IsTrue(add2.IsSuccess);

        // Act
        var result = _gameService.StartRound();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(_gameService.IsRoundActive);
        Assert.AreEqual(GamePhase.PreFlop, _gameService.Phase);
        Assert.AreEqual(2, _gameService.GetPlayersPublicState().Count());

        foreach (var playerState in _gameService.GetPlayersPublicState())
        {
            Assert.AreEqual(2, playerState.Hand.Count);
            Assert.AreEqual(0, playerState.CurrentBet);
        }

        Assert.GreaterOrEqual(_gameService.CurrentPlayerIndex, 0);
    }

    [Test]
    public void StartRound_WhenWithOnePlayer_ShouldFail()
    {
        // Arrange: hanya 1 pemain
        var add1 = _gameService.AddPlayer("Anhar", 1000, 0);
        Assert.IsTrue(add1.IsSuccess);

        // Act
        var result = _gameService.StartRound();

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Cannot start round. Ensure at least 2 players are seated.", result.Message);
        Assert.IsFalse(_gameService.IsRoundActive);
    }

    [Test]
    public void StartRound_WhenAlreadyStarted_ShouldFail()
    {
        // Arrange: 2 pemain seated
        var add1 = _gameService.AddPlayer("Anhar", 1000, 0);
        var add2 = _gameService.AddPlayer("Ahya", 1000, 1);
        Assert.IsTrue(add1.IsSuccess);
        Assert.IsTrue(add2.IsSuccess);

        // Act: start round pertama
        var firstStart = _gameService.StartRound();
        Assert.IsTrue(firstStart.IsSuccess);

        // Act: start round kedua
        var secondStart = _gameService.StartRound();

        // Assert
        Assert.IsFalse(secondStart.IsSuccess);
        Assert.AreEqual("Round already in progress.", secondStart.Message);
    }

    #region Player Management Tests
    [Test]
    public void AddPlayer_WhenWithValidData_ShouldSuccess()
    {
        var result = _gameService.AddPlayer("Anhar", 500, 2);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, _gameService.GetTotalPlayers());
    }

    [Test]
    public void AddPlayer_WhenDuplicateName_ShouldFail()
    {
        _gameService.AddPlayer("Anhar", 500, 0);
        var result = _gameService.AddPlayer("Anhar", 500, 1);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Player already exists", result.Message);
    }

    [Test]
    public void AddPlayer_WhenInvalidSeatIndex_ShouldFail()
    {
        var result = _gameService.AddPlayer("Ahya", 500, 10); // Max is 8 (0-7)
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Seat index invalid", result.Message);
    }

    [Test]
    public void AddPlayer_WhenSeatOccupied_ShouldFail()
    {
        _gameService.AddPlayer("Joko", 500, 0);
        var result = _gameService.AddPlayer("Mulyono", 500, 0);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Seat already occupied", result.Message);
    }

    [Test]
    public void RegisterPlayer_WhenWithValidPlayer_Success()
    {
        var result = _gameService.RegisterPlayer("Prabowo", 1000);
        Assert.IsTrue(result.IsSuccess);
        var player = _gameService.GetPlayerByName("Prabowo");
        Assert.IsNotNull(player);
        Assert.AreEqual(-1, player.SeatIndex);
    }

    [Test]
    public void RegisterPlayer_WhenDuplicate_ShouldFail()
    {
        _gameService.RegisterPlayer("Bahlil", 1000);
        var result = _gameService.RegisterPlayer("Bahlil", 1000);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("PlayerName sudah terdaftar", result.Message);
    }

    [Test]
    public void UpdatePlayerSeat_WhenValidSeatIndex_ShouldReturnSuccess()
    {
        _gameService.RegisterPlayer("Jack", 1000);
        var result = _gameService.UpdatePlayerSeat("Jack", 3);
        Assert.IsTrue(result.IsSuccess);
        var player = _gameService.GetPlayerByName("Jack");
        Assert.AreEqual(3, player.SeatIndex);
    }

    [Test]
    public void UpdatePlayerSeat_WhenInvalidSeat_ShouldFail()
    {
        _gameService.RegisterPlayer("Ahya", 1000);
        var result = _gameService.UpdatePlayerSeat("Ahya", -1);
        Assert.IsFalse(result.IsSuccess);
    }
    [Test]
    public void UpdatePlayerSeat_WhenOccupiedSeat_ShouldFail()
    {
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.RegisterPlayer("Ahya", 1000);
        var result = _gameService.UpdatePlayerSeat("Ahya", 1);
        Assert.IsFalse(result.IsSuccess);
    }
    [Test]
    public void UpdatePlayerSeat_WhenInvalidPlayername_ShouldFail()
    {
        _gameService.RegisterPlayer("Anhar", 1000);
        var result = _gameService.UpdatePlayerSeat("Ahya", 1);
        Assert.IsFalse(result.IsSuccess);
    }
    [Test]
    public void RemovePlayer_WhenPlayerExists_ShouldRemovePlayer()
    {
        _gameService.AddPlayer("Larry", 1000, 0);
        var player = _gameService.GetPlayerByName("Larry");
        var result = _gameService.RemovePlayer(player);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(_gameService.GetPlayerByName("Larry"));
    }

    [Test]
    public void RemovePlayer_WhenNotFound_ShouldFail()
    {
        var player = new Player("Anies", 1000);
        var result = _gameService.RemovePlayer(player);
        Assert.IsFalse(result.IsSuccess);
    }
    [Test]
    public void RemovePlayer_WhenWasCurrentPlayer_ShouldMoveTurnToNextPlayer()
    {
        // Susun: tambahkan 2 pemain
        _gameService.AddPlayer("Larry", 1000, 0);
        _gameService.AddPlayer("Squilliam", 1000, 1);

        // Mulai ronde agar ada giliran aktif
        _gameService.StartRound();

        // Ambil current player
        var currentPlayer = _gameService.GetCurrentPlayer();

        // Pastikan Larry adalah current player (opsional jika sistem seat-based)
        Assert.AreEqual("Larry", currentPlayer.Name);

        // Hapus current player
        var result = _gameService.RemovePlayer(currentPlayer);

        Assert.IsTrue(result.IsSuccess);

        // Ambil current player setelah penghapusan
        var newCurrentPlayer = _gameService.GetCurrentPlayer();

        // Pastikan giliran pindah ke Squilliam
        Assert.AreEqual("Squilliam", newCurrentPlayer.Name);
    }
    [Test]
    public void ActivePlayers_WhenPlayerIsFolded_ShouldExcludePlayer()
    {
        // Arrange
        _gameService.AddPlayer("Larry", 1000, 0);
        _gameService.AddPlayer("Squilliam", 1000, 1);

        var larry = _gameService.GetPlayerByName("Larry");
        var squilliam = _gameService.GetPlayerByName("Squilliam");

        // Set state
        _gameService.PlayerMap[larry].State = PlayerState.Active;
        _gameService.PlayerMap[squilliam].State = PlayerState.Folded;


        // Act
        var activePlayers = _gameService.ActivePlayers();

        // Assert
        Assert.AreEqual(1, activePlayers.Count);
        Assert.AreEqual("Larry", activePlayers[0].Name);
    }
    [Test]
    public void ActivePlayers_WhenPlayerIsAllIn_ShouldIncludePlayer()
    {
        // Arrange
        _gameService.AddPlayer("Larry", 1000, 0);
        _gameService.AddPlayer("Squilliam", 1000, 1);

        var larry = _gameService.GetPlayerByName("Larry");
        var squilliam = _gameService.GetPlayerByName("Squilliam");

        larry.State = PlayerState.Active;
        squilliam.State = PlayerState.AllIn;

        // Act
        var activePlayers = _gameService.ActivePlayers();

        // Assert
        Assert.AreEqual(2, activePlayers.Count);
    }

    [Test]
    public void ActivePlayers_WhenPlayerHasNoSeat_ShouldExcludePlayer()
    {
        // Arrange
        _gameService.RegisterPlayer("Larry", 1000); // SeatIndex = -1
        var larry = _gameService.GetPlayerByName("Larry");

        larry.State = PlayerState.Active;

        // Act
        var activePlayers = _gameService.ActivePlayers();

        // Assert
        Assert.AreEqual(0, activePlayers.Count);
    }
    #endregion

    #region Round Management Tests
    [Test]
    public void NextPhase_WhenRoundActive_ShouldAdvanceGamePhase()
    {
        // Setup game with >2 players
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound(); // PreFlop

        // PreFlop -> Flop
        var result = _gameService.NextPhase();
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(GamePhase.Flop, _gameService.Phase);
        Assert.AreEqual(3, _gameService.CommunityCards.Count);

        // Flop -> Turn
        _gameService.NextPhase();
        Assert.AreEqual(GamePhase.Turn, _gameService.Phase);
        Assert.AreEqual(4, _gameService.CommunityCards.Count);

        // Turn -> River
        _gameService.NextPhase();
        Assert.AreEqual(GamePhase.River, _gameService.Phase);
        Assert.AreEqual(5, _gameService.CommunityCards.Count);

        // River -> Showdown
        _gameService.NextPhase();
        Assert.AreEqual(GamePhase.Showdown, _gameService.Phase);
    }

    [Test]
    public void NextPhase_WhenNoRound_ShouldFail()
    {
        var result = _gameService.NextPhase();
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("No round in progress", result.Message);
    }

    [Test]
    public void ResetGame_WhenGameIsActive_ShouldClearAllGameState()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.StartRound();

        var result = _gameService.ResetGame();

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(_gameService.IsRoundActive);
        Assert.AreEqual(0, _gameService.GetTotalPlayers());
        Assert.AreEqual(0, _gameService.CommunityCards.Count);
        Assert.AreEqual(GamePhase.PreFlop, _gameService.Phase);
    }
    #endregion

    #region Betting Logic Tests
    [Test]
    public void HandleBet_WhenCalledOnPlayersTurn_ShouldDeductChipsAndIncreasePot()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        var result = _gameService.HandleBet(anhar, 100);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(900, anhar.ChipStack);
        Assert.AreEqual(100, _gameService.CurrentBet);
        Assert.AreEqual(100, _gameService.Pot.TotalChips);
    }

    [Test]
    public void HandleBet_WhenNotPlayersTurn_ShouldReturnFailure()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var ahya = _gameService.GetPlayerByName("Ahya"); // Anhar is first (SB/Active) usually, or explicitly check Turn
                                                         // Note: StartRound sets first player. 
                                                         // If Anhar is Seat 0, Ahya Seat 1.
                                                         // Let's assume Anhar is first.

        var result = _gameService.HandleBet(ahya, 100);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("It is not your turn", result.Message);
    }

    [Test]
    public void HandleCall_WhenCalledWithValidState_ShouldMatchCurrentBet()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        var ahya = _gameService.GetPlayerByName("ahya");

        // Anhar Bets
        _gameService.HandleBet(anhar, 100);

        // ahya Calls
        var result = _gameService.HandleCall(ahya);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(900, ahya.ChipStack);
        Assert.AreEqual(200, _gameService.Pot.TotalChips);
    }
    [Test]
    public void HandleCall_WhenToCallIsNegative_ShouldReturnFailure()
    {
        // Arrange
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");

        // Paksa state invalid
        _gameService.SetCurrentBetForTest(100);
        _gameService.PlayerMap[anhar].CurrentBet = 200; // Lebih besar dari CurrentBet

        // Act
        var result = _gameService.HandleCall(anhar);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Invalid call: Current bet is lower than your contribution", result.Message);
    }
    

    [Test]
    public void HandleRaise_WhenValidRaise_ShouldIncreaseCurrentBetAndDeductChips()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        var ahya = _gameService.GetPlayerByName("Ahya");

        // Anhar Bets 100
        _gameService.HandleBet(anhar, 100);

        // Ahya Raises 200 (Total 300 to match + raise?)
        // Helper: Raise amount is ON TOP of the Call.
        // CurrentBet = 100. Ahya needs to Call 100 + Raise 200 = 300 total?
        // Let's check logic:
        // toCall = CurrentBet - status.CurrentBet (100 - 0 = 100)
        // totalRequirement = toCall + raiseAmount (100 + 200 = 300)

        var result = _gameService.HandleRaise(ahya, 200);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(700, ahya.ChipStack); // 1000 - 300
        Assert.AreEqual(300, _gameService.CurrentBet); // Raise updates CurrentBet to status.CurrentBet (300)
    }

    [Test]
    public void HandleFold_WhenCalledOnPlayersTurn_ShouldMarkPlayerAsFolded()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.AddPlayer("Huda", 1000, 2);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        var result = _gameService.HandleFold(anhar);

        Assert.IsTrue(result.IsSuccess);

        Assert.AreEqual(PlayerState.Folded, _gameService.PlayerMap[anhar].State);
    }

    [Test]
    public void HandleCheck_WhenNoActiveBet_ShouldReturnSuccess()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        // Initial state: CurrentBet is 0.
        var anhar = _gameService.GetPlayerByName("Anhar");
        var result = _gameService.HandleCheck(anhar);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public void HandleCheck_WhenActiveBetExists_ShouldReturnFailure()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        _gameService.HandleBet(anhar, 100);

        var ahya = _gameService.GetPlayerByName("Ahya");
        var result = _gameService.HandleCheck(ahya);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Cannot check when there is an active bet. You must Call, Raise, or Fold.", result.Message);
    }

    [Test]
    public void HandleAllIn_WhenCalledWithValidPlayer_ShouldMoveAllChipsToPot()
    {
        _gameService.AddPlayer("Anhar", 1000, 0); // 1000 chips
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var anhar = _gameService.GetPlayerByName("Anhar");
        var result = _gameService.HandleAllIn("Anhar");

        Assert.IsTrue(result.IsSuccess);

        Assert.AreEqual(0, anhar.ChipStack);
        Assert.AreEqual(PlayerState.AllIn, _gameService.PlayerMap[anhar].State);
        Assert.AreEqual(1000, _gameService.Pot.TotalChips);
    }
    #endregion
    #region Information and Evaluation Tests
    [Test]
    public void GetPlayersPublicState_WhenRoundActive_ShouldReturnPlayersPublicInfo()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        var states = _gameService.GetPlayersPublicState().ToList();

        Assert.AreEqual(2, states.Count);
        Assert.AreEqual("Anhar", states[0].Name);
        Assert.IsNotEmpty(states[0].State);
        Assert.IsNotNull(states[0].Hand);
    }

    [Test]
    public void EvaluateVisibleForPlayer_WhenPlayerExists_ShouldReturnEvaluation()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.StartRound();

        var result = _gameService.EvaluateVisibleForPlayer("Anhar");
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }
    [Test]
    public void EvaluateVisibleForPlayer_WhenPlayerNotFound_ShouldReturnFailure()
    {
        // Arrange
        _gameService.AddPlayer("Anhar", 1000, 0);

        // Act
        var result = _gameService.EvaluateVisibleForPlayer("Ahya"); // Tidak ada

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Player not found", result.Message);
        Assert.IsNull(result.Data);
    }

    [Test]
    public void GetShowdownDetails_WhenCalled_ShouldReturnShowdownData()
    {
        _gameService.AddPlayer("Anhar", 1000, 0);
        _gameService.AddPlayer("Ahya", 1000, 1);
        _gameService.StartRound();

        // Force state to Showdown or simulate steps
        // Simplest is to just call GetShowdownDetails even if not in Showdown phase (it calculates winners based on current cards)
        // GameService.cs:150 logic handles it.

        var result = _gameService.GetShowdownDetails();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }
    [Test]
    public void ResolveShowdownDetailed_WhenNotShowdownAndMultipleActive_ShouldReturnEmpty()
    {
        _gameService.SetPhase(GamePhase.Turn);

        // 2 active players

        var p1 = new Player("Anhar", 1000) { SeatIndex = 0 };
        var p2 = new Player("Ahya", 1000) { SeatIndex = 1 };

        _gameService.LoadPlayers(new List<IPlayer> { p1, p2 });

        var result = _gameService.ResolveShowdownDetailed();

        Assert.IsEmpty(result.winners);
        Assert.AreEqual(HandRank.HighCard, result.rank);
    }
    [Test]
    public void ResolveShowdownDetailed_WhenSingleWinner_ShouldAwardFullPot()
    {
        _gameService.SetPhase(GamePhase.Showdown);

        var players = TestHelper.CreatePlayers(2, 1000);
        _gameService.LoadPlayers(players);

        _gameService.Pot.AddChips(200);

        // Mock EvaluateHands
        _gameService.OverrideHandEvaluation(new Dictionary<IPlayer, HandRank>
    {
        { players[0], HandRank.Flush },
        { players[1], HandRank.Pair }
    });

        var result = _gameService.ResolveShowdownDetailed();

        Assert.AreEqual(1, result.winners.Count);
        Assert.AreEqual(players[0], result.winners.First());
        Assert.AreEqual(1200, players[0].ChipStack);
        Assert.AreEqual(0, _gameService.Pot.TotalChips);
        Assert.IsFalse(_gameService.IsRoundActive);
    }

    [Test]
    public void ResolveShowdownDetailed_WithSplitPot_ShouldDivideEvenly()
    {
        _gameService.SetPhase(GamePhase.Showdown);

        var players = TestHelper.CreatePlayers(2, 1000);
        _gameService.LoadPlayers(players);

        _gameService.Pot.AddChips(200);

        _gameService.OverrideHandEvaluation(new Dictionary<IPlayer, HandRank>
    {
        { players[0], HandRank.Straight },
        { players[1], HandRank.Straight }
    });

        var result = _gameService.ResolveShowdownDetailed();

        Assert.AreEqual(2, result.winners.Count);
        Assert.AreEqual(1100, players[0].ChipStack);
        Assert.AreEqual(1100, players[1].ChipStack);
    }
    [Test]
    public void ResolveShowdownDetailed_WhenCompleted_ShouldResetRoundState()
    {
        _gameService.SetPhase(GamePhase.Showdown);

        var players = TestHelper.CreatePlayers(2, 1000);
        _gameService.LoadPlayers(players);

        _gameService.Pot.AddChips(100);

        _gameService.OverrideHandEvaluation(new Dictionary<IPlayer, HandRank>
    {
        { players[0], HandRank.FullHouse },
        { players[1], HandRank.Pair }
    });

        _gameService.ResolveShowdownDetailed();

        foreach (var status in _gameService.PlayerMap.Values)
        {
            Assert.AreEqual(0, status.CurrentBet);
            Assert.IsFalse(status.HasActed);
            Assert.AreEqual(PlayerState.Active, status.State);
        }

        Assert.AreEqual(GamePhase.PreFlop, _gameService.Phase);
    }
    [Test]
    public void GetPlayersPublicState_WhenCalled_ShouldMapBasicPlayerInformation()
    {
        var players = TestHelper.CreatePlayers(1, 1000);
        _gameService.LoadPlayers(players);

        var result = _gameService.GetPlayersPublicState().ToList();
        var dto = result.First();

        Assert.AreEqual(players[0].SeatIndex, dto.SeatIndex);
        Assert.AreEqual(players[0].Name, dto.Name);
        Assert.AreEqual(players[0].ChipStack, dto.ChipStack);
    }
    [Test]
    public void GetPlayersPublicState_WhenPlayerFolded_ShouldSetIsFolded()
    {
        var players = TestHelper.CreatePlayers(1, 1000);
        _gameService.LoadPlayers(players);

        _gameService.PlayerMap[players[0]].State = PlayerState.Folded;

        var dto = _gameService.GetPlayersPublicState().First();

        Assert.IsTrue(dto.IsFolded);
        Assert.AreEqual("Folded", dto.State);
    }
    [Test]
    public void GetPlayersPublicState_WhenNoCards_ShouldReturnEmptyPossibleHandRank()
    {
        var players = TestHelper.CreatePlayers(1, 1000);
        _gameService.LoadPlayers(players);

        var dto = _gameService.GetPlayersPublicState().First();

        Assert.AreEqual(string.Empty, dto.PossibleHandRank);
    }
    #endregion
}

