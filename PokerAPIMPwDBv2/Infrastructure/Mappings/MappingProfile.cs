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
            CreateMap<Table, TableInfoDto>()
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.PlayerCount, opt => opt.MapFrom(src => src.PlayerSeats.Count(ps => ps.PlayerId != null)));

            CreateMap<Table, TableStateDto>()
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.Id));

            // Player Mappings
            CreateMap<Player, PlayerPublicStateDto>()
                .ForMember(dest => dest.SeatIndex, opt => opt.MapFrom(src => src.PlayerSeat != null ? src.PlayerSeat.SeatNumber : -1));

            // User Mappings
            CreateMap<User, UserInfoDto>();
        }
    }
}
