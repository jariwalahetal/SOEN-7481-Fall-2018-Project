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
            try
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
                    String path = String.Format("repos/{0}/{1}/commits{2}", OwnerName, RepositoryName,queryString);

                    HttpResponseMessage response = await client.GetAsync(path);
                    if (response.IsSuccessStatusCode)
                    {
                        string s = await response.Content.ReadAsStringAsync();
                        var pagedCommits = JsonConvert.DeserializeObject<List<CommitResponse>>(s);
                        if (pagedCommits != null || pagedCommits.Count > 0) {
                            List<string> shaList = allCommits.Select(x => x.sha).ToList();
                            var newCommits = pagedCommits.FindAll(x => !shaList.Contains(x.sha));
                            allCommits.AddRange(
                               newCommits
                                );
                            string lastCommit = pagedCommits[pagedCommits.Count - 1].commit.committer.date;
                             queryString = "?until=" + lastCommit;
                            if(newCommits!=null && newCommits.Count >0)
                                continue;
                        }
                    }
                    break;

                }
                //Get commits count with error, bug or fix message
                var bugFixingCommits = allCommits.FindAll(c => {
                    return c.commit.message.ToLower().Contains("error") ||
                            c.commit.message.ToLower().Contains("bug") ||
                            c.commit.message.ToLower().Contains("fix");
                });

                return bugFixingCommits.Count;
            }
            catch (Exception ex)
            {
            }
            return 0;
        }


        private async Task<List<string>> GetAdminMembersAsync()
        {
            try
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
                
            }
            catch (Exception ex)
            {
            }
            return new List<string>();
        }

        private async Task<List<string>> GetPullRequestAcceptedLoginIdAsync()
        {
            try
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

            }
            catch (Exception ex)
            {
            }
            return new List<string>();
        }
    }
}