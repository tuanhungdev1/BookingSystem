using AutoMapper;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Application.DTOs.HomestayDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HostProfileDTO;
using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Application.DTOs.ReviewDTO;
using BookingSystem.Application.DTOs.RuleDTO;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.DTOs.UserPreferenceDTO;
using BookingSystem.Application.DTOs.Users;
using BookingSystem.Application.DTOs.WishlistDTO;
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
			ConfigureReviewMapping();
			ConfigureUserPreferenceMapping();
			ConfigureAmenitiesMappings();
			ConfigureAvailabilityCalendarMappings();
			ConfigureWishlistItemMappings();
			ConfigureBookingMappings();
			ConfigurePaymentMappings();
		}

		private void ConfigurePaymentMappings()
		{
			CreateMap<Payment, PaymentDto>();
		}

		private void ConfigureBookingMappings()
		{
			CreateMap<Booking, BookingDto>()
				.ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest.FullName))
				.ForMember(dest => dest.GuestEmail, opt => opt.MapFrom(src => src.Guest.Email))
				.ForMember(dest => dest.GuestPhone, opt => opt.MapFrom(src => src.Guest.PhoneNumber))
				.ForMember(dest => dest.GuestAvatar, opt => opt.MapFrom(src => src.Guest.Avatar))
				.AfterMap((src, dest) =>
				{
					dest.NumberOfNights = (src.CheckOutDate - src.CheckInDate).Days;
				})
				.ForMember(dest => dest.BookingStatusDisplay,
					opt => opt.MapFrom(src => src.BookingStatus.ToString()))
				.ForMember(dest => dest.Homestay, opt => opt.MapFrom(src => src.Homestay))
				.ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
				.ReverseMap();

			CreateMap<Homestay, BookingHomestayDto>()
				.ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.Owner.Id))
				.ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
				.ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.Owner.Email))
				.ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src => src.Owner.PhoneNumber))
				.ForMember(dest => dest.OwnerAvatar, opt => opt.MapFrom(src => src.Owner.Avatar))
				.ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType.TypeName))
				.ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
									src.HomestayImages.FirstOrDefault(i => i.IsPrimaryImage) != null
										? src.HomestayImages.FirstOrDefault(i => i.IsPrimaryImage)!.ImageUrl
										: null))
				.ReverseMap();

			CreateMap<Booking, BookingPaymentInfoDto>()
			   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
			   .ForMember(dest => dest.BookingCode, opt => opt.MapFrom(src => src.BookingCode))
			   .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
			   .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate))
			   .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
			   .ForMember(dest => dest.HomestayTitle, opt => opt.MapFrom(src => src.Homestay.HomestayTitle))
			   .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest.FullName))
			   .ForMember(dest => dest.HomestayTitle, opt => opt.MapFrom(src => src.Homestay != null ? src.Homestay.HomestayTitle : string.Empty))
			   .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : string.Empty));
		}

		private void ConfigureWishlistItemMappings()
		{
			CreateMap<WishlistItem, WishlistItemDto>();
		}



		private void ConfigureUserMappings()
		{
			CreateMap<User, UserProfileDto>()
					.ForMember(dest => dest.Roles, opt => opt.Ignore());
			CreateMap<User, UserDto>();
			CreateMap<CreateUserDto, User>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdateUserDto, User>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdateUserProfileDto, User>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
		}

		private void ConfigureAvailabilityCalendarMappings()
		{
			CreateMap<AvailabilityCalendar, AvailabilityCalendarDto>();
			CreateMap<CreateAvailabilityCalendarDto, AvailabilityCalendar>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdateAvailabilityCalendarDto, AvailabilityCalendar>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
		}

		public void ConfigureHomestayMappings()
		{
			CreateMap<CreateHomestayDto, Homestay>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdateHomestayDto, Homestay>()
				.ForMember(dest => dest.Id, opt => opt.Ignore()); ;
			// Main Homestay -> HomestayDto mapping (TỐI ƯU HÓA)
			CreateMap<Homestay, HomestayDto>()
				// Basic Info
				.ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
					src.HomestayImages
						.Where(img => !img.IsDeleted)
						.OrderBy(img => img.DisplayOrder)
						.FirstOrDefault() != null
							? src.HomestayImages
								.Where(img => !img.IsDeleted)
								.OrderBy(img => img.DisplayOrder)
								.FirstOrDefault()!.ImageUrl
							: null))

				// Owner Info
				.ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src =>
					$"{src.Owner.FirstName} {src.Owner.LastName}"))
				.ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src =>
					src.Owner.PhoneNumber))
				.ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src =>
					src.Owner.Email))
				.ForMember(dest => dest.OwnerAvatar, opt => opt.MapFrom(src =>
					src.Owner.Avatar))

				// Property Type Info
				.ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src =>
					src.PropertyType.TypeName))
				.ForMember(dest => dest.PropertyTypeIcon, opt => opt.MapFrom(src =>
					src.PropertyType.IconUrl))

				// Images
				.ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
					src.HomestayImages.Where(img => !img.IsDeleted).OrderBy(img => img.DisplayOrder)))

				// Amenities - CHỈ LẤY THÔNG TIN AMENITY, KHÔNG LẤY BẢNG TRUNG GIAN
				.ForMember(dest => dest.Amenities, opt => opt.MapFrom(src =>
					src.HomestayAmenities
						.Where(ha => ha.Amenity.IsActive)
						.OrderBy(ha => ha.Amenity.DisplayOrder)
						.Select(ha => new AmenitySimpleDto
						{
							Id = ha.Amenity.Id,
							AmenityName = ha.Amenity.AmenityName,
							AmenityDescription = ha.Amenity.AmenityDescription,
							IconUrl = ha.Amenity.IconUrl,
							Category = ha.Amenity.Category,
							DisplayOrder = ha.Amenity.DisplayOrder,
							CustomNote = ha.CustomNote
						})))

				// Rules - CHỈ LẤY THÔNG TIN RULE, KHÔNG LẤY BẢNG TRUNG GIAN
				.ForMember(dest => dest.Rules, opt => opt.MapFrom(src =>
					src.HomestayRules
						.Where(hr => hr.Rule.IsActive)
						.OrderBy(hr => hr.Rule.DisplayOrder)
						.Select(hr => new RuleSimpleDto
						{
							Id = hr.Rule.Id,
							RuleName = hr.Rule.RuleName,
							RuleDescription = hr.Rule.RuleDescription,
							IconUrl = hr.Rule.IconUrl,
							RuleType = hr.Rule.RuleType,
							DisplayOrder = hr.Rule.DisplayOrder,
							CustomNote = hr.CustomNote
						})))

				// Availability Calendars
				.ForMember(dest => dest.AvailabilityCalendars, opt => opt.MapFrom(src =>
					src.AvailabilityCalendars.Where(ac => !ac.IsDeleted)))

				// Statistics
				.ForMember(dest => dest.RatingAverage, opt => opt.MapFrom(src =>
					src.Reviews.Any() ? src.Reviews.Average(r => r.OverallRating) : 0))
				.ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src =>
					src.Reviews.Count));
		

			// Mapping cho AvailabilityCalendar
			CreateMap<AvailabilityCalendar, AvailabilityCalendarDto>()
				.ForMember(dest => dest.HomestayTitle, opt => opt.MapFrom(src =>
					src.Homestay.HomestayTitle))
				.ForMember(dest => dest.BaseNightlyPrice, opt => opt.MapFrom(src =>
					src.Homestay.BaseNightlyPrice));
		}

		public void ConfigureAmenitiesMappings()
		{
			CreateMap<CreateAmenityDto, Amenity>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdateAmenityDto, Amenity>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<Amenity, AmenityDto>();

			// Mapping cho bảng trung gian (chỉ dùng khi cần chi tiết)
			CreateMap<HomestayAmenity, HomestayAmenityDto>()
				.ForMember(dest => dest.Amenity, opt => opt.MapFrom(src => src.Amenity));

			// Mapping cho AmenitySimpleDto (đã xử lý trực tiếp trong HomestayDto mapping)
		}

		public void ConfigurePropertyTypeMappings()
		{
			CreateMap<CreatePropertyTypeDto, PropertyType>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<UpdatePropertyTypeDto, PropertyType>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());
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

			// Thêm mapping cho HomestayImage sang DTO
			CreateMap<HomestayImage, HomestayImageDto>();
		}

		public void ConfigureRuleMapping()
		{
			CreateMap<CreateRuleDto, Rule>();
			CreateMap<UpdateRuleDto, Rule>();
			CreateMap<Rule, RuleDto>();

			// Thêm mapping cho HomestayRule
			CreateMap<HomestayRule, HomestayRuleDto>()
				.ForMember(dest => dest.Rule, opt => opt.MapFrom(src => src.Rule));
		}

		public void ConfigureUserPreferenceMapping()
		{
			CreateMap<UserPreference, UserPreferenceDto>();
			CreateMap<CreateUserPreferenceDto, UserPreference>();
		}

		public void ConfigureReviewMapping()
		{
			CreateMap<Review, ReviewDto>()
				.ForMember(dest => dest.ReviewerName,
					opt => opt.MapFrom(src => src.Reviewer.FullName))
				.ForMember(dest => dest.ReviewerAvatar,
					opt => opt.MapFrom(src => src.Reviewer.Avatar))
				.ForMember(dest => dest.RevieweeName,
					opt => opt.MapFrom(src => src.Reviewee.FullName))
				.ForMember(dest => dest.HomestayTitle,
					opt => opt.MapFrom(src => src.Homestay.HomestayTitle));

			CreateMap<CreateReviewDto, Review>();
		}
	}
}
