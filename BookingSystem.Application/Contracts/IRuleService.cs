using BookingSystem.Application.DTOs.RuleDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
    public interface IRuleService
    {
		Task<RuleDto?> CreateAsync(CreateRuleDto request);
		Task<RuleDto?> UpdateAsync(int id, UpdateRuleDto request);
		Task<bool> DeleteAsync(int id);
		Task<RuleDto?> GetByIdAsync(int id);
		Task<PagedResult<RuleDto>> GetAllRulesAsync(RuleFilter filter);
		Task<IEnumerable<RuleDto>> GetActiveRulesAsync();
		Task<IEnumerable<RuleDto>> GetByRuleTypeAsync(string ruleType);
		Task<bool> ToggleActiveStatusAsync(int id);
	}
}
