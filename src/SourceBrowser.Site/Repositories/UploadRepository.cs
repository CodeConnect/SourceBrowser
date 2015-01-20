using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.IO;
using System.Configuration;

namespace SourceBrowser.Site.Repositories
{
    public class UploadRepository
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        internal static Generator.Model.WorkspaceModel ProcessSolution(string solutionPath, string repoRootPath)
        {
            SafeTokenHandle safeTokenHandle;
            string safeUserName = ConfigurationManager.AppSettings["safeUserName"];
            string safeUserPassword = ConfigurationManager.AppSettings["safePassword"];

            // When testing, give full trust to the developer's machine
            if (String.IsNullOrEmpty(safeUserName))
            {
                var sourceGenerator = new Generator.SolutionAnalayzer(solutionPath);
                var workspaceModel = sourceGenerator.BuildWorkspaceModel(repoRootPath);
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
                    var workspaceModel = sourceGenerator.BuildWorkspaceModel(repoRootPath);
                    return workspaceModel;
                }
                // Releasing the context object stops the impersonation 
            }
        }

        internal static void SaveReadme(string repoPath, string readmeInHtml)
        {
            string readmePath = Path.Combine(repoPath, "readme.html");
            File.WriteAllText(readmePath, readmeInHtml);
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
}