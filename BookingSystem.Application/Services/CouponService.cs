using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.CouponDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class CouponService : ICouponService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ICouponRepository _couponRepository;
		private readonly ICouponUsageRepository _couponUsageRepository;
		private readonly IBookingRepository _bookingRepository;
		private readonly IHomestayRepository _homestayRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<CouponService> _logger;

		public CouponService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ICouponRepository couponRepository,
			ICouponUsageRepository couponUsageRepository,
			IBookingRepository bookingRepository,
			IHomestayRepository homestayRepository,
			UserManager<User> userManager,
			ILogger<CouponService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_couponRepository = couponRepository;
			_couponUsageRepository = couponUsageRepository;
			_bookingRepository = bookingRepository;
			_homestayRepository = homestayRepository;
			_userManager = userManager;
			_logger = logger;
		}

		#region CRUD Operations

		public async Task<IEnumerable<CouponDto>> GetApplicableCouponsForHomestayAsync(
	int homestayId,
	bool includeInactive = false)
		{
			_logger.LogInformation("Getting applicable coupons for homestay {HomestayId}.", homestayId);

			// Validate homestay exists
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			// Lấy tất cả coupon áp dụng cho homestay này
			var coupons = await _couponRepository.GetApplicableCouponsForHomestayAsync(
				homestayId, includeInactive);

			return _mapper.Map<IEnumerable<CouponDto>>(coupons);
		}

		public async Task<IEnumerable<CouponDto>> GetActiveApplicableCouponsForHomestayAsync(
	int homestayId,
	int? userId = null,
	decimal? bookingAmount = null,
	int? numberOfNights = null)
		{
			_logger.LogInformation(
				"Getting active applicable coupons for homestay {HomestayId}, user {UserId}.",
				homestayId, userId);

			// Validate homestay
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (!homestay.IsActive || !homestay.IsApproved)
			{
				throw new BadRequestException("This homestay is not available for booking.");
			}

			// Lấy tất cả coupon active áp dụng cho homestay
			var coupons = await _couponRepository.GetApplicableCouponsForHomestayAsync(
				homestayId, includeInactive: false);

			var now = DateTime.UtcNow;
			var applicableCoupons = new List<Coupon>();

			foreach (var coupon in coupons)
			{
				// Kiểm tra ngày hiệu lực
				if (coupon.StartDate > now || coupon.EndDate < now)
					continue;

				// Kiểm tra usage limit
				if (coupon.TotalUsageLimit.HasValue &&
					coupon.CurrentUsageCount >= coupon.TotalUsageLimit.Value)
					continue;

				// Nếu có userId, kiểm tra usage per user
				if (userId.HasValue && coupon.UsagePerUser.HasValue)
				{
					var userUsageCount = await _couponRepository.GetUserCouponUsageCountAsync(
						coupon.Id, userId.Value);

					if (userUsageCount >= coupon.UsagePerUser.Value)
						continue;
				}

				// Nếu có bookingAmount, kiểm tra minimum booking amount
				if (bookingAmount.HasValue && coupon.MinimumBookingAmount.HasValue)
				{
					if (bookingAmount.Value < coupon.MinimumBookingAmount.Value)
						continue;
				}

				// Nếu có numberOfNights, kiểm tra minimum nights
				if (numberOfNights.HasValue && coupon.MinimumNights.HasValue)
				{
					if (numberOfNights.Value < coupon.MinimumNights.Value)
						continue;
				}

				// Nếu là first booking only, kiểm tra user đã có booking chưa
				if (userId.HasValue && coupon.IsFirstBookingOnly)
				{
					var hasCompletedBooking = await _bookingRepository.HasUserCompletedBookingAsync(
						userId.Value);

					if (hasCompletedBooking)
						continue;
				}

				applicableCoupons.Add(coupon);
			}

			// Sắp xếp theo priority (cao xuống thấp) và discount value
			var sortedCoupons = applicableCoupons
				.OrderByDescending(c => c.Priority)
				.ThenByDescending(c => c.DiscountValue)
				.ToList();

			return _mapper.Map<IEnumerable<CouponDto>>(sortedCoupons);
		}

		public async Task<CouponDto> CreateCouponAsync(int userId, CreateCouponDto request)
		{
			_logger.LogInformation("Creating coupon {CouponCode} by user {UserId}.", request.CouponCode, userId);

			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			if (!roles.Contains("Admin") && !roles.Contains("Host"))
			{
				throw new BadRequestException("Only admins and hosts can create coupons.");
			}

			// Validate coupon code uniqueness
			var isUnique = await _couponRepository.IsCouponCodeUniqueAsync(request.CouponCode);
			if (!isUnique)
			{
				throw new BadRequestException($"Coupon code '{request.CouponCode}' already exists.");
			}

			// Validate dates
			if (request.EndDate <= request.StartDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			if (request.StartDate < DateTime.UtcNow.Date)
			{
				throw new BadRequestException("Start date cannot be in the past.");
			}

			// Validate discount value
			ValidateDiscountValue(request.CouponType, request.DiscountValue);

			// **LOGIC SỬA: Xác định role đang sử dụng**
			var actingAsRole = request.ActingAsRole;

			// Nếu không chỉ định ActingAsRole, tự động xác định
			if (string.IsNullOrEmpty(actingAsRole))
			{
				// Ưu tiên Host nếu có cả 2 role (vì thường tạo cho homestay của mình)
				actingAsRole = roles.Contains("Host") ? "Host" : "Admin";
			}

			// Validate ActingAsRole hợp lệ
			if (!roles.Contains(actingAsRole))
			{
				throw new BadRequestException($"You don't have '{actingAsRole}' role.");
			}

			var isAdmin = actingAsRole == "Admin";
			var isHost = actingAsRole == "Host";

			// **Xử lý Scope dựa trên role đang sử dụng**
			if (isHost)
			{
				_logger.LogInformation("User {UserId} is creating coupon as Host role.", userId);

				// Lấy tất cả Homestay của Host
				var hostHomestays = await _homestayRepository.GetHomestaysByOwnerIdAsync(userId);

				if (!hostHomestays.Any())
				{
					throw new BadRequestException("You don't have any homestays to apply coupon.");
				}

				// Nếu Host chọn AllHomestays hoặc không chỉ định homestays
				if (request.Scope == CouponScope.AllHomestays ||
					(request.ApplicableHomestayIds == null || !request.ApplicableHomestayIds.Any()))
				{
					// Đổi Scope thành MultipleHomestays và áp dụng cho tất cả homestays của Host
					request.Scope = CouponScope.MultipleHomestays;
					request.ApplicableHomestayIds = hostHomestays.Select(h => h.Id).ToList();
					request.SpecificHomestayId = null;

					_logger.LogInformation("Host {UserId} creating coupon for all their {Count} homestays.",
						userId, request.ApplicableHomestayIds.Count);
				}
				else if (request.Scope == CouponScope.SpecificHomestay && request.SpecificHomestayId.HasValue)
				{
					// Validate homestay thuộc sở hữu của host
					if (!hostHomestays.Any(h => h.Id == request.SpecificHomestayId.Value))
					{
						throw new BadRequestException($"You don't own homestay with ID {request.SpecificHomestayId.Value}.");
					}
				}
				else if (request.Scope == CouponScope.MultipleHomestays && request.ApplicableHomestayIds != null)
				{
					// Validate tất cả homestays thuộc sở hữu của host
					var hostHomestayIds = hostHomestays.Select(h => h.Id).ToList();
					var invalidIds = request.ApplicableHomestayIds.Except(hostHomestayIds).ToList();

					if (invalidIds.Any())
					{
						throw new BadRequestException($"You don't own homestays with IDs: {string.Join(", ", invalidIds)}.");
					}
				}
			}
			else if (isAdmin)
			{
				_logger.LogInformation("User {UserId} is creating coupon as Admin role.", userId);

				// Admin validation - giữ nguyên logic cũ, cho phép AllHomestays
				await ValidateCouponScopeAsync(request.Scope, request.SpecificHomestayId,
					request.ApplicableHomestayIds, userId, roles);
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var coupon = new Coupon
				{
					CouponCode = request.CouponCode.ToUpper(),
					CouponName = request.CouponName,
					Description = request.Description,
					CouponType = request.CouponType,
					DiscountValue = request.DiscountValue,
					MaxDiscountAmount = request.MaxDiscountAmount,
					StartDate = request.StartDate,
					EndDate = request.EndDate,
					TotalUsageLimit = request.TotalUsageLimit,
					UsagePerUser = request.UsagePerUser,
					MinimumBookingAmount = request.MinimumBookingAmount,
					MinimumNights = request.MinimumNights,
					IsFirstBookingOnly = request.IsFirstBookingOnly,
					IsNewUserOnly = request.IsNewUserOnly,
					Scope = request.Scope,
					SpecificHomestayId = request.SpecificHomestayId,
					IsActive = true,
					IsPublic = request.IsPublic,
					Priority = request.Priority,
					CreatedByUserId = userId,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _couponRepository.AddAsync(coupon);
				await _couponRepository.SaveChangesAsync();

				// Add coupon-homestay relationships for MultipleHomestays scope
				if (request.Scope == CouponScope.MultipleHomestays && request.ApplicableHomestayIds != null)
				{
					var couponHomestays = request.ApplicableHomestayIds.Select(homestayId => new CouponHomestay
					{
						CouponId = coupon.Id,
						HomestayId = homestayId
					}).ToList();

					await _couponRepository.AddCouponHomestaysAsync(couponHomestays);
					_logger.LogInformation("Added {Count} homestay relationships for coupon {CouponId}.",
						couponHomestays.Count, coupon.Id);
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponCode} created successfully with ID {CouponId} by {Role}.",
					coupon.CouponCode, coupon.Id, actingAsRole);

				// Reload with details
				var savedCoupon = await _couponRepository.GetByIdWithDetailsAsync(coupon.Id);
				return _mapper.Map<CouponDto>(savedCoupon);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating coupon.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<CouponDto?> UpdateCouponAsync(int couponId, int userId, UpdateCouponDto request)
		{
			_logger.LogInformation("Updating coupon {CouponId} by user {UserId}.", couponId, userId);

			var coupon = await _couponRepository.GetByIdWithDetailsAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isOwner = coupon.CreatedByUserId == userId;

			// Only owner can update (or admin if not owner)
			if (!isOwner && !roles.Contains("Admin"))
			{
				throw new BadRequestException("You do not have permission to update this coupon.");
			}

			// **Xác định role đang sử dụng**
			var actingAsRole = request.ActingAsRole;

			if (string.IsNullOrEmpty(actingAsRole))
			{
				actingAsRole = roles.Contains("Host") ? "Host" : "Admin";
			}

			if (!roles.Contains(actingAsRole))
			{
				throw new BadRequestException($"You don't have '{actingAsRole}' role.");
			}

			var isAdmin = actingAsRole == "Admin";
			var isHost = actingAsRole == "Host";

			// Validate dates if being updated
			if (request.StartDate.HasValue && request.EndDate.HasValue)
			{
				if (request.EndDate.Value <= request.StartDate.Value)
				{
					throw new BadRequestException("End date must be after start date.");
				}
			}

			// Validate discount value if being updated
			if (request.DiscountValue.HasValue)
			{
				ValidateDiscountValue(coupon.CouponType, request.DiscountValue.Value);
			}

			// **Xử lý Scope cho Host**
			if (isHost)
			{
				var hostHomestays = await _homestayRepository.GetHomestaysByOwnerIdAsync(userId);

				if (!hostHomestays.Any())
				{
					throw new BadRequestException("You don't have any homestays.");
				}

				// Nếu đang update Scope hoặc ApplicableHomestayIds
				if (request.Scope.HasValue || request.ApplicableHomestayIds != null)
				{
					var newScope = request.Scope ?? coupon.Scope;

					// Nếu đổi sang AllHomestays
					if (newScope == CouponScope.AllHomestays)
					{
						newScope = CouponScope.MultipleHomestays;
						request.ApplicableHomestayIds = hostHomestays.Select(h => h.Id).ToList();
						request.SpecificHomestayId = null;

						_logger.LogInformation("Host {UserId} updating coupon {CouponId} to apply for all their homestays.",
							userId, couponId);
					}
					else if (newScope == CouponScope.SpecificHomestay)
					{
						var homestayIdToCheck = request.SpecificHomestayId ?? coupon.SpecificHomestayId;
						if (homestayIdToCheck.HasValue && !hostHomestays.Any(h => h.Id == homestayIdToCheck.Value))
						{
							throw new BadRequestException($"You don't own homestay with ID {homestayIdToCheck.Value}.");
						}
					}
					else if (newScope == CouponScope.MultipleHomestays && request.ApplicableHomestayIds != null)
					{
						var hostHomestayIds = hostHomestays.Select(h => h.Id).ToList();
						var invalidIds = request.ApplicableHomestayIds.Except(hostHomestayIds).ToList();

						if (invalidIds.Any())
						{
							throw new BadRequestException($"You don't own homestays with IDs: {string.Join(", ", invalidIds)}.");
						}
					}

					// Update scope nếu cần
					if (request.Scope.HasValue)
					{
						coupon.Scope = newScope;
					}
				}
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Update fields
				if (!string.IsNullOrWhiteSpace(request.CouponName))
					coupon.CouponName = request.CouponName;

				if (request.Description != null)
					coupon.Description = request.Description;

				if (request.DiscountValue.HasValue)
					coupon.DiscountValue = request.DiscountValue.Value;

				if (request.MaxDiscountAmount.HasValue)
					coupon.MaxDiscountAmount = request.MaxDiscountAmount;

				if (request.StartDate.HasValue)
					coupon.StartDate = request.StartDate.Value;

				if (request.EndDate.HasValue)
					coupon.EndDate = request.EndDate.Value;

				if (request.TotalUsageLimit.HasValue)
					coupon.TotalUsageLimit = request.TotalUsageLimit;

				if (request.UsagePerUser.HasValue)
					coupon.UsagePerUser = request.UsagePerUser;

				if (request.MinimumBookingAmount.HasValue)
					coupon.MinimumBookingAmount = request.MinimumBookingAmount;

				if (request.MinimumNights.HasValue)
					coupon.MinimumNights = request.MinimumNights;

				if (request.IsPublic.HasValue)
					coupon.IsPublic = request.IsPublic.Value;

				if (request.Priority.HasValue)
					coupon.Priority = request.Priority.Value;

				// Update applicable homestays for MultipleHomestays scope
				if (request.ApplicableHomestayIds != null && coupon.Scope == CouponScope.MultipleHomestays)
				{
					// Remove existing relationships
					var existingRelations = await _couponRepository.GetCouponHomestaysByCouponIdAsync(couponId);
					if (existingRelations.Any())
					{
						await _couponRepository.RemoveCouponHomestaysAsync(existingRelations);
						_logger.LogInformation("Removed {Count} existing homestay relationships for coupon {CouponId}.",
							existingRelations.Count, couponId);
					}

					// Add new relationships
					var newCouponHomestays = request.ApplicableHomestayIds.Select(homestayId => new CouponHomestay
					{
						CouponId = couponId,
						HomestayId = homestayId
					}).ToList();

					if (newCouponHomestays.Any())
					{
						await _couponRepository.AddCouponHomestaysAsync(newCouponHomestays);
						_logger.LogInformation("Added {Count} new homestay relationships for coupon {CouponId}.",
							newCouponHomestays.Count, couponId);
					}
				}

				coupon.UpdatedAt = DateTime.UtcNow;
				_couponRepository.Update(coupon);
				await _couponRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponId} updated successfully by {Role}.", couponId, actingAsRole);

				// Reload with details
				var updatedCoupon = await _couponRepository.GetByIdWithDetailsAsync(couponId);
				return _mapper.Map<CouponDto>(updatedCoupon);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating coupon {CouponId}.", couponId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeleteCouponAsync(int couponId, int userId)
		{
			_logger.LogInformation("Deleting coupon {CouponId} by user {UserId}.", couponId, userId);

			var coupon = await _couponRepository.GetByIdWithDetailsAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isOwner = coupon.CreatedByUserId == userId;

			// Only admin or creator can delete
			if (!isAdmin && !isOwner)
			{
				throw new BadRequestException("You do not have permission to delete this coupon.");
			}

			// Check if coupon has been used
			if (coupon.CurrentUsageCount > 0)
			{
				throw new BadRequestException("Cannot delete a coupon that has already been used. Deactivate it instead.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Remove coupon-homestay relationships
				var couponHomestays = await _couponRepository.GetCouponHomestaysByCouponIdAsync(couponId);
				if (couponHomestays.Any())
				{
					await _couponRepository.RemoveCouponHomestaysAsync(couponHomestays);
				}

				_couponRepository.Remove(coupon);
				await _couponRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponId} deleted successfully.", couponId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting coupon {CouponId}.", couponId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<CouponDto?> GetByIdAsync(int couponId)
		{
			var coupon = await _couponRepository.GetByIdWithDetailsAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			return _mapper.Map<CouponDto>(coupon);
		}

		public async Task<CouponDto?> GetByCodeAsync(string couponCode)
		{
			var coupon = await _couponRepository.GetByCodeAsync(couponCode);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with code '{couponCode}' not found.");
			}

			return _mapper.Map<CouponDto>(coupon);
		}

		public async Task<PagedResult<CouponDto>> GetAllCouponsAsync(CouponFilter filter)
		{
			var pagedCoupons = await _couponRepository.GetAllCouponsAsync(filter);
			var couponDtos = _mapper.Map<List<CouponDto>>(pagedCoupons.Items);

			return new PagedResult<CouponDto>
			{
				Items = couponDtos,
				TotalCount = pagedCoupons.TotalCount,
				PageSize = pagedCoupons.PageSize,
				PageNumber = pagedCoupons.PageNumber
			};
		}

		public async Task<PagedResult<CouponDto>> GetPublicCouponsAsync(CouponFilter filter)
		{
			filter.IsPublic = true;
			filter.IsActive = true;
			filter.IsExpired = false;

			var pagedCoupons = await _couponRepository.GetPublicCouponsAsync(filter);
			var couponDtos = _mapper.Map<List<CouponDto>>(pagedCoupons.Items);

			return new PagedResult<CouponDto>
			{
				Items = couponDtos,
				TotalCount = pagedCoupons.TotalCount,
				PageSize = pagedCoupons.PageSize,
				PageNumber = pagedCoupons.PageNumber
			};
		}

		#endregion

		#region Coupon Validation & Application

		public async Task<CouponValidationResultDto> ValidateCouponAsync(int userId, ValidateCouponDto request)
		{
			_logger.LogInformation("Validating coupon {CouponCode} for user {UserId}.", request.CouponCode, userId);

			var result = new CouponValidationResultDto
			{
				IsValid = false,
				Message = "Invalid coupon",
				DiscountAmount = 0,
				FinalAmount = request.BookingAmount
			};

			// Get coupon
			var coupon = await _couponRepository.GetByCodeAsync(request.CouponCode);
			if (coupon == null)
			{
				result.Message = "Coupon code not found.";
				return result;
			}

			// Check if active
			if (!coupon.IsActive)
			{
				result.Message = "This coupon is no longer active.";
				return result;
			}

			// Check dates
			var now = DateTime.UtcNow;
			if (coupon.StartDate > now)
			{
				result.Message = $"This coupon is not valid until {coupon.StartDate:yyyy-MM-dd}.";
				return result;
			}

			if (coupon.EndDate < now)
			{
				result.Message = "This coupon has expired.";
				return result;
			}

			// Check total usage limit
			if (coupon.TotalUsageLimit.HasValue && coupon.CurrentUsageCount >= coupon.TotalUsageLimit.Value)
			{
				result.Message = "This coupon has reached its usage limit.";
				return result;
			}

			// Check usage per user
			if (coupon.UsagePerUser.HasValue)
			{
				var userUsageCount = await _couponRepository.GetUserCouponUsageCountAsync(coupon.Id, userId);
				if (userUsageCount >= coupon.UsagePerUser.Value)
				{
					result.Message = "You have already used this coupon the maximum number of times.";
					return result;
				}
			}

			// Check minimum booking amount
			if (coupon.MinimumBookingAmount.HasValue && request.BookingAmount < coupon.MinimumBookingAmount.Value)
			{
				result.Message = $"Minimum booking amount of {coupon.MinimumBookingAmount.Value:C} required.";
				return result;
			}

			// Check minimum nights
			if (coupon.MinimumNights.HasValue && request.NumberOfNights < coupon.MinimumNights.Value)
			{
				result.Message = $"Minimum stay of {coupon.MinimumNights.Value} night(s) required.";
				return result;
			}

			// Check first booking only - Use BookingRepository
			if (coupon.IsFirstBookingOnly)
			{
				var hasCompletedBooking = await _bookingRepository.HasUserCompletedBookingAsync(userId);
				if (hasCompletedBooking)
				{
					result.Message = "This coupon is only valid for first-time bookings.";
					return result;
				}
			}

			// Check scope - homestay applicability
			var isApplicable = await IsCouponApplicableToHomestayAsync(coupon, request.HomestayId);
			if (!isApplicable)
			{
				result.Message = "This coupon is not applicable to this homestay.";
				return result;
			}

			var existingUsages = await _couponUsageRepository.GetAllByBookingIdAsync(request.BookingId); // Cần thêm BookingId vào ValidateCouponDto

			if (existingUsages.Any(u => u.CouponId == coupon.Id))
			{
				result.Message = "This coupon has already been applied to this booking.";
				return result;
			}

			// Calculate discount
			var discountAmount = CalculateDiscount(coupon, request.BookingAmount);
			var finalAmount = request.BookingAmount - discountAmount;

			result.IsValid = true;
			result.Message = "Coupon is valid!";
			result.Coupon = _mapper.Map<CouponDto>(coupon);
			result.DiscountAmount = discountAmount;
			result.FinalAmount = finalAmount;

			return result;
		}

		public async Task<decimal> CalculateDiscountAmountAsync(string couponCode, decimal bookingAmount, int nights)
		{
			var coupon = await _couponRepository.GetByCodeAsync(couponCode);
			if (coupon == null || !coupon.IsActive)
			{
				return 0;
			}

			var now = DateTime.UtcNow;
			if (coupon.StartDate > now || coupon.EndDate < now)
			{
				return 0;
			}

			// Check conditions
			if (coupon.MinimumBookingAmount.HasValue && bookingAmount < coupon.MinimumBookingAmount.Value)
			{
				return 0;
			}

			if (coupon.MinimumNights.HasValue && nights < coupon.MinimumNights.Value)
			{
				return 0;
			}

			return CalculateDiscount(coupon, bookingAmount);
		}

		public async Task<IEnumerable<CouponDto>> GetAvailableCouponsForUserAsync(
			int userId,
			int homestayId,
			decimal bookingAmount,
			int nights)
		{
			var coupons = await _couponRepository.GetUserAvailableCouponsAsync(
				userId, homestayId, bookingAmount, nights);

			return _mapper.Map<IEnumerable<CouponDto>>(coupons);
		}

		#endregion

		#region Coupon Usage

		public async Task<CouponUsageDto> ApplyCouponToBookingAsync(int bookingId, string couponCode, int userId)
		{
			_logger.LogInformation("Applying coupon {CouponCode} to booking {BookingId}.", couponCode, bookingId);

			// Get booking
			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			// Verify user owns the booking
			if (booking.GuestId != userId)
			{
				throw new BadRequestException("You can only apply coupons to your own bookings.");
			}

			// Check booking status
			if (booking.BookingStatus != BookingStatus.Pending)
			{
				throw new BadRequestException("Coupons can only be applied to pending bookings.");
			}

			// Check if booking already has a coupon
			var existingUsages = await _couponUsageRepository.GetAllByBookingIdAsync(bookingId); // Cần thêm method mới
			var coupon = await _couponRepository.GetByCodeAsync(couponCode);

			if (existingUsages.Any(u => u.CouponId == coupon.Id))
			{
				throw new BadRequestException("This coupon has already been applied to this booking.");
			}

			// Validate coupon
			var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
			var validationRequest = new ValidateCouponDto
			{
				CouponCode = couponCode,
				HomestayId = booking.HomestayId,
				BookingAmount = booking.BaseAmount,
				NumberOfNights = numberOfNights,
				CheckInDate = booking.CheckInDate,
				CheckOutDate = booking.CheckOutDate,
				BookingId = booking.Id,
			};

			var validation = await ValidateCouponAsync(userId, validationRequest);
			if (!validation.IsValid)
			{
				throw new BadRequestException(validation.Message);
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var coupo = await _couponRepository.GetByCodeAsync(couponCode);
				if (coupo == null)
				{
					throw new NotFoundException($"Coupon '{couponCode}' not found.");
				}

				// Create coupon usage record
				var couponUsage = new CouponUsage
				{
					CouponId = coupon.Id,
					UserId = userId,
					BookingId = bookingId,
					DiscountAmount = validation.DiscountAmount,
					UsedAt = DateTime.UtcNow,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _couponUsageRepository.AddAsync(couponUsage);

				// Update coupon usage count
				coupon.CurrentUsageCount++;
				_couponRepository.Update(coupon);

				// Update booking amounts
				booking.DiscountAmount += validation.DiscountAmount;
				booking.TotalAmount = booking.BaseAmount + booking.CleaningFee + booking.ServiceFee + booking.TaxAmount - booking.DiscountAmount;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);

				await _couponUsageRepository.SaveChangesAsync();
				await _couponRepository.SaveChangesAsync();
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponCode} applied to booking {BookingId} successfully.",
					couponCode, bookingId);

				// Reload with details
				var savedUsage = await _couponUsageRepository.GetByBookingIdAsync(bookingId);
				return _mapper.Map<CouponUsageDto>(savedUsage);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while applying coupon to booking.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> RemoveCouponFromBookingAsync(int bookingId, int userId, string couponCode)
		{
			_logger.LogInformation("Removing coupon from booking {BookingId}.", bookingId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			// Verify user owns the booking
			if (booking.GuestId != userId)
			{
				throw new BadRequestException("You can only remove coupons from your own bookings.");
			}

			// Check booking status
			if (booking.BookingStatus != BookingStatus.Pending)
			{
				throw new BadRequestException("Coupons can only be removed from pending bookings.");
			}

			var existingUsages = await _couponUsageRepository.GetAllByBookingIdAsync(bookingId);
			var couponUsage = existingUsages.FirstOrDefault(u => u.Coupon.CouponCode == couponCode);

			if (couponUsage == null)
			{
				throw new NotFoundException($"Coupon '{couponCode}' is not applied to this booking.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Get coupon
				var coupon = await _couponRepository.GetByIdAsync(couponUsage.CouponId);
				if (coupon != null)
				{
					// Decrease usage count
					coupon.CurrentUsageCount--;
					if (coupon.CurrentUsageCount < 0)
						coupon.CurrentUsageCount = 0;
					_couponRepository.Update(coupon);
				}

				// Remove coupon usage
				_couponUsageRepository.Remove(couponUsage);

				// Restore booking original amount
				booking.DiscountAmount -= couponUsage.DiscountAmount;
				if (booking.DiscountAmount < 0)
					booking.DiscountAmount = 0;

				booking.TotalAmount = booking.BaseAmount + booking.CleaningFee + booking.ServiceFee + booking.TaxAmount - booking.DiscountAmount;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);

				await _couponRepository.SaveChangesAsync();
				await _couponUsageRepository.SaveChangesAsync();
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon removed from booking {BookingId} successfully.", bookingId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while removing coupon from booking.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<IEnumerable<CouponUsageDto>> GetCouponUsagesByBookingAsync(int bookingId)
		{
			var couponUsages = await _couponUsageRepository.GetAllByBookingIdAsync(bookingId);
			return _mapper.Map<IEnumerable<CouponUsageDto>>(couponUsages);
		}

		public async Task<IEnumerable<CouponUsageDto>> GetUserCouponUsageHistoryAsync(int userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var usages = await _couponUsageRepository.GetByUserIdAsync(userId);
			return _mapper.Map<IEnumerable<CouponUsageDto>>(usages);
		}

		public async Task<IEnumerable<CouponUsageDto>> GetCouponUsageHistoryAsync(int couponId)
		{
			var coupon = await _couponRepository.GetByIdAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var usages = await _couponRepository.GetCouponUsageHistoryAsync(couponId);
			return _mapper.Map<IEnumerable<CouponUsageDto>>(usages);
		}

		#endregion

		#region Status Management

		public async Task<bool> ActivateCouponAsync(int couponId, int userId)
		{
			_logger.LogInformation("Activating coupon {CouponId} by user {UserId}.", couponId, userId);

			var coupon = await _couponRepository.GetByIdAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isOwner = coupon.CreatedByUserId == userId;

			if (!isAdmin && !isOwner)
			{
				throw new BadRequestException("You do not have permission to activate this coupon.");
			}

			if (coupon.IsActive)
			{
				throw new BadRequestException("Coupon is already active.");
			}

			// Check if expired
			if (coupon.EndDate < DateTime.UtcNow)
			{
				throw new BadRequestException("Cannot activate an expired coupon.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				coupon.IsActive = true;
				coupon.UpdatedAt = DateTime.UtcNow;
				_couponRepository.Update(coupon);
				await _couponRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponId} activated successfully.", couponId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while activating coupon {CouponId}.", couponId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeactivateCouponAsync(int couponId, int userId)
		{
			_logger.LogInformation("Deactivating coupon {CouponId} by user {UserId}.", couponId, userId);

			var coupon = await _couponRepository.GetByIdAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isOwner = coupon.CreatedByUserId == userId;

			if (!isAdmin && !isOwner)
			{
				throw new BadRequestException("You do not have permission to deactivate this coupon.");
			}

			if (!coupon.IsActive)
			{
				throw new BadRequestException("Coupon is already inactive.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				coupon.IsActive = false;
				coupon.UpdatedAt = DateTime.UtcNow;
				_couponRepository.Update(coupon);
				await _couponRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponId} deactivated successfully.", couponId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deactivating coupon {CouponId}.", couponId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> ExtendCouponExpiryAsync(int couponId, int userId, DateTime newEndDate)
		{
			_logger.LogInformation("Extending coupon {CouponId} expiry to {NewEndDate}.", couponId, newEndDate);

			var coupon = await _couponRepository.GetByIdAsync(couponId);
			if (coupon == null)
			{
				throw new NotFoundException($"Coupon with ID {couponId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isOwner = coupon.CreatedByUserId == userId;

			if (!isAdmin && !isOwner)
			{
				throw new BadRequestException("You do not have permission to extend this coupon.");
			}

			if (newEndDate <= coupon.EndDate)
			{
				throw new BadRequestException("New end date must be after current end date.");
			}

			if (newEndDate <= coupon.StartDate)
			{
				throw new BadRequestException("New end date must be after start date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				coupon.EndDate = newEndDate;
				coupon.UpdatedAt = DateTime.UtcNow;
				_couponRepository.Update(coupon);
				await _couponRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Coupon {CouponId} expiry extended successfully.", couponId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while extending coupon expiry.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Statistics & Background Processing

		public async Task<CouponStatisticsDto> GetCouponStatisticsAsync(int? createdByUserId = null)
		{
			_logger.LogInformation("Getting coupon statistics.");

			var filter = new CouponFilter
			{
				CreatedByUserId = createdByUserId,
				PageNumber = 1,
				PageSize = int.MaxValue
			};

			var pagedCoupons = await _couponRepository.GetAllCouponsAsync(filter);
			var coupons = pagedCoupons.Items.ToList();

			var now = DateTime.UtcNow;
			var totalCoupons = coupons.Count;
			var activeCoupons = coupons.Count(c => c.IsActive && c.StartDate <= now && c.EndDate >= now);
			var expiredCoupons = coupons.Count(c => c.EndDate < now);

			// Use CouponRepository instead of direct Context access
			var couponIds = coupons.Select(c => c.Id).ToList();
			var allUsages = await _couponRepository.GetAllCouponUsagesByCouponIdsAsync(couponIds);

			var totalUsageCount = allUsages.Count;
			var totalDiscountAmount = allUsages.Sum(u => u.DiscountAmount);
			var averageDiscountAmount = totalUsageCount > 0 ? totalDiscountAmount / totalUsageCount : 0;

			var couponTypeStats = coupons
				.GroupBy(c => c.CouponType)
				.Select(g => new CouponTypeStatistic
				{
					Type = g.Key,
					TypeDisplay = g.Key.ToString(),
					Count = g.Count(),
					UsageCount = allUsages.Count(u => g.Select(c => c.Id).Contains(u.CouponId)),
					TotalDiscount = allUsages
						.Where(u => g.Select(c => c.Id).Contains(u.CouponId))
						.Sum(u => u.DiscountAmount)
				})
				.ToList();

			return new CouponStatisticsDto
			{
				TotalCoupons = totalCoupons,
				ActiveCoupons = activeCoupons,
				ExpiredCoupons = expiredCoupons,
				TotalUsageCount = totalUsageCount,
				TotalDiscountAmount = Math.Round(totalDiscountAmount, 2),
				AverageDiscountAmount = Math.Round(averageDiscountAmount, 2),
				CouponTypeStats = couponTypeStats
			};
		}

		public async Task ProcessExpiredCouponsAsync()
		{
			_logger.LogInformation("Processing expired coupons.");

			// Use CouponRepository
			var expiredCoupons = await _couponRepository.GetExpiredActiveCouponsAsync();

			if (!expiredCoupons.Any())
			{
				_logger.LogInformation("No expired coupons found.");
				return;
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				foreach (var coupon in expiredCoupons)
				{
					coupon.IsActive = false;
					coupon.UpdatedAt = DateTime.UtcNow;
					_couponRepository.Update(coupon);
				}

				await _couponRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully processed {Count} expired coupons.", expiredCoupons.Count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while processing expired coupons.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Private Helper Methods

		private void ValidateDiscountValue(CouponType couponType, decimal discountValue)
		{
			if (couponType == CouponType.Percentage)
			{
				if (discountValue <= 0 || discountValue > 100)
				{
					throw new BadRequestException("Percentage discount must be between 0 and 100.");
				}
			}
			else
			{
				if (discountValue <= 0)
				{
					throw new BadRequestException("Discount value must be greater than 0.");
				}
			}
		}

		private async Task ValidateCouponScopeAsync(
			CouponScope scope,
			int? specificHomestayId,
			List<int>? applicableHomestayIds,
			int userId,
			IList<string> userRoles)
		{
			var isAdmin = userRoles.Contains("Admin");

			if (scope == CouponScope.SpecificHomestay)
			{
				if (!specificHomestayId.HasValue)
				{
					throw new BadRequestException("Specific homestay ID is required for SpecificHomestay scope.");
				}

				var homestay = await _homestayRepository.GetByIdAsync(specificHomestayId.Value);
				if (homestay == null)
				{
					throw new NotFoundException($"Homestay with ID {specificHomestayId.Value} not found.");
				}

				// Host can only create coupon for their own homestays
				if (userRoles.Contains("Host") && !isAdmin && homestay.OwnerId != userId)
				{
					throw new BadRequestException("You can only create coupons for your own homestays.");
				}
			}

			if (scope == CouponScope.MultipleHomestays)
			{
				if (applicableHomestayIds == null || !applicableHomestayIds.Any())
				{
					throw new BadRequestException("Applicable homestay IDs are required for MultipleHomestays scope.");
				}

				// Verify all homestays exist and belong to host
				foreach (var homestayId in applicableHomestayIds)
				{
					var homestay = await _homestayRepository.GetByIdAsync(homestayId);
					if (homestay == null)
					{
						throw new NotFoundException($"Homestay with ID {homestayId} not found.");
					}

					if (userRoles.Contains("Host") && !isAdmin && homestay.OwnerId != userId)
					{
						throw new BadRequestException($"You don't own homestay with ID {homestayId}.");
					}
				}
			}
		}

		private async Task<bool> IsCouponApplicableToHomestayAsync(Coupon coupon, int homestayId)
		{
			switch (coupon.Scope)
			{
				case CouponScope.AllHomestays:
					return true;

				case CouponScope.SpecificHomestay:
					return coupon.SpecificHomestayId == homestayId;

				case CouponScope.MultipleHomestays:
					return coupon.CouponHomestays.Any(ch => ch.HomestayId == homestayId);

				default:
					return false;
			}
		}

		private decimal CalculateDiscount(Coupon coupon, decimal bookingAmount)
		{
			decimal discount = 0;

			switch (coupon.CouponType)
			{
				case CouponType.Percentage:
					discount = bookingAmount * (coupon.DiscountValue / 100);
					// Apply max discount limit if set
					if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
					{
						discount = coupon.MaxDiscountAmount.Value;
					}
					break;

				case CouponType.FixedAmount:
				case CouponType.FirstBooking:
				case CouponType.Seasonal:
				case CouponType.LongStay:
				case CouponType.Referral:
					discount = coupon.DiscountValue;
					// Discount cannot exceed booking amount
					if (discount > bookingAmount)
					{
						discount = bookingAmount;
					}
					break;
			}

			return Math.Round(discount, 2);
		}

		#endregion
	}
}