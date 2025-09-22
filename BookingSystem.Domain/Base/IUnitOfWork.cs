using BookingSystem.Domain.Repositories;

namespace BookingSystem.Domain.Base
{
	public interface IUnitOfWork : IDisposable
	{
		public IUserRepository UserRepository { get; }
		Task SaveChangesAsync();
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
