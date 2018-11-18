using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOEN7481.Utility.ResponseHelper
{
   

    public class PullRequest
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string diff_url { get; set; }
        public string patch_url { get; set; }
    }

    public class IssuesResponse
    {
        public string url { get; set; }
        public string repository_url { get; set; }
        public string labels_url { get; set; }
        public string comments_url { get; set; }
        public string events_url { get; set; }
        public string html_url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public User user { get; set; }
        public List<object> labels { get; set; }
        public string state { get; set; }
        public bool locked { get; set; }
        public Assignee assignee { get; set; }
        public List<object> assignees { get; set; }
        public object milestone { get; set; }
        public int comments { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime closed_at { get; set; }
        public string author_association { get; set; }
        public string body { get; set; }
        public PullRequest pull_request { get; set; }
    }
}
