using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ReviewDTO
{
	public class HelpfulToggleResult
	{
		public bool IsNowHelpful { get; set; } // true = đang helpful, false = đã hủy
		public int HelpfulCount { get; set; }
	}
}
