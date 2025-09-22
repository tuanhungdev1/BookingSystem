using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class Enable2FARequest
	{
		[Required(ErrorMessage = "Enable flag is required.")]
		public bool Enable { get; set; }
	}
}
