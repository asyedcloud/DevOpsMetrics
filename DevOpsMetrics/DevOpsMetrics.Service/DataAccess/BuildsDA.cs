﻿using DevOpsMetrics.Core;
using DevOpsMetrics.Service.DataAccess.APIAccess;
using DevOpsMetrics.Service.DataAccess.TableStorage;
using DevOpsMetrics.Service.Models.AzureDevOps;
using DevOpsMetrics.Service.Models.Common;
using DevOpsMetrics.Service.Models.GitHub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevOpsMetrics.Service.DataAccess
{
    public class BuildsDA
    {
        public async Task<List<AzureDevOpsBuild>> GetAzureDevOpsBuilds(string patToken, TableStorageAuth tableStorageAuth,
                string organization, string project, string branch, string buildName, string buildId, bool useCache)
        {
            List<AzureDevOpsBuild> builds = new List<AzureDevOpsBuild>();
            Newtonsoft.Json.Linq.JArray list = null;
            if (useCache == true)
            {
                AzureTableStorageDA daTableStorage = new AzureTableStorageDA();
                list = daTableStorage.GetTableStorageItems(tableStorageAuth, tableStorageAuth.TableAzureDevOpsBuilds, daTableStorage.CreateAzureDevOpsBuildPartitionKey(organization, project, buildName));
            }
            else
            {
                AzureDevOpsAPIAccess api = new AzureDevOpsAPIAccess();
                list = await api.GetAzureDevOpsBuildsJArray(patToken, organization, project, branch, buildId);
            }
            if (list != null)
            {
                builds = JsonConvert.DeserializeObject<List<AzureDevOpsBuild>>(list.ToString());

                //We need to do some post processing and loop over the list a couple times to construct a usable url
                foreach (AzureDevOpsBuild item in builds)
                {
                    item.url = $"https://dev.azure.com/{organization}/{project}/_build/results?buildId={item.id}&view=results";
                }

                //sort the list
                builds = builds.OrderBy(o => o.queueTime).ToList();
            }

            return builds;
        }

        public async Task<List<GitHubActionsRun>> GetGitHubActionRuns(bool getSampleData, string clientId, string clientSecret, TableStorageAuth tableStorageAuth,
                string owner, string repo, string branch, string workflowName, string workflowId, bool useCache)
        {
            List<GitHubActionsRun> runs = new List<GitHubActionsRun>();
            Newtonsoft.Json.Linq.JArray list = null;
            if (useCache == true)
            {
                AzureTableStorageDA daTableStorage = new AzureTableStorageDA();
                list = daTableStorage.GetTableStorageItems(tableStorageAuth, tableStorageAuth.TableGitHubRuns, daTableStorage.CreateGitHubRunPartitionKey(owner, repo, workflowName));
            }
            else
            {
                GitHubAPIAccess api = new GitHubAPIAccess();
                list = await api.GetGitHubActionRunsJArray(clientId, clientSecret, owner, repo, branch, workflowId);
            }
            if (list != null)
            {
                runs = JsonConvert.DeserializeObject<List<GitHubActionsRun>>(list.ToString());

                //sort the list
                runs = runs.OrderBy(o => o.created_at).ToList();
            }

            return runs;
        }

    }
}
