using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;

namespace BookingSystem.Infrastructure.Repositories
{
    public class HomestayImageRepository : Repository<HomestayImage>, IHomestayImageRepository
	{
		public HomestayImageRepository(BookingDbContext context) : base(context)
		{
			
		}

		public async Task<List<HomestayImage>> GetByHomestayIdAsync(int homestayId)
		{
			return await Task.FromResult(_context.HomestayImages.Where(img => img.HomestayId == homestayId).ToList());
		}
	}
}
