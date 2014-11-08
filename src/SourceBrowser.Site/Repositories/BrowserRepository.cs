using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SourceBrowser.Generator;
using SourceBrowser.Site.Models;

namespace SourceBrowser.Site.Repositories
{
    internal static class BrowserRepository
    {
        static string _staticHtmlAbsolutePath = System.Web.Hosting.HostingEnvironment.MapPath("~/SB_Files/");

        /// <summary>
        /// Parses provided string and tries to retrieve:
        /// github user, repo, solution name, file within solution
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool GetFolderInfo(string id, out string githubUser, out string githubRepo, out string solutionName, out string fileName)
        {
            githubUser = String.Empty;
            githubRepo = String.Empty;
            solutionName = String.Empty;
            fileName = String.Empty;
            if (String.IsNullOrEmpty(id))
            {
                return false;
            }
            var pathParts = id.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 0)
            {
                return false;
            }
            if (pathParts.Length >= 1)
            {
                githubUser = pathParts[0];
            }
            if (pathParts.Length >= 2)
            {
                githubRepo = pathParts[1];
            }
            if (pathParts.Length >= 3)
            {
                solutionName = pathParts[2];
            }
            if (pathParts.Length >= 4)
            {
                fileName = String.Join("/", pathParts.Skip(3));
            }
            return true;
        }

        /// <summary>
        /// Returns a list of all Github users on file.
        /// </summary>
        public static List<string> GetAllGithubUsers()
        {
            //If, for some reason the directory doesn't exist, just return an empty list
            if (!Directory.Exists(_staticHtmlAbsolutePath))
                return new List<string>();

            //Otherwise, find them all
            var directories = Directory.GetDirectories(_staticHtmlAbsolutePath);
            List<string> userNames = new List<string>(directories.Length);
            foreach (var directory in directories)
            {
                userNames.Add(System.IO.Path.GetFileName(directory));
            }
            return userNames;
        }

        /// <summary>
        /// Returns a structure containing information on user's github repositories available at Source Browser
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static GithubUserStructure SetUpUserStructure(string userName)
        {
            var repoPath = Path.Combine(_staticHtmlAbsolutePath, userName);

            List<string> repoNames;
            if (Directory.Exists(repoPath))
            {
                var directories = Directory.GetDirectories(repoPath);
                repoNames = new List<string>(directories.Length);
                foreach (var directory in directories)
                {
                    repoNames.Add(Path.GetFileName(directory));
                }
            }
            else
            {
                //If, for some reason the directory doesn't exist, just supply an empty list
                repoNames = new List<string>();
            }

            var viewModel = new GithubUserStructure()
            {
                Username = userName,
                Repos = repoNames,
                Path = repoPath
            };
            return viewModel;
        }

        internal static GithubRepoStructure SetUpRepoStructure(string userName, string repoName)
        {
            var solutionPath = Path.Combine(_staticHtmlAbsolutePath, userName, repoName);

            List<string> solutionNames;
            if (Directory.Exists(solutionPath))
            {
                var directories = Directory.GetDirectories(solutionPath);
                solutionNames = new List<string>(directories.Length);
                foreach (var directory in directories)
                {
                    solutionNames.Add(Path.GetFileName(directory));
                }
            }
            else
            {
                //If, for some reason the directory doesn't exist, just supply an empty list
                solutionNames = new List<string>();
            }

            var viewModel = new GithubRepoStructure()
            {
                Name = repoName,
                Solutions = solutionNames,
                ParentUser = SetUpUserStructure(userName)
            };
            return viewModel;
        }

        internal static GithubSolutionStructure SetUpSolutionStructure(string userName, string repoName, string solutionName)
        {
            string solutionInfoPath;
            var solutionInfo = getSolutionInfo(userName, repoName, solutionName, out solutionInfoPath);

            var viewModel = new GithubSolutionStructure()
            {
                Name = solutionName,
                RelativePath = createPath(userName, repoName, solutionName),
                RelativeRootPath = createPath(userName, repoName, solutionName),
                SolutionInfo = solutionInfo,
                ParentRepo = SetUpRepoStructure(userName, repoName)
            };

            return viewModel;
        }

