using PokerAPIMPwDB.Domain.Enums;
using System;

namespace PokerAPIMPwDB.DTO.Table
{
    public class JoinTableDto
    {
        public Guid PlayerId { get; set; }
        public string? DisplayName { get; set; }
        public int ChipStack { get; set; }
        public int SeatIndex { get; set; }
    }
}

