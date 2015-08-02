namespace AzureScheduler.Data.Models {
	using System;
	using System.Collections.Generic;

	public class AzureInfo {
		public string SubscriptionId { get; set; }
		public string SubscriptionName { get; set; }
		public string SubscriptionStatus { get; set; }
		public AzureType AzureType { get; set; }
		public string Name { get; set; }
		public List<string> Urls { get; set; }
		public string Status { get; set; }
		public string Location { get; set; }
		public string AffinityGroup { get; set; }
		public string Label { get; set; }
		public string TypeDescription { get; set; }

		public Uri Uri { get; set; }
		public string CreationStatus { get; set; }
		public int MaxSizeGb { get; set; }
		public string HostedServiceName { get; set; }
		public string DeploymentName { get; set; }
		public string VirtualMachineName { get; set; }
		public string Redundancy { get; set; }
		public string ServerName { get; set; }
		public bool Online { get; set; }
	}

	public enum AzureType {
		VirtualMachine,
		VirtualDisk,
		WebApp,
		Storage,
		SqlDatabase
	}
}