        internal static GithubFileStructure SetUpFileStructure(DocumentInfo docInfo, string userName, string repoName, string solutionName, string pathRemainder)
        {
            string solutionInfoPath;
            var solutionInfo = getSolutionInfo(userName, repoName, solutionName, out solutionInfoPath);

            var viewModel = new GithubFileStructure
            {
                FileName = Path.GetFileName(pathRemainder),
                Directory = GetRelativeDirectory(pathRemainder),
                RelativePath = createPath(userName, repoName, solutionName, GetRelativeDirectory(pathRemainder)), // Used to expand nodes leading to this file
                RelativeRootPath = createPath(userName, repoName, solutionName), // Points to the root of the treeview
                SourceCode = docInfo.HtmlContent,
                NumberOfLines = docInfo.NumberOfLines,
                SolutionInfo = solutionInfo
            };

            return viewModel;
        }

        private static string createPath(string part1, string part2 = null, string part3 = null, string part4 = null)
        {
            string[] pathParts;
            if (part4 != null)
            {
                pathParts = new string[] { part1, part2, part3, part4 };
            }
            else if (part3 != null)
            {
                pathParts = new string[] { part1, part2, part3 };
            }
            else if (part2 != null)
            {
                pathParts = new string[] { part1, part2 };
            }
            else
            {
                pathParts = new string[] { part1 };
            }
            return String.Join("/", pathParts);
        }

        private static JObject getSolutionInfo(string userName, string repoName, string solutionName, out string solutionInfoPath)
        {
            string absolutePath = Path.Combine(_staticHtmlAbsolutePath, userName, repoName, solutionName);
            solutionInfoPath = Path.Combine(absolutePath, "solutionInfo.json");

            if (!File.Exists(solutionInfoPath))
                return null;

            using (var sr = new StreamReader(solutionInfoPath))
            {
                var rawJson = sr.ReadToEnd();
                var json = JObject.Parse(rawJson);
                return json;
            }
        }

        public static void FindPage(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);

            if(Directory.Exists(fullPath))
            {
                //It's a folder, we want to list the files.
                var files = FindFiles(fullPath);
                var folders = FindFolders(fullPath);
            }

            if (File.Exists(fullPath))
            {
                //It's a file, we want to list the file.
            }
        }

        public static bool IsFile(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);
            if (File.Exists(fullPath))
                return true;

            return false;
        }

        public static bool IsFolder(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);
            if (Directory.Exists(fullPath))
                return true;
            return false;
        }

        public static string GetRootDirectory(string path)
        {
            var splitPath = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var baseDirectory = splitPath.First();
            return baseDirectory;
        }

        public static string GetRelativeDirectory(string path)
        {
            var fileName = Path.GetFileName(path);
            var baseDirectory = path.Substring(0, path.Length - fileName.Length);
            return baseDirectory;
        }

        public static DocumentInfo FindFile(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);
            DocumentInfo docInfo;
            using (var sr = new StreamReader(fullPath))
            {
                string rawJson = sr.ReadToEnd();
                docInfo = JsonConvert.DeserializeObject<DocumentInfo>(rawJson);
            }
            return docInfo;
        }

        public static List<string> FindFiles(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);
            var files = new List<string>();
            if (!Directory.Exists(fullPath))
                return files;

            var filePaths = Directory.GetFiles(fullPath);
            files = new List<string>(filePaths);
            files = files.Where(n => n != "solutionInfo.json").ToList();
            return files;
        }

        public static List<string> FindFolders(string path)
        {
            var fullPath = Path.Combine(_staticHtmlAbsolutePath, path);
            var directories = new List<string>();
            if (!Directory.Exists(fullPath))
                return directories;

            var directoryPaths = Directory.GetDirectories(fullPath);
            directories = new List<string>(directoryPaths);
            return directories;
        }
    }
}