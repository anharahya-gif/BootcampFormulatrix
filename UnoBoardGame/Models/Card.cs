using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  
using System.Threading.Tasks;
namespace UnoBoardGame.Models;
public class Card
{
    public CardType Type { get; set; }
    public CardColor? Color { get; set; }
    public int? Number { get; set; }

    public override string ToString()
    {
        if (Type == CardType.Number)
            return $"{Color} {Number}";
        if (Type == CardType.Wild || Type == CardType.WildDrawFour)
            return Type.ToString();
        return $"{Color} {Type}";
    }       
}