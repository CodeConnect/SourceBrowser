namespace SourceBrowser.Site.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using SourceBrowser.Generator;
    using SourceBrowser.Site.Models;

    internal static class BrowserRepository
    {
        private static readonly string StaticHtmlAbsolutePath = System.Web.Hosting.HostingEnvironment.MapPath("~/SB_Files/");

        // TODO: DO NOT LOAD THIS INTO MEMORY.
        // This string could be really big. We should probably be distributing this
        // from a CDN. However, we're not saving them to a CDN, so we'll have to
        // figure that out first.
        internal static string GetDocumentHtml(string username, string repository, string path)
        {
            var fullPath = Path.Combine(StaticHtmlAbsolutePath, username, repository, path);

            using(var sr = new StreamReader(fullPath))
            {
                var rawHtml = sr.ReadToEnd();
                return rawHtml;
            }
        }

        internal static JObject GetMetaData(string username, string repository, string path)
        {
            var fullPath = Path.Combine(StaticHtmlAbsolutePath, username, repository, path);
            var metadataPath = fullPath + ".json";

            using(var sr = new StreamReader(metadataPath))
            {
                var metadata = JObject.Parse(sr.ReadToEnd());
                return metadata;
            }
        }

        /// <summary>
        /// Returns a list of all Github users on file.
        /// </summary>
        /// <returns>
        /// All Github users.
        /// </returns>
        public static List<string> GetAllGithubUsers()
        {
            // If, for some reason the directory doesn't exist, just return an empty list
            if (!Directory.Exists(StaticHtmlAbsolutePath))
            {
                return new List<string>();
            }

            // Otherwise, find them all
            var directories = Directory.GetDirectories(StaticHtmlAbsolutePath);
            var userNames = new List<string>(directories.Length);
            userNames.AddRange(directories.Select(Path.GetFileName));
            return userNames;
        }

        /// <summary>
        /// Returns a structure containing information on user's github repositories available at Source Browser
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <returns>The github structure.</returns>
        internal static GithubUserStructure SetUpUserStructure(string userName)
        {
            var repoPath = Path.Combine(StaticHtmlAbsolutePath, userName);

            List<string> repoNames;
            if (Directory.Exists(repoPath))
            {
                var directories = Directory.GetDirectories(repoPath);
                repoNames = new List<string>(directories.Length);
                repoNames.AddRange(directories.Select(Path.GetFileName));
            }
            else
            {
                // If, for some reason the directory doesn't exist, just supply an empty list
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
            var solutionPath = Path.Combine(StaticHtmlAbsolutePath, userName, repoName);

            List<string> solutionNames;
            if (Directory.Exists(solutionPath))
            {
                var directories = Directory.GetDirectories(solutionPath);
                solutionNames = new List<string>(directories.Length);
                solutionNames.AddRange(directories.Select(Path.GetFileName));
            }
            else
            {
                // If, for some reason the directory doesn't exist, just supply an empty list
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
    var viewModel = new GithubSolutionStructure()
            {
                Name = solutionName,
                RelativePath = CreatePath(userName, repoName, solutionName),
                RelativeRootPath = CreatePath(userName, repoName, solutionName),
                ParentRepo = SetUpRepoStructure(userName, repoName)
            };

            return viewModel;
        }

        internal static GithubFileStructure SetUpFileStructure(string userName, string repoName, string path, string html, int numLines)
        {
            var viewModel = new GithubFileStructure
            {
                FileName = Path.GetFileName(path),
                Directory = GetRelativeDirectory(path),
                RelativePath = CreatePath(userName, repoName, GetRelativeDirectory(path)), // Used to expand nodes leading to this file
                RelativeRootPath = CreatePath(userName, repoName, path), // Points to the root of the treeview
                SourceCode = html,
                NumberOfLines = numLines
            };

            return viewModel;
        }

        public static string GetRelativeDirectory(string path)
        {
            var fileName = Path.GetFileName(path);
            if (fileName != null)
            {
                var baseDirectory = path.Substring(0, path.Length - fileName.Length);
                return baseDirectory;
            }
            return null;
        }

        public static List<string> FindFiles(string path)
        {
            var fullPath = Path.Combine(StaticHtmlAbsolutePath, path);
            var files = new List<string>();
            if (!Directory.Exists(fullPath))
            {
                return files;
            }

            var filePaths = Directory.GetFiles(fullPath);
            files = new List<string>(filePaths);
            files = files.Where(n => n != "solutionInfo.json").ToList();
            return files;
        }

        internal static List<string> FindFolders(string path)
        {
            var fullPath = Path.Combine(StaticHtmlAbsolutePath, path);
            var directories = new List<string>();
            if (!Directory.Exists(fullPath))
            {
                return directories;
            }

            var directoryPaths = Directory.GetDirectories(fullPath);
            directories = new List<string>(directoryPaths);
            return directories;
        }


                                                                                                                        

        private static string CreatePath(string part1, string part2 = null, string part3 = null, string part4 = null)
        {
            string[] pathParts;
            if (part4 != null)
            {
                pathParts = new[] { part1, part2, part3, part4 };
            }
            else if (part3 != null)
            {
                pathParts = new[] { part1, part2, part3 };
            }
            else if (part2 != null)
            {
                pathParts = new[] { part1, part2 };
            }
            else
            {
                pathParts = new[] { part1 };
            }

            return string.Join("/", pathParts);
        }
    }
}