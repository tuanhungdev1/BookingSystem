using AutoMapper;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Application.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			ConfigureUserMappings();
			ConfigureAccommodationMappings();
		}

		private void ConfigureUserMappings()
		{
			CreateMap<User, UserProfileDto>()
					.ForMember(dest => dest.Roles, opt => opt.Ignore());
			CreateMap<User, UserProfileDto>();
			CreateMap<CreateUserDto, User>();
			CreateMap<UpdateUserDto, User>();
		}

		public void ConfigureAccommodationMappings()
		{
			CreateMap<CreateAccommodationRequest, Accommodation>();
			CreateMap<UpdateAccommodationRequest, Accommodation>();
			CreateMap<Accommodation, AccommodationDto>();
		}
	}
}
