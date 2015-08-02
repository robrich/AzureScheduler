namespace AzureScheduler.Data {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using AzureScheduler.Data.Models;

	public class SubscriptionRepository {

		public List<Subscription> GetSubscriptions() {

			// Get these from https://manage.windowsazure.com/publishsettings/index?client=powershell after authenticating to Azure as the account in question

			List<Subscription> results = new List<Subscription>();

			for (int i = 1; i < 10; i++) {
				string subscriptionId = ConfigurationManager.AppSettings["SubscriptionId" + i];
				string managementCert = ConfigurationManager.AppSettings["ManagementCert" + i]; // TODO: store certs in Azure as certs rather than app settings, see http://azure.microsoft.com/blog/2014/10/27/using-certificates-in-azure-websites-applications/
				if (!string.IsNullOrEmpty(subscriptionId) && !string.IsNullOrEmpty(managementCert)) {
					results.Add(new Subscription {SubscriptionId = subscriptionId, ManagementCert = managementCert});
				}
			}

			return results;
		}

		public Subscription GetSubscription(string SubscriptionId) {
			return (
				from s in this.GetSubscriptions()
				where string.Equals(s.SubscriptionId, SubscriptionId, StringComparison.InvariantCultureIgnoreCase)
				select s
			).FirstOrDefault();
		}

	}
}
