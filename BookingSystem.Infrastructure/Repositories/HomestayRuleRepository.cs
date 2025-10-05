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
    public class HomestayRuleRepository : Repository<HomestayRule>, IHomestayRuleRepository
	{
		public HomestayRuleRepository(BookingDbContext booking) : base(booking)
		{
			
		}

		public async Task<List<HomestayRule>> GetByHomestayIdAsync(int homestayId)
		{
			return await Task.FromResult(_context.HomestayRules.Where(rule => rule.HomestayId == homestayId).ToList());
		}
	}
}
