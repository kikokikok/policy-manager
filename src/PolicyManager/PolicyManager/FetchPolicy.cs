using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolicyManager.DataAccess;
using PolicyManager.DataAccess.Models;
using PolicyManager.DataAccess.Repositories;
using PolicyManager.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolicyManager
{
    public static class FetchPolicy
    {
        [FunctionName("FetchPolicy")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("Fetch Policy Invoked.");

            var claimsPrincipal = await AuthHelper.ValidateTokenAsync(req?.Headers?.Authorization, log);
            if (claimsPrincipal == null) return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            var userPrincipalName = claimsPrincipal.Identity.Name;

            var queryString = req.RequestUri.ParseQueryString();
            var id = Convert.ToString(queryString["id"]);
            var partition = Convert.ToString(queryString["category"]);

            var dataRepository = ServiceLocator.GetRequiredService<IDataRepository<PolicyRule>>();
            var policyRule = await dataRepository.ReadItemAsync(partition, id);

            return new OkObjectResult(policyRule);
        }
    }
}
