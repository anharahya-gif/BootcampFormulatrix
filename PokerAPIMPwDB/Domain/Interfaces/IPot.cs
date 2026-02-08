namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface IPot
    {
        int TotalChips { get; }
        void AddChips(int amount);
        void Reset();
    }
}
