using MigrateYamlPipeline.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MigrateYamlPipeline.Common
{
    public class DevOpsHttpClient : HttpClient
    {
        private readonly Dictionary<string, HttpResponseMessage> getCache = new Dictionary<string, HttpResponseMessage>();
        private readonly string Organization;
        private readonly string Project;

        public DevOpsHttpClient(string organization, string project, string personalAccessToken)
        {
            Organization = organization;
            Project = project;
            BaseAddress = new Uri($"https://dev.azure.com/{Organization}/");
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));
        }

        public async Task<List<EnvironmentDto>> GetEnvironmentsAsync(string searchName)
        {
            HttpResponseMessage response = await GetCacheAsync($"{Project}/_apis/pipelines/environments?name={searchName}&$top=200&api-version=7.2-preview.1");

            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListDto<EnvironmentDto>>(jsonStr).Value;
            }
            else
            {
                await Console.Out.WriteLineAsync(response.StatusCode.ToString());
                return new List<EnvironmentDto>();
            }
        }

        public async Task<List<RoleassignmentDto>> GetRoleassignmentsAsync(EnvironmentDto environment)
        {
            HttpResponseMessage response = await GetCacheAsync($"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{environment.Project.Id}_{environment.Id}?api-version=7.2-preview.1");

            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListDto<RoleassignmentDto>>(jsonStr).Value;
            }
            else
            {
                return new List<RoleassignmentDto>();
            }
        }

        public async Task<bool> PutRoleassignmentsAsync(EnvironmentDto environment, List<PutRoleassignmentDto> putRoleassignment)
        {
            await Console.Out.WriteLineAsync($"\nEnvironment: {environment.Name}");
            string jsonStr = JsonConvert.SerializeObject(putRoleassignment);
            StringContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await PutAsync($"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{environment.Project.Id}_{environment.Id}?api-version=7.2-preview.1", content);
            await Console.Out.WriteLineAsync($"Put Roleassignments Result: {response.IsSuccessStatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CopyRoleassignmentsAsync(EnvironmentDto sourceEnvironment, EnvironmentDto targetEnvironment)
        {
            List<RoleassignmentDto> roleAssignments = await GetRoleassignmentsAsync(sourceEnvironment);
            List<PutRoleassignmentDto> putRoleassignment = new List<PutRoleassignmentDto>();

            foreach (RoleassignmentDto roleAssignment in roleAssignments)
            {
                putRoleassignment.Add(new PutRoleassignmentDto()
                {
                    RoleName = roleAssignment.Role.Name,
                    UserId = roleAssignment.Identity.Id
                });
            }

            return await PutRoleassignmentsAsync(targetEnvironment, putRoleassignment);
        }

        public async Task<List<EnvironmentCheckDto>> GetApprovalCheckAsync(EnvironmentDto environment)
        {
            HttpResponseMessage response = await GetCacheAsync($"{Project}/_apis/pipelines/checks/configurations?resourceType=environment&resourceId={environment.Id}&api-version=7.1-preview.1");

            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListDto<EnvironmentCheckDto>>(jsonStr).Value;
            }
            else
            {
                return new List<EnvironmentCheckDto>();
            }
        }

        public async Task<bool> AddPreApprovalAsync(EnvironmentDto environment, List<string> approverIds)
        {
            await Console.Out.WriteLineAsync($"\nEnvironment: {environment.Name}");
            var approvalCheckDto = new ApprovalCheckDto();
            approvalCheckDto.Resource = new ResourceDto() { Id = environment.Id.ToString(), Type = "environment" };
            approvalCheckDto.Type = new EnvironmentCheckTypeDto() { Id = "8C6F20A7-A545-4486-9777-F762FAFE0D4D", Name = "Approval" };
            approvalCheckDto.Settings = new ApprovalCheckSettingsDto() { 
                Approvers = approverIds.Select(approverId => new ResourceDto() { Id = approverId }).ToList(), 
                DefinitionRef = new ResourceDto() { Id = "0f52a19b-c67e-468f-b8eb-0ae83b532c99" },
                BlockedApprovers = new List<ResourceDto>(),
                ExecutionOrder = 1,
                RequesterCannotBeApprover = false
            };
            approvalCheckDto.Timeout = 43200;
            string jsonStr = JsonConvert.SerializeObject(approvalCheckDto);
            StringContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await PostAsync($"{Project}/_apis/pipelines/checks/configurations?api-version=7.1-preview.1", content);
            await Console.Out.WriteLineAsync($"Add Approval check Result: {response.IsSuccessStatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<List<IdentityDto>> GetApprovalUserIdAsync(string userDisplayName)
        {   
            HttpResponseMessage response = await GetCacheAsync($"https://vssps.dev.azure.com/{Organization}/_apis/identities?searchFilter=DisplayName&filterValue={userDisplayName}&api-version=7.1-preview.1");
            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ListDto<IdentityDto>>(jsonStr).Value;
            }
            else
            {
                return new List<IdentityDto>();
            }
        }

        public async Task<HttpResponseMessage> GetCacheAsync(string requestUri, bool isCache = true) 
        {
            if (isCache && getCache.TryGetValue(requestUri, out var value)) {
                return value;
            }
            else
            {
                var response = await GetAsync(requestUri);
                if (getCache.ContainsKey(requestUri))
                {
                    getCache.Remove(requestUri);
                }
                getCache.Add(requestUri, response);
                return response;
            }
        }
    }
}
