namespace AzureScheduler.Starter {
	using System;
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
		const int sequence = 2;
		const string vmName = "richardsonbuild";

		public static void Main(string[] args) {

			Subscription subscription = subscriptionRepository.GetSubscriptionSequence(sequence);
			if (subscription == null) {
				throw new ArgumentException("Subscription sequence " + sequence + " is null");
			}
			azureRepository.SetVm(subscription, vmName, SetToRunning: true);

		}
	}
}
