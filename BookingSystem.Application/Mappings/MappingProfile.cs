using AutoMapper;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HostProfileDTO;
using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Application.DTOs.RuleDTO;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			ConfigureUserMappings();
			ConfigureHomestayMappings();
			ConfigurePropertyTypeMappings();
			ConfigureHostProfileMappings();
			ConfigureHomestayImageMappings();
			ConfigureRuleMapping();
			ConfigureBookingMapping();
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

		public void ConfigureHostProfileMappings()
		{
			CreateMap<CreateHostProfileDto, HostProfile>();
			CreateMap<UpdateHostProfileDto, HostProfile>();
			CreateMap<HostProfile, HostProfileDto>();
		}

		public void ConfigureHomestayImageMappings()
		{
			CreateMap<CreateHomestayImageDto, HomestayImage>();
			CreateMap<ImageMetadataDto, HomestayImage>();
		}

		public void ConfigureRuleMapping()
		{
			CreateMap<CreateRuleDto, Rule>();

			CreateMap<UpdateRuleDto, Rule>();

			CreateMap<Rule, RuleDto>();
		}

		public void ConfigureBookingMapping()
		{
			CreateMap<Booking, BookingDto>()
				.ForMember(dest => dest.NumberOfNights, opt => opt.MapFrom(src =>
					(src.CheckOutDate.Date - src.CheckInDate.Date).Days))
				.ForMember(dest => dest.BookingStatusDisplay, opt => opt.MapFrom(src =>
					src.BookingStatus.ToString()))
				.ForMember(dest => dest.GuestName, opt => opt.MapFrom(src =>
					src.Guest.FirstName + " " + src.Guest.LastName))
				.ForMember(dest => dest.GuestEmail, opt => opt.MapFrom(src =>
					src.Guest.Email))
				.ForMember(dest => dest.GuestPhone, opt => opt.MapFrom(src =>
					src.Guest.PhoneNumber))
				.ForMember(dest => dest.GuestAvatar, opt => opt.MapFrom(src =>
					src.Guest.Avatar))
				.ForMember(dest => dest.Homestay, opt => opt.MapFrom(src => src.Homestay))
				.ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
				.ForMember(dest => dest.CanReview, opt => opt.MapFrom(src =>
					(src.BookingStatus == BookingStatus.CheckedOut || src.BookingStatus == BookingStatus.Completed)))
				.ForMember(dest => dest.HasReviewed, opt => opt.MapFrom(src =>
					src.Reviews.Any(r => r.ReviewerId == src.GuestId)));

			CreateMap<Homestay, BookingHomestayDto>()
				.ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
					src.HomestayImages.OrderBy(img => img.DisplayOrder).FirstOrDefault() != null
						? src.HomestayImages.OrderBy(img => img.DisplayOrder).FirstOrDefault()!.ImageUrl
						: null))
				.ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src =>
					src.PropertyType.TypeName))
				.ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src =>
					src.Owner.FirstName + " " + src.Owner.LastName))
				.ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src =>
					src.Owner.PhoneNumber))
				.ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src =>
					src.Owner.Email))
				.ForMember(dest => dest.OwnerAvatar, opt => opt.MapFrom(src =>
					src.Owner.Avatar));

			CreateMap<Payment, PaymentDto>()
				.ForMember(dest => dest.PaymentMethodDisplay, opt => opt.MapFrom(src =>
					src.PaymentMethod.ToString()))
				.ForMember(dest => dest.PaymentStatusDisplay, opt => opt.MapFrom(src =>
					src.PaymentStatus.ToString()));
		}
	}
}
