using PokerAPIMPwDB.Domain.Enums;
using System;

namespace PokerAPIMPwDB.DTO.Table
{
    public class TableInfoDto
    {
        public Guid TableId { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; } = 6;
        public int PlayerCount { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public TableState State { get; set; }
    }
}
