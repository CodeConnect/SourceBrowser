using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.IO;
using System.Configuration;
using System.Linq;
using SourceBrowser.Generator.Transformers;
using SourceBrowser.SolutionRetriever;
using System.Threading.Tasks;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Site.Repositories
{
    public class UploadRepository
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        internal static bool ProcessRepo(GitHubRetriever retriever, string repoSourceStagingPath, string parsedRepoPath)
        {
            bool success = true;

            var stagingSolutionPaths = GetSolutionPaths(repoSourceStagingPath);
            if (stagingSolutionPaths.Length == 0)
            {
                throw new NoSolutionsFoundException();
            }

            try
            {
                // Create a WorkspaceModel that will contain every solution found in the repo.
                // This is also the root of the tree view.
                WorkspaceModel rootWorkspaceModel = new WorkspaceModel(parsedRepoPath, repoSourceStagingPath);

                // Create an array to store the resulting workspace models from the solutions.
                WorkspaceModel[] processedWorkspaces = new WorkspaceModel[stagingSolutionPaths.Count()];

                // Parallel execution.
                Parallel.For(0, stagingSolutionPaths.Count(), (i) =>
                {
                    string sln = stagingSolutionPaths[i]; // Get the current solutionPath
                    var workspaceModel = ProcessSolution(retriever, sln, repoSourceStagingPath, parsedRepoPath);
                    processedWorkspaces[i] = workspaceModel; // Set the result
                });

                // Add all the results to the root workspace model.
                foreach (var workspace in processedWorkspaces)
                    rootWorkspaceModel.Children.Add(workspace);

                // Generate HTML of the tree view.
                var treeViewTransformer = new TreeViewTransformer(parsedRepoPath, retriever.UserName, retriever.RepoName);
                treeViewTransformer.Visit(rootWorkspaceModel); // The tree view contains all the processed solution.

                try
                {
                    SaveReadme(parsedRepoPath, retriever.ProvideParsedReadme());
                }
                catch (Exception ex)
                {
                    // TODO: Log and swallow - readme is not essential.
                }
            }
            catch (Exception)
            {
                // TODO: Log this
                success = false;
            }

            return success;
        }

        private static WorkspaceModel ProcessSolution(GitHubRetriever retriever, string solutionPath, string repoSourceStagingPath, string parsedRepoPath)
        {
            var workspaceModel = ParseRepo(solutionPath, repoSourceStagingPath);

            //One pass to lookup all declarations
            var typeTransformer = new TokenLookupTransformer();
            typeTransformer.Visit(workspaceModel);
            var tokenLookup = typeTransformer.TokenLookup;

            //Another pass to generate HTMLs
            var htmlTransformer = new HtmlTransformer(tokenLookup, parsedRepoPath);
            htmlTransformer.Visit(workspaceModel);

            var searchTransformer = new SearchIndexTransformer(retriever.UserName, retriever.RepoName);
            searchTransformer.Visit(workspaceModel);

            return workspaceModel;
        }

        private static WorkspaceModel ParseRepo(string solutionPath, string repoSourceStagingPath)
        {
            SafeTokenHandle safeTokenHandle;
            string safeUserName = ConfigurationManager.AppSettings["safeUserName"];
            string safeUserPassword = ConfigurationManager.AppSettings["safePassword"];

            // When testing, give full trust to the developer's machine
            if (String.IsNullOrEmpty(safeUserName))
            {
                var sourceGenerator = new Generator.SolutionAnalayzer(solutionPath);
                var workspaceModel = sourceGenerator.BuildWorkspaceModel(repoSourceStagingPath);
                return workspaceModel;
            }
            // Otherwise, use impersonation to build the solution as user with low privileges.
            // See http://msdn.microsoft.com/en-us/library/vstudio/w070t6ka.aspx

            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // Call LogonUser to obtain a handle to an access token. 
            bool returnValue = LogonUser(safeUserName, null, safeUserPassword,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeTokenHandle);

            if (false == returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                throw new ApplicationException(String.Format("LogonUser failed with error code : {0}", ret));
            }
            using (safeTokenHandle)
            {
                // Use the token handle returned by LogonUser. 
                using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle()))
                {
                    var sourceGenerator = new Generator.SolutionAnalayzer(solutionPath);
                    var workspaceModel = sourceGenerator.BuildWorkspaceModel(repoSourceStagingPath);
                    return workspaceModel;
                }
                // Releasing the context object stops the impersonation 
            }
        }

        private static void SaveReadme(string repoPath, string readmeInHtml)
        {
            string readmePath = Path.Combine(repoPath, "readme.html");
            File.WriteAllText(readmePath, readmeInHtml);
        }

        /// <summary>
        /// Simply searches for the solution files and returns their paths.
        /// </summary>
        /// <param name="rootDirectory">
        /// The root Directory.
        /// </param>
        /// <returns>
        /// The solution paths.
        /// </returns>
        private static string[] GetSolutionPaths(string rootDirectory)
        {
            return Directory.GetFiles(rootDirectory, "*.sln", SearchOption.AllDirectories);
        }
    }

    sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }

    public class NoSolutionsFoundException : Exception
    {
    }
}