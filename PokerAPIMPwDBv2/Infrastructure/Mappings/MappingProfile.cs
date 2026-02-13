using AutoMapper;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.DTO.User;

namespace PokerAPIMPwDB.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Table Mappings
            CreateMap<Table, TableInfoDto>();
            CreateMap<Table, TableStateDto>();

            // Player Mappings
            CreateMap<Player, PlayerPublicStateDto>()
                .ForMember(dest => dest.SeatIndex, opt => opt.MapFrom(src => src.PlayerSeat != null ? src.PlayerSeat.SeatNumber : -1));

            // User Mappings
            CreateMap<User, UserInfoDto>();
        }
    }
}
