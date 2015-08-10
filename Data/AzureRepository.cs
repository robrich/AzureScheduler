namespace AzureScheduler.Data {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
	using AzureScheduler.Data.Models;
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.Management;
	using Microsoft.WindowsAzure.Management.Compute;
	using Microsoft.WindowsAzure.Management.Compute.Models;
	using Microsoft.WindowsAzure.Management.Models;
	using Microsoft.WindowsAzure.Management.Sql;
	using Microsoft.WindowsAzure.Management.Sql.Models;
	using Microsoft.WindowsAzure.Management.Storage;
	using Microsoft.WindowsAzure.Management.Storage.Models;
	using Microsoft.WindowsAzure.Management.WebSites;
	using Microsoft.WindowsAzure.Management.WebSites.Models;

	public class AzureRepository {

		private CertificateCloudCredentials GetCredentials(Subscription Subscription) {
			try {
				return new CertificateCloudCredentials(
					Subscription.SubscriptionId,
					new X509Certificate2(Convert.FromBase64String(Subscription.ManagementCert), (string)null, X509KeyStorageFlags.MachineKeySet)
				);
				// Azure throws if you don't use MachineKeySet, http://stackoverflow.com/a/27146917/702931
				// FRAGILE: Azure still throws if you're using the cert in the publishsettings file, use a self-signed cert instead, see README.md
			} catch (Exception ex) {
				throw new Exception("Error getting certificate for " + Subscription.SubscriptionId + ": " + ex.Message);
			}
		}

		// Enumerating services, from http://www.scip.be/index.php?Page=ArticlesNET39&Lang=EN
		public List<AzureInfo> GetForSubscription(Subscription Subscription) {
			List<AzureInfo> results = new List<AzureInfo>();

			CertificateCloudCredentials creds = this.GetCredentials(Subscription);

			string subscriptionName;
			string subscriptionStatus;
			using (ManagementClient mgmtClient = new ManagementClient(creds)) {
				SubscriptionGetResponse subscription = mgmtClient.Subscriptions.Get();
				subscriptionName = subscription.SubscriptionName;
				subscriptionStatus = subscription.SubscriptionStatus.ToString();
			}

			using (ComputeManagementClient vmClient = new ComputeManagementClient(creds)) {
				List<HostedServiceListResponse.HostedService> vms = vmClient.HostedServices.List().ToList();
				results.AddRange(
					from vm in vms
					let vmDetails = vmClient.Deployments.GetByName(vm.ServiceName, vm.ServiceName)
					select new AzureInfo {
						SubscriptionId = Subscription.SubscriptionId,
						SubscriptionName = subscriptionName,
						SubscriptionStatus = subscriptionStatus,
						AzureType = AzureType.VirtualMachine,
						Name = vm.ServiceName, Uri = vm.Uri,
						Urls = new List<string> { vmDetails.Uri.OriginalString }.Union((from i in vmDetails.VirtualIPAddresses select i.Address)).ToList(),
						CreationStatus = vm.Properties.Status.ToString(),
						Status = vmDetails.Status.ToString(),
						Location = vm.Properties.Location,
						AffinityGroup = vm.Properties.AffinityGroup,
						Label = vm.Properties.Label,
						Online = vmDetails.Status == DeploymentStatus.Running
					}
				);
			}

			using (ComputeManagementClient vmClient = new ComputeManagementClient(creds)) {
				List<VirtualMachineDiskListResponse.VirtualMachineDisk> disks = vmClient.VirtualMachineDisks.ListDisks().ToList();
				results.AddRange(
					from disk in disks
					select new AzureInfo {
						SubscriptionId = Subscription.SubscriptionId,
						SubscriptionName = subscriptionName,
						SubscriptionStatus = subscriptionStatus,
						AzureType = AzureType.VirtualDisk,
						Name = disk.Name,
						Location = disk.Location,
						AffinityGroup = disk.AffinityGroup,
						Label = disk.Label,
						MaxSizeGb = disk.LogicalSizeInGB,
						TypeDescription = disk.OperatingSystemType,
						HostedServiceName = disk.UsageDetails.HostedServiceName,
						DeploymentName = disk.UsageDetails.DeploymentName,
						VirtualMachineName = disk.UsageDetails.RoleName,
						Urls = new List<string>{disk.MediaLinkUri.OriginalString},
						Status = "Running",
						Online = !(disk.IsCorrupted ?? false)
					}
				);
			}

			using (WebSiteManagementClient siteClient = new WebSiteManagementClient(creds)) {
				WebSpacesListResponse spaces = siteClient.WebSpaces.List();
				results.AddRange(
					from space in spaces
					let sites = siteClient.WebSpaces.ListWebSites(space.Name, new WebSiteListParameters { PropertiesToInclude = {}})
					from site in sites
					let ws = siteClient.WebSites.Get(space.Name, site.Name, new WebSiteGetParameters()).WebSite
					select new AzureInfo {
						SubscriptionId = Subscription.SubscriptionId,
						SubscriptionName = subscriptionName,
						SubscriptionStatus = subscriptionStatus,
						AzureType = AzureType.WebApp,
						Name = site.Name,
						Uri = ws.Uri,
						Urls = site.HostNames.ToList(),
						Status = (site.RuntimeAvailabilityState != WebSiteRuntimeAvailabilityState.Normal) ? (site.RuntimeAvailabilityState ?? WebSiteRuntimeAvailabilityState.NotAvailable).ToString() : site.State,
						CreationStatus = "Created",
						Location = space.Name,
						TypeDescription = site.ServerFarm, // == ws.ServerFarm
						Online = (site.Enabled ?? false) && (site.RuntimeAvailabilityState == WebSiteRuntimeAvailabilityState.Normal) && (site.State == "Running")
					}
				);
			}

			using (StorageManagementClient storageClient = CloudContext.Clients.CreateStorageManagementClient(creds)) {
				List<StorageAccount> storages = storageClient.StorageAccounts.List().ToList();
				results.AddRange(
					from storage in storages
					select new AzureInfo {
						SubscriptionId = Subscription.SubscriptionId,
						SubscriptionName = subscriptionName,
						SubscriptionStatus = subscriptionStatus,
						AzureType = AzureType.Storage,
						Name = storage.Name,
						Uri = storage.Uri,
						Location = storage.Properties.Location,
						AffinityGroup = storage.Properties.AffinityGroup,
						Label = storage.Properties.Label,
						TypeDescription = storage.Properties.Description,
						Status = "Running",
						Online = storage.Properties.Status == StorageAccountStatus.Created,
						CreationStatus = storage.Properties.Status.ToString(),
						Redundancy = storage.Properties.AccountType,
						Urls = (
							from i in storage.Properties.Endpoints
							select i.OriginalString
						).ToList()
					}
				);
			}

			using (SqlManagementClient sqlClient = CloudContext.Clients.CreateSqlManagementClient(creds)) {
				List<Server> servers = sqlClient.Servers.List().ToList();
				results.AddRange(
					from server in servers
					let dbs = sqlClient.Databases.List(server.Name).ToList()
					from db in dbs
					select new AzureInfo {
						SubscriptionId = Subscription.SubscriptionId,
						SubscriptionName = subscriptionName,
						SubscriptionStatus = subscriptionStatus,
						AzureType = AzureType.SqlDatabase,
						Name = db.Name,
						Status = server.State,
						CreationStatus = server.State,
						Online = server.State == "Ready",
						Location = server.Location,
						TypeDescription = db.Edition,
						MaxSizeGb = db.MaximumDatabaseSizeInGB,
						ServerName = server.Name,
						HostedServiceName = server.FullyQualifiedDomainName,
					}
				);
			};

			return results;
		}

		public string SetWebApp(Subscription Subscription, string SpaceName, string SiteName, bool SetToRunning) {

			CertificateCloudCredentials creds = this.GetCredentials(Subscription);
			using (WebSiteManagementClient siteClient = new WebSiteManagementClient(creds)) {

				WebSiteGetResponse s = siteClient.WebSites.Get(SpaceName, SiteName, new WebSiteGetParameters());
				WebSite ws = s.WebSite;

				if (ws.State != "Running" && SetToRunning) {
					// Start it
					siteClient.WebSites.Update(SpaceName, SiteName, new WebSiteUpdateParameters {
						State = "Running",
						HostNames = ws.HostNames
					});
				}
				if (ws.State != "Stopped" && !SetToRunning) {
					// Stop it
					siteClient.WebSites.Update(SpaceName, SiteName, new WebSiteUpdateParameters {
						State = "Stopped",
						HostNames = ws.HostNames
					});
				}

				// What is it now?
				s = siteClient.WebSites.Get(SpaceName, SiteName, new WebSiteGetParameters());
				return s.WebSite.State;
			}
		}

		public string SetVm(Subscription Subscription, string VmName, bool SetToRunning) {

			CertificateCloudCredentials creds = this.GetCredentials(Subscription);
			using (ComputeManagementClient vmClient = new ComputeManagementClient(creds)) {

				// No idea why it wants the same name again and again or when this name would not be the same

				DeploymentGetResponse response = vmClient.Deployments.GetByName(VmName, VmName);

				if (response.Status != DeploymentStatus.Running && SetToRunning) {
					// Start it
					vmClient.VirtualMachines.Start(VmName, VmName, VmName);
				}

				if (response.Status == DeploymentStatus.Running && !SetToRunning) {
					// Stop it
					vmClient.VirtualMachines.Shutdown(
						VmName, VmName, VmName,
						new VirtualMachineShutdownParameters {PostShutdownAction = PostShutdownAction.StoppedDeallocated}
					);
				}

				// What is it now?
				response = vmClient.Deployments.GetByName(VmName, VmName);
				return response.Status.ToString();
			}
		}

	}
}