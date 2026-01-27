using UnoGame.Model;

namespace UnoGame.View
{
    public interface IGameInput
    {
        Card? ChooseCard(Player player);
        CardColor ChooseColor(Player player);
        bool CallUno(Player player);
    }
}
