using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class RemoveCouponDto
	{
		[Required]
		public string CouponCode { get; set; } = string.Empty;
	}
}
