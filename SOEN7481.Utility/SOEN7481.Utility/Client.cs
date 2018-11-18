using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOEN7481.Utility.ResponseHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SOEN7481.Utility
{
    class Client
    {
        private String RepositoryName { get; set; }
        private String OwnerName { get; set; }

        public Client(string ownerName, string repositoryName)
        {
            RepositoryName = repositoryName;
            OwnerName = ownerName;
        }

        public async Task<int> GetCommitsCountsWithErrorAsync()
        {

            List<CommitResponse> allCommits = new List<CommitResponse>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "408c951acfa303f4a6e0a8b0118dba9aab616f74");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/commits{2}", OwnerName, RepositoryName, queryString);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var pagedCommits = JsonConvert.DeserializeObject<List<CommitResponse>>(s);
                    if (pagedCommits != null || pagedCommits.Count > 0)
                    {
                        List<string> shaList = allCommits.Select(x => x.sha).ToList();
                        var newCommits = pagedCommits.FindAll(x => !shaList.Contains(x.sha));
                        allCommits.AddRange(
                           newCommits
                            );
                        string lastCommit = pagedCommits[pagedCommits.Count - 1].commit.committer.date;
                        queryString = "?until=" + lastCommit;
                        if (newCommits != null && newCommits.Count > 0)
                            continue;
                    }
                }
                break;

            }
            //Get commits count with error, bug or fix message
            var bugFixingCommits = allCommits.FindAll(c =>
            {
                return c.commit.message.ToLower().Contains("error") ||
                        c.commit.message.ToLower().Contains("bug") ||
                        c.commit.message.ToLower().Contains("fix");
            });

            return bugFixingCommits.Count;
        }

        public async Task<int> GetCoreDeveloerCountAsync()
        {
            Program.PrintInfo("Getting admin members");
            List<string> adminMembers = await GetAdminMembersAsync();
            Program.PrintInfo("Getting pull request accepted members");
            List<string> pullRequestsAcceptedMembers = await GetPullRequestAcceptedLoginIdAsync();
            Program.PrintInfo("Getting issues closed members");
            List<string> issuesClosedMembers = await GetIssuesClosedLoginIdAsync();

            var finalList = adminMembers.Union(pullRequestsAcceptedMembers).Union(issuesClosedMembers).Distinct();
            return finalList.Count();
        }

        private async Task<List<string>> GetAdminMembersAsync()
        {
            List<CommitResponse> allCommits = new List<CommitResponse>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "408c951acfa303f4a6e0a8b0118dba9aab616f74");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            while (true)
            {
                String path = String.Format("orgs/{0}/members?role=admin", OwnerName);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var members = JsonConvert.DeserializeObject<List<MemberResponse>>(s);
                    return members.Select(x => x.id.ToString()).ToList();
                }

            }
            return new List<string>();
        }

        private async Task<List<string>> GetPullRequestAcceptedLoginIdAsync()
        {
            List<string> coreDevelopersIds = new List<string>();
            List<PullRequestResponse> allRequests = new List<PullRequestResponse>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "408c951acfa303f4a6e0a8b0118dba9aab616f74");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            int pageno = 1;
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/pulls?state=closed&page={2}", OwnerName, RepositoryName, pageno++);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var pullRequests = JsonConvert.DeserializeObject<List<PullRequestResponse>>(s);
                    if (pullRequests != null && pullRequests.Count > 0)
                    {
                        allRequests.AddRange(pullRequests);
                       
                        continue;
                    }
                }
                break;
            }

            foreach ( var request in allRequests)
            {
                String path = String.Format("repos/{0}/{1}/commits/{2}", OwnerName, RepositoryName, request.merge_commit_sha);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var commitRequest = JsonConvert.DeserializeObject<CommitResponse>(s);
                    if (commitRequest?.sha == request?.merge_commit_sha)
                    {
                        if (commitRequest?.author?.id != commitRequest?.committer?.id)
                        {
                            coreDevelopersIds.Add(commitRequest.committer.id.ToString());
                        }
                    }
                }
            }

            return coreDevelopersIds;
        }

        private async Task<List<string>> GetIssuesClosedLoginIdAsync()
        {
            List<string> coreDevelopersIds = new List<string>();

            List<IssuesResponse> allClosedIssues = new List<IssuesResponse>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "408c951acfa303f4a6e0a8b0118dba9aab616f74");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");

            List<string> milestones = await GetMilestones();

            foreach (var milestone in milestones)
            {
                int pageno = 1;
                while (true)
                {
                    String path = String.Format("repos/{0}/{1}/issues?state=closed&page={2}&milestone={3}", OwnerName, RepositoryName, pageno++, milestone);

                    HttpResponseMessage response = await client.GetAsync(path);
                    if (response.IsSuccessStatusCode)
                    {
                        string s = await response.Content.ReadAsStringAsync();
                        var closedIssues = JsonConvert.DeserializeObject<List<IssuesResponse>>(s);
                        if (closedIssues != null && closedIssues.Count > 0)
                        {
                            allClosedIssues.AddRange(closedIssues);
                            continue;
                        }
                    }
                    break;
                }
            }

            foreach (var request in allClosedIssues)
            {
                String path = String.Format("repos/{0}/{1}/issues/{2}", OwnerName, RepositoryName, request.number);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var issueResponse = JsonConvert.DeserializeObject<ClosedIssueResponse>(s);
                    if (issueResponse.user.id != issueResponse.closed_by.id)
                    {
                        coreDevelopersIds.Add(issueResponse.closed_by.id.ToString());
                    }
                }
            }
            return coreDevelopersIds;
        }

        private async Task<List<string>> GetMilestones()
        {
            List<string> milestoneNumbers = new List<string>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "408c951acfa303f4a6e0a8b0118dba9aab616f74");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            int pageno = 1;
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/milestones?state=all&page={2}", OwnerName, RepositoryName, pageno++);

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var milestones = JsonConvert.DeserializeObject<List<MilestoneResponse>>(s);
                    if (milestones != null && milestones.Count > 0)
                    {
                        milestoneNumbers.AddRange(milestones.Select(m => m.number.ToString()));
                        continue;
                    }
                }
                break;
            }
            milestoneNumbers.Add("none");
            return milestoneNumbers;
        }
    }
}