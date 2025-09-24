using BookingSystem.Application.DTOs.AccommodationTypeDTO;
using BookingSystem.Domain.Base;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
	public interface IAccommodationTypeService
	{
		Task<AccommodationTypeDto?> CreateAsync(CreateAccommodationTypeDto request);
		Task<AccommodationTypeDto?> UpdateAsync(Guid id, UpdateAccommodationTypeDto request);
		Task<bool> DeleteAsync(Guid id);
		Task<AccommodationTypeDto?> GetByIdAsync(Guid id);
		Task<IEnumerable<AccommodationTypeDto>> GetAllAccommodationTypeAsync();
		Task<IEnumerable<AccommodationTypeDto>> GetAllActiveAsync();
		Task<IEnumerable<AccommodationTypeDto>> GetAllInactiveAsync();
		Task<bool> ActivateAsync(Guid id);
		Task<bool> DeactivateAsync(Guid id);
	}
}
