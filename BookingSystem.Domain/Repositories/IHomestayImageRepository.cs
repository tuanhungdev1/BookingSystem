using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Repositories
{
    public interface IHomestayImageRepository : IRepository<HomestayImage>
	{
        Task<List<HomestayImage>> GetByHomestayIdAsync(int homestayId);
	}
}
