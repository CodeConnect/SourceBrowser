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
                    Document(
                        @"
                        class C1
                        {{
                        }}")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's five tokens
            Assert.IsTrue(documentModel.Tokens.Count == 5);
        }

        [TestMethod]
        public void BasicLinking()
        {
            var solution = base.Solution(
             Project(
                 ProjectName("Project1"),
                 Sign,
                 Document(
                    @"
                    class C1
                    {
                        public void Method1()
                        {
                            Method2();
                        }
                        public void Method2()
                        {
                            Method1();
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's two links
            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            Assert.IsTrue(links.Count() == 2);

            //Make sure they're both symbol links
            Assert.IsTrue(links.First() is SymbolLink);
            Assert.IsTrue(links.Last() is SymbolLink);

            //Make sure they link correctly
            Assert.IsTrue(((SymbolLink)links.First()).ReferencedSymbolName == "C1.Method2()");
            Assert.IsTrue(((SymbolLink)links.Last()).ReferencedSymbolName == "C1.Method1()");
        }
    }
}
