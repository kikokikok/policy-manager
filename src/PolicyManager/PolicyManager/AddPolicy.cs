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
    public static class AddPolicy
    {
        [FunctionName(nameof(AddPolicy))]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"{nameof(AddPolicy)} Invoked");

            var claimsPrincipal = await AuthHelper.ValidateTokenAsync(req?.Headers?.Authorization, log);
            if (claimsPrincipal == null) return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            var userPrincipalName = claimsPrincipal.Identity.Name;

            var policyRule = await req.Content.ReadAsAsync<PolicyRule>();
            policyRule.RowKey = Guid.NewGuid().ToString();
            policyRule.PartitionKey = policyRule.Category;
            policyRule.CreatedBy = userPrincipalName;
            policyRule.CreatedDate = DateTime.UtcNow;
            policyRule.LastModifiedBy = userPrincipalName;
            policyRule.ModifiedDate = DateTime.UtcNow;

            var dataRepository = ServiceLocator.GetRequiredService<IDataRepository<PolicyRule>>();
            var resultPolicyRule = await dataRepository.CreateItemAsync(policyRule);

            return new OkObjectResult(resultPolicyRule);
        }
    }
}
