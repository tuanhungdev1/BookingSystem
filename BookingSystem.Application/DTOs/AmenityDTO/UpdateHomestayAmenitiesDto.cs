
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class UpdateHomestayAmenitiesDto
	{
		// Danh sách ID tiện nghi muốn giữ lại (không xóa)
		[Required(ErrorMessage = "KeepAmenityIds is required.")]
		public List<int> KeepAmenityIds { get; set; } = new List<int>();

		// Danh sách tiện nghi mới muốn thêm
		public List<CreateHomestayAmenityDto> NewAmenities { get; set; } = new List<CreateHomestayAmenityDto>();

		// Danh sách tiện nghi hiện có cần cập nhật thông tin (CustomNote)
		public List<UpdateHomestayAmenityDto> UpdateExistingAmenities { get; set; } = new List<UpdateHomestayAmenityDto>();
	}
}
