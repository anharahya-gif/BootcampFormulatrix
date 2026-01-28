namespace PokerAPI.Models
{
    public class Pot
    {
        public int TotalChips { get; private set; } = 0;
        public void AddChips(int amount) => TotalChips += amount;
        public void Reset() => TotalChips = 0;
    }
}
