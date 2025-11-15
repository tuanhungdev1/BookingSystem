using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class RefundStatusDto
	{
		public int PaymentId { get; set; }

		public decimal OriginalAmount { get; set; }

		public decimal RefundedAmount { get; set; }

		public decimal RefundableAmount { get; set; }

		public bool CanRefund { get; set; }


		public double RefundedPercentage => OriginalAmount > 0
			? Math.Round((double)(RefundedAmount / OriginalAmount) * 100, 2)
			: 0;


		public double RefundablePercentage => 100 - RefundedPercentage;

		public string RefundStatusDisplay => CanRefund
			? "Có thể hoàn tiền"
			: RefundedAmount >= OriginalAmount
				? "Đã hoàn tất"
				: "Không thể hoàn tiền";
		public string SuggestedColor => CanRefund
			? "success"   // xanh
			: RefundedAmount > 0
				? "warning" // vàng
				: "error";   // đỏ
	}
}
