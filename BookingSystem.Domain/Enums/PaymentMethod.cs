namespace BookingSystem.Domain.Enums
{
	public enum PaymentMethod
	{
		Cash = 0,              // Tiền mặt
		CreditCard = 1,        // Thẻ tín dụng
		DebitCard = 2,         // Thẻ ghi nợ
		BankTransfer = 3,      // Chuyển khoản
		EWallet = 4,           // Ví điện tử
		PayPal = 5,            // PayPal
		Stripe = 6,            // Stripe
		VNPay = 7,             // VNPay
		Momo = 8,              // Momo
		ZaloPay = 9            // ZaloPay
	}
}
