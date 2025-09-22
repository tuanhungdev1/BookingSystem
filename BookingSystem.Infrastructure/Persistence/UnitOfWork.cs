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

		public UnitOfWork(BookingDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
		}

		public IUserRepository UserRepository => _userRepository.Value;

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
