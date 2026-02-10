using NUnit.Framework;
using Moq;
using PokerAPI.Services;
using PokerAPI.Models;
using Serilog;
using System.Linq;

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
    public void StartRound_WithTwoPlayers_ShouldInitializeRound()
    {
        // Arrange: 2 pemain seated
        var add1 = _gameService.AddPlayer("Alice", 1000, 0);
        var add2 = _gameService.AddPlayer("Bob", 1000, 1);
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
    public void StartRound_WithOnePlayer_ShouldFail()
    {
        // Arrange: hanya 1 pemain
        var add1 = _gameService.AddPlayer("Alice", 1000, 0);
        Assert.IsTrue(add1.IsSuccess);

        // Act
        var result = _gameService.StartRound();

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Cannot start round. Ensure at least 2 players are seated.", result.Message);
        Assert.IsFalse(_gameService.IsRoundActive);
    }

    [Test]
    public void StartRound_AlreadyStarted_ShouldFail()
    {
        // Arrange: 2 pemain seated
        var add1 = _gameService.AddPlayer("Alice", 1000, 0);
        var add2 = _gameService.AddPlayer("Bob", 1000, 1);
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
    public void AddPlayer_WithValidData_ShouldSuccess()
    {
        var result = _gameService.AddPlayer("Charlie", 500, 2);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, _gameService.GetTotalPlayers());
    }

    [Test]
    public void AddPlayer_DuplicateName_ShouldFail()
    {
        _gameService.AddPlayer("Dave", 500, 0);
        var result = _gameService.AddPlayer("Dave", 500, 1);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Player already exists", result.Message);
    }

    [Test]
    public void AddPlayer_InvalidSeatIndex_ShouldFail()
    {
        var result = _gameService.AddPlayer("Eve", 500, 10); // Max is 8 (0-7)
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Seat index invalid", result.Message);
    }

    [Test]
    public void AddPlayer_SeatOccupied_ShouldFail()
    {
        _gameService.AddPlayer("Frank", 500, 0);
        var result = _gameService.AddPlayer("Grace", 500, 0);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Seat already occupied", result.Message);
    }

    [Test]
    public void RegisterPlayer_Success()
    {
        var result = _gameService.RegisterPlayer("Hank", 1000);
        Assert.IsTrue(result.IsSuccess);
        var player = _gameService.GetPlayerByName("Hank");
        Assert.IsNotNull(player);
        Assert.AreEqual(-1, player.SeatIndex);
    }

    [Test]
    public void RegisterPlayer_Duplicate_ShouldFail()
    {
        _gameService.RegisterPlayer("Ivy", 1000);
        var result = _gameService.RegisterPlayer("Ivy", 1000);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("PlayerName sudah terdaftar", result.Message);
    }

    [Test]
    public void UpdatePlayerSeat_Success()
    {
        _gameService.RegisterPlayer("Jack", 1000);
        var result = _gameService.UpdatePlayerSeat("Jack", 3);
        Assert.IsTrue(result.IsSuccess);
        var player = _gameService.GetPlayerByName("Jack");
        Assert.AreEqual(3, player.SeatIndex);
    }

    [Test]
    public void UpdatePlayerSeat_InvalidSeat_ShouldFail()
    {
        _gameService.RegisterPlayer("Kevin", 1000);
        var result = _gameService.UpdatePlayerSeat("Kevin", -1);
        Assert.IsFalse(result.IsSuccess);
    }

    [Test]
    public void RemovePlayer_Success()
    {
        _gameService.AddPlayer("Larry", 1000, 0);
        var player = _gameService.GetPlayerByName("Larry");
        var result = _gameService.RemovePlayer(player);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(_gameService.GetPlayerByName("Larry"));
    }

    [Test]
    public void RemovePlayer_NotFound_ShouldFail()
    {
        var player = new Player("Ghost", 1000);
        var result = _gameService.RemovePlayer(player);
        Assert.IsFalse(result.IsSuccess);
    }
    #endregion

    #region Round Management Tests
    [Test]
    public void NextPhase_Success()
    {
        // Setup game with >2 players
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
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
    public void NextPhase_NoRound_ShouldFail()
    {
        var result = _gameService.NextPhase();
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("No round in progress", result.Message);
    }

    [Test]
    public void ResetGame_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
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
    public void HandleBet_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound(); 

        var alice = _gameService.GetPlayerByName("Alice");
        var result = _gameService.HandleBet(alice, 100);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(900, alice.ChipStack);
        Assert.AreEqual(100, _gameService.CurrentBet);
        Assert.AreEqual(100, _gameService.Pot.TotalChips);
    }

    [Test]
    public void HandleBet_Failure_NotTurn()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var bob = _gameService.GetPlayerByName("Bob"); // Alice is first (SB/Active) usually, or explicitly check Turn
        // Note: StartRound sets first player. 
        // If Alice is Seat 0, Bob Seat 1.
        // Let's assume Alice is first.
        
        var result = _gameService.HandleBet(bob, 100);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("It is not your turn", result.Message);
    }

    [Test]
    public void HandleCall_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var alice = _gameService.GetPlayerByName("Alice");
        var bob = _gameService.GetPlayerByName("Bob");

        // Alice Bets
        _gameService.HandleBet(alice, 100);
        
        // Bob Calls
        var result = _gameService.HandleCall(bob);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(900, bob.ChipStack);
        Assert.AreEqual(200, _gameService.Pot.TotalChips);
    }

    [Test]
    public void HandleRaise_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var alice = _gameService.GetPlayerByName("Alice");
        var bob = _gameService.GetPlayerByName("Bob");

        // Alice Bets 100
        _gameService.HandleBet(alice, 100);

        // Bob Raises 200 (Total 300 to match + raise?)
        // Helper: Raise amount is ON TOP of the Call.
        // CurrentBet = 100. Bob needs to Call 100 + Raise 200 = 300 total?
        // Let's check logic:
        // toCall = CurrentBet - status.CurrentBet (100 - 0 = 100)
        // totalRequirement = toCall + raiseAmount (100 + 200 = 300)
        
        var result = _gameService.HandleRaise(bob, 200);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(700, bob.ChipStack); // 1000 - 300
        Assert.AreEqual(300, _gameService.CurrentBet); // Raise updates CurrentBet to status.CurrentBet (300)
    }

    [Test]
    public void HandleFold_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.AddPlayer("Charlie", 1000, 2);
        _gameService.StartRound();

        var alice = _gameService.GetPlayerByName("Alice");
        var result = _gameService.HandleFold(alice);

        Assert.IsTrue(result.IsSuccess);

        Assert.AreEqual(PlayerState.Folded, _gameService.PlayerMap[alice].State);
    }

    [Test]
    public void HandleCheck_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        // Initial state: CurrentBet is 0.
        var alice = _gameService.GetPlayerByName("Alice");
        var result = _gameService.HandleCheck(alice);

        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public void HandleCheck_Failure_WithBet()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var alice = _gameService.GetPlayerByName("Alice");
        _gameService.HandleBet(alice, 100);

        var bob = _gameService.GetPlayerByName("Bob");
        var result = _gameService.HandleCheck(bob);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Cannot check when there is an active bet. You must Call, Raise, or Fold.", result.Message);
    }

    [Test]
    public void HandleAllIn_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0); // 1000 chips
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var alice = _gameService.GetPlayerByName("Alice");
        var result = _gameService.HandleAllIn("Alice");

        Assert.IsTrue(result.IsSuccess);

        Assert.AreEqual(0, alice.ChipStack);
        Assert.AreEqual(PlayerState.AllIn, _gameService.PlayerMap[alice].State);
        Assert.AreEqual(1000, _gameService.Pot.TotalChips);
    }
    #endregion
    #region Information and Evaluation Tests
    [Test]
    public void GetPlayersPublicState_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();

        var states = _gameService.GetPlayersPublicState().ToList();
        
        Assert.AreEqual(2, states.Count);
        Assert.AreEqual("Alice", states[0].Name);
        Assert.IsNotEmpty(states[0].State);
        Assert.IsNotNull(states[0].Hand);
    }

    [Test]
    public void EvaluateVisibleForPlayer_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.StartRound();

        var result = _gameService.EvaluateVisibleForPlayer("Alice");
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }

    [Test]
    public void GetShowdownDetails_Success()
    {
        _gameService.AddPlayer("Alice", 1000, 0);
        _gameService.AddPlayer("Bob", 1000, 1);
        _gameService.StartRound();
        
        // Force state to Showdown or simulate steps
        // Simplest is to just call GetShowdownDetails even if not in Showdown phase (it calculates winners based on current cards)
        // GameService.cs:150 logic handles it.

        var result = _gameService.GetShowdownDetails();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }
    #endregion
}

