using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.UnitTests.SolutionGeneration;
using Microsoft.CodeAnalysis;
using System.Linq;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator;

namespace SourceBrowser.Tests
{
    [TestClass]
    public class CSharpTests : MSBuildWorkspaceTestBase
    {
        [TestMethod]
        public void SanityCheck()
        {
            //Set up the absolute minimum
            var solution = base.Solution(
                Project(
                    ProjectName("Project1"),
                    Sign,
                    Document(string.Format(
                        @"
                        class C1
                        {{
                        }}", PublicKey))));
            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            Assert.IsTrue(documentModel.Tokens.Count == 5);

        }
    }
}
