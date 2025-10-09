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
    public class CouponHomestaysRepository : Repository<CouponHomestay>, ICouponHomestaysRepository
	{
		public CouponHomestaysRepository(BookingDbContext context) : base(context)
		{
		}
	}
}
