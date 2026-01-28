namespace PokerAPI.Models
{
    public class PlayerStatus
    {
        public List<Card> Hand { get; set; } = new List<Card>();
        public PlayerState State { get; set; } = PlayerState.Active;
        public int CurrentBet { get; set; } = 0;

        public void ResetStatus()
        {
            Hand.Clear();
            State = PlayerState.Active;
            CurrentBet = 0;
        }
    }
}
