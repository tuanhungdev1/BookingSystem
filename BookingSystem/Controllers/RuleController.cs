using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.RuleDTO;
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
	public class RuleController : ControllerBase
	{
		private readonly IRuleService _ruleService;

		public RuleController(IRuleService ruleService)
		{
			_ruleService = ruleService;
		}

		/// <summary>
		/// Lấy danh sách Rule (có phân trang và filter)
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<RuleDto>>>> GetAll([FromQuery] RuleFilter filter)
		{
			var rules = await _ruleService.GetAllRulesAsync(filter);
			return Ok(new ApiResponse<PagedResult<RuleDto>>
			{
				Success = true,
				Message = "Rules retrieved successfully",
				Data = rules
			});
		}

		/// <summary>
		/// Lấy Rule theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<RuleDto>>> GetById(int id)
		{
			var rule = await _ruleService.GetByIdAsync(id);
			if (rule == null)
			{
				return NotFound(new ApiResponse<RuleDto>
				{
					Success = false,
					Message = "Rule not found"
				});
			}

			return Ok(new ApiResponse<RuleDto>
			{
				Success = true,
				Message = "Rule retrieved successfully",
				Data = rule
			});
		}

		/// <summary>
		/// Lấy danh sách Rule đang active
		/// </summary>
		[HttpGet("active")]
		public async Task<ActionResult<ApiResponse<IEnumerable<RuleDto>>>> GetActiveRules()
		{
			var rules = await _ruleService.GetActiveRulesAsync();
			return Ok(new ApiResponse<IEnumerable<RuleDto>>
			{
				Success = true,
				Message = "Active rules retrieved successfully",
				Data = rules
			});
		}

		/// <summary>
		/// Lấy danh sách Rule theo loại (Allowed / NotAllowed / Required)
		/// </summary>
		[HttpGet("type/{ruleType}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<RuleDto>>>> GetByRuleType(string ruleType)
		{
			var rules = await _ruleService.GetByRuleTypeAsync(ruleType);
			return Ok(new ApiResponse<IEnumerable<RuleDto>>
			{
				Success = true,
				Message = "Rules by type retrieved successfully",
				Data = rules
			});
		}

		/// <summary>
		/// Tạo mới Rule (Admin)
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<RuleDto>>> Create([FromForm] CreateRuleDto request)
		{
			var rule = await _ruleService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = rule?.Id }, new ApiResponse<RuleDto>
			{
				Success = true,
				Message = "Rule created successfully",
				Data = rule
			});
		}

		/// <summary>
		/// Cập nhật Rule (Admin)
		/// </summary>
		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<RuleDto>>> Update(int id, [FromForm] UpdateRuleDto request)
		{
			var rule = await _ruleService.UpdateAsync(id, request);
			if (rule == null)
			{
				return NotFound(new ApiResponse<RuleDto>
				{
					Success = false,
					Message = "Rule not found"
				});
			}

			return Ok(new ApiResponse<RuleDto>
			{
				Success = true,
				Message = "Rule updated successfully",
				Data = rule
			});
		}

		/// <summary>
		/// Xóa Rule (Admin)
		/// </summary>
		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			var success = await _ruleService.DeleteAsync(id);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Rule not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Rule deleted successfully"
			});
		}

		/// <summary>
		/// Chuyển đổi trạng thái Active / Inactive của Rule
		/// </summary>
		[HttpPut("{id:int}/toggle")]
		public async Task<ActionResult<ApiResponse<object>>> ToggleActiveStatus(int id)
		{
			var success = await _ruleService.ToggleActiveStatusAsync(id);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to toggle rule active status"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Rule active status toggled successfully"
			});
		}
	}
}
