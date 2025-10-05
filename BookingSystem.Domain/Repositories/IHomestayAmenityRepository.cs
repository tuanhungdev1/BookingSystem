using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Repositories
{
    public interface IHomestayAmenityRepository : IRepository<HomestayAmenity>
	{
        Task<List<HomestayAmenity>> GetByHomestayIdAsync(int homestayId);
	}
}
