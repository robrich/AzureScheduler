namespace AzureScheduler.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using AzureScheduler.Data;
	using AzureScheduler.Data.Models;

	[Authorize]
	public class AzureController : Controller {
		private readonly AzureRepository azureRepository;
		private readonly SubscriptionRepository subscriptionRepository;

		public AzureController() {
			this.azureRepository = new AzureRepository();
			this.subscriptionRepository = new SubscriptionRepository();
		}

		public ActionResult Index() {

			List<AzureInfo> model = new List<AzureInfo>();
			List<Subscription> subscriptions = this.subscriptionRepository.GetSubscriptions();
			foreach (Subscription subscription in subscriptions) {
				List<AzureInfo> data = this.azureRepository.GetForSubscription(subscription);
				if (data.Count > 0) {
					model.AddRange(data);
				}
			}
			model = (
				from m in model
				orderby m.Name, m.AzureType
				select m
			).ToList();

			return this.View(model);
		}

		public ActionResult SetWebApp(string SubscriptionId, string Location, string SiteName, bool SetToRunning) {

			try {
				Subscription subscription = this.subscriptionRepository.GetSubscription(SubscriptionId);
				string newStatus = this.azureRepository.SetWebApp(subscription, Location, SiteName, SetToRunning);

				return this.Json(new {Success = true, Status = newStatus});
			} catch (Exception ex) {
				return this.Json(new {Success = false, Message = ex.Message});
			}
		}

		public ActionResult SetVm(string SubscriptionId, string VmName, bool SetToRunning) {

			try {
				Subscription subscription = this.subscriptionRepository.GetSubscription(SubscriptionId);
				string newStatus = this.azureRepository.SetVm(subscription, VmName, SetToRunning);

				return this.Json(new {Success = true, Status = newStatus});
			} catch (Exception ex) {
				return this.Json(new {Success = false, Message = ex.Message});
			}
		}

	}
}
