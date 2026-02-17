using AutoMapper;
using MeetingRoomBookingAPI.Application.DTOs.Auth;
using MeetingRoomBookingAPI.Application.DTOs.Booking;
using MeetingRoomBookingAPI.Application.DTOs.Room;
using MeetingRoomBookingAPI.Application.DTOs.User;
using MeetingRoomBookingAPI.Domain.Entities;

namespace MeetingRoomBookingAPI.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Room Mappings
            CreateMap<Room, RoomReadDto>();
            CreateMap<RoomCreateDto, Room>();
            CreateMap<RoomUpdateDto, Room>();

            // Booking Mappings
            CreateMap<Booking, BookingReadDto>()
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room != null ? src.Room.Name : null))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName : null));
            CreateMap<BookingCreateDto, Booking>();

            // Participant Mappings
            CreateMap<BookingParticipant, ParticipantDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null && src.User.Profile != null ? src.User.Profile.FullName : src.User != null ? src.User.UserName : null));

            // User Mappings
            CreateMap<ApplicationUser, UserReadDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.FullName : null))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.Department : null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.PhoneNumber : null))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.AvatarUrl : null));
            
            CreateMap<UserUpdateDto, UserProfile>();
        }
    }
}
