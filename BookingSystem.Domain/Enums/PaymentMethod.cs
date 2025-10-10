namespace BookingSystem.Domain.Enums
{
	public enum PaymentMethod
	{
		Cash = 0,              // Tiền mặt
		BankTransfer = 1,      // Chuyển khoản
		VNPay = 2,             // VNPay
		ZaloPay = 3,           // ZaloPay
		Momo = 4,              // Momo
		Stripe = 5,            // Stripe (International)
		PayPal = 6             // PayPal (International)
	}
}
