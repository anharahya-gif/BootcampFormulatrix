using System;
using UnoGame.Core;

namespace UnoGame.Cards
{
    // ===== ENUM =====
    public enum CardColor
    {
        Red,
        Yellow,
        Green,
        Blue
    }

    // ===== BASE CARD =====
    public abstract class Card
    {
        public CardColor? Color { get; protected set; }
        public abstract bool CanBePlayedOn(Card topCard);
    }

    // ===== NUMBER CARD =====
    public class NumberCard : Card
    {
        public int Number { get; }

        public NumberCard(CardColor color, int number)
        {
            Color = color;
            Number = number;
        }

        public override bool CanBePlayedOn(Card topCard)
        {
            if (topCard is NumberCard numberCard)
                return Number == numberCard.Number || Color == numberCard.Color;

            return Color == topCard.Color;
        }
    }

    // ===== ACTION CARD =====
    public abstract class ActionCard : Card
    {
        public override bool CanBePlayedOn(Card topCard)
        {
            return Color == topCard.Color || GetType() == topCard.GetType();
        }

        public abstract void ApplyEffect(Core.IGameContext context);
    }

    // ===== SKIP =====
    public class SkipCard : ActionCard
    {
        public SkipCard(CardColor color)
        {
            Color = color;
        }

        public override void ApplyEffect(IGameContext context)
        {
            context.SkipNextPlayer();
        }
    }

    // ===== REVERSE =====
    public class ReverseCard : ActionCard
    {
        public ReverseCard(CardColor color)
        {
            Color = color;
        }

        public override void ApplyEffect(IGameContext context)
        {
            context.ReverseDirection();
        }
    }

    // ===== DRAW TWO =====
    public class DrawTwoCard : ActionCard
    {
        public DrawTwoCard(CardColor color)
        {
            Color = color;
        }

        public override void ApplyEffect(IGameContext context)
        {
            context.SkipNextPlayer();
        }
    }

    // ===== WILD =====
    public class WildCard : Card
    {
        public CardColor? ChosenColor { get; private set; }

        public WildCard()
        {
            Color = null;
        }

        public override bool CanBePlayedOn(Card topCard)
        {
            return true;
        }

        public void ChooseColor(CardColor color)
        {
            ChosenColor = color;
        }

        public void ApplyEffect(IGameContext context)
        {
            if (ChosenColor == null)
                throw new InvalidOperationException("Wild color not chosen!");

            context.SetCurrentColor(ChosenColor.Value);
        }
    }

        public interface IWildDrawFourCard
        {
            CardColor? ChosenColor { get; }

            void ApplyEffect(IGameContext context);
            bool CanBePlayedOn(Card topCard);
            void ChooseColor(CardColor color);
        }
    
        // ===== WILD DRAW FOUR =====
        public class WildDrawFourCard : Card, IWildDrawFourCard
        {
            public CardColor? ChosenColor { get; private set; }

            public WildDrawFourCard()
            {
                Color = null; // wild card tidak punya warna awal
            }

            public override bool CanBePlayedOn(Card topCard)
            {
                return true;
            }

            public void ChooseColor(CardColor color)
            {
                ChosenColor = color;
            }

            public void ApplyEffect(IGameContext context)
            {
                if (ChosenColor == null)
                    throw new InvalidOperationException("Wild Draw Four color not chosen!");

                context.SetCurrentColor(ChosenColor.Value);
                context.ForceDraw(context.CurrentPlayer, 4);
                context.SkipNextPlayer();
            }
        }
    }

