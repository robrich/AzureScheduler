namespace AzureScheduler.Stopper {
	using System.Configuration;
	using AzureScheduler.Data;
	using AzureScheduler.Data.Models;

	public static class Program {
		private static readonly AzureRepository azureRepository;
		private static readonly SubscriptionRepository subscriptionRepository;

		static Program() {
			azureRepository = new AzureRepository();
			subscriptionRepository = new SubscriptionRepository();
		}

		// TODO: move these to AppSettings?
		const string subscriptionName = "subscription1";
		const string vmName = "richardsonbuild";

		public static void Main(string[] args) {

			Subscription subscription = subscriptionRepository.GetSubscription(ConfigurationManager.AppSettings[subscriptionName]);
			azureRepository.SetVm(subscription, vmName, SetToRunning: false);

		}
	}
}
