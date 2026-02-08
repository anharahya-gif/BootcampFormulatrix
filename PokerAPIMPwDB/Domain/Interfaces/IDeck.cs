using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface IDeck
    {
        void Shuffle();
        ICard Draw();
        int RemainingCards();
    }
}
