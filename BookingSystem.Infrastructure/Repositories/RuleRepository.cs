using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
    public class RuleRepository : Repository<Rule>, IRuleRepository
	{
		public RuleRepository(BookingDbContext context) : base(context) { }

		public async Task<PagedResult<Rule>> GetAllRulesAsync(RuleFilter filter)
		{
			var query = _dbSet.AsQueryable();

			// Apply filters

			if (!string.IsNullOrEmpty(filter.Search))
			{
				var searchTerm = filter.Search.ToLower();
				query = query.Where(r => r.RuleName.ToLower().Contains(searchTerm) ||
										 r.RuleType.ToLower().Contains(searchTerm) ||
										 r.RuleDescription.ToLower().Contains(searchTerm)
										 );
			}

			if (!string.IsNullOrWhiteSpace(filter.RuleName))
			{
				query = query.Where(r => r.RuleName.Contains(filter.RuleName));
			}

			if (!string.IsNullOrWhiteSpace(filter.RuleType))
			{
				query = query.Where(r => r.RuleType == filter.RuleType);
			}

			if (filter.IsActive.HasValue)
			{
				query = query.Where(r => r.IsActive == filter.IsActive.Value);
			}

			// Get total count
			var totalCount = await query.CountAsync();

			// Apply sorting
			query = filter.SortBy switch
			{
				"ruleName" => filter.SortOrder == "desc" ? query.OrderByDescending(r => r.RuleName) : query.OrderBy(r => r.RuleName),
				"ruleDescription" => filter.SortOrder == "desc" ? query.OrderByDescending(r => r.RuleDescription) : query.OrderBy(r => r.RuleDescription),
				"ruleType" => filter.SortOrder == "desc" ? query.OrderByDescending(r => r.RuleType) : query.OrderBy(r => r.RuleType),
				"createdAt" => filter.SortOrder == "desc" ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
				"displayOrder" => filter.SortOrder == "desc" ? query.OrderByDescending(r => r.DisplayOrder) : query.OrderBy(r => r.DisplayOrder),
				_ => query.OrderBy(r => r.CreatedAt) // Default sorting
			};

			// Apply pagination
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Rule>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<IEnumerable<Rule>> GetActiveRulesAsync()
		{
			return await _dbSet
				.Where(r => r.IsActive)
				.OrderBy(r => r.DisplayOrder)
				.ThenBy(r => r.RuleName)
				.ToListAsync();
		}

		public async Task<Rule?> GetByRuleNameAsync(string ruleName)
		{
			return await _dbSet
				.FirstOrDefaultAsync(r => r.RuleName == ruleName);
		}

		public async Task<IEnumerable<Rule>> GetByRuleTypeAsync(string ruleType)
		{
			return await _dbSet
				.Where(r => r.RuleType == ruleType && r.IsActive)
				.OrderBy(r => r.DisplayOrder)
				.ToListAsync();
		}
	}
}
