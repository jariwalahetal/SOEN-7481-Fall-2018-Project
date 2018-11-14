using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                await Task.Delay(4);

                return 11;
            }
            catch (Exception ex)
            {
            }
            return 0;
        }
    }
}
