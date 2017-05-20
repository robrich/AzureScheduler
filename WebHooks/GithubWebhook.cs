namespace AzureScheduler.WebHooks {
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using AzureScheduler.Data;
    using AzureScheduler.Data.Models;

    public static class GithubWebhook {
        private static readonly AzureRepository azureRepository;
        private static readonly SubscriptionRepository subscriptionRepository;

        static GithubWebhook() {
            azureRepository = new AzureRepository();
            subscriptionRepository = new SubscriptionRepository();
        }

        // TODO: move these to AppSettings?
        const int sequence = 2;
        const string vmName = "richardsonbuild";
        
        [FunctionName("GithubWebhook")]        
        public static async Task<HttpResponseMessage> Run([HttpTrigger(WebHookType = "github")]HttpRequestMessage req) {

            Subscription subscription = subscriptionRepository.GetSubscriptionSequence(sequence);
            if (subscription == null) {
                throw new ArgumentException("Subscription sequence " + sequence + " is null");
            }
            azureRepository.SetVm(subscription, vmName, SetToRunning: true);

            await Task.FromResult(0);

            return req.CreateResponse(HttpStatusCode.OK, "awoken");
        }

    }
}
