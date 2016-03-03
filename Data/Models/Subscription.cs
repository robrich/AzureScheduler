namespace AzureScheduler.Data.Models {
	public class Subscription {
		public int Sequence { get; set; }
		public string SubscriptionId { get; set; }
		public string ManagementCert { get; set; }
        public string ManagementCertPassword { get; set; }
    }
}