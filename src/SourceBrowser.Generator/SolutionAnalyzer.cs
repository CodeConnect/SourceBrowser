using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using SourceBrowser.Generator.Extensions;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Generator
{
    public class SolutionAnalayzer
    {
        MSBuildWorkspace _workspace;
        Solution _solution;
        private ReferencesourceLinkProvider _refsourceLinkProvider = new ReferencesourceLinkProvider();
        string _saveDirectory = string.Empty;

        public SolutionAnalayzer(string solutionPath)
        {
            _workspace = MSBuildWorkspace.Create();
            _workspace.WorkspaceFailed += _workspace_WorkspaceFailed;
            _solution = _workspace.OpenSolutionAsync(solutionPath).Result;
            _refsourceLinkProvider.Init();
        }

        private void _workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            try
            {
                var logDirectory = System.Web.Hosting.HostingEnvironment.MapPath("/WorkspaceLogs/");
                if (logDirectory == null)
                {
                    // If we are not running within a web server, logDirectory will be null.
                    // Whoever invoked this SolutionAnalyze will handle this issue.
                    var wrapperException = new Exception();
                    wrapperException.Data["Diagnostic"] = e.Diagnostic;
                    throw wrapperException;
                }

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
                var logPath = logDirectory + "log.txt";
                using (var sw = new StreamWriter(logPath))
                {
                    sw.Write(e);
                }
            }
            catch
            {
                // All issues with logging are rethrown.
                throw;
            }
        }

        public WorkspaceModel BuildWorkspaceModel(string saveDirectory)
        {
            string solutionName = Path.GetFileName(_solution.FilePath);
            WorkspaceModel workspaceModel = new WorkspaceModel(solutionName);
            //Build document model for every file.
            foreach (var doc in _solution.Projects.SelectMany(n => n.Documents))
            {
                buildDocumentModel(workspaceModel, doc);
            }

            return workspaceModel;
        }

        private void buildDocumentModel(WorkspaceModel workspaceModel, Document document)
        {
            var syntaxRoot = document.GetSyntaxRootAsync().Result;
            var containingFolder = findDocumentParent(workspaceModel, document);
            var docWalker = new DocumentWalker(containingFolder, document, _refsourceLinkProvider);
            docWalker.Visit(syntaxRoot);
            
            //Save it
            var documentModel = docWalker.DocumentModel;
            containingFolder.Children.Add(documentModel);
        }

        private IProjectItem findDocumentParent(WorkspaceModel workspaceModel, Document document)
        {
            IProjectItem currentNode = workspaceModel;
            foreach (var folder in document.Folders)
            {
                var childFolder = currentNode.Children.Where(n => n.Name == folder).SingleOrDefault();
                if (childFolder == null)
                {
                    childFolder = new FolderModel(currentNode, folder);
                    currentNode.Children.Add(childFolder);
                }
                currentNode = childFolder;
            }
            return currentNode;
        }
    }
}
