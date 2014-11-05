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
        WorkspaceModel _workspaceModel;
        SolutionFolderAnalyzer _folderAnalyzer;
        private ReferencesourceLinkProvider _refsourceLinkProvider = new ReferencesourceLinkProvider();
        string _saveDirectory = string.Empty;
        const string solutionInfoFileName = "solutionInfo.json";

        public SolutionAnalayzer(string solutionPath)
        {
            _workspace = MSBuildWorkspace.Create();
            _workspace.WorkspaceFailed += _workspace_WorkspaceFailed;
            _solution = _workspace.OpenSolutionAsync(solutionPath).Result;
            _folderAnalyzer = new SolutionFolderAnalyzer(_solution);
            string solutionName = Path.GetFileName(solutionPath);
            _workspaceModel = new WorkspaceModel(solutionName);

            _refsourceLinkProvider.Init().Wait();
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

        public void AnalyzeAndSave(string saveDirectory)
        {
            _saveDirectory = saveDirectory;

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            //Generate solution/folder info
            var solutionInfo = this.GenerateFolderStructureAsJson();
            string solutionInfoPath = Path.Combine(_saveDirectory, solutionInfoFileName);

            using (StreamWriter stream = new StreamWriter(solutionInfoPath, append: false))
            {
                stream.Write(solutionInfo);
            }

            foreach (var doc in _solution.Projects.SelectMany(n => n.Documents))
            {
                //Generate info
                string url = doc.GetRelativeFilePath();
                string folderPath = Path.Combine(_saveDirectory, doc.GetContainingFolderPath());
                string fullPath = Path.Combine(_saveDirectory, url);
                var docInfo = buildDocumentInfo(doc);

                if (!Directory.Exists(folderPath))
                {
                    DirectoryInfo di = Directory.CreateDirectory(folderPath);
                }

                var jsonDocInfo = JsonConvert.SerializeObject(docInfo);

                //Save it
                using (var sw = new StreamWriter(fullPath, append: false))
                {
                    sw.Write(jsonDocInfo);
                }
            }
        }

        private DocumentModel buildDocumentInfo(Document document)
        {
            //var root = document.GetSyntaxRootAsync().Result;
            //var docWalker = new DocumentWalker(document, _refsourceLinkProvider);
            //docWalker.Visit(root);

            return null;
        }

        /// <summary>
        /// Returns the folder structure for the solution being analyzed.
        /// </summary>
        public string GenerateFolderStructureAsJson()
        {
            var root = _folderAnalyzer.AnalzeSolutionStructure();
            string json = JsonConvert.SerializeObject(root, Formatting.Indented);
            return json;
        }

    }
}
