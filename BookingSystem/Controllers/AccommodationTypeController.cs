using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationTypeDTO;
using BookingSystem.Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AccommodationTypeController : ControllerBase
	{
		private readonly IAccommodationTypeService _accommodationTypeService;

		public AccommodationTypeController(IAccommodationTypeService accommodationTypeService)
		{
			_accommodationTypeService = accommodationTypeService;
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse<AccommodationTypeDto>>> Create([FromForm] CreateAccommodationTypeDto request)
		{
			var accommodationType = await _accommodationTypeService.CreateAsync(request);
			return Ok(new ApiResponse<AccommodationTypeDto>
			{
				Success = true,
				Message = "AccommodationType created successfully",
				Data = accommodationType
			});
		}

		[HttpPut("{id:guid}")]
		public async Task<ActionResult<ApiResponse<AccommodationTypeDto>>> Update(Guid id, [FromForm] UpdateAccommodationTypeDto request)
		{
			var accommodationType = await _accommodationTypeService.UpdateAsync(id, request);
			return Ok(new ApiResponse<AccommodationTypeDto>
			{
				Success = true,
				Message = "AccommodationType updated successfully",
				Data = accommodationType
			});
		}

		[HttpDelete("{id:guid}")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
		{
			var success = await _accommodationTypeService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "AccommodationType deleted successfully" : "AccommodationType not found"
			});
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<ApiResponse<AccommodationTypeDto>>> GetById(Guid id)
		{
			var accommodationType = await _accommodationTypeService.GetByIdAsync(id);
			if (accommodationType == null)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "AccommodationType not found"
				});
			}

			return Ok(new ApiResponse<AccommodationTypeDto>
			{
				Success = true,
				Message = "AccommodationType retrieved successfully",
				Data = accommodationType
			});
		}

		[HttpGet]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationTypeDto>>>> GetAll()
		{
			var list = await _accommodationTypeService.GetAllAccommodationTypeAsync();
			return Ok(new ApiResponse<IEnumerable<AccommodationTypeDto>>
			{
				Success = true,
				Message = "AccommodationTypes retrieved successfully",
				Data = list
			});
		}

		[HttpGet("active")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationTypeDto>>>> GetAllActive()
		{
			var list = await _accommodationTypeService.GetAllActiveAsync();
			return Ok(new ApiResponse<IEnumerable<AccommodationTypeDto>>
			{
				Success = true,
				Message = "Active AccommodationTypes retrieved successfully",
				Data = list
			});
		}

		[HttpGet("inactive")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AccommodationTypeDto>>>> GetAllInactive()
		{
			var list = await _accommodationTypeService.GetAllInactiveAsync();
			return Ok(new ApiResponse<IEnumerable<AccommodationTypeDto>>
			{
				Success = true,
				Message = "Inactive AccommodationTypes retrieved successfully",
				Data = list
			});
		}

		[HttpPost("{id:guid}/activate")]
		public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
		{
			var success = await _accommodationTypeService.ActivateAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "AccommodationType activated" : "AccommodationType not found"
			});
		}

		[HttpPost("{id:guid}/deactivate")]
		public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
		{
			var success = await _accommodationTypeService.DeactivateAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "AccommodationType deactivated" : "AccommodationType not found"
			});
		}
	}
}
