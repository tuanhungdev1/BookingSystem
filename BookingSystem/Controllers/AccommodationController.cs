using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AccommodationController : ControllerBase
	{
		private readonly IAccommodationService _accommodationService;

		public AccommodationController(IAccommodationService accommodationService)
		{
			_accommodationService = accommodationService;
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse<AccommodationDto>>> Create([FromBody] CreateAccommodationRequest request)
		{
			var accommodation = await _accommodationService.CreateAsync(request);
			return Ok(new ApiResponse<AccommodationDto>
			{
				Success = true,
				Message = "Accommodation created successfully",
				Data = accommodation
			});
		}

		[HttpPut("{id:guid}")]
		public async Task<ActionResult<ApiResponse<AccommodationDto>>> Update(Guid id, [FromBody] UpdateAccommodationRequest request)
		{
			var accommodation = await _accommodationService.UpdateAsync(id, request);
			return Ok(new ApiResponse<AccommodationDto>
			{
				Success = true,
				Message = "Accommodation updated successfully",
				Data = accommodation
			});
		}

		[HttpDelete("{id:guid}")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
		{
			var success = await _accommodationService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Accommodation deleted successfully" : "Accommodation not found"
			});
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<ApiResponse<AccommodationDto>>> GetById(Guid id)
		{
			var accommodation = await _accommodationService.GetByIdAsync(id);
			if (accommodation == null)
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Accommodation not found"
				});

			return Ok(new ApiResponse<AccommodationDto>
			{
				Success = true,
				Message = "Accommodation retrieved successfully",
				Data = accommodation
			});
		}

		[HttpGet]
		public async Task<ActionResult<ApiResponse<PagedResult<AccommodationDto>>>> GetPaged([FromQuery] AccommodationFilter filter)
		{
			var pagedResult = await _accommodationService.GetPagedAsync(filter);
			return Ok(new ApiResponse<PagedResult<AccommodationDto>>
			{
				Success = true,
				Message = "Accommodations retrieved successfully",
				Data = pagedResult
			});
		}

		[HttpGet("search")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationDto>>>> Search([FromQuery] string searchTerm)
		{
			var accommodations = await _accommodationService.SearchAsync(searchTerm);
			return Ok(new ApiResponse<IEnumerable<AccommodationDto>>
			{
				Success = true,
				Message = "Search completed successfully",
				Data = accommodations
			});
		}

		[HttpGet("by-location")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationDto>>>> GetByLocation([FromQuery] string city, [FromQuery] string country)
		{
			var accommodations = await _accommodationService.GetByLocationAsync(city, country);
			return Ok(new ApiResponse<IEnumerable<AccommodationDto>>
			{
				Success = true,
				Message = "Accommodations retrieved by location",
				Data = accommodations
			});
		}

		[HttpGet("by-type/{id}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationDto>>>> GetByType([FromQuery] Guid id)
		{
			var accommodations = await _accommodationService.GetByTypeAsync(id);
			return Ok(new ApiResponse<IEnumerable<AccommodationDto>>
			{
				Success = true,
				Message = "Accommodations retrieved by type",
				Data = accommodations
			});
		}

		[HttpPost("{id:guid}/activate")]
		public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
		{
			var success = await _accommodationService.ActivateAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Accommodation activated" : "Accommodation not found"
			});
		}

		[HttpPost("{id:guid}/deactivate")]
		public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
		{
			var success = await _accommodationService.DeactivateAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Accommodation deactivated" : "Accommodation not found"
			});
		}
	}
}
