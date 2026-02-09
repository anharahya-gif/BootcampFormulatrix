using PokerAPI.Models;
using System.Collections.Generic;

namespace PokerAPI.Services.Interfaces
{
    public interface IDeck
    {

        void Shuffle();


        ICard? Draw();


        int RemainingCards();
    }
}
