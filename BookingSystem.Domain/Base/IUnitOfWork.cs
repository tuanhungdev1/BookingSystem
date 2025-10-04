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

		Task SaveChangesAsync();
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
