namespace BookingSystem.Application.Models.Constants
{
	public static class PreferenceKeys
	{
		// Display preferences
		public const string Language = "language";
		public const string Theme = "theme";
		public const string Currency = "currency";
		public const string DateFormat = "date_format";
		public const string TimeFormat = "time_format";

		// Notification preferences
		public const string EmailNotifications = "email_notifications";
		public const string PushNotifications = "push_notifications";
		public const string SmsNotifications = "sms_notifications";
		public const string MarketingEmails = "marketing_emails";

		// Booking preferences
		public const string DefaultGuests = "default_guests";
		public const string DefaultCheckInTime = "default_checkin_time";
		public const string DefaultCheckOutTime = "default_checkout_time";
		public const string PreferredPaymentMethod = "preferred_payment_method";

		// Privacy preferences
		public const string ProfileVisibility = "profile_visibility";
		public const string ShowEmail = "show_email";
		public const string ShowPhone = "show_phone";

		// Search preferences
		public const string DefaultSearchRadius = "default_search_radius";
		public const string DefaultPriceRange = "default_price_range";
		public const string PreferredAmenities = "preferred_amenities";

		// Host preferences
		public const string AutoAcceptBookings = "auto_accept_bookings";
		public const string MinimumAdvanceBooking = "minimum_advance_booking";
		public const string MaximumAdvanceBooking = "maximum_advance_booking";
	}
}
