using BookingSystem.Domain.Repositories;

namespace BookingSystem.Domain.Base
{
	public interface IUnitOfWork : IDisposable
	{
		public IUserRepository UserRepository { get; }
		public IAmenityRepository AmenityRepository { get; }
		public IHomestayRepository HomestayRepository { get; }
		public IPropertyTypeRepository PropertyTypeRepository { get; }
		public IHostProfileRepository HostProfileRepository { get; }
		public IHomestayImageRepository HomestayImageRepository { get; }
		public IHomestayAmenityRepository HomestayAmenityRepository { get; }
		public IRuleRepository RuleRepository { get; }
		public IHomestayRuleRepository HomestayRuleRepository { get; }

		Task SaveChangesAsync();
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
