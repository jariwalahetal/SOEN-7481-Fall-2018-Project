using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SOEN7481.Utility
{
    class Program
    {

        static String filePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\projects-list.csv";
        static String resultPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\result.csv";
        static List<string> repoName = new List<string>();
        static List<string> ownerlogin = new List<string>();
        static List<int> numberOfCoreBugs = new List<int>();
        static List<int> coreDevelopersCounts = new List<int>();
        static List<int> mergedRequestCounts = new List<int>();
        static List<int> rejectedRequestsCounts = new List<int>();
        static List<int> mergedRequestByCoreDevelopersCount = new List<int>();
        static List<int> rejectedRequestByCoreDevelopersCount = new List<int>();
        static List<int> mergedRequestByNonCoreDevelopersCount = new List<int>();
        static List<int> rejectedRequestByNonCoreDevelopersCount = new List<int>();

        static void Main(string[] args)
        {
            getProjectList();

            Task t = new Task(async () => {
                await getGitHubInfoAsync();
            });
            t.Start();

            Console.ReadLine();
        }

        private static void getProjectList()
        {
            try
            {
                PrintInfo("Reading project namaes...");
                //read repo and project name
                using (var reader = new StreamReader(filePath))
                {
                    reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        repoName.Add(values[0]);
                        ownerlogin.Add(values[1]);

                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
        }

        private static async System.Threading.Tasks.Task getGitHubInfoAsync()
        {
            int counter = 0;
            try
            {
                PrintInfo("Gettng data from github...");
                //Get information from Github
                for (int i = counter; i < repoName.Count; i++)
                {
                    PrintInfo("Current reponame - " + repoName[i]);

                    Client c = new Client(ownerlogin[i], repoName[i]);

                    PrintInfo("Getting core bugs");
                    int coreBugs = await c.GetCommitsCountsWithErrorAsync();

                    if (c.breakOperation)
                    {
                        break;
                    }
                    numberOfCoreBugs.Add(coreBugs);
                    PrintInfo(repoName[i] + " - Number of bugs:" + coreBugs);

                    PrintInfo("Getting core developers");
                    int count = await c.GetCoreDeveloerCountAsync();
                    if (c.breakOperation)
                    {
                        break;
                    }
                    coreDevelopersCounts.Add(count);
                    PrintInfo(repoName[i] + " - Core developers count:" + count);

                    mergedRequestCounts.Add(c.GetTotalMergedPullRequestCount());
                    rejectedRequestsCounts.Add(c.GetTotalRejectedPullRequestCount());

                    mergedRequestByCoreDevelopersCount.Add(c.GetTotalMergedRequestByCoreDevelopersCount());
                    rejectedRequestByCoreDevelopersCount.Add(c.GetTotalRejectedRequestByCoreDevelopersCount());

                    mergedRequestByNonCoreDevelopersCount.Add(c.GetTotalMergedRequestByNonCoreDevelopersCount());
                    rejectedRequestByNonCoreDevelopersCount.Add(c.GetTotalRejectedRequestByNonCoreDevelopersCount());
                    counter++;
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }

            printResult(counter);
        }

        private static void printResult(int count)
        {
            PrintInfo("Generating result...");

            var csv = new StringBuilder();
            csv.AppendLine("repo_name,owner_login,core_bugs,core_developer,total_merged_pr,total_rejected_pr,merged_pr_by_core_developer,rejected_pr_by_core_developers,merged_pr_by_ext_developer,rejected_pr_by_ext_developer");

            for (int i = 0; i < rejectedRequestByNonCoreDevelopersCount.Count; i++)
            {
                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", 
                            repoName[i], 
                            ownerlogin[i], 
                            numberOfCoreBugs[i], 
                            coreDevelopersCounts[i],
                            mergedRequestCounts[i],
                            rejectedRequestsCounts[i],
                            mergedRequestByCoreDevelopersCount[i],
                            rejectedRequestByCoreDevelopersCount[i],
                            mergedRequestByNonCoreDevelopersCount[i],
                            rejectedRequestByNonCoreDevelopersCount[i]);
                csv.AppendLine(newLine);
            }

            File.WriteAllText(resultPath, csv.ToString());
            PrintInfo("Result generated...");
        }

        public static void PrintInfo(String str)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        private static void PrintError(String str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
        }

    }
}

