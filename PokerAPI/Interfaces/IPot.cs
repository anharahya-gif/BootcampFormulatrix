namespace PokerAPI.Services.Interfaces
{
    public interface IPot
    {
        /// <summary>
        /// Total chip di pot saat ini
        /// </summary>
        int TotalChips { get; }

        /// <summary>
        /// Tambahkan chip ke pot
        /// </summary>
        /// <param name="amount">Jumlah chip</param>
        void AddChips(int amount);

        /// <summary>
        /// Reset pot menjadi 0
        /// </summary>
        void Reset();
    }
}
