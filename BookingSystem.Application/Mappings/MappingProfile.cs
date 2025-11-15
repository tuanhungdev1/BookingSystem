using AutoMapper;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Application.DTOs.CouponDTO;
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
			ConfigureCouponMappings();
		}

		private void ConfigureCouponMappings()
		{

			CreateMap<CouponUsage, CouponUsageDto>()
				.ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Coupon.CouponCode))
				.ForMember(dest => dest.CouponName, opt => opt.MapFrom(src => src.Coupon.CouponName)) // THÊM DÒNG NÀY
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
				.ForMember(dest => dest.BookingCode, opt => opt.MapFrom(src => src.Booking.BookingCode))
				.ForMember(dest => dest.Coupon, opt => opt.MapFrom(src => src.Coupon)); // THÊM DÒNG NÀY


			CreateMap<Coupon, CouponDto>()
			   .ForMember(dest => dest.CouponTypeDisplay,
				   opt => opt.MapFrom(src => src.CouponType.ToString()))
			   .ForMember(dest => dest.ScopeDisplay,
				   opt => opt.MapFrom(src => src.Scope.ToString()))
			   .ForMember(dest => dest.SpecificHomestayName,
				   opt => opt.MapFrom(src => src.SpecificHomestay != null ? src.SpecificHomestay.HomestayTitle : null))
			   .ForMember(dest => dest.ApplicableHomestayIds,
				   opt => opt.MapFrom(src => src.CouponHomestays.Select(ch => ch.HomestayId).ToList()))
			   .ForMember(dest => dest.CreatedByUserName,
				   opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : null))
			   .ForMember(dest => dest.IsExpired,
				   opt => opt.MapFrom(src => src.EndDate < DateTime.UtcNow))
			   .ForMember(dest => dest.IsAvailable,
				   opt => opt.MapFrom(src =>
					   src.IsActive &&
					   src.StartDate <= DateTime.UtcNow &&
					   src.EndDate >= DateTime.UtcNow &&
					   (src.TotalUsageLimit == null || src.CurrentUsageCount < src.TotalUsageLimit)))
			   .ReverseMap()
			   .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
			   .ForMember(dest => dest.SpecificHomestay, opt => opt.Ignore())
			   .ForMember(dest => dest.CouponUsages, opt => opt.Ignore())
			   .ForMember(dest => dest.CouponHomestays, opt => opt.Ignore())
			   .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			   .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

			CreateMap<CreateCouponDto, Coupon>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
				.ForMember(dest => dest.SpecificHomestay, opt => opt.Ignore())
				.ForMember(dest => dest.CouponUsages, opt => opt.Ignore())
				.ForMember(dest => dest.CouponHomestays, opt => opt.Ignore())
				.ForMember(dest => dest.CurrentUsageCount, opt => opt.MapFrom(src => 0))
				.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
				.ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.CouponCode.Trim().ToUpper()))
				.AfterMap((src, dest) =>
				{
					if (src.Scope != Domain.Enums.CouponScope.AllHomestays && src.ApplicableHomestayIds != null)
					{
						dest.CouponHomestays = src.ApplicableHomestayIds
							.Distinct()
							.Select(id => new CouponHomestay
							{
								HomestayId = id,
								Coupon = dest
							})
							.ToList();
					}
				});

			CreateMap<UpdateCouponDto, Coupon>()
			   .ForMember(dest => dest.Id, opt => opt.Ignore());
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
				.ForMember(dest => dest.PaymentNotes, opt => opt.MapFrom(src => src.PaymentNotes))
				.ForMember(dest => dest.CouponDiscountAmount, opt => opt.MapFrom(src =>
					src.CouponUsages.Sum(cu => cu.DiscountAmount)))
				.ForMember(dest => dest.AppliedCoupons, opt => opt.MapFrom(src => src.CouponUsages))
				.AfterMap((src, dest) =>
				{
					dest.NumberOfNights = (src.CheckOutDate - src.CheckInDate).Days;
				})
				.ForMember(dest => dest.BookingStatusDisplay,
					opt => opt.MapFrom(src => src.BookingStatus.ToString()))
				.ForMember(dest => dest.Homestay, opt => opt.MapFrom(src => src.Homestay))
				.ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
				.ForMember(dest => dest.GuestFullName, opt => opt.MapFrom(src => src.GuestFullName))
			.ForMember(dest => dest.GuestEmail, opt => opt.MapFrom(src => src.GuestEmail))
			.ForMember(dest => dest.GuestPhoneNumber, opt => opt.MapFrom(src => src.GuestPhoneNumber))
			.ForMember(dest => dest.IsBookingForSomeoneElse, opt => opt.MapFrom(src => src.IsBookingForSomeoneElse))
			.ForMember(dest => dest.ActualGuestFullName, opt => opt.MapFrom(src => src.ActualGuestFullName))
			.ForMember(dest => dest.ActualGuestEmail, opt => opt.MapFrom(src => src.ActualGuestEmail))
			.ForMember(dest => dest.ActualGuestPhoneNumber, opt => opt.MapFrom(src => src.ActualGuestPhoneNumber))
			.ForMember(dest => dest.ActualGuestIdNumber, opt => opt.MapFrom(src => src.ActualGuestIdNumber))
			.ForMember(dest => dest.ActualGuestNotes, opt => opt.MapFrom(src => src.ActualGuestNotes))
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
				.ForMember(dest => dest.RatingAverage, opt => opt.MapFrom(src =>
					src.RatingAverage))
				.ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src =>
					src.TotalReviews))
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
					src.AvailabilityCalendars.Where(ac => !ac.IsDeleted)));

				// Statistics
				//.ForMember(dest => dest.RatingAverage, opt => opt.MapFrom(src =>
				//	src.Reviews.Any() ? src.Reviews.Average(r => r.OverallRating) : 0))
				//.ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src =>
				//	src.Reviews.Count));
		

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
