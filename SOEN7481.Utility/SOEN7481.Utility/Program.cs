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

        static void Main(string[] args)
        {
            getProjectList();

            Task t = new Task(async () => {
                await getGitHubInfoAsync();
            });
            t.Start();

            Console.ReadLine();
        }

        private static void getProjectList() {
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
            catch (Exception ex) {
                PrintError(ex.Message);
            }
        }

        private static async System.Threading.Tasks.Task getGitHubInfoAsync() {

            try
            {
                PrintInfo("Gettng data from github...");
                //Get information from Github
                for (int i = 0; i < repoName.Count; i++)
                {
                    Client c = new Client(ownerlogin[i], repoName[i]);
                    int coreBugs = await c.GetCommitsCountsWithErrorAsync();
                    numberOfCoreBugs.Add(coreBugs);
                    PrintInfo(repoName[i] + " - Number of bugs" + coreBugs);
                }

                printResult();
            }
            catch (Exception ex) {
                PrintInfo(ex.Message);
            }
        }

        private static void printResult() {
            PrintInfo("Generating result...");

            var csv = new StringBuilder();
            csv.AppendLine("repo_name,owner_login,core_bugs,test");

            for (int i = 0; i < repoName.Count; i++)
            {
                var newLine = string.Format("{0},{1},{2},{3}", repoName[i], ownerlogin[i], numberOfCoreBugs[i], "11");
                csv.AppendLine(newLine);
            }

            File.WriteAllText(resultPath, csv.ToString());
            PrintInfo("Result generated...");
        }

        private static void PrintInfo(String str) {
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
