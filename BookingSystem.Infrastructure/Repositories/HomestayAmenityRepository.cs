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
    public class HomestayAmenityRepository : Repository<HomestayAmenity>, IHomestayAmenityRepository
	{
		public HomestayAmenityRepository(BookingDbContext context) : base(context) { }

		public async Task<List<HomestayAmenity>> GetByHomestayIdAsync(int homestayId)
		{
			return await Task.FromResult(_context.HomestayAmenities.Where(amenity => amenity.HomestayId == homestayId).ToList());
		}
	}
}
