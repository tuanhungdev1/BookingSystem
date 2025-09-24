using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class AccommodationTypeRepository : Repository<AccommodationType>, IAccommodationTypeRepository
	{
		public AccommodationTypeRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<bool> IsNameExistsAsync(string name, Guid id)
		{
			return await _context.AccommodationTypes
				.AnyAsync(x => x.Name == name && x.Id != id);
		}

		public async Task<bool> IsNameExistsAsync(string name)
		{
			return await _dbSet.AnyAsync(at => at.Name == name);
		}

		public async Task<AccommodationType?> GetByNameAsync(string name)
		{
			return await _dbSet.SingleOrDefaultAsync(at => at.Name == name);
		}

		public async Task<IEnumerable<AccommodationType>> GetAllActiveAsync()
		{
			return await _dbSet.Where(at => at.IsActive).ToListAsync();
		}

		public async Task<IEnumerable<AccommodationType>> GetAllInactiveAsync()
		{
			return await _dbSet.Where(at => !at.IsActive).ToListAsync();
		}

		public async Task ActivateAsync(Guid id)
		{
			var accommodationType = await _dbSet.FindAsync(id);
			if (accommodationType != null)
			{
				accommodationType.IsActive = true;
				_dbSet.Update(accommodationType);
				await _context.SaveChangesAsync();
			}
		}

		public async Task DeactivateAsync(Guid id)
		{
			var accommodationType = await _dbSet.FindAsync(id);
			if (accommodationType != null)
			{
				accommodationType.IsActive = false;
				_dbSet.Update(accommodationType);
				await _context.SaveChangesAsync();
			}
		}
	}
}
