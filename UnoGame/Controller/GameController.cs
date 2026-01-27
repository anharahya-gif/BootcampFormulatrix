using System;
using System.Collections.Generic;
using System.Linq;
using UnoGame.Model;
using UnoGame.View;

namespace UnoGame.Controller
{
    public class GameController
    {
        public List<Player> Players { get; }
        public Deck Deck { get; }
        public DiscardPile DiscardPile { get; }
        private Card? _lastPlayedCard;
        private Player? _lastPlayer;


        public int CurrentPlayerIndex { get; private set; }
        public Direction Direction { get; private set; }
        public CardColor CurrentColor { get; private set; }
        public bool IsGameOver { get; private set; }

        private readonly IGameView _view;
        private readonly IGameInput _input;

        public GameController(
            List<Player> players,
            Deck deck,
            DiscardPile discardPile,
            IGameView view,
            IGameInput input)
        {
            Players = players;
            Deck = deck;
            DiscardPile = discardPile;
            _view = view;
            _input = input;

            Direction = Direction.Clockwise;
            CurrentPlayerIndex = 0;
            IsGameOver = false;
        }
        private void BuildNewDeck()
        {
            Deck.Cards.Clear();
            DiscardPile.Cards.Clear();

            foreach (CardColor color in Enum.GetValues<CardColor>())
            {
                Deck.Cards.Push(new Card(CardType.Number, color, 0));

                for (int i = 1; i <= 9; i++)
                {
                    Deck.Cards.Push(new Card(CardType.Number, color, i));
                    Deck.Cards.Push(new Card(CardType.Number, color, i));
                }

                Deck.Cards.Push(new Card(CardType.Skip, color));
                Deck.Cards.Push(new Card(CardType.Skip, color));

                Deck.Cards.Push(new Card(CardType.Reverse, color));
                Deck.Cards.Push(new Card(CardType.Reverse, color));

                Deck.Cards.Push(new Card(CardType.DrawTwo, color));
                Deck.Cards.Push(new Card(CardType.DrawTwo, color));
            }

            for (int i = 0; i < 4; i++)
            {
                Deck.Cards.Push(new Card(CardType.Wild));
                Deck.Cards.Push(new Card(CardType.WildDrawFour));
            }
        }
        private Player GetNextPlayerForView()
        {
            return GetNextPlayer(1);
        }


        public void StartGame()
        {
            BuildNewDeck();
            ShuffleDeck();
            DealInitialCards();

            var firstCard = DrawFromDeck();
            DiscardPile.Cards.Push(firstCard);
            CurrentColor = firstCard.Color ?? CardColor.Red;

            _view.ShowGameStart(Players);

            while (!IsGameOver)
            {
                PlayTurn();
            }
        }


        public void PlayTurn()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            var nextPlayer = GetNextPlayerForView();

            _view.ShowGameStatus(
                Deck.Cards.Count,
                DiscardPile.Cards.Count,
                Players.ToDictionary(p => p, p => p.CardCount),
                _lastPlayedCard,
                DiscardPile.Cards.Peek(),
                CurrentColor,
                _lastPlayer,
                currentPlayer,
                nextPlayer,
                Direction
            );

            _view.ShowPlayerTurn(currentPlayer);

            Card? chosenCard;

            if (currentPlayer is HumanPlayer)
            {
                chosenCard = _input.ChooseCard(currentPlayer);
            }
            else
            {
                Thread.Sleep(600); // bot thinking 🤖
                chosenCard = GetBotMove(currentPlayer);
            }

            if (chosenCard == null)
            {
                DrawCard(currentPlayer);
                MoveToNextPlayer(1);
                return;
            }

            if (!CanPlayCard(currentPlayer, chosenCard))
            {
                DrawCard(currentPlayer);
                MoveToNextPlayer(1);
                return;
            }

            PlayCard(currentPlayer, chosenCard);
        }


        public void PlayCard(Player player, Card card)
        {
            _lastPlayedCard = card;
            _lastPlayer = player;
            RemoveCardFromPlayer(player, card);
            DiscardPile.Cards.Push(card);

            if (card.Color.HasValue)
                CurrentColor = card.Color.Value;

            _view.ShowCardPlayed(player, card);

            HandleCardEffect(card);
            CheckUno(player);

            if (player.Cards.Sum(c => c.Value.Count) == 0)
            {
                EndGame(player);
                return;
            }

            MoveToNextPlayer(1);
        }

