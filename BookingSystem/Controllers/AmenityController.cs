using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] 
	public class AmenityController : ControllerBase
	{
		private readonly IAmenityService _amenityService;
		private readonly IGenericExportService _exportService;

		public AmenityController(IAmenityService amenityService, IGenericExportService exportService)
		{
			_amenityService = amenityService;
			_exportService = exportService;
		}


		
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<AmenityDto>>> GetById(int id)
		{
			var amenity = await _amenityService.GetByIdAsync(id);
			if (amenity == null)
			{
				return NotFound(new ApiResponse<AmenityDto>
				{
					Success = false,
					Message = "Amenity not found"
				});
			}

			return Ok(new ApiResponse<AmenityDto>
			{
				Success = true,
				Message = "Amenity retrieved successfully",
				Data = amenity
			});
		}

		
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<AmenityDto>>>> GetPaged([FromQuery] AmenityFilter filter)
		{
			var amenities = await _amenityService.GetPagedResultAsync(filter);

			return Ok(new ApiResponse<PagedResult<AmenityDto>>
			{
				Success = true,
				Message = "Amenities retrieved successfully",
				Data = amenities
			});
		}

	
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<AmenityDto>>> Create([FromForm] CreateAmenityDto request)
		{
			var amenity = await _amenityService.CreateAsync(request);
			return Ok(new ApiResponse<AmenityDto>
			{
				Success = true,
				Message = "Amenity created successfully",
				Data = amenity
			});
		}

	
		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<AmenityDto>>> Update(int id, [FromForm] UpdateAmenityDto request)
		{
			var amenity = await _amenityService.UpdateAsync(id, request);
			if (amenity == null)
			{
				return NotFound(new ApiResponse<AmenityDto>
				{
					Success = false,
					Message = "Amenity not found"
				});
			}

			return Ok(new ApiResponse<AmenityDto>
			{
				Success = true,
				Message = "Amenity updated successfully",
				Data = amenity
			});
		}


		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			await _amenityService.DeleteAsync(id);

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Amenity deleted successfully"
			});
		}

		/// <summary>
		/// Thay đổi trạng thái hoạt động của Amenity (Admin only)
		/// </summary>
		[HttpPut("{id:int}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> SetActiveStatus(int id, [FromBody] bool isActive)
		{
			var success = await _amenityService.SetActiveStatusAsync(id, isActive);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Amenity not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Amenity status set to {(isActive ? "active" : "inactive")} successfully"
			});
		}
	}
}
