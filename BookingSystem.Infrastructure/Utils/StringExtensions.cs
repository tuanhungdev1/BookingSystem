using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Utils
{
	public static class StringExtensions
	{
		private static readonly string[] VietnameseSigns = new string[]
		{
		"aAeEoOuUiIdDyY",
		"áàạảãâấầậẩẫăắằặẳẵ",
		"ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
		"éèẹẻẽêếềệểễ",
		"ÉÈẸẺẼÊẾỀỆỂỄ",
		"óòọỏõôốồộổỗơớờợởỡ",
		"ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
		"úùụủũưứừựửữ",
		"ÚÙỤỦŨƯỨỪỰỬỮ",
		"íìịỉĩ",
		"ÍÌỊỈĨ",
		"đ",
		"Đ",
		"ýỳỵỷỹ",
		"ÝỲỴỶỸ"
		};

		public static string RemoveDiacritics(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			for (int i = 1; i < VietnameseSigns.Length; i++)
			{
				for (int j = 0; j < VietnameseSigns[i].Length; j++)
					text = text.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
			}
			return text;
		}
	}
}
