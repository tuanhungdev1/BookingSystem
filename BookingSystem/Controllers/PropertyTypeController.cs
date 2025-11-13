using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] // Require authentication for all endpoints
	public class PropertyTypeController : ControllerBase
	{
		private readonly IPropertyTypeService _propertyTypeService;

		public PropertyTypeController(IPropertyTypeService propertyTypeService)
		{
			_propertyTypeService = propertyTypeService;
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<PropertyTypeDto>>> GetById(int id)
		{
			var propertyType = await _propertyTypeService.GetByIdAsync(id);
			if (propertyType == null)
			{
				return NotFound(new ApiResponse<PropertyTypeDto>
				{
					Success = false,
					Message = "Property type not found"
				});
			}

			return Ok(new ApiResponse<PropertyTypeDto>
			{
				Success = true,
				Message = "Property type retrieved successfully",
				Data = propertyType
			});
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<PagedResult<PropertyTypeDto>>>> GetPaged([FromQuery] PropertyTypeFilter filter)
		{
			var propertyTypes = await _propertyTypeService.GetPagedResultAsync(filter);

			return Ok(new ApiResponse<PagedResult<PropertyTypeDto>>
			{
				Success = true,
				Message = "Property types retrieved successfully",
				Data = propertyTypes
			});
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PropertyTypeDto>>> Create([FromForm] CreatePropertyTypeDto request)
		{
			var propertyType = await _propertyTypeService.CreateAsync(request);
			return Ok(new ApiResponse<PropertyTypeDto>
			{
				Success = true,
				Message = "Property type created successfully",
				Data = propertyType
			});
		}

		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PropertyTypeDto>>> Update(int id, [FromForm] UpdatePropertyTypeDto request)
		{
			var propertyType = await _propertyTypeService.UpdateAsync(id, request);
			if (propertyType == null)
			{
				return NotFound(new ApiResponse<PropertyTypeDto>
				{
					Success = false,
					Message = "Property type not found"
				});
			}

			return Ok(new ApiResponse<PropertyTypeDto>
			{
				Success = true,
				Message = "Property type updated successfully",
				Data = propertyType
			});
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			await _propertyTypeService.DeleteAsync(id);

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Property type deleted successfully"
			});
		}

		[HttpPut("{id:int}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> SetActiveStatus(int id, [FromBody] bool isActive)
		{
			var success = await _propertyTypeService.SetActiveStatusAsync(id, isActive);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Property type not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Property type status set to {(isActive ? "active" : "inactive")} successfully"
			});
		}
	}
}
