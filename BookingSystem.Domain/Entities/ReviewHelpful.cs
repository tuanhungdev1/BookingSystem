using BookingSystem.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	public class ReviewHelpful : BaseEntity
	{
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		public int ReviewId { get; set; }
		public Review Review { get; set; } = null!;
	}
}