        public void DrawCard(Player player)
        {
            var card = DrawFromDeck();
            AddCardToPlayer(player, card);
            _view.ShowCardDrawn(player, card);
        }
        private bool CanPlayCard(Player player, Card card)
        {
            var top = DiscardPile.Cards.Peek();

            if (card.Type == CardType.Wild || card.Type == CardType.WildDrawFour)
                return true;

            if (card.Color == CurrentColor)
                return true;

            if (card.Type == top.Type && card.Type != CardType.Number)
                return true;

            if (card.Type == CardType.Number && top.Type == CardType.Number
                && card.Number == top.Number)
                return true;

            return false;
        }
        private void HandleCardEffect(Card card)
        {
            switch (card.Type)
            {
                case CardType.Skip:
                    MoveToNextPlayer(1);
                    break;

                case CardType.Reverse:
                    ReverseDirection();
                    _view.ShowDirectionChanged(Direction);
                    break;

                case CardType.DrawTwo:
                    var next = GetNextPlayer(1);
                    DrawCard(next);
                    DrawCard(next);
                    MoveToNextPlayer(1);
                    break;

                case CardType.Wild:
                    CurrentColor = _input.ChooseColor(CurrentPlayer());
                    break;

                case CardType.WildDrawFour:
                    var target = GetNextPlayer(1);
                    for (int i = 0; i < 4; i++)
                        DrawCard(target);

                    CurrentColor = _input.ChooseColor(CurrentPlayer());
                    MoveToNextPlayer(1);
                    break;
            }
        }
        private void CheckUno(Player player)
        {
            if (player.CardCount == 1)
            {
                if (player is BotPlayer)
                {
                    player.HasCalledUno = true;
                    _view.ShowUnoCalled(player);
                }
                else
                {
                    player.HasCalledUno = _input.CallUno(player);
                }
            }
        }


        private void ApplyUnoPenalty(Player player)
        {
            _view.ShowPenalty(
                player,
                "Failed to call UNO",
                2
            );

            DrawCard(player);
            DrawCard(player);

            _view.ShowUnoPenalty(player);
        }

        private void MoveToNextPlayer(int skip)
        {
            int step = Direction == Direction.Clockwise ? skip : -skip;
            CurrentPlayerIndex = (CurrentPlayerIndex + step + Players.Count) % Players.Count;
        }

        private void ReverseDirection()
        {
            Direction = Direction == Direction.Clockwise
                ? Direction.CounterClockwise
                : Direction.Clockwise;
        }

        private Player GetNextPlayer(int offset)
        {
            int step = Direction == Direction.Clockwise ? offset : -offset;
            int index = (CurrentPlayerIndex + step + Players.Count) % Players.Count;
            return Players[index];
        }

        private Player CurrentPlayer()
        {
            return Players[CurrentPlayerIndex];
        }
        private void ShuffleDeck()
        {
            var rnd = new Random();

            var cards = Deck.Cards.ToList();
            Deck.Cards.Clear();

            foreach (var card in cards.OrderBy(c => rnd.Next()))
            {
                Deck.Cards.Push(card);
            }
        }


        private Card DrawFromDeck()
        {
            if (Deck.Cards.Count == 0)
                RefillDeckFromDiscard();

            return Deck.Cards.Pop();
        }

        private void RefillDeckFromDiscard()
        {
            var top = DiscardPile.Cards.Pop();

            var refill = DiscardPile.Cards.ToList();
            DiscardPile.Cards.Clear();
            DiscardPile.Cards.Push(top);

            var rnd = new Random();
            foreach (var card in refill.OrderBy(_ => rnd.Next()))
                Deck.Cards.Push(card);
        }


        private void AddCardToPlayer(Player player, Card card)
        {
            if (!player.Cards.ContainsKey(card.Type))
                player.Cards[card.Type] = new List<Card>();

            player.Cards[card.Type].Add(card);
        }

        private void RemoveCardFromPlayer(Player player, Card card)
        {
            player.Cards[card.Type].Remove(card);
        }

        private void DealInitialCards()
        {
            foreach (var player in Players)
            {
                for (int i = 0; i < 7; i++)
                    AddCardToPlayer(player, DrawFromDeck());
            }
        }

        private void EndGame(Player winner)
        {
            IsGameOver = true;
            _view.ShowGameOver(winner);
        }

        private Card? GetBotMove(Player bot)
        {
            var cards = bot.Cards.SelectMany(c => c.Value).ToList();

            // 1️⃣ cari kartu yang bisa dimainkan
            foreach (var card in cards)
            {
                if (CanPlayCard(bot, card))
                    return card;
            }

            // 2️⃣ tidak ada → draw
            return null;
        }

    }
}
