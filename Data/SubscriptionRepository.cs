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
                string managementCertPassword = ConfigurationManager.AppSettings["ManagementCertPassword" + i];
                if (!string.IsNullOrEmpty(subscriptionId) && !string.IsNullOrEmpty(managementCert) && !string.IsNullOrEmpty(managementCertPassword))
                {
                    results.Add(new Subscription
                    {
                        Sequence = i,
                        SubscriptionId = subscriptionId,
                        ManagementCert = managementCert,
                        ManagementCertPassword = managementCertPassword,
                    });
                }
            }

			return results;
		}

		public Subscription GetSubscriptionSequence(int Sequence) {
			return (
				from s in this.GetSubscriptions()
				where s.Sequence == Sequence
				select s
			).FirstOrDefault();
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
