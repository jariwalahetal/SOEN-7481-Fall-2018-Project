using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOEN7481.Utility.ResponseHelper
{
    public class MilestoneResponse
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string labels_url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Creator creator { get; set; }
        public int open_issues { get; set; }
        public int closed_issues { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object due_on { get; set; }
        public DateTime? closed_at { get; set; }
    }
}
