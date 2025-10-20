using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.RuleDTO;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class RuleService : IRuleService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IRuleRepository _ruleRepository;
		private readonly ILogger<RuleService> _logger;
		private readonly ICloudinaryService _cloudinaryService;

		public RuleService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IRuleRepository ruleRepository,
			ILogger<RuleService> logger,
			ICloudinaryService cloudinaryService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_ruleRepository = ruleRepository;
			_logger = logger;
			_cloudinaryService = cloudinaryService;
		}

		public async Task<RuleDto?> CreateAsync(CreateRuleDto request)
		{
			_logger.LogInformation("Starting rule creation process.");

			// Check if rule name already exists
			var existingRule = await _ruleRepository.GetByRuleNameAsync(request.RuleName);
			if (existingRule != null)
			{
				_logger.LogWarning("Rule with name {RuleName} already exists.", request.RuleName);
				throw new BadRequestException($"Rule with name {request.RuleName} already exists.");
			}

			string? uploadedPublicId = null;

			try
			{

				// Map DTO to entity
				var rule = _mapper.Map<Rule>(request);
				rule.CreatedAt = DateTime.UtcNow;
				rule.UpdatedAt = DateTime.UtcNow;

				// Upload icon if provided
				if (request.IconFile != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = request.IconFile,
						Folder = FolderImages.Rules
					});

					if (!uploadResult.Success || uploadResult.Data == null)
					{
						_logger.LogError("Icon upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException($"Icon upload failed: {uploadResult.ErrorMessage}");
					}

					uploadedPublicId = uploadResult.Data.PublicId;
					rule.IconUrl = uploadResult.Data.Url;
				}

				await _ruleRepository.AddAsync(rule);
				await _ruleRepository.SaveChangesAsync();

				_logger.LogInformation("Rule with ID {RuleId} created successfully.", rule.Id);

				return _mapper.Map<RuleDto>(rule);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during rule creation. Initiating rollback.");

				// Rollback uploaded icon in Cloudinary
				if (uploadedPublicId != null)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(uploadedPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete icon with PublicId {PublicId} during rollback.", uploadedPublicId);
					}
				}
				throw;
			}
		}

		public async Task<RuleDto?> UpdateAsync(int id, UpdateRuleDto request)
		{
			var rule = await _ruleRepository.GetByIdAsync(id);
			if (rule == null)
			{
				_logger.LogWarning("Rule with ID {RuleId} not found.", id);
				throw new NotFoundException($"Rule with ID {id} not found.");
			}

			// Check if new rule name conflicts with existing rules
			if (!string.IsNullOrWhiteSpace(request.RuleName) && request.RuleName != rule.RuleName)
			{
				var existingRule = await _ruleRepository.GetByRuleNameAsync(request.RuleName);
				if (existingRule != null)
				{
					_logger.LogWarning("Rule with name {RuleName} already exists.", request.RuleName);
					throw new BadRequestException($"Rule with name {request.RuleName} already exists.");
				}
			}

			string? oldPublicId = null;
			string? newPublicId = null;
			var currentIconUrl = rule.IconUrl;

			try
			{
				// Map updated fields from DTO to entity
				_mapper.Map(request, rule);
				rule.IconUrl = currentIconUrl;
				// Handle icon update
				if (request.IconFile != null)
				{
					// Delete old icon if exists
					if (!string.IsNullOrWhiteSpace(rule.IconUrl))
					{
						oldPublicId = _cloudinaryService.GetPublicIdFromUrl(rule.IconUrl);
					}

					// Upload new icon
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = request.IconFile,
						Folder = FolderImages.Rules
					});

					if (!uploadResult.Success || uploadResult.Data == null)
					{
						_logger.LogError("Icon upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException($"Icon upload failed: {uploadResult.ErrorMessage}");
					}

					newPublicId = uploadResult.Data.PublicId;
					rule.IconUrl = uploadResult.Data.Url;
				}
				rule.UpdatedAt = DateTime.UtcNow;

				_ruleRepository.Update(rule);
				await _ruleRepository.SaveChangesAsync();

				// Delete old icon from Cloudinary after successful update
				if (oldPublicId != null)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(oldPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete old icon with PublicId {PublicId} from Cloudinary.", oldPublicId);
					}
				}
				_logger.LogInformation("Rule with ID {RuleId} updated successfully.", id);

				return _mapper.Map<RuleDto>(rule);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during rule update. Initiating rollback.");

				// Rollback new uploaded icon
				if (newPublicId != null)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(newPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete icon with PublicId {PublicId} during rollback.", newPublicId);
					}
				}
				throw;
			}
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var rule = await _ruleRepository.GetByIdAsync(id);
			if (rule == null)
			{
				_logger.LogWarning("Rule with ID {RuleId} not found.", id);
				throw new NotFoundException($"Rule with ID {id} not found.");
			}

			try
			{
				// Delete icon from Cloudinary if exists
				if (!string.IsNullOrWhiteSpace(rule.IconUrl))
				{
					var publicId = _cloudinaryService.GetPublicIdFromUrl(rule.IconUrl);
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete icon with PublicId {PublicId} from Cloudinary.", publicId);
						throw new BadRequestException($"Failed to delete icon with PublicId {publicId} from Cloudinary.");
					}
				}

				_ruleRepository.Remove(rule);
				await _ruleRepository.SaveChangesAsync();
				_logger.LogInformation("Rule with ID {RuleId} deleted successfully.", id);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during rule deletion. Initiating rollback.");
				throw;
			}
		}

		public async Task<RuleDto?> GetByIdAsync(int id)
		{
			var rule = await _ruleRepository.GetByIdAsync(id);
			if (rule == null)
			{
				_logger.LogWarning("Rule with ID {RuleId} not found.", id);
				throw new NotFoundException($"Rule with ID {id} not found.");
			}

			return _mapper.Map<RuleDto>(rule);
		}

		public async Task<PagedResult<RuleDto>> GetAllRulesAsync(RuleFilter filter)
		{
			var pagedRules = await _ruleRepository.GetAllRulesAsync(filter);
			var ruleDtos = _mapper.Map<List<RuleDto>>(pagedRules.Items);

			return new PagedResult<RuleDto>
			{
				Items = ruleDtos,
				TotalCount = pagedRules.TotalCount,
				PageSize = pagedRules.PageSize,
				PageNumber = pagedRules.PageNumber
			};
		}

		public async Task<IEnumerable<RuleDto>> GetActiveRulesAsync()
		{
			var rules = await _ruleRepository.GetActiveRulesAsync();
			return _mapper.Map<IEnumerable<RuleDto>>(rules);
		}

		public async Task<IEnumerable<RuleDto>> GetByRuleTypeAsync(string ruleType)
		{
			// Validate rule type
			var validRuleTypes = new[] { "Allowed", "NotAllowed", "Required" };
			if (!validRuleTypes.Contains(ruleType))
			{
				_logger.LogWarning("Invalid rule type: {RuleType}", ruleType);
				throw new BadRequestException($"Invalid rule type: {ruleType}. Must be Allowed, NotAllowed, or Required.");
			}

			var rules = await _ruleRepository.GetByRuleTypeAsync(ruleType);
			return _mapper.Map<IEnumerable<RuleDto>>(rules);
		}

		public async Task<bool> ToggleActiveStatusAsync(int id)
		{
			var rule = await _ruleRepository.GetByIdAsync(id);
			if (rule == null)
			{
				_logger.LogWarning("Rule with ID {RuleId} not found.", id);
				throw new NotFoundException($"Rule with ID {id} not found.");
			}

			rule.IsActive = !rule.IsActive;
			rule.UpdatedAt = DateTime.UtcNow;

			_ruleRepository.Update(rule);
			await _ruleRepository.SaveChangesAsync();

			_logger.LogInformation("Rule with ID {RuleId} active status toggled to {IsActive}.", id, rule.IsActive);
			return true;
		}
	}
}