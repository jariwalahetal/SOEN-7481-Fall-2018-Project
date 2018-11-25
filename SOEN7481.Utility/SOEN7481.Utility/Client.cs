using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOEN7481.Utility.ResponseHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SOEN7481.Utility
{
    class Client
    {
        private String RepositoryName { get; set; }
        private String OwnerName { get; set; }

        List<PullRequestResponse> allRequests = new List<PullRequestResponse>();
        List<CommitResponse> allCommits = new List<CommitResponse>();

        List<string> requestMergedBy = new List<string>();
        private List<string> coreDevelopers;

        static List<string> tokens = new List<string> {
            "7b84ebec3dd096e3c24a5277bc0c01229bff31a2",
        };

        static int tokenId = 0;

        public Boolean breakOperation = false;
        public Client(string ownerName, string repositoryName)
        {
            RepositoryName = repositoryName;
            OwnerName = ownerName;
        }

        public async Task<int> GetCommitsCountsWithErrorAsync()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[tokenId]);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("shriyans16");
            string queryString = "";
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/commits{2}", OwnerName, RepositoryName, queryString);

                HttpResponseMessage response = await client.GetAsync(path);
                if (!validateRateLimit(response))
                {
                    breakOperation = true;
                    return -1;
                }
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
            if (breakOperation)
                return -1;

            Program.PrintInfo("Getting pull request accepted members");
            List<string> pullRequestsAcceptedMembers = await GetPullRequestAcceptedLoginIdAsync();
            if (breakOperation)
                return -1;

            Program.PrintInfo("Getting issues closed members");
            List<string> issuesClosedMembers = await GetIssuesClosedLoginIdAsync();
            if (breakOperation)
                return -1;

            coreDevelopers = adminMembers.Union(pullRequestsAcceptedMembers).Union(issuesClosedMembers).Distinct().ToList();
         
            return coreDevelopers.Count();
        }

        private async Task<List<string>> GetAdminMembersAsync()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[tokenId]);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            while (true)
            {
                String path = String.Format("orgs/{0}/members?role=admin", OwnerName);

                HttpResponseMessage response = await client.GetAsync(path);
                if (!validateRateLimit(response))
                {
                    breakOperation = true;
                    return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    string s = await response.Content.ReadAsStringAsync();
                    var members = JsonConvert.DeserializeObject<List<MemberResponse>>(s);
                    return members.Select(x => x.id.ToString()).ToList();
                }

            }
        }

        private async Task<List<string>> GetPullRequestAcceptedLoginIdAsync()
        {
            List<string> coreDevelopersIds = new List<string>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept", " application/vnd.github.inertia-preview+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[tokenId]);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            int pageno = 1;
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/pulls?state=closed&page={2}", OwnerName, RepositoryName, pageno++);

                HttpResponseMessage response = await client.GetAsync(path);
                if (!validateRateLimit(response))
                {
                    breakOperation = true;
                    return null;
                }
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

            Program.PrintInfo("Processing pull requests..");
            //If merged_at is null then PR is rejected
            int index = 0;
            foreach ( var request in allRequests)
            {
                var commit = allCommits.FirstOrDefault(c => c.sha == request.merge_commit_sha);
                if (commit != null)
                {
                    requestMergedBy.Add(commit?.author?.id.ToString());
                    if (commit?.sha == request?.merge_commit_sha)
                    {
                        if (commit?.author?.id != commit?.committer?.id)
                        {
                            coreDevelopersIds.Add(commit.committer.id.ToString());
                        }
                    }
                }
                else {
                    requestMergedBy.Add("");
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[tokenId]);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");

            List<string> milestones = await GetMilestones();

            foreach (var milestone in milestones)
            {
                int pageno = 1;
                while (true)
                {
                    String path = String.Format("repos/{0}/{1}/issues?state=closed&page={2}&milestone={3}", OwnerName, RepositoryName, pageno++, milestone);

                    HttpResponseMessage response = await client.GetAsync(path);
                    if (!validateRateLimit(response))
                    {
                        breakOperation = true;
                    }
                    else
                    {
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
                    }
                    break;
                }
            }

            foreach (var request in allClosedIssues)
            {
                String path = String.Format("repos/{0}/{1}/issues/{2}", OwnerName, RepositoryName, request.number);

                HttpResponseMessage response = await client.GetAsync(path);
                if (!validateRateLimit(response))
                {
                    breakOperation = true;
                    return null;
                }
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[tokenId]);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("jariwalahetal");
            string queryString = "";
            int pageno = 1;
            while (true)
            {
                String path = String.Format("repos/{0}/{1}/milestones?state=all&page={2}", OwnerName, RepositoryName, pageno++);

                HttpResponseMessage response = await client.GetAsync(path);
                if (!validateRateLimit(response))
                {
                    breakOperation = true;
                    break;
                }
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

        public int GetTotalPullRequestCount() {
            return allRequests.Count();
        }

        public int GetTotalMergedPullRequestCount()
        {
            return allRequests.Where(x => !String.IsNullOrEmpty(x.merged_at)).Count();
        }

        public int GetTotalRejectedPullRequestCount()
        {
            return allRequests.Where(x => String.IsNullOrEmpty(x.merged_at)).Count();
        }

        public int GetTotalMergedRequestByCoreDevelopersCount()
        {
            int count = 0;
            for (int i = 0; i < allRequests.Count(); i++) {
                if (!String.IsNullOrEmpty(allRequests[i].merged_at) && coreDevelopers.Contains(requestMergedBy[i])) {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalRejectedRequestByCoreDevelopersCount()
        {
            int count = 0;
            for (int i = 0; i < allRequests.Count(); i++)
            {
                if (String.IsNullOrEmpty(allRequests[i].merged_at) && coreDevelopers.Contains(requestMergedBy[i]))
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalMergedRequestByNonCoreDevelopersCount()
        {
            int count = 0;
            for (int i = 0; i < allRequests.Count(); i++)
            {
                if (!String.IsNullOrEmpty(allRequests[i].merged_at) && !coreDevelopers.Contains(requestMergedBy[i]))
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalRejectedRequestByNonCoreDevelopersCount()
        {
            int count = 0;
            for (int i = 0; i < allRequests.Count(); i++)
            {
                if (String.IsNullOrEmpty(allRequests[i].merged_at) && !coreDevelopers.Contains(requestMergedBy[i]))
                {
                    count++;
                }
            }
            return count;
        }

        private bool validateRateLimit(HttpResponseMessage response)
        {
            string headers = response.Headers.ToString();

            int pendingRequests = Convert.ToInt32(headers.Substring(headers.IndexOf("X-RateLimit-Remaining: ") + 23).Substring(0, headers.Substring(headers.IndexOf("X-RateLimit-Remaining: ") + 23).IndexOf("\r\n")));

            double limitRest = Convert.ToDouble(headers.Substring(headers.IndexOf("X-RateLimit-Reset: ") + 19).Substring(0, headers.Substring(headers.IndexOf("X-RateLimit-Reset: ") + 19).IndexOf("\r\n")));

            DateTime resumeTime = FromUnixTime(limitRest);
            if (pendingRequests < 5)
            {
                return false;
            }
            return true;
        }

        private DateTime FromUnixTime(double limitRest)
        {
            return epoch.AddSeconds(limitRest);
        }
        
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}