using PokerAPI.Services.Interfaces;
namespace PokerAPI.Models
{
    public class PlayerStatus
    {
        public List<ICard> Hand { get; set; } = new List<ICard>();
        public PlayerState State { get; set; } = PlayerState.Active;
        public int CurrentBet { get; set; } = 0;
        public bool HasActed { get; set; }


        public void ResetStatus()
        {
            Hand.Clear();
            State = PlayerState.Active;
            CurrentBet = 0;
            HasActed = false;
        }
    }
}
