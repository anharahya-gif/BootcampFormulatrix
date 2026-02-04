using PokerAPI.Models;
using System.Collections.Generic;

namespace PokerAPI.Services.Interfaces
{
    public interface IDeck
    {
        /// <summary>
        /// Acak urutan kartu
        /// </summary>
        void Shuffle();

        /// <summary>
        /// Ambil 1 kartu dari deck
        /// </summary>
        /// <returns>Card yang diambil, atau null jika habis</returns>
        Card Draw();

        /// <summary>
        /// Jumlah kartu tersisa di deck
        /// </summary>
        /// <returns>Integer jumlah kartu</returns>
        int RemainingCards();
    }
}
