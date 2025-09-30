using AutoMapper;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
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
			ConfigureHomestayMappings();
			ConfigurePropertyTypeMappings();
		}

		private void ConfigureUserMappings()
		{
			CreateMap<User, UserProfileDto>()
					.ForMember(dest => dest.Roles, opt => opt.Ignore());
			CreateMap<User, UserProfileDto>();
			CreateMap<CreateUserDto, User>();
			CreateMap<UpdateUserDto, User>();
		}

		public void ConfigureHomestayMappings()
		{
			CreateMap<CreateHomestayDto, Homestay>();
			CreateMap<UpdateHomestayDto, Homestay>();
			CreateMap<Homestay, HomestayDto>();
		}

		public void ConfigurePropertyTypeMappings()
		{
			CreateMap<CreatePropertyTypeDto, PropertyType>();
			CreateMap<UpdateHomestayDto, PropertyType>();
			CreateMap<PropertyType, PropertyTypeDto>();
		}
	}
}
