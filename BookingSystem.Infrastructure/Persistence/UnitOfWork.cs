using BookingSystem.Domain.Base;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingSystem.Infrastructure.Persistence
{
	public class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
	{
		private readonly BookingDbContext _context;
		private IDbContextTransaction? _transaction;
		private bool _disposed;

		private readonly Lazy<IUserRepository> _userRepository;
		private readonly Lazy<IAmenityRepository> _amenityRepository;
		private readonly Lazy<IHomestayRepository> _homestayRepository;
		private readonly Lazy<IPropertyTypeRepository> _propertyTypeRepository;
		private readonly Lazy<IHostProfileRepository> _hostProfileRepository;
		private readonly Lazy<IHomestayImageRepository> _homestayImageRepository;
		private readonly Lazy<IHomestayAmenityRepository> _homestayAmenityRepository;
		private readonly Lazy<IRuleRepository> _ruleRepository;
		private readonly Lazy<IHomestayRuleRepository> _homestayRuleRepository;
		private readonly Lazy<IWishlistItemRepository> _wishlistItemRepository;
		private readonly Lazy<IAvailabilityCalendarRepository> _availabilityCalendarRepository;
		private readonly Lazy<IBookingRepository> _bookingRepository;
		private readonly Lazy<IReviewRepository> _reviewRepository;
		private readonly Lazy<ICouponRepository> _couponRepository;
		private readonly Lazy<ICouponUsageRepository> _couponUsageRepository;
		private readonly Lazy<ICouponHomestaysRepository> _couponHomestaysRepository;
		private readonly Lazy<IUserPreferenceRepository> _userPreferenceRepository;
		private readonly Lazy<IDashboardRepository> _dashboardRepository;
		private readonly Lazy<IHostDashboardRepository> _hostDashboardRepository;

		public UnitOfWork(BookingDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
			_amenityRepository = new Lazy<IAmenityRepository>(() => new AmenityRepository(_context));
			_homestayRepository = new Lazy<IHomestayRepository>(() => new HomestayRepository(_context));
			_propertyTypeRepository = new Lazy<IPropertyTypeRepository>(() => new PropertyTypeRepository(_context));
			_hostProfileRepository = new Lazy<IHostProfileRepository>(() => new HostProfileRepository(_context));
			_homestayImageRepository = new Lazy<IHomestayImageRepository>(() => new HomestayImageRepository(_context));
			_homestayAmenityRepository = new Lazy<IHomestayAmenityRepository>(() => new HomestayAmenityRepository(_context));
			_ruleRepository = new Lazy<IRuleRepository>(() => new RuleRepository(_context));
			_homestayRuleRepository = new Lazy<IHomestayRuleRepository>(() => new HomestayRuleRepository(_context));
			_wishlistItemRepository = new Lazy<IWishlistItemRepository>(() => new WishlistItemRepository(_context));
			_availabilityCalendarRepository = new Lazy<IAvailabilityCalendarRepository>(() => new AvailabilityCalendarRepository(_context));
			_bookingRepository = new Lazy<IBookingRepository>(() => new BookingRepository(_context));
			_reviewRepository = new Lazy<IReviewRepository>(() => new ReviewRepository(_context));
			_couponRepository = new Lazy<ICouponRepository>(() => new CouponRepository(_context));
			_couponUsageRepository = new Lazy<ICouponUsageRepository>(() => new CouponUsageRepository(_context));
			_couponHomestaysRepository = new Lazy<ICouponHomestaysRepository>(() => new CouponHomestaysRepository(_context));
			_userPreferenceRepository = new Lazy<IUserPreferenceRepository>(() => new UserPreferenceRepository(_context));
			_dashboardRepository = new Lazy<IDashboardRepository>(() => new DashboardRepository(_context));
			_hostDashboardRepository = new Lazy<IHostDashboardRepository>(() => new HostDashboardRepository(_context));
		}

		public IUserRepository UserRepository => _userRepository.Value;
		public IAmenityRepository AmenityRepository => _amenityRepository.Value;
		public IHomestayRepository HomestayRepository => _homestayRepository.Value;
		public IPropertyTypeRepository PropertyTypeRepository => _propertyTypeRepository.Value;
		public IHostProfileRepository HostProfileRepository => _hostProfileRepository.Value;
		public IHomestayImageRepository HomestayImageRepository => _homestayImageRepository.Value;
		public IHomestayAmenityRepository HomestayAmenityRepository => _homestayAmenityRepository.Value;
		public IRuleRepository RuleRepository => _ruleRepository.Value;
		public IHomestayRuleRepository HomestayRuleRepository => _homestayRuleRepository.Value;
		public IWishlistItemRepository WishlistItemRepository => _wishlistItemRepository.Value;
		public IAvailabilityCalendarRepository AvailabilityCalendarRepository => _availabilityCalendarRepository.Value;
		public IBookingRepository BookingRepository => _bookingRepository.Value;
		public IReviewRepository ReviewRepository => _reviewRepository.Value;
		public ICouponRepository CouponRepository => _couponRepository.Value;
		public ICouponUsageRepository CouponUsageRepository => _couponUsageRepository.Value;
		public ICouponHomestaysRepository CouponHomestaysRepository => _couponHomestaysRepository.Value;
		public IUserPreferenceRepository UserPreferenceRepository => _userPreferenceRepository.Value;
		public IDashboardRepository DashboardRepository => _dashboardRepository.Value;
		public IHostDashboardRepository HostDashboardRepository => _hostDashboardRepository.Value;

		public async Task BeginTransactionAsync()
		{
			if (_transaction != null)
				throw new InvalidOperationException("A transaction is already in progress.");

			_transaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			if (_transaction == null)
				throw new InvalidOperationException("No transaction in progress.");

			try
			{
				await SaveChangesAsync();
				await _transaction.CommitAsync();
			}
			finally
			{
				await DisposeTransactionAsync();
			}
		}

		public async Task RollbackTransactionAsync()
		{
			if (_transaction == null)
				return;

			try
			{
				await _transaction.RollbackAsync();
			}
			finally
			{
				await DisposeTransactionAsync();
			}
		}

		#region Transaction (Safe for ExecutionStrategy)
		/// <summary>
		/// Thực thi logic nghiệp vụ trong transaction an toàn với ExecutionStrategy (retry).
		/// </summary>
		public async Task ExecuteInTransactionAsync(Func<Task> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			// ✅ Phải dùng ExecutionStrategy nếu đã enable globally
			var strategy = _context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// ✅ THÊM: Clear ChangeTracker để tránh tracked entities
				_context.ChangeTracker.Clear();

				await using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					await action();
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
			});
		}
		#endregion

		private async Task DisposeTransactionAsync()
		{
			if (_transaction != null)
			{
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		public async Task SaveChangesAsync()
		{
			if (_context == null)
				throw new InvalidOperationException("DbContext is not initialized.");

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				throw new Exception("An error occurred while saving changes to the database.", ex);
			}
		}

		// IAsyncDisposable
		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore();
			Dispose(false);
			GC.SuppressFinalize(this);
		}

		protected virtual async ValueTask DisposeAsyncCore()
		{
			if (!_disposed)
			{
				if (_transaction != null)
				{
					await _transaction.DisposeAsync();
					_transaction = null;
				}

				if (_context != null)
				{
					await _context.DisposeAsync();
				}

				_disposed = true;
			}
		}

		// IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_transaction?.Dispose();
					_context?.Dispose();
				}

				_disposed = true;
			}
		}
	}
}
